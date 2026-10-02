using UnityEngine;

namespace Curupira
{
    /// <summary>Rio (fase 1) ou poço de brasas (fase 2): quem cai perde uma vida.</summary>
    public class ZonaLiquida : MonoBehaviour
    {
        public bool brasas;

        void OnTriggerEnter2D(Collider2D outro)
        {
            Jogador j = outro.GetComponent<Jogador>();
            if (j == null || !j.Vivo)
                return;
            SomJogo.Tocar(Som.Splash);
            Color cor = brasas ? new Color(1f, 0.6f, 0.2f) : new Color(0.6f, 0.9f, 1f);
            Efeitos.Particulas(brasas ? "brasa" : "brilho", j.transform.position, 14, cor, 3f, 6f, 0.6f, 12f, true, 45f, 135f);
            if (GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.JogadorCaiuNoAbismo();
        }
    }
}
