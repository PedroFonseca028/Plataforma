using UnityEngine;

namespace RumoAoHexa
{
    /// <summary>
    /// O craque canarinho: movimento, pulo (com "coyote time" e buffer), chutes
    /// (rasteiro, cavadinha e folha seca carregada), embaixadinha quando está
    /// parado, pisão que deixa o argentino tonto e dano.
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

        // ---- chute e embaixadinha
        const float TempoAnimChute = 0.28f;
        const float EsperaEmbaixadinha = 0.6f;
        /// <summary>Duração de um ciclo da embaixadinha (a bola sobe e volta ao pé).</summary>
        public const float CicloEmbaixadinha = 0.6f;

        Rigidbody2D rb;
        BoxCollider2D col;
        SpriteRenderer sr;
        AnimadorSprite anim;
        Transform visual;
        readonly ControleChute controle = new ControleChute();

        float entradaH;
        float timerCoyote;
        float timerBuffer;
        bool noChao;
        bool cortouPulo;
        bool olhandoDireita = true;
        float timerInvulneravel;
        float timerSemControle;
        float timerAnimChute;
        float timerDescer;
        float tempoParado;
        bool avisouCargaCheia;
        Collider2D plataformaIgnorada;
        PlataformaMovel plataformaMovel;
        bool comemorando;
        float yAntes;

        public Bola Bola { get; private set; }
        public bool Vivo { get; private set; }
        public bool NoChao
        {
            get { return noChao; }
        }
        public float VelocidadeY
        {
            get { return rb != null ? rb.linearVelocity.y : 0f; }
        }
        public float VelocidadeX
        {
            get { return rb != null ? rb.linearVelocity.x : 0f; }
        }
        /// <summary>1 olhando para a direita, -1 para a esquerda.</summary>
        public int Direcao
        {
            get { return olhandoDireita ? 1 : -1; }
        }
        public bool Invulneravel
        {
            get { return timerInvulneravel > 0f; }
        }
        public bool Comemorando
        {
            get { return comemorando; }
        }
        /// <summary>0 a 1: barra da folha seca (para o HUD).</summary>
        public float CargaChute
        {
            get { return controle.Carga; }
        }
        public bool CarregandoChute
        {
            get { return controle.Carregando; }
        }
        /// <summary>Parado com a bola no pé: faz embaixadinhas.</summary>
        public bool FazendoEmbaixadinha
        {
            get { return tempoParado >= EsperaEmbaixadinha && Bola != null && Bola.NoPe && noChao && Vivo && !comemorando && !controle.Segurando; }
        }
        /// <summary>Tempo desde o começo da embaixadinha (a bola usa para acompanhar o pé).</summary>
        public float TempoEmbaixadinha
        {
            get { return Mathf.Max(0f, tempoParado - EsperaEmbaixadinha); }
        }
        /// <summary>Onde a bola fica quando está dominada.</summary>
        public Vector2 PosicaoPe
        {
            get { return (Vector2)transform.position + new Vector2(Direcao * 0.5f, 0.26f); }
        }

        public static Jogador Criar(Vector3 posicao)
        {
            var go = new GameObject("Craque (Jogador)");
            go.layer = Camadas.Jogador;
            go.transform.position = posicao;
            Jogador j = go.AddComponent<Jogador>();
            j.Bola = Bola.Criar(j);
            return j;
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
            anim.Tocar("craque_parado", 6f);
        }

        void OnDestroy()
        {
            if (Atual == this)
                Atual = null;
            if (Bola != null)
                Destroy(Bola.gameObject);
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

            if (controlavel)
                LerChute(dt);
            else
                controle.Cancelar();

            timerBuffer -= dt;
            timerInvulneravel -= dt;
            timerSemControle -= dt;
            timerAnimChute -= dt;

            if (timerDescer > 0f)
            {
                timerDescer -= dt;
                if (timerDescer <= 0f && plataformaIgnorada != null)
                {
                    Physics2D.IgnoreCollision(col, plataformaIgnorada, false);
                    plataformaIgnorada = null;
                }
            }

            // conta quanto tempo está parado (para começar a embaixadinha)
            bool parado = noChao && Mathf.Abs(entradaH) < 0.1f &&
                          Mathf.Abs(rb.linearVelocity.x - VelocidadePlataforma()) < 0.3f && timerAnimChute <= 0f;
            tempoParado = parado ? tempoParado + dt : 0f;

            // pisca enquanto está invulnerável
            sr.enabled = !(timerInvulneravel > 0f && Vivo && Mathf.Repeat(Time.time * 14f, 1f) < 0.35f);

            // volta a escala do "squash & stretch" (tremendo enquanto carrega a folha seca)
            Vector3 alvoEscala = Vector3.one;
            if (controle.CargaCheia)
                alvoEscala = new Vector3(1f + Mathf.Sin(Time.time * 60f) * 0.04f, 1f, 1f);
            visual.localScale = Vector3.Lerp(visual.localScale, alvoEscala, 1f - Mathf.Exp(-14f * dt));

            if (entradaH > 0.01f) olhandoDireita = true;
            else if (entradaH < -0.01f) olhandoDireita = false;
            sr.flipX = !olhandoDireita;

            Animar();

            GerenciadorFase g = GerenciadorFase.Instancia;
            if (Vivo && g != null && transform.position.y < g.LimiteQueda)
                g.JogadorCaiuNoAbismo();
        }

        /// <summary>J: toque = rasteiro (ou cavadinha com ↑), segurar = folha seca. K = cavadinha.</summary>
        void LerChute(float dt)
        {
            if (Bola == null)
                return;
            if (Entrada.CavadinhaApertada)
            {
                if (Bola.NoPe)
                {
                    controle.Cancelar();
                    Chutar(TipoChute.Cavadinha);
                }
                else
                {
                    Bola.Chamar();
                }
            }
            if (Entrada.ChuteApertado)
            {
                if (Bola.NoPe)
                    controle.Apertar();
                else
                    Bola.Chamar();       // apertar J com a bola longe faz ela voltar
            }
            if (!controle.Segurando)
            {
                avisouCargaCheia = false;
                return;
            }
            if (!Bola.NoPe)
            {
                controle.Cancelar();
                return;
            }
            controle.Atualizar(dt);
            if (controle.CargaCheia && !avisouCargaCheia)
            {
                avisouCargaCheia = true;
                SomJogo.Tocar(Som.CargaCheia, 0.8f);
                Efeitos.Particulas("brilho", (Vector3)PosicaoPe, 6, new Color(1f, 0.85f, 0.3f), 1.5f, 3f, 0.5f, 0f, true);
            }
            if (!Entrada.ChuteSegurado)
                Chutar(controle.Soltar(Entrada.CimaSegurado));
        }

        /// <summary>Chuta a bola (se ela estiver no pé). Público para os testes.</summary>
        public bool Chutar(TipoChute tipo)
        {
            if (Bola == null || !Bola.NoPe || !Vivo)
                return false;
            if (!Bola.Chutar(tipo, Direcao, rb.linearVelocity.x))
                return false;
            timerAnimChute = TempoAnimChute;
            tempoParado = 0f;
            anim.Tocar("craque_chute", 3f / TempoAnimChute, false, true);
            if (tipo == TipoChute.FolhaSeca)
            {
                visual.localScale = new Vector3(1.25f, 0.85f, 1f);
                CameraSeguir.Tremer(0.15f, 0.2f);
                AnelExpansivo.Criar((Vector3)PosicaoPe, 1.6f, new Color(1f, 0.85f, 0.3f, 0.9f));
            }
            Efeitos.Animacao("poeira", transform.position + new Vector3(Direcao * 0.3f, 0.2f, 0f), 18f, 0.7f);
            return true;
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
            float alvo = entradaH * Velocidade + VelocidadePlataforma();
            // carregando a folha seca o craque anda devagar
            if (controle.Carregando)
                alvo = entradaH * Velocidade * 0.35f + VelocidadePlataforma();
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

        float VelocidadePlataforma()
        {
            return plataformaMovel != null ? plataformaMovel.Velocidade.x : 0f;
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
                anim.Tocar("craque_dano", 1f);
                return;
            }
            if (comemorando)
            {
                anim.Tocar("craque_vitoria", 5f);
                return;
            }
            if (timerAnimChute > 0f)
                return; // a animação do chute toca até o fim
            if (controle.Carregando)
            {
                anim.Tocar("craque_carregar", controle.CargaCheia ? 16f : 8f);
                return;
            }
            if (!noChao)
            {
                anim.Tocar(rb.linearVelocity.y > 0f ? "craque_pulo" : "craque_queda", 8f);
                return;
            }
            if (Mathf.Abs(rb.linearVelocity.x - VelocidadePlataforma()) > 0.6f)
            {
                anim.Tocar("craque_correr", 12f);
                return;
            }
            if (FazendoEmbaixadinha)
            {
                // 4 quadros por ciclo: o pé sobe no quadro 0, quando a bola volta
                anim.Tocar("craque_embaixadinha", 4f / CicloEmbaixadinha, true, anim.Atual != "craque_embaixadinha");
                return;
            }
            anim.Tocar("craque_parado", 5f);
        }

        /// <summary>Quica depois de pisar num argentino.</summary>
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
            controle.Cancelar();
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
            controle.Cancelar();
            sr.enabled = true;
            col.enabled = false;
            rb.linearVelocity = new Vector2(0f, 12f);
            rb.gravityScale = Gravidade;
            sr.sortingOrder = Ordem.TextoFlutuante;
            SomJogo.Tocar(Som.Queda);
        }

        /// <summary>Volta para a última bandeirinha (depois de cair no buraco ou no mar).</summary>
        public void Renascer(Vector3 posicao)
        {
            transform.position = posicao;
            rb.position = posicao;
            rb.linearVelocity = Vector2.zero;
            timerInvulneravel = 1.6f;
            timerSemControle = 0f;
            controle.Cancelar();
            if (Bola != null)
                Bola.VoltarParaOPe();
            Efeitos.Animacao("brilho", posicao + Vector3.up * 0.8f, 10f, 2f, true);
            CameraSeguir.Teleportar();
        }

        public void Comemorar()
        {
            comemorando = true;
            timerInvulneravel = 0f;
            controle.Cancelar();
        }

        void OnCollisionStay2D(Collision2D c)
        {
            if (c.gameObject.layer == Camadas.Perigo)
                TomarDano(new Vector2(transform.position.x - (rb.linearVelocity.x >= 0f ? 0.5f : -0.5f), transform.position.y - 1f));
        }
    }
}
