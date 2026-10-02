using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Curupira
{
    /// <summary>
    /// Base de todos os inimigos. Cuida do contato com o jogador (pisão ou dano),
    /// do estado "tonto" causado pelo assobio e da morte.
    /// Os inimigos são corpos Kinematic com colisor Trigger.
    /// </summary>
    public abstract class Inimigo : MonoBehaviour
    {
        public static readonly List<Inimigo> Todos = new List<Inimigo>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            Todos.Clear();
        }

        protected SpriteRenderer sr;
        protected AnimadorSprite anim;
        protected Rigidbody2D rb;
        protected BoxCollider2D col;
        protected Transform visual;
        protected int direcao = -1;
        protected float tempoTonto;
        protected bool morto;
        Transform estrelas;

        public bool EstaMorto
        {
            get { return morto; }
        }
        public bool EstaTonto
        {
            get { return tempoTonto > 0f; }
        }
        public Vector2 Centro
        {
            get { return col != null ? (Vector2)col.bounds.center : (Vector2)transform.position; }
        }

        /// <summary>Prefixo dos sprites (serra, drone, fagulha).</summary>
        protected abstract string Prefixo { get; }
        protected abstract string AnimNormal { get; }
        protected virtual float QpsNormal
        {
            get { return 10f; }
        }
        /// <summary>A Fagulha só pode ser pisada quando está tonta.</summary>
        protected virtual bool PodeSerPisado
        {
            get { return true; }
        }
        protected virtual int PontosPisao
        {
            get { return 100; }
        }
        protected virtual bool Brilha
        {
            get { return false; }
        }
        protected abstract Vector2 TamanhoColisor { get; }

        protected abstract void Mover(float dt);

        public static T Criar<T>(Vector3 posicao) where T : Inimigo
        {
            var go = new GameObject(typeof(T).Name);
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
            Materiais2D.Aplicar(sr, Brilha);
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
            if (morto)
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

        public void Atordoar(float duracao)
        {
            if (morto)
                return;
            tempoTonto = Mathf.Max(tempoTonto, duracao);
            anim.Tocar(Prefixo + "_tonto", 4f);
            sr.color = new Color(0.75f, 0.85f, 1f);
            if (estrelas == null)
            {
                estrelas = new GameObject("Estrelas").transform;
                estrelas.SetParent(transform, false);
                estrelas.localPosition = new Vector3(0f, TamanhoColisor.y + 0.35f, 0f);
                for (int i = 0; i < 3; i++)
                {
                    SpriteRenderer e = Efeitos.NovoSprite("estrela", estrelas, Arte.Sprite("estrela_" + (i % 2)), Ordem.Efeitos, true);
                    float a = i * Mathf.PI * 2f / 3f;
                    e.transform.localPosition = new Vector3(Mathf.Cos(a) * 0.45f, Mathf.Sin(a) * 0.18f, 0f);
                }
            }
        }

        void SairDoTonto()
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
            if (morto)
                return;
            Jogador j = outro.GetComponent<Jogador>();
            if (j == null || !j.Vivo)
                return;

            bool vindoDeCima = j.transform.position.y > Centro.y && j.VelocidadeY < 2f;
            if (vindoDeCima && (PodeSerPisado || EstaTonto))
            {
                j.Quicar();
                Esmagar();
                return;
            }
            if (EstaTonto)
                return; // tonto não machuca ninguém
            j.TomarDano(Centro);
        }

        public void Esmagar()
        {
            if (morto)
                return;
            morto = true;
            col.enabled = false;
            if (estrelas != null)
                Destroy(estrelas.gameObject);
            sr.color = Color.white;
            anim.Tocar(Prefixo + "_esmagado", 1f);
            SomJogo.Tocar(Som.Pisao);
            CameraSeguir.Tremer(0.12f, 0.15f);
            Efeitos.Animacao("fumaca", Centro, 12f, 1.3f);
            Efeitos.Particulas("brilho", Centro, 5, Color.white, 2f, 4f, 0.4f, 3f, true);
            Sessao.SomarPontos(PontosPisao);
            Efeitos.TextoSubindo("+" + PontosPisao, (Vector3)Centro + Vector3.up * 0.8f, FabricaUI.CorDestaque);
            StartCoroutine(Sumir());
        }

        protected virtual IEnumerator Sumir()
        {
            float t = 0f;
            Vector3 escala = visual.localScale;
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                visual.localScale = new Vector3(escala.x * (1f + t * 0.4f), escala.y * Mathf.Max(0.3f, 1f - t), 1f);
                Color c = sr.color;
                c.a = 1f - t / 0.6f;
                sr.color = c;
                yield return null;
            }
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
    }
}
