using UnityEngine;

namespace RumoAoHexa
{
    /// <summary>Monta as camadas de fundo (parallax) de cada tema.</summary>
    public static class Fundo
    {
        /// <param name="rolagemAutomatica">No menu o fundo rola sozinho (efeito de corrida).</param>
        /// <param name="comPlacar">No estádio, mostra o placar ao vivo (BRA x ARG).</param>
        public static void Criar(Tema tema, Transform cam, float rolagemAutomatica = 0f, bool comPlacar = false)
        {
            float a = rolagemAutomatica;
            if (tema == Tema.Praia)
            {
                Camada("fundo_praia_ceu", cam, Ordem.Ceu, 1f, 1f, 0f, 0f, a);
                Camada("fundo_praia_nuvens", cam, Ordem.FundoDistante, 0.95f, 0.95f, 0.5f, -0.2f - a * 0.1f, a);
                Camada("fundo_praia_morros", cam, Ordem.Fundo1, 0.9f, 0.92f, 0.3f, -a * 0.1f, a);
                Camada("fundo_praia_mar", cam, Ordem.Fundo2, 0.8f, 0.88f, 0.3f, -a * 0.2f, a);
                Camada("fundo_praia_orla", cam, Ordem.Fundo3, 0.6f, 0.75f, -0.6f, -a * 0.4f, a);
            }
            else
            {
                Camada("fundo_estadio_ceu", cam, Ordem.Ceu, 1f, 1f, 0f, 0f, a);
                Camada("fundo_estadio_refletores", cam, Ordem.Fundo1, 0.95f, 0.95f, 0.4f, -a * 0.05f, a);
                CamadaParallax torcida = Camada("fundo_estadio_torcida_0", cam, Ordem.Fundo2, 0.82f, 0.88f, 0.2f, -a * 0.18f, a);
                if (torcida != null)
                {
                    // a torcida pula: alterna os dois desenhos da arquibancada
                    var anim = torcida.gameObject.AddComponent<AnimadorSprite>();
                    anim.alvo = torcida.GetComponent<SpriteRenderer>();
                    anim.usarTempoReal = a != 0f;
                    anim.Tocar("fundo_estadio_torcida", 2.5f);
                }
                Camada("fundo_estadio_placas", cam, Ordem.Fundo3, 0.6f, 0.75f, -0.6f, -a * 0.4f, a);
                if (comPlacar)
                    Placar.Criar(cam);
            }
        }

        static CamadaParallax Camada(string sprite, Transform cam, int ordem, float fx, float fy, float dy, float vel, float auto)
        {
            CamadaParallax c = CamadaParallax.Criar(sprite, cam, ordem, fx, fy, dy, vel);
            if (c != null && auto != 0f)
                c.usarTempoReal = true;
            return c;
        }
    }
}
