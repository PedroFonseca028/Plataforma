using UnityEngine;

namespace Curupira
{
    /// <summary>Onda sonora do assobio: um anel que cresce e some.</summary>
    public class AnelAssobio : MonoBehaviour
    {
        float raio;
        float idade;
        SpriteRenderer sr;
        const float Duracao = 0.45f;

        public static void Criar(Vector3 centro, float raio)
        {
            Sprite s = Arte.Sprite("anel");
            if (s == null)
                return;
            SpriteRenderer sr = Efeitos.NovoSprite("Anel do assobio", null, s, Ordem.Efeitos, true);
            sr.transform.position = centro;
            sr.color = new Color(0.75f, 1f, 0.6f, 0.9f);
            var a = sr.gameObject.AddComponent<AnelAssobio>();
            a.raio = raio;
            a.sr = sr;
            a.Update();
        }

        void Update()
        {
            idade += Time.deltaTime;
            float t = Mathf.Clamp01(idade / Duracao);
            // o sprite tem 64 px = 4 unidades de diâmetro
            float diametro = Mathf.Lerp(0.5f, raio * 2f, 1f - (1f - t) * (1f - t));
            transform.localScale = Vector3.one * (diametro / 4f);
            Color c = sr.color;
            c.a = 0.9f * (1f - t);
            sr.color = c;
            if (idade >= Duracao)
                Destroy(gameObject);
        }
    }
}
