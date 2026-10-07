using System.Collections.Generic;

namespace RumoAoHexa
{
    /// <summary>
    /// Desenho das letras da fonte pixelada (5x7, só maiúsculas, com os acentos
    /// do português). A <c>FontePixel</c> monta a textura a partir daqui; os
    /// testes usam esta classe para garantir que todo texto do jogo tem letra.
    /// </summary>
    public static class GlifosFonte
    {
        public const int AlturaGlifo = 10;   // 2 linhas de acento + 7 da letra + 1 abaixo
        public const int LinhaBase = 2;      // onde a letra começa (abaixo dos acentos)

        public static readonly Dictionary<char, string[]> Base = new Dictionary<char, string[]>
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
            { '←', new[] { ".....", "..#..", ".#...", "#####", ".#...", "..#..", "....." } },
            { '→', new[] { ".....", "..#..", "...#.", "#####", "...#.", "..#..", "....." } },
            { '↑', new[] { "..#..", ".###.", "#.#.#", "..#..", "..#..", "..#..", "..#.." } },
            { '↓', new[] { "..#..", "..#..", "..#..", "..#..", "#.#.#", ".###.", "..#.." } },
            { '♥', new[] { ".....", ".#.#.", "#####", "#####", ".###.", "..#..", "....." } },
            { '★', new[] { "..#..", "..#..", "#####", ".###.", ".###.", "##.##", "#...#" } },
        };

        /// <summary>Letras acentuadas: letra base + 2 linhas desenhadas acima.</summary>
        public static readonly Dictionary<char, KeyValuePair<char, string[]>> Acentuadas =
            new Dictionary<char, KeyValuePair<char, string[]>>
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
                { 'Ñ', Acento('N', ".##.#", "#..#.") },
            };

        static KeyValuePair<char, string[]> Acento(char letra, string linha0, string linha1)
        {
            return new KeyValuePair<char, string[]>(letra, new[] { linha0, linha1 });
        }

        /// <summary>
        /// Todos os glifos já com as 10 linhas (acento + letra + cedilha), na
        /// ordem em que entram no atlas.
        /// </summary>
        public static List<KeyValuePair<char, string[]>> Todos()
        {
            var todos = new List<KeyValuePair<char, string[]>>();
            foreach (KeyValuePair<char, string[]> par in Base)
                todos.Add(new KeyValuePair<char, string[]>(par.Key, Completo(par.Value, null, null)));
            foreach (KeyValuePair<char, KeyValuePair<char, string[]>> par in Acentuadas)
                todos.Add(new KeyValuePair<char, string[]>(par.Key, Completo(Base[par.Value.Key], par.Value.Value, null)));
            todos.Add(new KeyValuePair<char, string[]>('Ç', Completo(Base['C'], null, "..#..")));
            return todos;
        }

        static string[] Completo(string[] letra, string[] acento, string abaixo)
        {
            int largura = 0;
            foreach (string l in letra)
                largura = System.Math.Max(largura, l.Length);
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

        static HashSet<char> existentes;

        static bool Existe(char c)
        {
            if (existentes == null)
            {
                var h = new HashSet<char>();
                foreach (KeyValuePair<char, string[]> par in Todos())
                    h.Add(par.Key);
                existentes = h;
            }
            return existentes.Contains(c);
        }

        /// <summary>Converte o caractere para um que exista na fonte ('?' se não houver).</summary>
        public static char Normalizar(char c)
        {
            char m = char.ToUpperInvariant(c);
            if (Existe(m) || m == ' ' || m == '\n')
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
                case 'Î': return 'I';
                case 'Û': return 'U';
            }
            return '?';
        }

        /// <summary>O texto pode ser escrito sem nenhum '?' de substituição?</summary>
        public static bool TextoSuportado(string texto, out char faltando)
        {
            faltando = '\0';
            foreach (char c in texto ?? "")
            {
                if (c == '?')
                    continue;
                if (Normalizar(c) == '?')
                {
                    faltando = c;
                    return false;
                }
            }
            return true;
        }
    }
}
