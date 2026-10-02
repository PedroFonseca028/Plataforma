using UnityEngine;

namespace Curupira
{
    /// <summary>
    /// O Curupira: movimento, pulo (com "coyote time" e buffer), pisão nos
    /// inimigos, assobio mágico, dano e pegadas viradas para trás.
    /// </summary>
    public class Jogador : MonoBehaviour
    {
        public static Jogador Atual { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            Atual = null;
        }

        // ---- ajustes de movimento
        const float Velocidade = 7f;
        const float AceleracaoChao = 70f;
        const float AceleracaoAr = 45f;
        const float Gravidade = 4.3f;          // gravityScale
        const float MultiplicadorQueda = 1.6f;
        const float ForcaPulo = 17.8f;
        const float QuedaMaxima = 18f;
        const float TempoCoyote = 0.1f;
        const float TempoBuffer = 0.12f;
        const float ForcaQuique = 14f;

        // ---- assobio
        public const float RecargaAssobio = 4f;
        const float RaioAssobio = 4.5f;
        const float TempoTonto = 3.5f;

        Rigidbody2D rb;
        BoxCollider2D col;
        SpriteRenderer sr;
        AnimadorSprite anim;
        Transform visual;

        float entradaH;
        float timerCoyote;
        float timerBuffer;
        bool noChao;
        bool cortouPulo;
        bool olhandoDireita = true;
        float timerInvulneravel;
        float timerSemControle;
        float timerAssobioAnim;
        float timerPegada;
        float timerDescer;
        Collider2D plataformaIgnorada;
        PlataformaMovel plataformaMovel;
        bool comemorando;
        float yAntes;

        public bool Vivo { get; private set; }
        public bool NoChao
        {
            get { return noChao; }
        }
        public float VelocidadeY
        {
            get { return rb != null ? rb.linearVelocity.y : 0f; }
        }
        public float RecargaRestante { get; private set; }
        public bool Invulneravel
        {
            get { return timerInvulneravel > 0f; }
        }

        public static Jogador Criar(Vector3 posicao)
        {
            var go = new GameObject("Curupira (Jogador)");
            go.layer = Camadas.Jogador;
            go.transform.position = posicao;
            return go.AddComponent<Jogador>();
        }

        void Awake()
        {
            Atual = this;
            Vivo = true;

            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = Gravidade;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

            col = gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.62f, 1.3f);
            col.offset = new Vector2(0f, 0.66f);
            col.edgeRadius = 0.02f;
            col.sharedMaterial = new PhysicsMaterial2D("SemAtrito") { friction = 0f, bounciness = 0f };

            visual = new GameObject("Visual").transform;
            visual.SetParent(transform, false);
            sr = visual.gameObject.AddComponent<SpriteRenderer>();
            sr.sortingOrder = Ordem.Jogador;
            Materiais2D.Aplicar(sr, false);
            anim = visual.gameObject.AddComponent<AnimadorSprite>();
            anim.alvo = sr;
            anim.Tocar("curupira_parado", 6f);
        }

        void OnDestroy()
        {
            if (Atual == this)
                Atual = null;
        }

        void Update()
        {
            if (Time.timeScale <= 0f)
                return;
            float dt = Time.deltaTime;

            bool controlavel = Vivo && timerSemControle <= 0f && !comemorando;
            entradaH = controlavel ? Entrada.Horizontal : 0f;

            if (controlavel && Entrada.PuloApertado)
            {
                if (Entrada.BaixoSegurado && TentarDescerPlataforma())
                    timerBuffer = 0f;
                else
                    timerBuffer = TempoBuffer;
            }
            if (controlavel && Entrada.AssobioApertado)
                Assobiar();

            timerBuffer -= dt;
            timerInvulneravel -= dt;
            timerSemControle -= dt;
            timerAssobioAnim -= dt;
            RecargaRestante = Mathf.Max(0f, RecargaRestante - dt);

            if (timerDescer > 0f)
            {
                timerDescer -= dt;
                if (timerDescer <= 0f && plataformaIgnorada != null)
                {
                    Physics2D.IgnoreCollision(col, plataformaIgnorada, false);
                    plataformaIgnorada = null;
                }
            }

            // pisca enquanto está invulnerável
            sr.enabled = !(timerInvulneravel > 0f && Vivo && Mathf.Repeat(Time.time * 14f, 1f) < 0.35f);

            // volta a escala do "squash & stretch"
            visual.localScale = Vector3.Lerp(visual.localScale, Vector3.one, 1f - Mathf.Exp(-14f * dt));

            if (entradaH > 0.01f) olhandoDireita = true;
            else if (entradaH < -0.01f) olhandoDireita = false;
            sr.flipX = !olhandoDireita;

            Animar();
            Pegadas(dt);

            GerenciadorFase g = GerenciadorFase.Instancia;
            if (Vivo && g != null && transform.position.y < g.LimiteQueda)
                g.JogadorCaiuNoAbismo();
        }

        void FixedUpdate()
        {
            bool estavaNoChao = noChao;
            Vector2 pos = rb.position;
            Collider2D chao = null;
            if (rb.linearVelocity.y <= 0.1f)
                chao = Physics2D.OverlapBox(pos + new Vector2(0f, -0.03f), new Vector2(0.56f, 0.08f), 0f, Camadas.MascaraPisavel);
            if (chao != null && chao == plataformaIgnorada)
                chao = null;
            noChao = chao != null;
            plataformaMovel = noChao ? chao.GetComponent<PlataformaMovel>() : null;

            if (noChao)
            {
                timerCoyote = TempoCoyote;
                cortouPulo = false;
                if (!estavaNoChao && yAntes < -6f)
                {
                    visual.localScale = new Vector3(1.25f, 0.78f, 1f);
                    Efeitos.Animacao("poeira", transform.position + new Vector3(0f, 0.3f, 0f), 16f);
                }
            }
            else
            {
                timerCoyote -= Time.fixedDeltaTime;
            }

            Vector2 v = rb.linearVelocity;
            float alvo = entradaH * Velocidade;
            if (plataformaMovel != null)
                alvo += plataformaMovel.Velocidade.x;
            float acel = noChao ? AceleracaoChao : AceleracaoAr;
            if (timerSemControle <= 0f)
                v.x = Mathf.MoveTowards(v.x, alvo, acel * Time.fixedDeltaTime);

            if (timerBuffer > 0f && timerCoyote > 0f && Vivo)
            {
                v.y = ForcaPulo;
                timerBuffer = 0f;
                timerCoyote = 0f;
                noChao = false;
                visual.localScale = new Vector3(0.8f, 1.2f, 1f);
                SomJogo.Tocar(Som.Pulo, 0.7f);
                Efeitos.Animacao("poeira", transform.position + new Vector3(0f, 0.3f, 0f), 18f, 0.8f);
            }

            // pulo variável: soltar o botão cedo faz um pulo mais baixo
            if (!cortouPulo && v.y > 0f && !Entrada.PuloSegurado && timerSemControle <= 0f)
            {
                v.y *= 0.5f;
                cortouPulo = true;
            }

            rb.gravityScale = v.y < 0f ? Gravidade * MultiplicadorQueda : Gravidade;
            if (v.y < -QuedaMaxima)
                v.y = -QuedaMaxima;
            yAntes = v.y;
            rb.linearVelocity = v;
        }

        bool TentarDescerPlataforma()
        {
            if (!noChao)
                return false;
            Collider2D c = Physics2D.OverlapBox(rb.position + new Vector2(0f, -0.03f), new Vector2(0.56f, 0.08f), 0f,
                1 << Camadas.Plataforma);
            if (c == null || c.GetComponent<PlatformEffector2D>() == null)
                return false;
            Physics2D.IgnoreCollision(col, c, true);
            plataformaIgnorada = c;
            timerDescer = 0.35f;
            return true;
        }

        void Animar()
        {
            if (!Vivo || timerSemControle > 0f)
            {
                anim.Tocar("curupira_dano", 1f);
                return;
            }
            if (comemorando)
            {
                anim.Tocar("curupira_vitoria", 5f);
                return;
            }
            if (timerAssobioAnim > 0f)
            {
                anim.Tocar("curupira_assobio", 8f);
                return;
            }
            if (!noChao)
            {
                anim.Tocar(rb.linearVelocity.y > 0f ? "curupira_pulo" : "curupira_queda", 8f);
                return;
            }
            if (Mathf.Abs(rb.linearVelocity.x - (plataformaMovel != null ? plataformaMovel.Velocidade.x : 0f)) > 0.6f)
                anim.Tocar("curupira_correr", 12f);
            else
                anim.Tocar("curupira_parado", 5f);
        }

        /// <summary>O Curupira deixa pegadas viradas para trás (para enganar os caçadores!).</summary>
        void Pegadas(float dt)
        {
            if (!noChao || Mathf.Abs(entradaH) < 0.1f)
                return;
            timerPegada -= dt;
            if (timerPegada > 0f)
                return;
            timerPegada = 0.2f;
            Sprite s = Arte.Sprite("pegada");
            if (s == null)
                return;
            SpriteRenderer p = Efeitos.NovoSprite("pegada", null, s, Ordem.Pegadas);
            p.transform.position = transform.position + new Vector3(0f, 0.06f, 0f);
            // a pegada aponta para o lado contrário ao que ele corre
            p.flipX = !olhandoDireita;
            var part = p.gameObject.AddComponent<Particula>();
            part.vida = 1.6f;
        }

        void Assobiar()
        {
            if (RecargaRestante > 0f)
                return;
            RecargaRestante = RecargaAssobio;
            timerAssobioAnim = 0.45f;
            SomJogo.Tocar(Som.Assobio);
            Vector3 centro = transform.position + new Vector3(0f, 0.9f, 0f);
            AnelAssobio.Criar(centro, RaioAssobio);
            Efeitos.Particulas("nota", centro, 6, Color.white, 1.5f, 3f, 0.9f, -1f, true, 30f, 150f);
            int atingidos = 0;
            for (int i = Inimigo.Todos.Count - 1; i >= 0; i--)
            {
                Inimigo ini = Inimigo.Todos[i];
                if (ini == null || ini.EstaMorto)
                    continue;
                if (Vector2.Distance(ini.Centro, centro) <= RaioAssobio)
                {
                    ini.Atordoar(TempoTonto);
                    atingidos++;
                }
            }
            if (atingidos > 0 && GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.Mensagem(atingidos > 1 ? "MÁQUINAS TONTAS!" : "MÁQUINA TONTA!", FabricaUI.CorVerde, 1f);
        }

        /// <summary>Quica depois de pisar num inimigo.</summary>
        public void Quicar()
        {
            Vector2 v = rb.linearVelocity;
            v.y = Entrada.PuloSegurado ? ForcaQuique * 1.2f : ForcaQuique;
            rb.linearVelocity = v;
            cortouPulo = !Entrada.PuloSegurado;
            timerCoyote = 0f;
            visual.localScale = new Vector3(0.8f, 1.2f, 1f);
        }

        public void TomarDano(Vector2 origem)
        {
            if (!Vivo || timerInvulneravel > 0f || comemorando)
                return;
            SomJogo.Tocar(Som.Dano);
            timerInvulneravel = 1.6f;
            timerSemControle = 0.35f;
            float lado = transform.position.x < origem.x ? -1f : 1f;
            rb.linearVelocity = new Vector2(lado * 6f, 9f);
            CameraSeguir.Tremer(0.25f, 0.25f);
            Efeitos.Particulas("brilho", transform.position + Vector3.up * 0.8f, 6, FabricaUI.CorVermelha, 2f, 4f, 0.4f, 4f, true);
            if (GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.JogadorFerido();
        }

        /// <summary>Chamado quando as vidas acabam.</summary>
        public void Morrer()
        {
            Vivo = false;
            sr.enabled = true;
            col.enabled = false;
            rb.linearVelocity = new Vector2(0f, 12f);
            rb.gravityScale = Gravidade;
            sr.sortingOrder = Ordem.TextoFlutuante;
            SomJogo.Tocar(Som.Queda);
        }

        /// <summary>Volta para o último totem (depois de cair no abismo/rio).</summary>
        public void Renascer(Vector3 posicao)
        {
            transform.position = posicao;
            rb.position = posicao;
            rb.linearVelocity = Vector2.zero;
            timerInvulneravel = 1.6f;
            timerSemControle = 0f;
            Efeitos.Animacao("brilho", posicao + Vector3.up * 0.8f, 10f, 2f, true);
            CameraSeguir.Teleportar();
        }

        public void Comemorar()
        {
            comemorando = true;
            timerInvulneravel = 0f;
        }

        void OnCollisionStay2D(Collision2D c)
        {
            if (c.gameObject.layer == Camadas.Perigo)
                TomarDano(new Vector2(transform.position.x - (rb.linearVelocity.x >= 0f ? 0.5f : -0.5f), transform.position.y - 1f));
        }
    }
}
