using System.Collections.Generic;
using UnityEngine;

namespace Curupira
{
    /// <summary>
    /// Fonte "pixel art" desenhada à mão (5x7 pixels, só maiúsculas) com os
    /// acentos do português. O atlas é montado em tempo de execução, então o
    /// jogo não depende de nenhum arquivo de fonte.
    /// </summary>
    public static class FontePixel
    {
        public const int AlturaGlifo = 10;   // 2 linhas de acento + 7 da letra + 1 abaixo
        public const int LinhaBase = 2;      // onde a letra começa (abaixo dos acentos)
        const int CelulaL = 7;
        const int CelulaA = 12;
        const int TamanhoAtlas = 128;

        public struct Glifo
        {
            public int largura;
            public Rect uv;          // em coordenadas 0..1 do atlas
            public Rect pixels;      // em pixels do atlas
        }

        static Texture2D atlas;
        static Dictionary<char, Glifo> glifos;
        static Dictionary<char, Sprite> spritesGlifo;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            atlas = null;
            glifos = null;
            spritesGlifo = null;
        }

        // ------------------------------------------------------------ desenho das letras (7 linhas cada)
        static readonly Dictionary<char, string[]> Base = new Dictionary<char, string[]>
        {
            { 'A', new[] { ".###.", "#...#", "#...#", "#####", "#...#", "#...#", "#...#" } },
            { 'B', new[] { "####.", "#...#", "#...#", "####.", "#...#", "#...#", "####." } },
            { 'C', new[] { ".###.", "#...#", "#....", "#....", "#....", "#...#", ".###." } },
            { 'D', new[] { "####.", "#...#", "#...#", "#...#", "#...#", "#...#", "####." } },
            { 'E', new[] { "#####", "#....", "#....", "####.", "#....", "#....", "#####" } },
            { 'F', new[] { "#####", "#....", "#....", "####.", "#....", "#....", "#...." } },
            { 'G', new[] { ".###.", "#...#", "#....", "#.###", "#...#", "#...#", ".####" } },
            { 'H', new[] { "#...#", "#...#", "#...#", "#####", "#...#", "#...#", "#...#" } },
            { 'I', new[] { "###", ".#.", ".#.", ".#.", ".#.", ".#.", "###" } },
            { 'J', new[] { "..###", "...#.", "...#.", "...#.", "#..#.", "#..#.", ".##.." } },
            { 'K', new[] { "#...#", "#..#.", "#.#..", "##...", "#.#..", "#..#.", "#...#" } },
            { 'L', new[] { "#....", "#....", "#....", "#....", "#....", "#....", "#####" } },
            { 'M', new[] { "#...#", "##.##", "#.#.#", "#.#.#", "#...#", "#...#", "#...#" } },
            { 'N', new[] { "#...#", "##..#", "#.#.#", "#..##", "#...#", "#...#", "#...#" } },
            { 'O', new[] { ".###.", "#...#", "#...#", "#...#", "#...#", "#...#", ".###." } },
            { 'P', new[] { "####.", "#...#", "#...#", "####.", "#....", "#....", "#...." } },
            { 'Q', new[] { ".###.", "#...#", "#...#", "#...#", "#.#.#", "#..#.", ".##.#" } },
            { 'R', new[] { "####.", "#...#", "#...#", "####.", "#.#..", "#..#.", "#...#" } },
            { 'S', new[] { ".####", "#....", "#....", ".###.", "....#", "....#", "####." } },
            { 'T', new[] { "#####", "..#..", "..#..", "..#..", "..#..", "..#..", "..#.." } },
            { 'U', new[] { "#...#", "#...#", "#...#", "#...#", "#...#", "#...#", ".###." } },
            { 'V', new[] { "#...#", "#...#", "#...#", "#...#", "#...#", ".#.#.", "..#.." } },
            { 'W', new[] { "#...#", "#...#", "#...#", "#.#.#", "#.#.#", "#.#.#", ".#.#." } },
            { 'X', new[] { "#...#", "#...#", ".#.#.", "..#..", ".#.#.", "#...#", "#...#" } },
            { 'Y', new[] { "#...#", "#...#", ".#.#.", "..#..", "..#..", "..#..", "..#.." } },
            { 'Z', new[] { "#####", "....#", "...#.", "..#..", ".#...", "#....", "#####" } },
            { '0', new[] { ".###.", "#...#", "#..##", "#.#.#", "##..#", "#...#", ".###." } },
            { '1', new[] { ".#.", "##.", ".#.", ".#.", ".#.", ".#.", "###" } },
            { '2', new[] { ".###.", "#...#", "....#", "...#.", "..#..", ".#...", "#####" } },
            { '3', new[] { "####.", "....#", "....#", ".###.", "....#", "....#", "####." } },
            { '4', new[] { "...#.", "..##.", ".#.#.", "#..#.", "#####", "...#.", "...#." } },
            { '5', new[] { "#####", "#....", "####.", "....#", "....#", "#...#", ".###." } },
            { '6', new[] { ".###.", "#....", "#....", "####.", "#...#", "#...#", ".###." } },
            { '7', new[] { "#####", "....#", "...#.", "..#..", ".#...", ".#...", ".#..." } },
            { '8', new[] { ".###.", "#...#", "#...#", ".###.", "#...#", "#...#", ".###." } },
            { '9', new[] { ".###.", "#...#", "#...#", ".####", "....#", "....#", ".###." } },
            { '.', new[] { ".", ".", ".", ".", ".", ".", "#" } },
            { ',', new[] { ".", ".", ".", ".", ".", "#", "#" } },
            { ':', new[] { ".", ".", "#", ".", ".", "#", "." } },
            { ';', new[] { ".", ".", "#", ".", ".", "#", "#" } },
            { '!', new[] { "#", "#", "#", "#", "#", ".", "#" } },
            { '?', new[] { ".###.", "#...#", "....#", "...#.", "..#..", ".....", "..#.." } },
            { '-', new[] { "....", "....", "....", "####", "....", "....", "...." } },
            { '+', new[] { ".....", "..#..", "..#..", "#####", "..#..", "..#..", "....." } },
            { '/', new[] { "....#", "....#", "...#.", "..#..", ".#...", "#....", "#...." } },
            { '(', new[] { ".#", "#.", "#.", "#.", "#.", "#.", ".#" } },
            { ')', new[] { "#.", ".#", ".#", ".#", ".#", ".#", "#." } },
            { '[', new[] { "##", "#.", "#.", "#.", "#.", "#.", "##" } },
            { ']', new[] { "##", ".#", ".#", ".#", ".#", ".#", "##" } },
            { '\'', new[] { "#", "#", ".", ".", ".", ".", "." } },
            { '"', new[] { "#.#", "#.#", "...", "...", "...", "...", "..." } },
            { '%', new[] { "##..#", "##..#", "...#.", "..#..", ".#...", "#..##", "#..##" } },
            { '*', new[] { ".....", "#.#.#", ".###.", "#####", ".###.", "#.#.#", "....." } },
            { '=', new[] { ".....", ".....", "#####", ".....", "#####", ".....", "....." } },
            { '<', new[] { "...#", "..#.", ".#..", "#...", ".#..", "..#.", "...#" } },
            { '>', new[] { "#...", ".#..", "..#.", "...#", "..#.", ".#..", "#..." } },
            { '_', new[] { ".....", ".....", ".....", ".....", ".....", ".....", "#####" } },
            { '#', new[] { ".#.#.", ".#.#.", "#####", ".#.#.", "#####", ".#.#.", ".#.#." } },
            { '←', new[] { ".....", "..#..", ".#...", "#####", ".#...", "..#..", "....." } }, // ←
            { '→', new[] { ".....", "..#..", "...#.", "#####", "...#.", "..#..", "....." } }, // →
            { '↑', new[] { "..#..", ".###.", "#.#.#", "..#..", "..#..", "..#..", "..#.." } }, // ↑
            { '↓', new[] { "..#..", "..#..", "..#..", "..#..", "#.#.#", ".###.", "..#.." } }, // ↓
            { '♥', new[] { ".....", ".#.#.", "#####", "#####", ".###.", "..#..", "....." } }, // ♥
        };

        // acentos: letra base + 2 linhas desenhadas acima
        static readonly Dictionary<char, KeyValuePair<char, string[]>> Acentuadas = new Dictionary<char, KeyValuePair<char, string[]>>
        {
            { 'Á', Acento('A', "...#.", "..#..") },
            { 'À', Acento('A', ".#...", "..#..") },
            { 'Â', Acento('A', "..#..", ".#.#.") },
            { 'Ã', Acento('A', ".##.#", "#..#.") },
            { 'É', Acento('E', "...#.", "..#..") },
            { 'Ê', Acento('E', "..#..", ".#.#.") },
            { 'Í', Acento('I', "..#", ".#.") },
            { 'Ó', Acento('O', "...#.", "..#..") },
            { 'Ô', Acento('O', "..#..", ".#.#.") },
            { 'Õ', Acento('O', ".##.#", "#..#.") },
            { 'Ú', Acento('U', "...#.", "..#..") },
            { 'Ü', Acento('U', ".....", ".#.#.") },
        };

        static KeyValuePair<char, string[]> Acento(char letra, string linha0, string linha1)
        {
            return new KeyValuePair<char, string[]>(letra, new[] { linha0, linha1 });
        }

        public static Texture2D Atlas
        {
            get
            {
                Montar();
                return atlas;
            }
        }

        static void Montar()
        {
            if (atlas != null && glifos != null)
                return;

            atlas = new Texture2D(TamanhoAtlas, TamanhoAtlas, TextureFormat.RGBA32, false);
            atlas.name = "FontePixel (atlas)";
            atlas.filterMode = FilterMode.Point;
            atlas.wrapMode = TextureWrapMode.Clamp;
            var cores = new Color32[TamanhoAtlas * TamanhoAtlas];
            for (int i = 0; i < cores.Length; i++)
                cores[i] = new Color32(255, 255, 255, 0);

            glifos = new Dictionary<char, Glifo>();
            int indice = 0;

            // lista de todos os caracteres: base + acentuados + Ç
            var todos = new List<KeyValuePair<char, string[]>>();
            foreach (KeyValuePair<char, string[]> par in Base)
                todos.Add(new KeyValuePair<char, string[]>(par.Key, Completo(par.Value, null, null)));
            foreach (KeyValuePair<char, KeyValuePair<char, string[]>> par in Acentuadas)
                todos.Add(new KeyValuePair<char, string[]>(par.Key, Completo(Base[par.Value.Key], par.Value.Value, null)));
            todos.Add(new KeyValuePair<char, string[]>('Ç', Completo(Base['C'], null, "..#..")));

            int colunas = TamanhoAtlas / CelulaL;
            foreach (KeyValuePair<char, string[]> par in todos)
            {
                string[] linhas = par.Value;
                int largura = 0;
                foreach (string l in linhas)
                    largura = Mathf.Max(largura, l.Length);

                int cx = (indice % colunas) * CelulaL + 1;
                int cyTopo = TamanhoAtlas - 1 - (indice / colunas) * CelulaA - 1;
                for (int r = 0; r < AlturaGlifo; r++)
                {
                    string l = linhas[r];
                    for (int c = 0; c < l.Length; c++)
                    {
                        if (l[c] != '#')
                            continue;
                        int px = cx + c;
                        int py = cyTopo - r;
                        cores[py * TamanhoAtlas + px] = new Color32(255, 255, 255, 255);
                    }
                }

                var g = new Glifo();
                g.largura = largura;
                g.pixels = new Rect(cx, cyTopo - AlturaGlifo + 1, largura, AlturaGlifo);
                g.uv = new Rect(g.pixels.x / TamanhoAtlas, g.pixels.y / TamanhoAtlas,
                    g.pixels.width / TamanhoAtlas, g.pixels.height / TamanhoAtlas);
                glifos[par.Key] = g;
                indice++;
            }

            atlas.SetPixels32(cores);
            atlas.Apply(false, false);
        }

        /// <summary>Monta as 10 linhas do glifo: 2 de acento, 7 da letra e 1 abaixo.</summary>
        static string[] Completo(string[] letra, string[] acento, string abaixo)
        {
            int largura = 0;
            foreach (string l in letra)
                largura = Mathf.Max(largura, l.Length);
            string vazio = new string('.', largura);
            var r = new string[AlturaGlifo];
            r[0] = acento != null ? Ajustar(acento[0], largura) : vazio;
            r[1] = acento != null ? Ajustar(acento[1], largura) : vazio;
            for (int i = 0; i < 7; i++)
                r[LinhaBase + i] = Ajustar(letra[i], largura);
            r[9] = abaixo != null ? Ajustar(abaixo, largura) : vazio;
            return r;
        }

        static string Ajustar(string s, int largura)
        {
            if (s.Length == largura)
                return s;
            if (s.Length > largura)
                return s.Substring(0, largura);
            return s + new string('.', largura - s.Length);
        }

        /// <summary>Converte o caractere para um que exista na fonte.</summary>
        public static char Normalizar(char c)
        {
            char m = char.ToUpperInvariant(c);
            Montar();
            if (glifos.ContainsKey(m) || m == ' ' || m == '\n')
                return m;
            switch (m)
            {
                case 'È': return 'É';
                case 'Ì': return 'Í';
                case 'Ò': return 'Ó';
                case 'Ù': return 'Ú';
                case 'Ä': return 'A';
                case 'Ë': return 'E';
                case 'Ï': return 'I';
                case 'Ö': return 'O';
                case 'Ñ': return 'N';
                case 'Î': return 'I';
                case 'Û': return 'U';
            }
            return '?';
        }

        public static bool TentarGlifo(char c, out Glifo g)
        {
            Montar();
            return glifos.TryGetValue(c, out g);
        }

        /// <summary>Largura (em pixels da fonte) de uma linha de texto.</summary>
        public static int LarguraLinha(string linha)
        {
            Montar();
            int total = 0;
            for (int i = 0; i < linha.Length; i++)
            {
                char c = Normalizar(linha[i]);
                Glifo g;
                if (c == ' ')
                    total += 3;
                else if (glifos.TryGetValue(c, out g))
                    total += g.largura;
                if (i < linha.Length - 1)
                    total += 1;
            }
            return total;
        }

        /// <summary>Sprite de um caractere (usado nos textos que flutuam no cenário).</summary>
        public static Sprite SpriteDoGlifo(char c)
        {
            Montar();
            if (spritesGlifo == null)
                spritesGlifo = new Dictionary<char, Sprite>();
            c = Normalizar(c);
            Sprite s;
            if (spritesGlifo.TryGetValue(c, out s) && s != null)
                return s;
            Glifo g;
            if (!glifos.TryGetValue(c, out g))
                return null;
            s = Sprite.Create(atlas, g.pixels, new Vector2(0f, 0f), Arte.PixelsPorUnidade, 0, SpriteMeshType.FullRect);
            s.name = "glifo_" + c;
            spritesGlifo[c] = s;
            return s;
        }
    }
}
