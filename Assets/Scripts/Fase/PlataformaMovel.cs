using UnityEngine;

namespace Curupira
{
    /// <summary>Plataforma de madeira que vai e volta entre dois pontos (o jogador é levado junto).</summary>
    public class PlataformaMovel : MonoBehaviour
    {
        public Vector2 pontoA;
        public Vector2 pontoB;
        public float velocidade = 2f;
        public Vector2 Velocidade { get; private set; }

        Rigidbody2D rb;
        float t;
        float duracao;

        public static PlataformaMovel Criar(Vector2 a, Vector2 b, int largura, string tema)
        {
            var go = new GameObject("Plataforma movel");
            go.layer = Camadas.Chao;
            go.transform.position = a;
            var p = go.AddComponent<PlataformaMovel>();
            p.pontoA = a;
            p.pontoB = b;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(largura, 0.4f);
            col.offset = new Vector2(0f, 0.3f);
            for (int i = 0; i < largura; i++)
            {
                string tipo = largura == 1 ? "u" : (i == 0 ? "e" : (i == largura - 1 ? "d" : "m"));
                SpriteRenderer sr = Efeitos.NovoSprite("Tabua", go.transform, Arte.Sprite(tema + "_plataforma_" + tipo), Ordem.Plataformas);
                sr.transform.localPosition = new Vector3(i - (largura - 1) * 0.5f, 0f, 0f);
            }
            return p;
        }

        void Awake()
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        void Start()
        {
            duracao = Mathf.Max(0.5f, Vector2.Distance(pontoA, pontoB) / velocidade);
        }

        void FixedUpdate()
        {
            t += Time.fixedDeltaTime;
            // vai e volta suavemente (movimento senoidal)
            float s = 0.5f - 0.5f * Mathf.Cos(t / duracao * Mathf.PI);
            Vector2 alvo = Vector2.Lerp(pontoA, pontoB, s);
            Velocidade = (alvo - rb.position) / Time.fixedDeltaTime;
            rb.MovePosition(alvo);
        }
    }
}
