using UnityEngine;

namespace Curupira
{
    /// <summary>
    /// Camadas de física usadas pelo jogo (os nomes aparecem em
    /// Project Settings > Tags and Layers).
    /// </summary>
    public static class Camadas
    {
        public const int Chao = 6;
        public const int Jogador = 7;
        public const int Inimigo = 8;
        public const int Item = 9;
        public const int Perigo = 10;
        public const int Plataforma = 11;

        /// <summary>Tudo em que o jogador consegue pisar (chão, plataformas e espinhos).</summary>
        public const int MascaraPisavel = (1 << Chao) | (1 << Plataforma) | (1 << Perigo);

        /// <summary>Somente chão/paredes sólidos (usado pelos inimigos para achar paredes).</summary>
        public const int MascaraSolida = 1 << Chao;

        /// <summary>Chão onde os inimigos andam (chão e plataformas).</summary>
        public const int MascaraChaoInimigo = (1 << Chao) | (1 << Plataforma);

        /// <summary>
        /// Define quais camadas interagem. Os inimigos só precisam "enxergar"
        /// o jogador; o resto é ignorado para economizar processamento.
        /// </summary>
        public static void ConfigurarColisoes()
        {
            Physics2D.IgnoreLayerCollision(Inimigo, Inimigo, true);
            Physics2D.IgnoreLayerCollision(Inimigo, Chao, true);
            Physics2D.IgnoreLayerCollision(Inimigo, Plataforma, true);
            Physics2D.IgnoreLayerCollision(Inimigo, Perigo, true);
            Physics2D.IgnoreLayerCollision(Inimigo, Item, true);
            Physics2D.IgnoreLayerCollision(Item, Item, true);
            Physics2D.IgnoreLayerCollision(Jogador, Inimigo, false);
            Physics2D.IgnoreLayerCollision(Jogador, Item, false);
            Physics2D.IgnoreLayerCollision(Jogador, Perigo, false);
            Physics2D.IgnoreLayerCollision(Jogador, Chao, false);
            Physics2D.IgnoreLayerCollision(Jogador, Plataforma, false);
        }
    }

    /// <summary>Ordem de desenho (Sorting Order) de cada elemento.</summary>
    public static class Ordem
    {
        public const int Ceu = -1000;
        public const int FundoDistante = -950;
        public const int Fundo1 = -900;
        public const int Fundo2 = -850;
        public const int Fundo3 = -800;
        public const int ArvoresGrandes = -60;
        public const int Decoracao = -10;
        public const int Pegadas = -5;
        public const int Chao = 0;
        public const int Plataformas = 1;
        public const int Perigos = 2;
        public const int Itens = 10;
        public const int Totem = 8;
        public const int Inimigos = 20;
        public const int Jogador = 30;
        public const int Liquido = 35;
        public const int Efeitos = 40;
        public const int TextoFlutuante = 50;
    }

    /// <summary>Nomes das cenas (precisam estar em File > Build Profiles > Scene List).</summary>
    public static class Cenas
    {
        public const string Menu = "Menu";
        public const string Fase1 = "Fase1";
        public const string Fase2 = "Fase2";
        public const string FimDeJogo = "FimDeJogo";
    }

    /// <summary>Tema visual de uma fase.</summary>
    public enum Tema
    {
        Mata = 0,
        Queimada = 1,
    }
}
