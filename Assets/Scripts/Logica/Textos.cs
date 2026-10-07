namespace RumoAoHexa
{
    /// <summary>
    /// Textos fixos das telas. Ficam juntos para facilitar a revisão e para o
    /// teste conferir que a fonte pixelada tem todas as letras usadas.
    /// </summary>
    public static class Textos
    {
        public const string Titulo = "RUMO AO HEXA";
        public const string Subtitulo = "O CRAQUE CANARINHO CONTRA OS HERMANOS";

        public const string Historia =
            "ANTES DA FINAL, OS ARGENTINOS ROUBARAM AS 5 ESTRELAS DA AMARELINHA!\n" +
            "RECUPERE AS ESTRELAS NA PRAIA E JUNTE AS LETRAS H-E-X-A NO MARACANÃ.";

        /// <summary>Controles: tecla à esquerda, ação à direita (aparecem direto na tela inicial).</summary>
        public static readonly string[,] Controles =
        {
            { "A D / ← →", "ANDAR" },
            { "ESPAÇO / Z", "PULAR (SEGURE PARA IR MAIS ALTO)" },
            { "J / X", "CHUTE RASTEIRO (QUICA E RICOCHETEIA)" },
            { "↑+J / K", "CAVADINHA: A BOLA PASSA POR CIMA" },
            { "SEGURE J", "FOLHA SECA: ATRAVESSA OS ARGENTINOS" },
            { "J (BOLA LONGE)", "A BOLA VOLTA PARA O PÉ" },
            { "↓ + ESPAÇO", "DESCER DA PLATAFORMA" },
            { "ESC / P    M", "PAUSAR    LIGA/DESLIGA O SOM" },
        };

        public const string Dicas =
            "SÓ A BOLA EXPULSA OS ARGENTINOS. PULAR NA CABEÇA SÓ DEIXA TONTO.\n" +
            "MOEDAS DÃO PONTOS, O CORAÇÃO DÁ VIDA E A BANDEIRINHA SALVA O CAMINHO.";

        public const string Fase1Nome = "FASE 1 - PELADA NA PRAIA";
        public const string Fase2Nome = "FASE 2 - A FINAL NO MARACANÃ";
        public const string Fase1Objetivo = "RECUPERE AS 5 ESTRELAS DA CAMISA!";
        public const string Fase2Objetivo = "JUNTE AS LETRAS H-E-X-A!";

        public const string HudControles = "J: CHUTE   ↑+J / K: CAVADINHA   SEGURE J: FOLHA SECA";

        public const string VitoriaTitulo = "VITÓRIA!";
        public const string VitoriaFrase = "É HEXA! AS ESTRELAS VOLTARAM PARA A AMARELINHA\nE O BRASIL É HEXACAMPEÃO!";
        public const string DerrotaTitulo = "DERROTA...";
        public const string DerrotaFrase = "SUAS VIDAS ACABARAM E OS HERMANOS LEVARAM A MELHOR.\nBORA BUSCAR A REVANCHE!";
        public const string FimSemResultado = "JOGUE A PARTIR DO MENU PARA VER O RESULTADO.";

        public const string FaseConcluida = "FASE CONCLUÍDA!";
        public const string EHexa = "É HEXA!!!";
        public const string PerdeuTudo = "OS HERMANOS VENCERAM...";
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
                Titulo, Subtitulo, Historia, Dicas, Fase1Nome, Fase2Nome, Fase1Objetivo, Fase2Objetivo,
                HudControles, VitoriaTitulo, VitoriaFrase, DerrotaTitulo, DerrotaFrase, FimSemResultado,
                FaseConcluida, EHexa, PerdeuTudo, Cuidado, BandeirinhaAtivada, Tonto, MaisUmaVida, Expulso,
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
