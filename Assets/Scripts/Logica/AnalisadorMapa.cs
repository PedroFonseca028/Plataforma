using System;
using System.Collections.Generic;

namespace RumoAoHexa
{
    /// <summary>
    /// Confere, de forma aproximada, se o jogador consegue chegar a cada ponto
    /// do mapa a partir do início, usando os limites do pulo (sobe até 3 tiles,
    /// pula buracos de até 4 tiles). Serve para os testes pegarem fases
    /// impossíveis (item fora do alcance, buraco largo demais...).
    /// </summary>
    public static class AnalisadorMapa
    {
        /// <summary>O pulo sobe ~3,7 tiles: 3 tiles inteiros com folga.</summary>
        public const int SubidaMaxima = 3;
        /// <summary>Maior distância (em células) entre onde sai e onde cai um pulo.</summary>
        public const int DistanciaMaximaCaindo = 6;

        /// <summary>
        /// Distância horizontal segura (em células) para um pulo que sobe <paramref name="dy"/>:
        /// subindo 2 ou 3 tiles alcança 4; no mesmo nível, 5 (buraco de 4); caindo de 3 ou mais, 6.
        /// </summary>
        public static int AlcanceHorizontal(int dy)
        {
            if (dy >= 2)
                return 4;
            if (dy >= -2)
                return 5;
            return DistanciaMaximaCaindo;
        }

        public static bool Solido(char c)
        {
            return c == Legenda.Chao;
        }

        static bool Perigoso(char c)
        {
            return c == Legenda.Espinho || c == Legenda.LiquidoTopo || c == Legenda.LiquidoFundo;
        }

        /// <summary>Célula que serve de apoio para os pés: chão, plataforma ou plataforma móvel.</summary>
        public static bool Apoio(MapaFase m, int x, int y)
        {
            char c = m.Em(x, y);
            if (c == Legenda.Chao || c == Legenda.Plataforma)
                return true;
            return NaRotaDePlataformaMovel(m, x, y);
        }

        /// <summary>Células por onde passa uma plataforma móvel ('&lt;' ... 'M' ... '&gt;').</summary>
        static bool NaRotaDePlataformaMovel(MapaFase m, int x, int y)
        {
            char c = m.Em(x, y);
            if (c == Legenda.PlataformaMovel || c == Legenda.LimiteEsquerdo || c == Legenda.LimiteDireito)
                return true;
            // procura um '<' à esquerda e um '>' à direita com um 'M' no meio
            int e = -1;
            for (int i = x; i >= Math.Max(0, x - 30); i--)
            {
                char ci = m.Em(i, y);
                if (ci == Legenda.LimiteEsquerdo) { e = i; break; }
                if (ci == Legenda.Chao || ci == Legenda.Plataforma) return false;
            }
            if (e < 0)
                return false;
            bool temM = false;
            for (int i = e; i < Math.Min(m.Largura, e + 31); i++)
            {
                char ci = m.Em(i, y);
                if (ci == Legenda.PlataformaMovel) temM = true;
                if (ci == Legenda.LimiteDireito) return temM && i >= x;
            }
            return false;
        }

        static bool Livre(MapaFase m, int x, int y)
        {
            if (x < 0 || x >= m.Largura)
                return false; // paredes invisíveis nas laterais
            return !Solido(m.Em(x, y));
        }

        /// <summary>O jogador (2 tiles de altura) cabe em (x, y) com os pés apoiados em (x, y - 1).</summary>
        public static bool PodeFicar(MapaFase m, int x, int y)
        {
            if (!m.Dentro(x, y))
                return false;
            if (!Livre(m, x, y) || !Livre(m, x, y + 1))
                return false;
            if (Perigoso(m.Em(x, y)) || NaRotaDePlataformaMovel(m, x, y))
                return false;
            if (m.Em(x, y - 1) == Legenda.Espinho)
                return false;
            return Apoio(m, x, y - 1);
        }

        /// <summary>Dá para ir de A até B andando ou pulando (com espaço livre no caminho)?</summary>
        public static bool Salto(MapaFase m, int xa, int ya, int xb, int yb)
        {
            int dx = Math.Abs(xb - xa);
            int dy = yb - ya;
            if (dy > SubidaMaxima)
                return false;
            if (dx > AlcanceHorizontal(dy))
                return false;

            int topo = Math.Max(ya, yb) + 1; // a cabeça passa pelo menos nesta altura
            if (dy > 0)
                topo = yb + 1;
            // sobe na coluna de saída
            for (int y = ya; y <= topo; y++)
                if (!Livre(m, xa, y))
                    return false;
            // desce na coluna de chegada
            for (int y = yb; y <= topo; y++)
                if (!Livre(m, xb, y))
                    return false;
            // atravessa por cima
            int passo = xb > xa ? 1 : -1;
            for (int x = xa + passo; x != xb; x += passo)
            {
                if (!Livre(m, x, topo) || !Livre(m, x, topo - 1))
                    return false;
            }
            return true;
        }

        static long Chave(int x, int y)
        {
            return ((long)x << 20) | (uint)y;
        }

        /// <summary>Todas as posições em pé que o jogador alcança a partir do 'P'.</summary>
        public static HashSet<long> PosicoesAlcancaveis(MapaFase m)
        {
            var visitadas = new HashSet<long>();
            List<Celula> inicios = m.Encontrar(Legenda.Inicio);
            if (inicios.Count == 0)
                return visitadas;
            var fila = new Queue<Celula>();
            Celula ini = inicios[0];
            visitadas.Add(Chave(ini.X, ini.Y));
            fila.Enqueue(ini);
            while (fila.Count > 0)
            {
                Celula a = fila.Dequeue();
                for (int x = a.X - DistanciaMaximaCaindo; x <= a.X + DistanciaMaximaCaindo; x++)
                {
                    for (int y = 0; y <= a.Y + SubidaMaxima; y++)
                    {
                        if (x == a.X && y == a.Y)
                            continue;
                        long k = Chave(x, y);
                        if (visitadas.Contains(k) || !PodeFicar(m, x, y) || !Salto(m, a.X, a.Y, x, y))
                            continue;
                        visitadas.Add(k);
                        fila.Enqueue(new Celula(x, y, m.Em(x, y)));
                    }
                }
            }
            return visitadas;
        }

        /// <summary>
        /// O jogador encosta na célula (x, y) pulando de algum lugar alcançável? No alto do
        /// pulo o corpo chega 4 tiles acima dos pés, cerca de 3 tiles à frente de onde saiu.
        /// </summary>
        public static bool AlcancaCelula(MapaFase m, HashSet<long> alcancaveis, int x, int y)
        {
            for (int sx = x - 5; sx <= x + 5; sx++)
            {
                for (int sy = y - 4; sy <= y + 1; sy++)
                {
                    if (!alcancaveis.Contains(Chave(sx, sy)))
                        continue;
                    int dx = Math.Abs(x - sx);
                    int dy = y - sy;
                    bool perto = (dx <= 3 && dy <= 4) || (dx <= 4 && dy <= 3) || (dx <= 5 && dy <= 2);
                    if (perto && dy >= -1 && CaminhoLivre(m, sx, sy, x, y))
                        return true;
                }
            }
            return false;
        }

        /// <summary>Sobe na coluna de onde pula e vai na horizontal até a célula, sem atravessar chão.</summary>
        static bool CaminhoLivre(MapaFase m, int sx, int sy, int x, int y)
        {
            for (int yy = sy; yy <= Math.Max(sy, y); yy++)
                if (Solido(m.Em(sx, yy)))
                    return false;
            int passo = x >= sx ? 1 : -1;
            for (int xx = sx; xx != x + passo; xx += passo)
                if (Solido(m.Em(xx, y)))
                    return false;
            return true;
        }

        /// <summary>Células do tipo pedido que o jogador não consegue alcançar.</summary>
        public static List<Celula> Inalcancaveis(MapaFase m, char tipo)
        {
            HashSet<long> alc = PosicoesAlcancaveis(m);
            var r = new List<Celula>();
            foreach (Celula c in m.Encontrar(tipo))
                if (!AlcancaCelula(m, alc, c.X, c.Y))
                    r.Add(c);
            return r;
        }

        /// <summary>Inimigos de chão precisam estar em cima de chão ou plataforma.</summary>
        public static List<Celula> InimigosFlutuando(MapaFase m)
        {
            var r = new List<Celula>();
            for (int y = 0; y < m.Altura; y++)
                for (int x = 0; x < m.Largura; x++)
                {
                    char c = m.Em(x, y);
                    if (Legenda.EhInimigo(c) && !Apoio(m, x, y - 1))
                        r.Add(new Celula(x, y, c));
                }
            return r;
        }
    }
}
