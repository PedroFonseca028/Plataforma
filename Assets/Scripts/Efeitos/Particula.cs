using UnityEngine;

namespace Curupira
{
    /// <summary>Movimento simples de uma partícula.</summary>
    public class Particula : MonoBehaviour
    {
        public Vector2 velocidade;
        public float gravidade;
        public float vida = 0.6f;
        public float giro;

        float idade;
        SpriteRenderer sr;
        Color corInicial;

        void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                corInicial = sr.color;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            idade += dt;
            velocidade.y -= gravidade * dt;
            transform.position += (Vector3)(velocidade * dt);
            transform.Rotate(0f, 0f, giro * dt);
            if (sr != null)
            {
                Color c = corInicial;
                c.a = corInicial.a * Mathf.Clamp01(1f - idade / vida);
                sr.color = c;
            }
            if (idade >= vida)
                Destroy(gameObject);
        }
    }
}
