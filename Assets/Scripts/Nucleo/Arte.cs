using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RumoAoHexa
{
    /// <summary>
    /// Biblioteca de sprites do jogo. Lê o arquivo Resources/Sprites/manifest.json
    /// (gerado por Ferramentas/gerar_arte.py) e monta cada sprite a partir das
    /// folhas PNG, com o pivô e os 16 pixels por unidade corretos.
    ///
    /// Nomes terminados em _0, _1, _2... formam uma animação:
    /// Arte.Anim("craque_correr") devolve todos os quadros em ordem.
    /// </summary>
    public static class Arte
    {
#pragma warning disable 0649 // campos preenchidos pelo JsonUtility
        [Serializable]
        class InfoSprite
        {
            public string n;
            public int x;
            public int y;
            public int w;
            public int h;
            public float px;
            public float py;
            public int[] b;
        }

        [Serializable]
        class InfoFolha
        {
            public string folha;
            public InfoSprite[] sprites;
        }

        [Serializable]
        class InfoManifesto
        {
            public int ppu;
            public InfoFolha[] folhas;
        }
#pragma warning restore 0649

        public const float PixelsPorUnidade = 16f;

        static Dictionary<string, Sprite> sprites;
        static Dictionary<string, Sprite[]> animacoes;
        static Dictionary<string, TileBase> tiles;
        static readonly Sprite[] Vazio = new Sprite[0];

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            sprites = null;
            animacoes = null;
            tiles = null;
        }

        static void GarantirCarregado()
        {
            if (sprites != null)
                return;

            sprites = new Dictionary<string, Sprite>();
            animacoes = new Dictionary<string, Sprite[]>();
            tiles = new Dictionary<string, TileBase>();

            TextAsset texto = Resources.Load<TextAsset>("Sprites/manifest");
            if (texto == null)
            {
                Debug.LogError("[RumoAoHexa] Não encontrei Resources/Sprites/manifest.json. Rode Ferramentas/gerar_arte.py.");
                return;
            }

            InfoManifesto manifesto = JsonUtility.FromJson<InfoManifesto>(texto.text);
            float ppu = manifesto.ppu > 0 ? manifesto.ppu : PixelsPorUnidade;
            var grupos = new Dictionary<string, SortedDictionary<int, Sprite>>();

            foreach (InfoFolha folha in manifesto.folhas)
            {
                Texture2D textura = Resources.Load<Texture2D>("Sprites/" + folha.folha);
                if (textura == null)
                {
                    Debug.LogError("[RumoAoHexa] Textura não encontrada: Resources/Sprites/" + folha.folha);
                    continue;
                }

                // Garante o visual "pixelado" mesmo que o .meta seja alterado.
                textura.filterMode = FilterMode.Point;
                textura.wrapMode = TextureWrapMode.Clamp;

                foreach (InfoSprite info in folha.sprites)
                {
                    Vector4 borda = Vector4.zero;
                    if (info.b != null && info.b.Length == 4)
                        borda = new Vector4(info.b[0], info.b[1], info.b[2], info.b[3]);

                    Sprite s = UnityEngine.Sprite.Create(textura, new Rect(info.x, info.y, info.w, info.h),
                        new Vector2(info.px, info.py), ppu, 0, SpriteMeshType.FullRect, borda, false);
                    s.name = info.n;
                    sprites[info.n] = s;

                    // agrupa quadros de animação: "nome_3" -> grupo "nome", índice 3
                    int sep = info.n.LastIndexOf('_');
                    int indice;
                    if (sep > 0 && int.TryParse(info.n.Substring(sep + 1), out indice))
                    {
                        string grupo = info.n.Substring(0, sep);
                        SortedDictionary<int, Sprite> lista;
                        if (!grupos.TryGetValue(grupo, out lista))
                        {
                            lista = new SortedDictionary<int, Sprite>();
                            grupos[grupo] = lista;
                        }
                        lista[indice] = s;
                    }
                }
            }

            foreach (KeyValuePair<string, SortedDictionary<int, Sprite>> par in grupos)
            {
                var quadros = new Sprite[par.Value.Count];
                par.Value.Values.CopyTo(quadros, 0);
                animacoes[par.Key] = quadros;
            }
        }

        /// <summary>Sprite pelo nome (ou null se não existir).</summary>
        public static Sprite Sprite(string nome)
        {
            GarantirCarregado();
            Sprite s;
            if (sprites.TryGetValue(nome, out s))
                return s;
            Debug.LogWarning("[RumoAoHexa] Sprite não encontrado: " + nome);
            return null;
        }

        public static bool Existe(string nome)
        {
            GarantirCarregado();
            return sprites.ContainsKey(nome);
        }

        /// <summary>Quadros de uma animação ("craque_correr" -> craque_correr_0..5).</summary>
        public static Sprite[] Anim(string prefixo)
        {
            GarantirCarregado();
            Sprite[] quadros;
            if (animacoes.TryGetValue(prefixo, out quadros))
                return quadros;
            Sprite unico;
            if (sprites.TryGetValue(prefixo, out unico))
                return new[] { unico };
            Debug.LogWarning("[RumoAoHexa] Animação não encontrada: " + prefixo);
            return Vazio;
        }

        /// <summary>Tile simples (com colisão de grade) criado a partir de um sprite.</summary>
        public static TileBase Tile(string nomeSprite, bool comColisao)
        {
            GarantirCarregado();
            string chave = nomeSprite + (comColisao ? "#c" : "#s");
            TileBase existente;
            if (tiles.TryGetValue(chave, out existente) && existente != null)
                return existente;

            Sprite s = Sprite(nomeSprite);
            if (s == null)
                return null;

            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = nomeSprite;
            tile.sprite = s;
            tile.colliderType = comColisao ? UnityEngine.Tilemaps.Tile.ColliderType.Grid : UnityEngine.Tilemaps.Tile.ColliderType.None;
            tiles[chave] = tile;
            return tile;
        }

        /// <summary>Tile animado (água, brasas...) com os quadros de uma animação.</summary>
        public static TileBase TileAnimado(string prefixo, float quadrosPorSegundo)
        {
            GarantirCarregado();
            string chave = prefixo + "#anim";
            TileBase existente;
            if (tiles.TryGetValue(chave, out existente) && existente != null)
                return existente;

            Sprite[] quadros = Anim(prefixo);
            if (quadros.Length == 0)
                return null;

            TileAnimado tile = ScriptableObject.CreateInstance<TileAnimado>();
            tile.name = prefixo;
            tile.quadros = quadros;
            tile.velocidade = quadrosPorSegundo;
            tiles[chave] = tile;
            return tile;
        }
    }

    /// <summary>
    /// Guarda os materiais do URP 2D usados nos sprites criados por código.
    /// "Aceso" recebe as luzes 2D; "Brilho" ignora as luzes (parece brilhar no escuro).
    /// </summary>
    public static class Materiais2D
    {
        public static Material Aceso;
        public static Material Brilho;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            Aceso = null;
            Brilho = null;
        }

        public static void Definir(Material aceso, Material brilho)
        {
            if (aceso != null) Aceso = aceso;
            if (brilho != null) Brilho = brilho;
        }

        public static void Aplicar(Renderer r, bool brilha)
        {
            if (r == null)
                return;
            Material m = brilha ? Brilho : Aceso;
            if (m != null)
                r.sharedMaterial = m;
        }
    }
}
