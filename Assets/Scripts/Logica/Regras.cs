namespace RumoAoHexa
{
    /// <summary>
    /// Números do jogo num lugar só (pontuação, vidas e itens obrigatórios),
    /// para ficar fácil de ajustar. Não depende do Unity: é testado fora dele.
    /// </summary>
    public static class Regras
    {
        public const int VidasIniciais = 3;
        public const int VidasMaximas = 5;

        public const int PontosMoeda = 10;
        public const int PontosItemObrigatorio = 250;
        public const int PontosExpulsao = 100;
        /// <summary>Coração pego com as vidas no máximo vira pontos.</summary>
        public const int PontosCoracaoExtra = 200;
        public const int BonusFase = 1000;

        /// <summary>As 5 estrelas da camisa (fase 1), na ordem em que aparecem no mapa.</summary>
        public static readonly int[] AnosEstrelas = { 1958, 1962, 1970, 1994, 2002 };

        /// <summary>As letras da fase 2, na ordem em que aparecem no mapa.</summary>
        public const string LetrasHexa = "HEXA";

        public static int ItensObrigatorios(int fase)
        {
            return fase == 1 ? AnosEstrelas.Length : LetrasHexa.Length;
        }

        public static int TotalItensDoJogo
        {
            get { return AnosEstrelas.Length + LetrasHexa.Length; }
        }

        /// <summary>
        /// Rótulo do item obrigatório de número <paramref name="indice"/> (0 = o mais à esquerda):
        /// o ano da estrela na fase 1 ou a letra na fase 2.
        /// </summary>
        public static string RotuloItem(int fase, int indice)
        {
            if (fase == 1)
                return indice >= 0 && indice < AnosEstrelas.Length ? AnosEstrelas[indice].ToString() : "";
            return indice >= 0 && indice < LetrasHexa.Length ? LetrasHexa[indice].ToString() : "";
        }

        /// <summary>
        /// Bônus acumulado de uma bola que expulsou vários argentinos:
        /// 2 = DOBLETE, 3 = HAT-TRICK, 4 ou mais = GOLEADA.
        /// </summary>
        public static int BonusCombo(int acertos)
        {
            if (acertos <= 1)
                return 0;
            if (acertos == 2)
                return 100;
            if (acertos == 3)
                return 300;
            return 500 + (acertos - 4) * 200;
        }

        public static string NomeCombo(int acertos)
        {
            if (acertos == 2)
                return "DOBLETE!";
            if (acertos == 3)
                return "HAT-TRICK!";
            if (acertos >= 4)
                return "GOLEADA!";
            return null;
        }
    }
}
