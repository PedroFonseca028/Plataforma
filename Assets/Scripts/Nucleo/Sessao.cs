using UnityEngine;

namespace Curupira
{
    public enum ResultadoJogo
    {
        Nenhum,
        Vitoria,
        Derrota,
    }

    /// <summary>
    /// Dados da partida que atravessam as cenas: pontos, vidas, sementes e
    /// o resultado final. O recorde fica salvo no PlayerPrefs.
    /// </summary>
    public static class Sessao
    {
        public const int VidasIniciais = 3;
        public const int VidasMaximas = 5;
        public const int TotalSementesDoJogo = 18; // 8 na fase 1 + 10 na fase 2
        const string ChaveRecorde = "curupira_recorde";

        public static int Pontos;
        public static int Vidas;
        public static int SementesColetadas;
        public static int FaseAtual;
        public static ResultadoJogo Resultado;
        public static bool NovoRecorde;
        public static bool Iniciada;

        // Valores guardados no começo da fase (usados em "Reiniciar fase").
        static int pontosInicioFase;
        static int vidasInicioFase;
        static int sementesInicioFase;

        /// <summary>
        /// O projeto usa "Enter Play Mode Options" sem recarregar o domínio, então
        /// campos estáticos precisam ser zerados manualmente a cada Play.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            Iniciada = false;
            Pontos = 0;
            Vidas = VidasIniciais;
            SementesColetadas = 0;
            FaseAtual = 0;
            Resultado = ResultadoJogo.Nenhum;
            NovoRecorde = false;
        }

        public static int Recorde
        {
            get { return PlayerPrefs.GetInt(ChaveRecorde, 0); }
        }

        public static void NovoJogo()
        {
            Iniciada = true;
            Pontos = 0;
            Vidas = VidasIniciais;
            SementesColetadas = 0;
            FaseAtual = 1;
            Resultado = ResultadoJogo.Nenhum;
            NovoRecorde = false;
        }

        /// <summary>Permite dar Play direto em qualquer fase pelo Editor.</summary>
        public static void GarantirIniciada()
        {
            if (!Iniciada)
                NovoJogo();
        }

        public static void MarcarInicioFase(int fase)
        {
            FaseAtual = fase;
            pontosInicioFase = Pontos;
            vidasInicioFase = Vidas;
            sementesInicioFase = SementesColetadas;
        }

        public static void RestaurarInicioFase()
        {
            Pontos = pontosInicioFase;
            Vidas = Mathf.Max(1, vidasInicioFase);
            SementesColetadas = sementesInicioFase;
        }

        public static void SomarPontos(int valor)
        {
            Pontos = Mathf.Max(0, Pontos + valor);
        }

        /// <summary>Encerra a partida e atualiza o recorde.</summary>
        public static void Finalizar(ResultadoJogo resultado)
        {
            Resultado = resultado;
            NovoRecorde = Pontos > Recorde;
            if (NovoRecorde)
            {
                PlayerPrefs.SetInt(ChaveRecorde, Pontos);
                PlayerPrefs.Save();
            }
        }
    }
}
