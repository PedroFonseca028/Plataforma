using UnityEngine;

namespace RumoAoHexa
{
    /// <summary>Mar (praia) ou fosso (estádio): quem cai perde uma vida; a bola volta para o pé.</summary>
    public class ZonaLiquida : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D outro)
        {
            Bola bola = outro.GetComponent<Bola>();
            if (bola != null && bola.Voando)
            {
                Efeitos.Particulas("brilho", bola.transform.position, 8, new Color(0.7f, 0.95f, 1f), 2f, 4f, 0.5f, 10f, true, 50f, 130f);
                SomJogo.Tocar(Som.Splash, 0.5f);
                bola.Chamar();
                return;
            }
            Jogador j = outro.GetComponent<Jogador>();
            if (j == null || !j.Vivo)
                return;
            SomJogo.Tocar(Som.Splash);
            Efeitos.Particulas("brilho", j.transform.position, 14, new Color(0.6f, 0.9f, 1f), 3f, 6f, 0.6f, 12f, true, 45f, 135f);
            if (GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.JogadorCaiuNoAbismo();
        }
    }
}
