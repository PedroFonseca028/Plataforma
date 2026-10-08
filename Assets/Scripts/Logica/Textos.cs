namespace RumoAoHexa
{
    /// <summary>
    /// Textos fixos das telas. Ficam juntos para facilitar a revisão e para o
    /// teste conferir que a fonte pixelada tem todas as letras usadas.
    /// </summary>
    public static class Textos
    {
        public const string Titulo = "RUMO AO HEXA";
        public const string Subtitulo = "OS ARGENTINOS ROUBARAM AS ESTRELAS DA AMARELINHA. RECUPERE TUDO!";

        // ------------------------------------------------------------ tela inicial: como jogar
        public const string ComoJogarTitulo = "COMO JOGAR";

        /// <summary>Controles: tecla à esquerda, ação à direita (aparecem direto na tela inicial).</summary>
        public static readonly string[,] Controles =
        {
            { "← → / A D", "ANDAR" },
            { "ESPAÇO / Z", "PULAR" },
            { "J / X", "CHUTAR A BOLA" },
            { "↑ + J / K", "CAVADINHA (POR CIMA)" },
            { "SEGURE J", "FOLHA SECA (FORTE)" },
            { "J (BOLA LONGE)", "CHAMAR A BOLA" },
            { "↓ + ESPAÇO", "DESCER DA PLATAFORMA" },
            { "ESC / P", "PAUSAR" },
            { "M", "LIGA/DESLIGA O SOM" },
        };

        public const string ObjetivoTitulo = "OBJETIVO";
        public const string ObjetivoTexto = "FASE 1: AS 5 ESTRELAS DA CAMISA\nFASE 2: AS LETRAS H-E-X-A";
        public const string ComoGanharTitulo = "COMO GANHAR";
        public const string ComoGanharTexto = "PEGUE TODOS OS ITENS\nDAS DUAS FASES";
        public const string ComoPerderTitulo = "COMO PERDER";
        public const string ComoPerderTexto = "ARGENTINO TE MACHUCOU: -1 VIDA\nVIDAS EM ZERO: DERROTA";
        public const string DicaBola = "SÓ A BOLA EXPULSA OS ARGENTINOS!";

        // ------------------------------------------------------------ fases
        public const string Fase1Nome = "FASE 1 - PELADA NA PRAIA";
        public const string Fase2Nome = "FASE 2 - A FINAL NO MARACANÃ";
        public const string Fase1Objetivo = "PEGUE AS 5 ESTRELAS DA CAMISA!";
        public const string Fase2Objetivo = "JUNTE AS LETRAS H-E-X-A!";
        public const string Fase1ObjetivoCurto = "OBJETIVO: 5 ESTRELAS";
        public const string Fase2ObjetivoCurto = "OBJETIVO: H-E-X-A";

        public const string HudControles = "J: CHUTE   ↑+J / K: CAVADINHA   SEGURE J: FOLHA SECA";

        // ------------------------------------------------------------ tela final
        public const string VitoriaTitulo = "VITÓRIA!";
        public const string VitoriaFrase =
            "VOCÊ COLETOU TODOS OS ITENS: AS 5 ESTRELAS E AS LETRAS H-E-X-A.\nÉ HEXA! O BRASIL É HEXACAMPEÃO!";
        public const string DerrotaTitulo = "DERROTA...";
        public const string DerrotaFrase =
            "OS ARGENTINOS TE MACHUCARAM E SUAS VIDAS CHEGARAM A ZERO.\nAPERTE REINICIAR PARA TENTAR DE NOVO!";
        public const string BotaoReiniciar = "REINICIAR";
        public const string BotaoMenu = "MENU PRINCIPAL";
        public const string DicaReiniciar = "ENTER: REINICIAR";
        public const string FimSemResultado = "JOGUE A PARTIR DO MENU PARA VER O RESULTADO.";

        // ------------------------------------------------------------ mensagens durante o jogo
        public const string FaseConcluida = "FASE CONCLUÍDA!";
        public const string EHexa = "É HEXA!!!";
        public const string PerdeuTudo = "SUAS VIDAS ACABARAM!";
        public const string Cuidado = "CUIDADO!";
        public const string BandeirinhaAtivada = "BANDEIRINHA! CAMINHO SALVO";
        public const string Tonto = "FICOU TONTO!";
        public const string MaisUmaVida = "+1 VIDA!";
        public const string Expulso = "EXPULSO!";

        /// <summary>Todos os textos fixos (usado pelo teste da fonte).</summary>
        public static string[] TodosOsTextos()
        {
            var lista = new System.Collections.Generic.List<string>
            {
                Titulo, Subtitulo, ComoJogarTitulo, ObjetivoTitulo, ObjetivoTexto, ComoGanharTitulo, ComoGanharTexto,
                ComoPerderTitulo, ComoPerderTexto, DicaBola, Fase1Nome, Fase2Nome, Fase1Objetivo, Fase2Objetivo,
                Fase1ObjetivoCurto, Fase2ObjetivoCurto, HudControles, VitoriaTitulo, VitoriaFrase, DerrotaTitulo,
                DerrotaFrase, BotaoReiniciar, BotaoMenu, DicaReiniciar, FimSemResultado, FaseConcluida, EHexa, PerdeuTudo, Cuidado,
                BandeirinhaAtivada, Tonto, MaisUmaVida, Expulso,
            };
            foreach (string c in Controles)
                lista.Add(c);
            lista.AddRange(Narrador.FrasesRasteiro);
            lista.AddRange(Narrador.FrasesCavadinha);
            lista.AddRange(Narrador.FrasesFolhaSeca);
            lista.AddRange(Narrador.FrasesEstadio);
            for (int n = 2; n <= 6; n++)
                lista.Add(Regras.NomeCombo(n));
            return lista.ToArray();
        }
    }
}
