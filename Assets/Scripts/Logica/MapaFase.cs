using System;
using System.Collections.Generic;

namespace RumoAoHexa
{
    /// <summary>Caracteres usados nos mapas em texto (Resources/Fases/*.txt).</summary>
    public static class Legenda
    {
        public const char Vazio = '.';
        public const char Chao = '#';
        public const char Plataforma = '=';
        public const char Espinho = '^';
        public const char LiquidoTopo = '~';
        public const char LiquidoFundo = '%';
        public const char Inicio = 'P';
        public const char Bandeirinha = 'K';
        public const char ItemObrigatorio = '*';
        public const char Moeda = 'o';
        public const char Coracao = 'h';
        public const char Zagueiro = 'z';
        public const char Lateral = 'r';
        public const char Cabeceador = 'c';
        public const char PlataformaMovel = 'M';
        public const char LimiteEsquerdo = '<';
        public const char LimiteDireito = '>';
        /// <summary>Decorações grandes (coqueiro, trave, guarda-sol...).</summary>
        public const string Decoracoes = "TYBba";

        public static bool EhInimigo(char c)
        {
            return c == Zagueiro || c == Lateral || c == Cabeceador;
        }

        public static bool EhConhecido(char c)
        {
            return ".#=^~%PK*ohzrcM<>".IndexOf(c) >= 0 || Decoracoes.IndexOf(c) >= 0;
        }
    }

    /// <summary>Uma célula do mapa (x cresce para a direita, y para cima; y = 0 é a linha de baixo).</summary>
    public struct Celula
    {
        public int X;
        public int Y;
        public char C;

        public Celula(int x, int y, char c)
        {
            X = x;
            Y = y;
            C = c;
        }

        public override string ToString()
        {
            return "'" + C + "' em (" + X + ", " + Y + ")";
        }
    }

    /// <summary>
    /// Mapa de uma fase lido de um texto. A primeira linha do arquivo é o topo
    /// da fase; linhas começando com // são comentários (a legenda).
    /// </summary>
    public class MapaFase
    {
        readonly char[,] celulas;

        public int Largura { get; private set; }
        public int Altura { get; private set; }

        MapaFase(char[,] c, int largura, int altura)
        {
            celulas = c;
            Largura = largura;
            Altura = altura;
        }

        public static MapaFase Ler(string texto)
        {
            var linhas = new List<string>();
            foreach (string bruta in (texto ?? "").Split('\n'))
            {
                string l = bruta.TrimEnd('\r');
                if (l.StartsWith("//"))
                    continue;
                linhas.Add(l);
            }
            while (linhas.Count > 0 && linhas[linhas.Count - 1].Trim().Length == 0)
                linhas.RemoveAt(linhas.Count - 1);
            while (linhas.Count > 0 && linhas[0].Trim().Length == 0)
                linhas.RemoveAt(0);

            int altura = linhas.Count;
            int largura = 0;
            foreach (string l in linhas)
                largura = Math.Max(largura, l.Length);
            var c = new char[largura, altura];
            for (int linha = 0; linha < altura; linha++)
            {
                int y = altura - 1 - linha;
                for (int x = 0; x < largura; x++)
                    c[x, y] = x < linhas[linha].Length ? linhas[linha][x] : Legenda.Vazio;
            }
            return new MapaFase(c, largura, altura);
        }

        public bool Dentro(int x, int y)
        {
            return x >= 0 && x < Largura && y >= 0 && y < Altura;
        }

        /// <summary>Caractere da célula (fora do mapa = vazio).</summary>
        public char Em(int x, int y)
        {
            return Dentro(x, y) ? celulas[x, y] : Legenda.Vazio;
        }

        /// <summary>Chão para o auto-tiling: abaixo do mapa e nas laterais o chão continua.</summary>
        public bool EhChao(int x, int y)
        {
            if (y < 0)
                return true;
            if (y >= Altura)
                return false;
            if (x < 0 || x >= Largura)
                return true;
            return celulas[x, y] == Legenda.Chao;
        }

        /// <summary>Vizinhos com chão: 1 = cima, 2 = direita, 4 = baixo, 8 = esquerda.</summary>
        public int Mascara(int x, int y)
        {
            int m = 0;
            if (EhChao(x, y + 1)) m |= 1;
            if (EhChao(x + 1, y)) m |= 2;
            if (EhChao(x, y - 1)) m |= 4;
            if (EhChao(x - 1, y)) m |= 8;
            return m;
        }

        /// <summary>Pedaço da plataforma: e (ponta esquerda), m (meio), d (ponta direita) ou u (única).</summary>
        public string TipoPlataforma(int x, int y)
        {
            bool esq = Em(x - 1, y) == Legenda.Plataforma;
            bool dir = Em(x + 1, y) == Legenda.Plataforma;
            if (esq && dir) return "m";
            if (dir) return "e";
            if (esq) return "d";
            return "u";
        }

        /// <summary>Número "aleatório" fixo por posição (para variar tiles e decoração).</summary>
        public static int Hash(int x, int y)
        {
            unchecked
            {
                int h = x * 73856093 ^ y * 19349663;
                return (h ^ (h >> 13)) & 0x7fffffff;
            }
        }

        public List<Celula> Encontrar(char c)
        {
            var r = new List<Celula>();
            for (int y = 0; y < Altura; y++)
                for (int x = 0; x < Largura; x++)
                    if (celulas[x, y] == c)
                        r.Add(new Celula(x, y, c));
            return r;
        }

        public int Contar(char c)
        {
            return Encontrar(c).Count;
        }

        /// <summary>Itens obrigatórios em ordem da esquerda para a direita (define ano/letra de cada um).</summary>
        public List<Celula> ItensObrigatoriosEmOrdem()
        {
            List<Celula> itens = Encontrar(Legenda.ItemObrigatorio);
            itens.Sort((a, b) => a.X != b.X ? a.X.CompareTo(b.X) : b.Y.CompareTo(a.Y));
            return itens;
        }

        /// <summary>
        /// Limites da plataforma móvel que está em (x, y): procura '&lt;' à esquerda e
        /// '&gt;' à direita na mesma linha (até 30 células).
        /// </summary>
        public void LimitesPlataformaMovel(int x, int y, out int esquerda, out int direita)
        {
            esquerda = x;
            direita = x;
            for (int i = x - 1; i >= Math.Max(0, x - 30); i--)
                if (Em(i, y) == Legenda.LimiteEsquerdo) { esquerda = i; break; }
            for (int i = x + 1; i < Math.Min(Largura, x + 30); i++)
                if (Em(i, y) == Legenda.LimiteDireito) { direita = i; break; }
        }

        /// <summary>
        /// Confere se o mapa é jogável: um início, a quantidade certa de itens
        /// obrigatórios, só caracteres conhecidos e plataformas móveis com limites.
        /// Devolve a lista de problemas (vazia = tudo certo).
        /// </summary>
        public List<string> Validar(int itensEsperados)
        {
            var problemas = new List<string>();
            if (Largura == 0 || Altura == 0)
            {
                problemas.Add("mapa vazio");
                return problemas;
            }
            int inicios = Contar(Legenda.Inicio);
            if (inicios != 1)
                problemas.Add("o mapa precisa de exatamente 1 'P' (tem " + inicios + ")");
            int itens = Contar(Legenda.ItemObrigatorio);
            if (itens != itensEsperados)
                problemas.Add("esperava " + itensEsperados + " itens obrigatórios '*', mas tem " + itens);
            for (int y = 0; y < Altura; y++)
                for (int x = 0; x < Largura; x++)
                    if (!Legenda.EhConhecido(celulas[x, y]))
                        problemas.Add("caractere desconhecido '" + celulas[x, y] + "' em (" + x + ", " + y + ")");
            foreach (Celula m in Encontrar(Legenda.PlataformaMovel))
            {
                int e, d;
                LimitesPlataformaMovel(m.X, m.Y, out e, out d);
                if (e == m.X || d == m.X)
                    problemas.Add("plataforma móvel em (" + m.X + ", " + m.Y + ") sem '<' e '>' na mesma linha");
            }
            foreach (Celula p in Encontrar(Legenda.Inicio))
                if (!AnalisadorMapa.PodeFicar(this, p.X, p.Y))
                    problemas.Add("o início 'P' não está em cima de chão firme");
            return problemas;
        }
    }
}
