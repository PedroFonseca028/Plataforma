using System.Collections.Generic;
using UnityEngine;

namespace RumoAoHexa
{
    /// <summary>
    /// Fonte "pixel art" desenhada à mão (5x7 pixels, só maiúsculas) com os
    /// acentos do português. O desenho das letras fica em <see cref="GlifosFonte"/>;
    /// aqui o atlas é montado em tempo de execução, então o jogo não depende de
    /// nenhum arquivo de fonte.
    /// </summary>
    public static class FontePixel
    {
        public const int AlturaGlifo = GlifosFonte.AlturaGlifo;
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
            int colunas = TamanhoAtlas / CelulaL;
            foreach (KeyValuePair<char, string[]> par in GlifosFonte.Todos())
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
                        cores[(cyTopo - r) * TamanhoAtlas + cx + c] = new Color32(255, 255, 255, 255);
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

        /// <summary>Converte o caractere para um que exista na fonte.</summary>
        public static char Normalizar(char c)
        {
            return GlifosFonte.Normalizar(c);
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

        /// <summary>Sprite de um caractere (usado nos textos que aparecem no cenário).</summary>
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
