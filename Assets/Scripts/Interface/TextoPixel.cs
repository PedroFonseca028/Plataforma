using UnityEngine;
using UnityEngine.UI;

namespace RumoAoHexa
{
    /// <summary>
    /// Texto de interface desenhado com a <see cref="FontePixel"/>.
    /// Funciona como um componente de UI comum (herda de MaskableGraphic).
    /// </summary>
    [AddComponentMenu("Rumo ao Hexa/Texto Pixel")]
    [RequireComponent(typeof(CanvasRenderer))]
    public class TextoPixel : MaskableGraphic
    {
        [SerializeField, TextArea] string texto = "TEXTO";
        [Tooltip("Tamanho de cada pixel da fonte, em unidades de UI.")]
        public float escala = 2f;
        public TextAnchor alinhamento = TextAnchor.MiddleCenter;
        public float espacoEntreLinhas = 3f;
        public bool sombra = true;
        public Color corSombra = new Color(0.08f, 0.04f, 0.1f, 0.75f);
        public bool contorno;
        public Color corContorno = new Color(0.1f, 0.05f, 0.1f, 1f);

        public string Texto
        {
            get { return texto; }
            set
            {
                if (texto == value)
                    return;
                texto = value;
                SetVerticesDirty();
            }
        }

        public override Texture mainTexture
        {
            get { return FontePixel.Atlas; }
        }

        /// <summary>Tamanho que o texto ocupa (em unidades de UI).</summary>
        public Vector2 TamanhoTexto()
        {
            string[] linhas = (texto ?? "").Split('\n');
            float largura = 0f;
            foreach (string l in linhas)
                largura = Mathf.Max(largura, FontePixel.LarguraLinha(l) * escala);
            float altura = linhas.Length * FontePixel.AlturaGlifo * escala + (linhas.Length - 1) * espacoEntreLinhas * escala;
            return new Vector2(largura, altura);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (string.IsNullOrEmpty(texto))
                return;

            Rect r = rectTransform.rect;
            string[] linhas = texto.Split('\n');
            float alturaLinha = FontePixel.AlturaGlifo * escala;
            float passoLinha = alturaLinha + espacoEntreLinhas * escala;
            float alturaTotal = linhas.Length * alturaLinha + (linhas.Length - 1) * espacoEntreLinhas * escala;

            // posição vertical do topo do bloco de texto
            float topo;
            switch (alinhamento)
            {
                case TextAnchor.UpperLeft:
                case TextAnchor.UpperCenter:
                case TextAnchor.UpperRight:
                    topo = r.yMax;
                    break;
                case TextAnchor.LowerLeft:
                case TextAnchor.LowerCenter:
                case TextAnchor.LowerRight:
                    topo = r.yMin + alturaTotal;
                    break;
                default:
                    topo = r.center.y + alturaTotal * 0.5f;
                    break;
            }
            // as linhas de acento ficam "em cima"; desloca um pouco para centralizar as letras
            if (alinhamento == TextAnchor.MiddleLeft || alinhamento == TextAnchor.MiddleCenter || alinhamento == TextAnchor.MiddleRight)
                topo += escala;

            Color32 corPrincipal = color;
            Color32 corS = new Color(corSombra.r, corSombra.g, corSombra.b, corSombra.a * color.a);
            Color32 corC = new Color(corContorno.r, corContorno.g, corContorno.b, corContorno.a * color.a);

            // camadas: contorno -> sombra -> texto
            if (contorno)
            {
                float e = escala;
                Vector2[] desl = { new Vector2(-e, 0), new Vector2(e, 0), new Vector2(0, e), new Vector2(0, -e),
                                   new Vector2(-e, -e), new Vector2(e, -e), new Vector2(-e, e), new Vector2(e, e) };
                foreach (Vector2 d in desl)
                    Escrever(vh, linhas, r, topo, passoLinha, d, corC);
            }
            if (sombra)
                Escrever(vh, linhas, r, topo, passoLinha, new Vector2(escala, -escala) * (contorno ? 2f : 1f), corS);
            Escrever(vh, linhas, r, topo, passoLinha, Vector2.zero, corPrincipal);
        }

        void Escrever(VertexHelper vh, string[] linhas, Rect r, float topo, float passoLinha, Vector2 desloc, Color32 cor)
        {
            float y = topo;
            foreach (string linha in linhas)
            {
                float larguraLinha = FontePixel.LarguraLinha(linha) * escala;
                float x;
                switch (alinhamento)
                {
                    case TextAnchor.UpperLeft:
                    case TextAnchor.MiddleLeft:
                    case TextAnchor.LowerLeft:
                        x = r.xMin;
                        break;
                    case TextAnchor.UpperRight:
                    case TextAnchor.MiddleRight:
                    case TextAnchor.LowerRight:
                        x = r.xMax - larguraLinha;
                        break;
                    default:
                        x = r.center.x - larguraLinha * 0.5f;
                        break;
                }
                // alinha ao "pixel" da fonte para ficar nítido
                x = Mathf.Round(x);
                float yl = Mathf.Round(y);

                for (int i = 0; i < linha.Length; i++)
                {
                    char c = FontePixel.Normalizar(linha[i]);
                    if (c == ' ')
                    {
                        x += 4 * escala;
                        continue;
                    }
                    FontePixel.Glifo g;
                    if (!FontePixel.TentarGlifo(c, out g))
                        continue;

                    float w = g.largura * escala;
                    float h = FontePixel.AlturaGlifo * escala;
                    float x0 = x + desloc.x;
                    float y1 = yl + desloc.y;
                    float y0 = y1 - h;
                    int inicio = vh.currentVertCount;
                    vh.AddVert(new Vector3(x0, y0), cor, new Vector2(g.uv.xMin, g.uv.yMin));
                    vh.AddVert(new Vector3(x0, y1), cor, new Vector2(g.uv.xMin, g.uv.yMax));
                    vh.AddVert(new Vector3(x0 + w, y1), cor, new Vector2(g.uv.xMax, g.uv.yMax));
                    vh.AddVert(new Vector3(x0 + w, y0), cor, new Vector2(g.uv.xMax, g.uv.yMin));
                    vh.AddTriangle(inicio, inicio + 1, inicio + 2);
                    vh.AddTriangle(inicio + 2, inicio + 3, inicio);
                    x += w + escala;
                }
                y -= passoLinha;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetVerticesDirty();
        }
#endif
    }
}
