using UnityEngine;

namespace RumoAoHexa
{
    /// <summary>
    /// Dados da partida que atravessam as cenas (pontos, vidas, moedas, itens e
    /// resultado). As regras ficam em <see cref="EstadoPartida"/>; aqui só se
    /// guarda a instância e o recorde (no PlayerPrefs).
    /// </summary>
    public static class Sessao
    {
        const string ChaveRecorde = "rumoaohexa_recorde";

        static EstadoPartida estado;

        /// <summary>O estado da partida (nunca é nulo).</summary>
        public static EstadoPartida Estado
        {
            get
            {
                if (estado == null)
                    estado = new EstadoPartida();
                return estado;
            }
        }

        public static bool Iniciada { get; private set; }

        /// <summary>
        /// O projeto usa "Enter Play Mode Options" sem recarregar o domínio, então
        /// campos estáticos precisam ser zerados manualmente a cada Play.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            estado = new EstadoPartida();
            Iniciada = false;
        }

        static EstadoPartida E
        {
            get { return Estado; }
        }

        public static int Pontos
        {
            get { return E.Pontos; }
        }
        public static int Vidas
        {
            get { return E.Vidas; }
        }
        public static int Moedas
        {
            get { return E.Moedas; }
        }
        public static int ItensColetados
        {
            get { return E.ItensColetados; }
        }
        public static int Expulsos
        {
            get { return E.Expulsos; }
        }
        public static int FaseAtual
        {
            get { return E.FaseAtual; }
        }
        public static ResultadoJogo Resultado
        {
            get { return E.Resultado; }
        }
        public static bool NovoRecorde
        {
            get { return E.NovoRecorde; }
        }

        public static int Recorde
        {
            get { return PlayerPrefs.GetInt(ChaveRecorde, 0); }
        }

        public static void NovoJogo()
        {
            E.NovoJogo();
            Iniciada = true;
        }

        /// <summary>Volta ao menu: a partida acaba (para jogar de novo é preciso apertar JOGAR).</summary>
        public static void Encerrar()
        {
            Iniciada = false;
        }

        /// <summary>Permite dar Play direto em qualquer fase pelo Editor.</summary>
        public static void GarantirIniciada()
        {
            if (!Iniciada)
                NovoJogo();
        }

        public static void MarcarInicioFase(int fase)
        {
            E.MarcarInicioFase(fase);
        }

        public static void RestaurarInicioFase()
        {
            E.RestaurarInicioFase();
        }

        /// <summary>Encerra a partida e atualiza o recorde.</summary>
        public static void Finalizar(ResultadoJogo resultado)
        {
            if (E.Finalizar(resultado, Recorde))
            {
                PlayerPrefs.SetInt(ChaveRecorde, E.Pontos);
                PlayerPrefs.Save();
            }
        }
    }
}
