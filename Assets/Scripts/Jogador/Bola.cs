using UnityEngine;

namespace RumoAoHexa
{
    /// <summary>
    /// A bola do craque. No pé ela acompanha o jogador (e sobe e desce na
    /// embaixadinha); chutada vira um corpo físico que quica no chão e
    /// ricocheteia nas paredes. Cada argentino que ela acerta recebe cartão
    /// vermelho e some. Depois de um tempo (ou apertando J) ela volta sozinha.
    /// As decisões ficam em <see cref="EstadoBola"/>.
    /// </summary>
    public class Bola : MonoBehaviour
    {
        public const float Raio = 0.24f;

        Jogador dono;
        Rigidbody2D rb;
        CircleCollider2D col;
        Transform visual;
        SpriteRenderer sr;
        SpriteRenderer brilho;
        ParametrosChute parametros;
        float tempoRastro;
        float giro;
        float tempoQuique;
        Vector2 velocidadeAntes;

        public EstadoBola Estado { get; private set; }

        public bool NoPe
        {
            get { return Estado.Situacao == SituacaoBola.NoPe; }
        }

        public bool Voando
        {
            get { return Estado.Situacao == SituacaoBola.Voando; }
        }

        public Vector2 Velocidade
        {
            get { return Voando ? rb.linearVelocity : Vector2.zero; }
        }

        public static Bola Criar(Jogador dono)
        {
            var go = new GameObject("Bola");
            go.layer = Camadas.Bola;
            go.transform.position = dono.PosicaoPe;
            var b = go.AddComponent<Bola>();
            b.dono = dono;
            return b;
        }

        void Awake()
        {
            Estado = new EstadoBola();

            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.mass = 0.3f;
            rb.linearDamping = 0.15f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
            rb.simulated = false;   // no pé, quem move a bola é o código

            col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = Raio;
            col.sharedMaterial = new PhysicsMaterial2D("Bola") { bounciness = 0.62f, friction = 0.35f };

            visual = new GameObject("Visual").transform;
            visual.SetParent(transform, false);
            brilho = Efeitos.NovoSprite("Brilho", visual, Arte.Sprite("luz_suave"), Ordem.Bola - 1, true);
            brilho.color = new Color(1f, 0.7f, 0.2f, 0.75f);
            brilho.transform.localScale = Vector3.one * 0.9f;
            brilho.enabled = false;
            sr = Efeitos.NovoSprite("Sprite", visual, Arte.Sprite("bola"), Ordem.Bola, false);
        }

        void Update()
        {
            if (dono == null)
            {
                Destroy(gameObject);
                return;
            }
            if (Time.timeScale <= 0f)
                return;
            float dt = Time.deltaTime;

            switch (Estado.Situacao)
            {
                case SituacaoBola.NoPe:
                    SeguirOPe(dt);
                    break;
                case SituacaoBola.Voltando:
                    Voltar(dt);
                    break;
            }

            // a bola gira conforme anda
            float vx = Estado.Situacao == SituacaoBola.Voando ? rb.linearVelocity.x :
                (Estado.Situacao == SituacaoBola.NoPe ? dono.VelocidadeX : 0f);
            giro -= vx * dt * (180f / (Mathf.PI * Raio));
            if (Estado.Situacao == SituacaoBola.Voltando)
                giro += 900f * dt;
            visual.localRotation = Quaternion.Euler(0f, 0f, giro);

            bool rapida = Estado.Situacao != SituacaoBola.NoPe &&
                          (Estado.Situacao == SituacaoBola.Voltando || rb.linearVelocity.sqrMagnitude > 60f);
            if (rapida)
                Rastro(dt);
        }

        void FixedUpdate()
        {
            if (Estado.Situacao != SituacaoBola.Voando)
                return;
            float dt = Time.fixedDeltaTime;
            Vector2 v = rb.linearVelocity;
            // a folha seca não cai: ela "mergulha" no fim (curva)
            if (parametros.Curva > 0f)
            {
                v.y -= parametros.Curva * dt;
                rb.linearVelocity = v;
            }
            velocidadeAntes = v;
            tempoQuique -= dt;

            GerenciadorFase g = GerenciadorFase.Instancia;
            bool caiu = g != null && rb.position.y < g.LimiteQueda;
            float distancia = Vector2.Distance(rb.position, dono.PosicaoPe);
            Estado.AtualizarVoo(dt, v.magnitude, dono.Vivo ? distancia : 999f, caiu);
            if (Estado.Situacao == SituacaoBola.NoPe)
                Dominada();
            else if (Estado.Situacao == SituacaoBola.Voltando)
                ComecarVolta();
        }

        // ------------------------------------------------------------ no pé
        void SeguirOPe(float dt)
        {
            Vector2 alvo = dono.PosicaoPe;
            if (dono.FazendoEmbaixadinha)
            {
                // sobe e desce sincronizada com o pé (o pé sobe no começo de cada ciclo)
                float f = Mathf.Repeat(dono.TempoEmbaixadinha / Jogador.CicloEmbaixadinha, 1f);
                alvo = (Vector2)dono.transform.position + new Vector2(dono.Direcao * 0.34f, 0.62f + Mathf.Sin(f * Mathf.PI) * 1.05f);
                transform.position = alvo;
                return;
            }
            if (Mathf.Abs(dono.VelocidadeX) > 0.6f && dono.NoChao)
            {
                // conduzindo: a bola vai um pouquinho à frente e volta
                alvo.x += dono.Direcao * (0.08f + Mathf.Sin(Time.time * 14f) * 0.08f);
            }
            if (!dono.NoChao)
                alvo.y += 0.05f;
            Vector2 atual = transform.position;
            float suave = 1f - Mathf.Exp(-30f * dt);
            transform.position = Vector2.Lerp(atual, alvo, suave);
        }

        /// <summary>Chamado pelo jogador. Devolve false se a bola não estava no pé.</summary>
        public bool Chutar(TipoChute tipo, int direcao, float velocidadeJogadorX)
        {
            if (!Estado.Chutar(tipo))
                return false;
            parametros = Chutes.Parametros(tipo);
            float vx, vy;
            Chutes.VelocidadeInicial(tipo, direcao, velocidadeJogadorX, out vx, out vy);

            // se o pé está encostado na parede, a bola sai de dentro do jogador
            Vector2 inicio = dono.PosicaoPe;
            if (dono.FazendoEmbaixadinha || !dono.NoChao)
                inicio = transform.position;
            if (Physics2D.OverlapCircle(inicio, Raio, Camadas.MascaraSolida) != null)
                inicio = new Vector2(dono.transform.position.x, inicio.y);

            transform.position = inicio;
            rb.simulated = true;
            rb.position = inicio;
            rb.gravityScale = parametros.Gravidade;
            rb.linearVelocity = new Vector2(vx, vy);
            velocidadeAntes = rb.linearVelocity;
            col.enabled = true;
            brilho.enabled = tipo == TipoChute.FolhaSeca;
            sr.color = tipo == TipoChute.FolhaSeca ? new Color(1f, 0.92f, 0.6f) : Color.white;
            SomJogo.Tocar(tipo == TipoChute.FolhaSeca ? Som.ChuteForte : (tipo == TipoChute.Cavadinha ? Som.Cavadinha : Som.Chute));
            return true;
        }

        /// <summary>Faz a bola voltar para o pé (apertar J com a bola longe).</summary>
        public void Chamar()
        {
            if (!Voando)
                return;
            Estado.Chamar();
            ComecarVolta();
        }

        /// <summary>Coloca a bola direto no pé (ex.: quando o jogador renasce).</summary>
        public void VoltarParaOPe()
        {
            Estado.Dominar();
            Dominada();
            transform.position = dono.PosicaoPe;
        }

        void ComecarVolta()
        {
            rb.simulated = false;
            brilho.enabled = false;
            sr.color = Color.white;
            SomJogo.Tocar(Som.BolaVolta, 0.5f);
        }

        void Voltar(float dt)
        {
            Vector2 pos = transform.position;
            Vector2 alvo = dono.PosicaoPe;
            float vel = EstadoBola.VelocidadeVolta(Estado.TempoVoltando);
            pos = Vector2.MoveTowards(pos, alvo, vel * dt);
            transform.position = pos;
            Estado.AtualizarVolta(dt, Vector2.Distance(pos, alvo));
            if (Estado.Situacao == SituacaoBola.NoPe)
                Dominada();
        }

        void Dominada()
        {
            rb.simulated = false;
            brilho.enabled = false;
            sr.color = Color.white;
        }

        void Rastro(float dt)
        {
            tempoRastro -= dt;
            if (tempoRastro > 0f)
                return;
            tempoRastro = 0.03f;
            bool folhaSeca = Voando && Estado.UltimoChute == TipoChute.FolhaSeca;
            Color c = folhaSeca ? new Color(1f, 0.75f, 0.25f, 0.9f) : new Color(1f, 1f, 1f, 0.45f);
            Efeitos.Particulas("rastro", transform.position, 1, c, 0f, 0.2f, folhaSeca ? 0.35f : 0.2f, 0f, true);
        }

        // ------------------------------------------------------------ colisões
        void OnCollisionEnter2D(Collision2D c)
        {
            if (!Voando)
                return;
            if (tempoQuique <= 0f && velocidadeAntes.sqrMagnitude > 9f)
            {
                tempoQuique = 0.08f;
                SomJogo.Tocar(Som.Quique, Mathf.Clamp01(velocidadeAntes.magnitude / 16f) * 0.6f);
            }
        }

        void OnTriggerEnter2D(Collider2D outro)
        {
            TentarAcertar(outro);
        }

        void OnTriggerStay2D(Collider2D outro)
        {
            TentarAcertar(outro);
        }

        void TentarAcertar(Collider2D outro)
        {
            if (!Estado.PodeAcertar)
                return;
            Inimigo ini = outro.GetComponentInParent<Inimigo>();
            if (ini == null || ini.EstaExpulso)
                return;
            int bonus = Estado.Combo.RegistrarAcerto();
            Vector2 v = rb.linearVelocity;
            ini.Expulsar(new Vector2(Mathf.Sign(v.x), 0f));
            if (GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.ArgentinoExpulso(ini, Estado.UltimoChute, Estado.Combo.Acertos, bonus);
            if (!Estado.Atravessa)
            {
                // ricochete: a bola volta e sobe um pouco (dá para pegar outro na volta!)
                rb.linearVelocity = new Vector2(-v.x * 0.55f, Mathf.Max(v.y, 5.5f));
            }
        }
    }
}
