using UnityEngine;

namespace Curupira
{
    /// <summary>
    /// Fábrica de efeitos visuais simples: animações de uma vez só (poeira,
    /// brilho), partículas e textos de pontuação que sobem e somem.
    /// </summary>
    public static class Efeitos
    {
        static Transform raiz;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            raiz = null;
        }

        static Transform Raiz
        {
            get
            {
                if (raiz == null)
                    raiz = new GameObject("Efeitos").transform;
                return raiz;
            }
        }

        public static SpriteRenderer NovoSprite(string nome, Transform pai, Sprite sprite, int ordem, bool brilha = false)
        {
            var go = new GameObject(nome);
            if (pai != null)
                go.transform.SetParent(pai, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = ordem;
            Materiais2D.Aplicar(sr, brilha);
            return sr;
        }

        /// <summary>Toca uma animação uma vez e some.</summary>
        public static void Animacao(string anim, Vector3 posicao, float qps = 14f, float escala = 1f,
            bool brilha = false, int ordem = Ordem.Efeitos, bool espelhar = false)
        {
            Sprite[] quadros = Arte.Anim(anim);
            if (quadros.Length == 0)
                return;
            SpriteRenderer sr = NovoSprite("fx_" + anim, Raiz, quadros[0], ordem, brilha);
            sr.flipX = espelhar;
            sr.transform.position = posicao;
            sr.transform.localScale = Vector3.one * escala;
            var an = sr.gameObject.AddComponent<AnimadorSprite>();
            an.alvo = sr;
            an.Tocar(quadros, qps, false);
            Object.Destroy(sr.gameObject, quadros.Length / qps + 0.05f);
        }

        /// <summary>Espalha partículas (sprites que voam, caem e desaparecem).</summary>
        public static void Particulas(string sprite, Vector3 posicao, int quantidade, Color cor,
            float velMin, float velMax, float vida, float gravidade, bool brilha = false,
            float anguloMin = 0f, float anguloMax = 360f, float escala = 1f)
        {
            Sprite[] quadros = Arte.Anim(sprite);
            if (quadros.Length == 0)
                return;
            for (int i = 0; i < quantidade; i++)
            {
                Sprite s = quadros[Random.Range(0, quadros.Length)];
                SpriteRenderer sr = NovoSprite("particula", Raiz, s, Ordem.Efeitos, brilha);
                sr.color = cor;
                sr.transform.position = posicao;
                sr.transform.localScale = Vector3.one * escala;
                var p = sr.gameObject.AddComponent<Particula>();
                float ang = Random.Range(anguloMin, anguloMax) * Mathf.Deg2Rad;
                float v = Random.Range(velMin, velMax);
                p.velocidade = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * v;
                p.gravidade = gravidade;
                p.vida = vida * Random.Range(0.75f, 1.15f);
                p.giro = Random.Range(-240f, 240f);
            }
        }

        /// <summary>Texto que sobe e some (ex.: "+100").</summary>
        public static void TextoSubindo(string texto, Vector3 posicao, Color cor)
        {
            var go = new GameObject("texto_" + texto);
            go.transform.SetParent(Raiz, false);
            go.transform.position = posicao;
            float largura = FontePixel.LarguraLinha(texto) / Arte.PixelsPorUnidade;
            float x = -largura * 0.5f;
            for (int i = 0; i < texto.Length; i++)
            {
                char c = FontePixel.Normalizar(texto[i]);
                if (c == ' ')
                {
                    x += 4f / Arte.PixelsPorUnidade;
                    continue;
                }
                Sprite s = FontePixel.SpriteDoGlifo(c);
                if (s == null)
                    continue;
                // sombra + letra
                SpriteRenderer sombra = NovoSprite("s", go.transform, s, Ordem.TextoFlutuante, true);
                sombra.color = new Color(0.1f, 0.05f, 0.1f, 0.85f);
                sombra.transform.localPosition = new Vector3(x + 1f / 16f, -1f / 16f - 0.3f, 0f);
                SpriteRenderer letra = NovoSprite("l", go.transform, s, Ordem.TextoFlutuante + 1, true);
                letra.color = cor;
                letra.transform.localPosition = new Vector3(x, -0.3f, 0f);
                x += (s.rect.width + 1f) / Arte.PixelsPorUnidade;
            }
            go.AddComponent<TextoFlutuante>();
        }
    }
}
