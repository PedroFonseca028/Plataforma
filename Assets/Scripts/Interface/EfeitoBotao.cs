using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Curupira
{
    /// <summary>
    /// Deixa o botão "vivo": cresce um pouco quando está selecionado ou com o
    /// mouse em cima, toca um som e mostra setinhas ao lado do texto.
    /// </summary>
    public class EfeitoBotao : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        bool destacado;
        Selectable selectable;
        TextoPixel rotulo;
        string textoOriginal;

        void Awake()
        {
            selectable = GetComponent<Selectable>();
            rotulo = GetComponentInChildren<TextoPixel>();
            if (rotulo != null)
                textoOriginal = rotulo.Texto;
        }

        void Update()
        {
            float alvo = destacado ? 1.08f : 1f;
            float s = Mathf.Lerp(transform.localScale.x, alvo, 1f - Mathf.Exp(-18f * Time.unscaledDeltaTime));
            transform.localScale = new Vector3(s, s, 1f);
        }

        void Destacar(bool valor)
        {
            if (destacado == valor)
                return;
            destacado = valor;
            if (rotulo != null && textoOriginal != null)
            {
                rotulo.Texto = valor ? "> " + textoOriginal + " <" : textoOriginal;
                rotulo.color = valor ? FabricaUI.CorDestaque : FabricaUI.CorTexto;
            }
            if (valor)
                SomJogo.Tocar(Som.Foco, 0.6f);
        }

        public void OnSelect(BaseEventData dados)
        {
            Destacar(true);
        }

        public void OnDeselect(BaseEventData dados)
        {
            Destacar(false);
        }

        public void OnPointerEnter(PointerEventData dados)
        {
            // o mouse também "seleciona" o botão, igual ao teclado
            if (selectable != null && selectable.interactable && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void OnPointerExit(PointerEventData dados)
        {
        }

        void OnDisable()
        {
            destacado = false;
            transform.localScale = Vector3.one;
            if (rotulo != null && textoOriginal != null)
            {
                rotulo.Texto = textoOriginal;
                rotulo.color = FabricaUI.CorTexto;
            }
        }
    }
}
