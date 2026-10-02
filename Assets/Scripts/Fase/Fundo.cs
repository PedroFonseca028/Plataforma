using UnityEngine;

namespace Curupira
{
    /// <summary>Monta as camadas de fundo (parallax) de cada tema.</summary>
    public static class Fundo
    {
        /// <param name="rolagemAutomatica">No menu o fundo rola sozinho (efeito de corrida).</param>
        public static void Criar(Tema tema, Transform cam, float rolagemAutomatica = 0f)
        {
            float a = rolagemAutomatica;
            if (tema == Tema.Mata)
            {
                Camada("fundo_mata_ceu", cam, Ordem.Ceu, 1f, 1f, 0f, 0f, a);
                Camada("fundo_mata_nuvens", cam, Ordem.FundoDistante, 0.95f, 0.95f, 0.5f, -0.25f - a * 0.1f, a);
                Camada("fundo_mata_serra", cam, Ordem.Fundo1, 0.9f, 0.92f, 0f, -a * 0.1f, a);
                Camada("fundo_mata_morros", cam, Ordem.Fundo2, 0.75f, 0.85f, -0.5f, -a * 0.25f, a);
                Camada("fundo_mata_floresta", cam, Ordem.Fundo3, 0.55f, 0.75f, -0.8f, -a * 0.45f, a);
            }
            else
            {
                Camada("fundo_queimada_ceu", cam, Ordem.Ceu, 1f, 1f, 0f, 0f, a);
                Camada("fundo_queimada_fumaca", cam, Ordem.FundoDistante, 0.93f, 0.95f, 0f, -0.35f - a * 0.1f, a);
                Camada("fundo_queimada_serra", cam, Ordem.Fundo1, 0.9f, 0.92f, 0f, -a * 0.1f, a);
                Camada("fundo_queimada_troncos", cam, Ordem.Fundo2, 0.72f, 0.85f, -0.5f, -a * 0.25f, a);
                Camada("fundo_queimada_perto", cam, Ordem.Fundo3, 0.5f, 0.75f, -0.8f, -a * 0.45f, a);
            }
        }

        static void Camada(string sprite, Transform cam, int ordem, float fx, float fy, float dy, float vel, float auto)
        {
            CamadaParallax c = CamadaParallax.Criar(sprite, cam, ordem, fx, fy, dy, vel);
            if (c != null && auto != 0f)
                c.usarTempoReal = true;
        }
    }
}
