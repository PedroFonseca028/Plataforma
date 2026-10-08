namespace RumoAoHexa
{
    /// <summary>Frases do narrador que aparecem na tela quando um argentino é expulso.</summary>
    public static class Narrador
    {
        public static readonly string[] FrasesRasteiro =
        {
            "QUE CHUTAÇO!",
            "PRO CHUVEIRO, HERMANO!",
            "CARTÃO VERMELHO!",
            "QUE PINTURA!",
            "NA CARA DO ARGENTINO!",
            "OLHA O QUE ELE FEZ!",
            "TÁ EXPULSO!",
            "QUE BOLAÇO!",
        };

        public static readonly string[] FrasesCavadinha =
        {
            "DE CAVADINHA!",
            "POR COBERTURA!",
            "DE LEVINHO, POR CIMA!",
        };

        public static readonly string[] FrasesFolhaSeca =
        {
            "FOLHA SECA! QUE ISSO!",
            "SOLTOU A BOMBA!",
            "A BOLA FEZ A CURVA!",
        };

        public static readonly string[] FrasesEstadio =
        {
            "GOOOL DO BRASIL!",
            "É DO BRASIL!",
            "A TORCIDA VAI À LOUCURA!",
        };

        /// <summary>
        /// Escolhe a frase. <paramref name="sorteio"/> é qualquer número inteiro (positivo ou
        /// negativo); no estádio, metade das vezes o narrador grita gol.
        /// </summary>
        public static string FraseExpulsao(TipoChute tipo, int sorteio, bool noEstadio)
        {
            int s = sorteio < 0 ? -(sorteio + 1) : sorteio;
            if (noEstadio && s % 2 == 0)
                return FrasesEstadio[(s / 2) % FrasesEstadio.Length];
            string[] lista;
            switch (tipo)
            {
                case TipoChute.Cavadinha:
                    lista = FrasesCavadinha;
                    break;
                case TipoChute.FolhaSeca:
                    lista = FrasesFolhaSeca;
                    break;
                default:
                    lista = FrasesRasteiro;
                    break;
            }
            return lista[s % lista.Length];
        }

        /// <summary>Mensagem ao pegar um item obrigatório.</summary>
        public static string FraseItem(int fase, string rotulo, int faltam)
        {
            string pegou = fase == 1 ? "ESTRELA DE " + rotulo + " RECUPERADA!" : "LETRA " + rotulo + "!";
            if (faltam <= 0)
                return pegou;
            return pegou + (faltam == 1 ? "  FALTA 1" : "  FALTAM " + faltam);
        }
    }

    /// <summary>Placar do estádio: cada expulsão é gol do Brasil, cada vida perdida é gol da Argentina.</summary>
    public class PlacarFase
    {
        public int Brasil { get; private set; }
        public int Argentina { get; private set; }

        public void GolBrasil()
        {
            Brasil++;
        }

        public void GolArgentina()
        {
            Argentina++;
        }

        public string Texto
        {
            get { return "BRA " + Brasil + " X " + Argentina + " ARG"; }
        }
    }
}
