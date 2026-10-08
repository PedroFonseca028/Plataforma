using UnityEngine;

namespace RumoAoHexa
{
    /// <summary>Faz um texto do cenário subir e desaparecer.</summary>
    public class TextoFlutuante : MonoBehaviour
    {
        float idade;
        SpriteRenderer[] partes;
        Color[] cores;

        void Start()
        {
            partes = GetComponentsInChildren<SpriteRenderer>();
            cores = new Color[partes.Length];
            for (int i = 0; i < partes.Length; i++)
                cores[i] = partes[i].color;
        }

        void Update()
        {
            idade += Time.deltaTime;
            transform.position += Vector3.up * (1.6f * Time.deltaTime * Mathf.Max(0.2f, 1f - idade));
            float a = Mathf.Clamp01(1.4f - idade * 1.4f);
            for (int i = 0; i < partes.Length; i++)
            {
                Color c = cores[i];
                c.a *= a;
                partes[i].color = c;
            }
            if (idade > 1f)
                Destroy(gameObject);
        }
    }
}
