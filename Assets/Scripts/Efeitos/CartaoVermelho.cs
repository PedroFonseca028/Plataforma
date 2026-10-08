using UnityEngine;

namespace RumoAoHexa
{
    /// <summary>
    /// O cartão vermelho que aparece em cima do argentino expulso: sobe
    /// girando, para um instante no ar e some.
    /// </summary>
    public class CartaoVermelho : MonoBehaviour
    {
        const float Duracao = 1.1f;

        float idade;
        Vector3 inicio;
        SpriteRenderer sr;

        public static void Mostrar(Vector3 posicao)
        {
            Sprite s = Arte.Sprite("cartao");
            if (s == null)
                return;
            SpriteRenderer sr = Efeitos.NovoSprite("Cartao vermelho", null, s, Ordem.TextoFlutuante, true);
            sr.transform.position = posicao;
            var c = sr.gameObject.AddComponent<CartaoVermelho>();
            c.sr = sr;
            c.inicio = posicao;
            c.Update();
        }

        void Update()
        {
            idade += Time.deltaTime;
            float t = Mathf.Clamp01(idade / Duracao);
            // sobe rápido e desacelera
            float subida = 1.1f * (1f - (1f - Mathf.Min(1f, t * 2.5f)) * (1f - Mathf.Min(1f, t * 2.5f)));
            transform.position = inicio + new Vector3(0f, subida, 0f);
            // gira até ficar de frente e "balança" como na mão do juiz
            float giro = t < 0.3f ? Mathf.Lerp(-200f, 0f, t / 0.3f) : Mathf.Sin(idade * 18f) * 6f * (1f - t);
            transform.rotation = Quaternion.Euler(0f, 0f, giro);
            float escala = t < 0.3f ? Mathf.Lerp(0.5f, 1.6f, t / 0.3f) : 1.6f;
            transform.localScale = Vector3.one * escala;
            Color c = sr.color;
            c.a = t < 0.75f ? 1f : 1f - (t - 0.75f) / 0.25f;
            sr.color = c;
            if (idade >= Duracao)
                Destroy(gameObject);
        }
    }
}
