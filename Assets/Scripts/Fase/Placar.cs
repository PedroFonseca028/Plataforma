using UnityEngine;

namespace RumoAoHexa
{
    /// <summary>
    /// Placar do estádio, no alto da arquibancada. Cada argentino expulso é gol
    /// do Brasil e cada vida perdida é gol da Argentina. Fica bem longe, então
    /// quase acompanha a câmera (parallax de 97%).
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class Placar : MonoBehaviour
    {
        const float Fator = 0.97f;
        const float PiscarDuracao = 1.2f;

        Transform cam;
        Vector2 deslocamento;
        SpriteRenderer[] brasil;
        SpriteRenderer[] argentina;
        SpriteRenderer corpo;
        int ultimoBrasil = -1;
        int ultimoArgentina = -1;
        float piscar;

        public int Brasil
        {
            get { return Mathf.Max(0, ultimoBrasil); }
        }
        public int Argentina
        {
            get { return Mathf.Max(0, ultimoArgentina); }
        }

        public static Placar Criar(Transform cam)
        {
            Sprite s = Arte.Sprite("placar");
            if (s == null)
                return null;
            SpriteRenderer sr = Efeitos.NovoSprite("Placar do estadio", null, s, Ordem.Placar, true);
            var p = sr.gameObject.AddComponent<Placar>();
            p.corpo = sr;
            p.cam = cam;
            // começa um pouco à direita do centro da tela e vai "ficando para trás" bem devagar
            p.deslocamento = new Vector2(cam.position.x * (1f - Fator) + 3f, cam.position.y * (1f - Fator) + 4.4f);
            p.brasil = p.Digitos(-1.25f);
            p.argentina = p.Digitos(1.25f);
            p.Mostrar(0, 0);
            p.LateUpdate();
            return p;
        }

        SpriteRenderer[] Digitos(float x)
        {
            var r = new SpriteRenderer[2];
            for (int i = 0; i < 2; i++)
            {
                SpriteRenderer d = Efeitos.NovoSprite("Digito", transform, null, Ordem.Placar + 1, true);
                d.transform.localScale = new Vector3(2f, 2f, 1f);
                d.transform.localPosition = new Vector3(x + (i == 0 ? -0.38f : 0.38f), -0.4f, 0f);
                r[i] = d;
            }
            return r;
        }

        void Mostrar(int b, int a)
        {
            Escrever(brasil, b, -1.25f);
            Escrever(argentina, a, 1.25f);
            if (ultimoBrasil >= 0 && (b != ultimoBrasil || a != ultimoArgentina))
                piscar = PiscarDuracao;
            ultimoBrasil = b;
            ultimoArgentina = a;
        }

        /// <summary>Escreve o número (0 a 99) centralizado em <paramref name="centro"/>.</summary>
        static void Escrever(SpriteRenderer[] d, int valor, float centro)
        {
            valor = Mathf.Clamp(valor, 0, 99);
            float y = d[1].transform.localPosition.y;
            if (valor < 10)
            {
                d[0].enabled = false;
                d[1].enabled = true;
                d[1].sprite = Arte.Sprite("led_" + valor);
                d[1].transform.localPosition = new Vector3(centro, y, 0f);
                return;
            }
            d[0].enabled = true;
            d[1].enabled = true;
            d[0].sprite = Arte.Sprite("led_" + (valor / 10));
            d[1].sprite = Arte.Sprite("led_" + (valor % 10));
            d[0].transform.localPosition = new Vector3(centro - 0.38f, y, 0f);
            d[1].transform.localPosition = new Vector3(centro + 0.38f, y, 0f);
        }

        void LateUpdate()
        {
            if (cam == null)
                return;
            Vector3 c = cam.position;
            transform.position = new Vector3(c.x * Fator + deslocamento.x, c.y * Fator + deslocamento.y, 9f);

            GerenciadorFase g = GerenciadorFase.Instancia;
            if (g != null && g.Placar != null && (g.Placar.Brasil != ultimoBrasil || g.Placar.Argentina != ultimoArgentina))
                Mostrar(g.Placar.Brasil, g.Placar.Argentina);

            if (piscar > 0f)
            {
                piscar -= Time.deltaTime;
                bool aceso = piscar <= 0f || Mathf.Repeat(piscar * 6f, 1f) < 0.5f;
                Color cor = aceso ? Color.white : new Color(1f, 0.85f, 0.3f);
                foreach (SpriteRenderer d in brasil)
                    d.color = cor;
                corpo.color = aceso ? Color.white : new Color(1f, 0.95f, 0.8f);
            }
        }
    }
}
