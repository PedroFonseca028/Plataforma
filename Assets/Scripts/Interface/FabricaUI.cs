using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace Curupira
{
    /// <summary>
    /// Funções para montar a interface (Canvas, textos, imagens, botões e painéis)
    /// por código, sempre com o mesmo estilo visual.
    /// Resolução de referência: 960 x 540.
    /// </summary>
    public static class FabricaUI
    {
        public static readonly Vector2 Referencia = new Vector2(960f, 540f);
        public static readonly Color CorTexto = new Color(1f, 0.97f, 0.88f);
        public static readonly Color CorDestaque = new Color(1f, 0.85f, 0.3f);
        public static readonly Color CorVerde = new Color(0.62f, 0.92f, 0.45f);
        public static readonly Color CorVermelha = new Color(1f, 0.42f, 0.36f);

        public static Canvas CriarCanvas(string nome, int ordem)
        {
            var go = new GameObject(nome);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = ordem;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = Referencia;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = Arte.PixelsPorUnidade;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        /// <summary>Cria o EventSystem (com o módulo do Input System) se a cena não tiver um.</summary>
        public static void GarantirEventSystem()
        {
            if (EventSystem.current != null)
                return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }

        public static RectTransform Retangulo(string nome, Transform pai, Vector2 ancora, Vector2 posicao, Vector2 tamanho)
        {
            var go = new GameObject(nome, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(pai, false);
            rt.anchorMin = ancora;
            rt.anchorMax = ancora;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = posicao;
            rt.sizeDelta = tamanho;
            return rt;
        }

        /// <summary>Estica o retângulo para ocupar todo o pai.</summary>
        public static void Esticar(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static TextoPixel Texto(Transform pai, string texto, Vector2 ancora, Vector2 posicao,
            float escala, Color cor, TextAnchor alinhamento = TextAnchor.MiddleCenter, Vector2? tamanho = null)
        {
            RectTransform rt = Retangulo("Texto", pai, ancora, posicao, tamanho ?? new Vector2(900f, 60f));
            var t = rt.gameObject.AddComponent<TextoPixel>();
            t.Texto = texto;
            t.escala = escala;
            t.color = cor;
            t.alinhamento = alinhamento;
            t.raycastTarget = false;
            return t;
        }

        public static Image Imagem(Transform pai, Sprite sprite, Vector2 ancora, Vector2 posicao, Vector2 tamanho,
            Color? cor = null, bool fatiada = false)
        {
            RectTransform rt = Retangulo(sprite != null ? sprite.name : "Imagem", pai, ancora, posicao, tamanho);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = cor ?? Color.white;
            img.raycastTarget = false;
            if (fatiada)
            {
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 0.5f; // bordas com 2x o tamanho do pixel
            }
            return img;
        }

        public static Image Painel(Transform pai, Vector2 ancora, Vector2 posicao, Vector2 tamanho)
        {
            Image img = Imagem(pai, Arte.Sprite("ui_painel"), ancora, posicao, tamanho, Color.white, true);
            img.raycastTarget = true; // bloqueia cliques no que estiver atrás
            return img;
        }

        /// <summary>Fundo escuro que cobre a tela inteira.</summary>
        public static Image Cortina(Transform pai, Color cor)
        {
            RectTransform rt = Retangulo("Cortina", pai, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Esticar(rt);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = Arte.Sprite("ui_branco");
            img.color = cor;
            img.raycastTarget = true;
            return img;
        }

        public static Button Botao(Transform pai, string texto, Vector2 ancora, Vector2 posicao, Vector2 tamanho,
            UnityAction aoClicar)
        {
            Image fundo = Imagem(pai, Arte.Sprite("ui_botao"), ancora, posicao, tamanho, Color.white, true);
            fundo.gameObject.name = "Botao " + texto;
            fundo.raycastTarget = true;
            var botao = fundo.gameObject.AddComponent<Button>();
            botao.targetGraphic = fundo;
            ColorBlock cores = botao.colors;
            cores.normalColor = new Color(0.85f, 0.85f, 0.85f);
            cores.highlightedColor = new Color(1f, 1f, 1f);
            cores.selectedColor = new Color(1f, 1f, 1f);
            cores.pressedColor = new Color(0.65f, 0.65f, 0.65f);
            cores.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
            cores.fadeDuration = 0.08f;
            botao.colors = cores;
            if (aoClicar != null)
                botao.onClick.AddListener(aoClicar);
            botao.onClick.AddListener(() => SomJogo.Tocar(Som.Clique));

            TextoPixel rotulo = Texto(fundo.transform, texto, new Vector2(0.5f, 0.5f), new Vector2(0f, 1f), 2f,
                CorTexto, TextAnchor.MiddleCenter, tamanho);
            rotulo.gameObject.name = "Rotulo";

            fundo.gameObject.AddComponent<EfeitoBotao>();
            return botao;
        }

        /// <summary>Liga a navegação por teclado/controle entre botões em coluna.</summary>
        public static void NavegacaoVertical(params Button[] botoes)
        {
            for (int i = 0; i < botoes.Length; i++)
            {
                var nav = new Navigation();
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = botoes[(i - 1 + botoes.Length) % botoes.Length];
                nav.selectOnDown = botoes[(i + 1) % botoes.Length];
                botoes[i].navigation = nav;
            }
        }

        /// <summary>Seleciona um botão (para funcionar com Enter/setas/controle).</summary>
        public static void Selecionar(Selectable s)
        {
            if (s == null || EventSystem.current == null)
                return;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(s.gameObject);
        }
    }
}
