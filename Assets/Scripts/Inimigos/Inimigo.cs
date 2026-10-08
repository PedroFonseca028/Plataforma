using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RumoAoHexa
{
    /// <summary>
    /// Base dos argentinos. Encostar neles machuca; pular na cabeça deixa tonto
    /// (e o craque quica); só a bola expulsa: cartão vermelho e eles somem.
    /// São corpos Kinematic com colisor Trigger.
    /// </summary>
    public abstract class Inimigo : MonoBehaviour
    {
        public static readonly List<Inimigo> Todos = new List<Inimigo>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            Todos.Clear();
        }

        public const float TempoTonto = 2.5f;

        protected SpriteRenderer sr;
        protected AnimadorSprite anim;
        protected Rigidbody2D rb;
        protected BoxCollider2D col;
        protected Transform visual;
        protected int direcao = -1;
        protected float tempoTonto;
        protected bool expulso;
        Transform estrelas;

        public bool EstaExpulso
        {
            get { return expulso; }
        }
        public bool EstaTonto
        {
            get { return tempoTonto > 0f; }
        }
        public Vector2 Centro
        {
            get { return col != null ? (Vector2)col.bounds.center : (Vector2)transform.position; }
        }
        public float Altura
        {
            get { return TamanhoColisor.y; }
        }

        /// <summary>Prefixo dos sprites (zagueiro, lateral, cabeceador).</summary>
        protected abstract string Prefixo { get; }
        protected virtual string AnimNormal
        {
            get { return Prefixo + "_andar"; }
        }
        protected virtual float QpsNormal
        {
            get { return 8f; }
        }
        protected virtual Vector2 TamanhoColisor
        {
            get { return new Vector2(0.72f, 1.45f); }
        }

        protected abstract void Mover(float dt);

        public static T Criar<T>(Vector3 posicao) where T : Inimigo
        {
            var go = new GameObject(typeof(T).Name + " (Argentina)");
            go.layer = Camadas.Inimigo;
            go.transform.position = posicao;
            return go.AddComponent<T>();
        }

        protected virtual void Awake()
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = TamanhoColisor;
            col.offset = new Vector2(0f, TamanhoColisor.y * 0.5f);

            visual = new GameObject("Visual").transform;
            visual.SetParent(transform, false);
            sr = visual.gameObject.AddComponent<SpriteRenderer>();
            sr.sortingOrder = Ordem.Inimigos;
            Materiais2D.Aplicar(sr, false);
            anim = visual.gameObject.AddComponent<AnimadorSprite>();
            anim.alvo = sr;
            anim.Tocar(AnimNormal, QpsNormal);
            anim.Dessincronizar();
        }

        protected virtual void OnEnable()
        {
            Todos.Add(this);
        }

        protected virtual void OnDisable()
        {
            Todos.Remove(this);
        }

        void FixedUpdate()
        {
            if (expulso)
                return;
            float dt = Time.fixedDeltaTime;
            if (tempoTonto > 0f)
            {
                tempoTonto -= dt;
                if (tempoTonto <= 0f)
                    SairDoTonto();
                return;
            }
            Mover(dt);
            // os sprites olham para a direita; direcao -1 = espelhado
            sr.flipX = direcao < 0;
        }

        void Update()
        {
            if (estrelas != null)
            {
                estrelas.Rotate(0f, 0f, 240f * Time.deltaTime);
                for (int i = 0; i < estrelas.childCount; i++)
                    estrelas.GetChild(i).rotation = Quaternion.identity;
            }
        }

        /// <summary>Pisão do craque: fica tonto (parado e inofensivo) por um tempo.</summary>
        public void Atordoar(float duracao)
        {
            if (expulso)
                return;
            tempoTonto = Mathf.Max(tempoTonto, duracao);
            anim.Tocar(Prefixo + "_tonto", 4f);
            sr.color = new Color(0.85f, 0.9f, 1f);
            AoFicarTonto();
            if (estrelas == null)
            {
                estrelas = new GameObject("Estrelinhas").transform;
                estrelas.SetParent(transform, false);
                estrelas.localPosition = new Vector3(0f, TamanhoColisor.y + 0.4f, 0f);
                for (int i = 0; i < 3; i++)
                {
                    SpriteRenderer e = Efeitos.NovoSprite("estrelinha", estrelas, Arte.Sprite("tontura_" + (i % 2)), Ordem.Efeitos, true);
                    float a = i * Mathf.PI * 2f / 3f;
                    e.transform.localPosition = new Vector3(Mathf.Cos(a) * 0.45f, Mathf.Sin(a) * 0.18f, 0f);
                }
            }
        }

        /// <summary>Para as subclasses que precisam parar o que estavam fazendo (pulo, arrancada).</summary>
        protected virtual void AoFicarTonto()
        {
        }

        protected virtual void SairDoTonto()
        {
            tempoTonto = 0f;
            anim.Tocar(AnimNormal, QpsNormal);
            sr.color = Color.white;
            if (estrelas != null)
            {
                Destroy(estrelas.gameObject);
                estrelas = null;
            }
        }

        void OnTriggerStay2D(Collider2D outro)
        {
            if (expulso)
                return;
            Jogador j = outro.GetComponent<Jogador>();
            if (j == null || !j.Vivo)
                return;

            bool vindoDeCima = j.transform.position.y > Centro.y + 0.15f && j.VelocidadeY < 2f;
            if (vindoDeCima)
            {
                j.Quicar();
                if (!EstaTonto)
                {
                    Atordoar(TempoTonto);
                    SomJogo.Tocar(Som.Pisao);
                    Efeitos.Animacao("poeira", (Vector3)Centro + Vector3.up * (Altura * 0.5f), 16f);
                    if (GerenciadorFase.Instancia != null)
                        GerenciadorFase.Instancia.ArgentinoAtordoado(this);
                }
                return;
            }
            if (EstaTonto)
                return; // tonto não machuca ninguém
            j.TomarDano(Centro);
        }

        /// <summary>A bola acertou: cartão vermelho e o argentino some.</summary>
        public void Expulsar(Vector2 empurrao)
        {
            if (expulso)
                return;
            expulso = true;
            col.enabled = false;
            tempoTonto = 0f;
            if (estrelas != null)
            {
                Destroy(estrelas.gameObject);
                estrelas = null;
            }
            sr.color = Color.white;
            anim.Tocar(Prefixo + "_expulso", 1f);
            SomJogo.Tocar(Som.Expulso);
            CameraSeguir.Tremer(0.12f, 0.15f);
            CartaoVermelho.Mostrar((Vector3)Centro + new Vector3(0f, Altura * 0.5f + 0.5f, 0f));
            AnelExpansivo.Criar(Centro, 1.3f, new Color(1f, 0.35f, 0.3f, 0.8f), 0.3f);
            Efeitos.Particulas("brilho", Centro, 6, Color.white, 2f, 4.5f, 0.45f, 3f, true);
            StartCoroutine(Sumir(empurrao));
        }

        IEnumerator Sumir(Vector2 empurrao)
        {
            // empurrado pela bolada, fica um instante e some numa nuvem
            float t = 0f;
            Vector3 inicio = transform.position;
            while (t < 0.45f)
            {
                t += Time.deltaTime;
                float k = 1f - (1f - t / 0.45f) * (1f - t / 0.45f);
                transform.position = inicio + new Vector3(empurrao.x * 0.6f * k, 0.25f * Mathf.Sin(k * Mathf.PI), 0f);
                visual.localRotation = Quaternion.Euler(0f, 0f, -empurrao.x * 12f * k);
                Color c = sr.color;
                c.a = 1f - Mathf.Max(0f, t - 0.25f) / 0.2f;
                sr.color = c;
                yield return null;
            }
            Efeitos.Animacao("fumaca", (Vector3)Centro + Vector3.up * 0.2f, 12f, 1.6f);
            Destroy(gameObject);
        }

        // ---- utilidades para as subclasses
        protected bool TemChaoEm(Vector2 ponto, float distancia)
        {
            return Physics2D.Raycast(ponto, Vector2.down, distancia, Camadas.MascaraChaoInimigo).collider != null;
        }

        protected bool TemParedeEm(Vector2 ponto, Vector2 tamanho)
        {
            return Physics2D.OverlapBox(ponto, tamanho, 0f, Camadas.MascaraSolida) != null;
        }

        /// <summary>Anda na direção atual e vira na parede ou na beirada. Devolve true se virou.</summary>
        protected bool Patrulhar(float velocidade, float dt)
        {
            Vector2 pos = rb.position;
            Vector2 frente = pos + new Vector2(direcao * (TamanhoColisor.x * 0.5f + 0.1f), 0f);
            bool parede = TemParedeEm(frente + new Vector2(0f, 0.6f), new Vector2(0.1f, 0.6f));
            bool beirada = !TemChaoEm(frente + new Vector2(0f, 0.1f), 0.35f);
            if (parede || beirada)
            {
                direcao = -direcao;
                return true;
            }
            rb.MovePosition(pos + new Vector2(direcao * velocidade * dt, 0f));
            return false;
        }
    }
}
