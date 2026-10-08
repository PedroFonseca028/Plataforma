using UnityEngine;
using UnityEngine.UI;

namespace RumoAoHexa
{
    /// <summary>Menu de pausa (Esc / P / Start).</summary>
    public class MenuPausa : MonoBehaviour
    {
        GameObject conteudo;
        Button continuar;

        public bool Aberto
        {
            get { return conteudo != null && conteudo.activeSelf; }
        }

        public static MenuPausa Criar(GerenciadorFase g)
        {
            Canvas canvas = FabricaUI.CriarCanvas("Menu de pausa", 20);
            var m = canvas.gameObject.AddComponent<MenuPausa>();
            Image cortina = FabricaUI.Cortina(canvas.transform, new Color(0.03f, 0.02f, 0.06f, 0.6f));
            m.conteudo = cortina.gameObject;
            Vector2 c = new Vector2(0.5f, 0.5f);
            FabricaUI.Painel(cortina.transform, c, Vector2.zero, new Vector2(420f, 330f));
            FabricaUI.Texto(cortina.transform, "PAUSADO", c, new Vector2(0f, 110f), 4f, FabricaUI.CorDestaque).contorno = true;
            m.continuar = FabricaUI.Botao(cortina.transform, "CONTINUAR", c, new Vector2(0f, 30f), new Vector2(280f, 48f), g.AlternarPausa);
            Button reiniciar = FabricaUI.Botao(cortina.transform, "REINICIAR FASE", c, new Vector2(0f, -30f), new Vector2(280f, 48f), g.ReiniciarFase);
            Button menu = FabricaUI.Botao(cortina.transform, "MENU PRINCIPAL", c, new Vector2(0f, -90f), new Vector2(280f, 48f), g.IrParaMenu);
            FabricaUI.NavegacaoVertical(m.continuar, reiniciar, menu);
            FabricaUI.Texto(cortina.transform, "M: LIGA/DESLIGA O SOM", c, new Vector2(0f, -140f), 2f, new Color(1f, 1f, 1f, 0.7f));
            m.conteudo.SetActive(false);
            return m;
        }

        public void Mostrar(bool mostrar)
        {
            conteudo.SetActive(mostrar);
            if (mostrar)
                FabricaUI.Selecionar(continuar);
        }
    }
}
