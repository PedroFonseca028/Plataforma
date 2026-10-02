using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Curupira
{
    /// <summary>
    /// Monta a fase inteira a partir de um mapa em texto (Resources/Fases/*.txt):
    /// Grid + Tilemaps (chão com Tilemap Collider 2D + Composite Collider 2D +
    /// Rigidbody 2D Static), plataformas, perigos, inimigos, itens, fundo com
    /// parallax, câmera e HUD. Veja a legenda no começo de cada arquivo de fase.
    /// </summary>
    public class ConstrutorFase : MonoBehaviour
    {
        [Header("Fase")]
        public string arquivoFase = "Fase1";
        public int numeroFase = 1;
        public string nomeFase = "FASE 1 - A MATA AO ENTARDECER";
        public Tema tema = Tema.Mata;
        [Tooltip("Cena carregada ao coletar todas as sementes. Vazio = fim do jogo (vitória).")]
        public string proximaCena = "Fase2";

        [Header("Materiais do URP 2D")]
        public Material materialAceso;
        public Material materialBrilho;

        [Header("Luzes 2D opcionais (modelos desativados na cena)")]
        public GameObject modeloLuzSemente;

        // dados do mapa
        char[,] mapa;
        int largura;
        int altura;
        string prefixoTema;

        Tilemap tmChao;
        Tilemap tmDeco;
        Tilemap tmPlataformas;
        readonly List<Coletavel> sementes = new List<Coletavel>();
        readonly HashSet<Vector2Int> renascidos = new HashSet<Vector2Int>();

        public int Largura
        {
            get { return largura; }
        }
        public int Altura
        {
            get { return altura; }
        }

        void Awake()
        {
            Materiais2D.Definir(materialAceso, materialBrilho);
            Camadas.ConfigurarColisoes();
            Sessao.GarantirIniciada();
            Sessao.MarcarInicioFase(numeroFase);
            prefixoTema = tema == Tema.Mata ? "mata" : "queimada";

            if (!LerMapa())
                return;

            Transform grade = CriarTilemaps();
            Vector3 inicio = CriarEntidades();
            Jogador jogador = Jogador.Criar(inicio);

            Camera cam = Camera.main;
            if (cam == null)
            {
                cam = new GameObject("Main Camera").AddComponent<Camera>();
                cam.tag = "MainCamera";
            }
            CameraSeguir seguir = cam.GetComponent<CameraSeguir>();
            if (seguir == null)
                seguir = cam.gameObject.AddComponent<CameraSeguir>();
            seguir.alvo = jogador.transform;
            seguir.limites = new Rect(0f, 0f, largura, altura + 4f);
            // posiciona a câmera antes de criar o fundo (o parallax usa a posição inicial)
            cam.transform.position = new Vector3(inicio.x, Mathf.Max(inicio.y + 1.6f, CameraSeguir.TamanhoOrtografico), -10f);
            Fundo.Criar(tema, cam.transform);

            var g = gameObject.AddComponent<GerenciadorFase>();
            g.Iniciar(this, jogador, inicio, sementes, numeroFase, nomeFase, proximaCena, tema);
            grade.name = "Grid (fase " + numeroFase + ")";
        }

        // ------------------------------------------------------------ leitura do mapa
        bool LerMapa()
        {
            TextAsset arquivo = Resources.Load<TextAsset>("Fases/" + arquivoFase);
            if (arquivo == null)
            {
                Debug.LogError("[Curupira] Mapa não encontrado: Resources/Fases/" + arquivoFase + ".txt");
                return false;
            }
            var linhas = new List<string>();
            foreach (string bruta in arquivo.text.Split('\n'))
            {
                string l = bruta.TrimEnd('\r');
                if (l.StartsWith("//"))
                    continue; // comentários (legenda)
                linhas.Add(l);
            }
            // remove linhas vazias no fim
            while (linhas.Count > 0 && linhas[linhas.Count - 1].Trim().Length == 0)
                linhas.RemoveAt(linhas.Count - 1);

            altura = linhas.Count;
            largura = 0;
            foreach (string l in linhas)
                largura = Mathf.Max(largura, l.Length);
            mapa = new char[largura, altura];
            for (int linha = 0; linha < altura; linha++)
            {
                int y = altura - 1 - linha; // a primeira linha do arquivo é o topo
                for (int x = 0; x < largura; x++)
                    mapa[x, y] = x < linhas[linha].Length ? linhas[linha][x] : '.';
            }
            return true;
        }

        char Em(int x, int y)
        {
            if (x < 0 || x >= largura || y < 0 || y >= altura)
                return '.';
            return mapa[x, y];
        }

        bool EhChao(int x, int y)
        {
            if (y < 0)
                return true;           // abaixo do mapa: continua sendo terra
            if (y >= altura)
                return false;
            if (x < 0 || x >= largura)
                return true;           // bordas laterais: terra continua
            return mapa[x, y] == '#';
        }

        int Mascara(int x, int y)
        {
            int m = 0;
            if (EhChao(x, y + 1)) m |= 1;
            if (EhChao(x + 1, y)) m |= 2;
            if (EhChao(x, y - 1)) m |= 4;
            if (EhChao(x - 1, y)) m |= 8;
            return m;
        }

        static int Hash(int x, int y)
        {
            unchecked
            {
                int h = x * 73856093 ^ y * 19349663;
                return (h ^ (h >> 13)) & 0x7fffffff;
            }
        }

        string NomeTerra(string prefixo, int x, int y)
        {
            int m = Mascara(x, y);
            string nome = prefixo + "_terra_" + m;
            if (m == 15)
            {
                int v = Hash(x, y) % 6;
                if (v == 0) nome += "b";
                else if (v == 1) nome += "c";
            }
            return nome;
        }

        // ------------------------------------------------------------ tilemaps
        Tilemap NovoTilemap(Transform grade, string nome, int ordem, int camada, bool brilha)
        {
            var go = new GameObject(nome);
            go.layer = camada;
            go.transform.SetParent(grade, false);
            Tilemap tm = go.AddComponent<Tilemap>();
            TilemapRenderer tr = go.AddComponent<TilemapRenderer>();
            tr.sortingOrder = ordem;
            Materiais2D.Aplicar(tr, brilha);
            return tm;
        }

        Transform CriarTilemaps()
        {
            var gridGo = new GameObject("Grid");
            Grid grid = gridGo.AddComponent<Grid>();
            grid.cellSize = new Vector3(1f, 1f, 0f);

            // --- chão (com colisão, igual ao passo a passo dos slides)
            tmChao = NovoTilemap(gridGo.transform, "Chao (Tilemap)", Ordem.Chao, Camadas.Chao, false);
            Rigidbody2D rb = tmChao.gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            TilemapCollider2D tc = tmChao.gameObject.AddComponent<TilemapCollider2D>();
            tc.compositeOperation = Collider2D.CompositeOperation.Merge;
            CompositeCollider2D cc = tmChao.gameObject.AddComponent<CompositeCollider2D>();
            cc.geometryType = CompositeCollider2D.GeometryType.Polygons;

            tmDeco = NovoTilemap(gridGo.transform, "Decoracao (Tilemap)", Ordem.Decoracao, 0, false);
            tmPlataformas = NovoTilemap(gridGo.transform, "Plataformas (Tilemap)", Ordem.Plataformas, Camadas.Plataforma, false);
            Tilemap tmPerigos = NovoTilemap(gridGo.transform, "Espinhos (Tilemap)", Ordem.Perigos, Camadas.Perigo, tema == Tema.Queimada);
            Tilemap tmLiquido = NovoTilemap(gridGo.transform, "Liquido (Tilemap)", Ordem.Liquido, 0, tema == Tema.Queimada);

            var colisores = new GameObject("Colisores extras").transform;
            colisores.SetParent(gridGo.transform, false);

            TileBase espinho = tema == Tema.Mata
                ? Arte.Tile("mata_espinho_0", false)
                : Arte.TileAnimado("queimada_espinho", 5f);
            TileBase liquidoTopo = Arte.TileAnimado(prefixoTema + "_liquido_topo", 5f);
            TileBase liquidoFundo = Arte.TileAnimado(prefixoTema + "_liquido_fundo", 3f);

            for (int y = 0; y < altura; y++)
            {
                for (int x = 0; x < largura; x++)
                {
                    char c = mapa[x, y];
                    var cel = new Vector3Int(x, y, 0);
                    switch (c)
                    {
                        case '#':
                            tmChao.SetTile(cel, Arte.Tile(NomeTerra(prefixoTema, x, y), true));
                            break;
                        case '=':
                            tmPlataformas.SetTile(cel, Arte.Tile(prefixoTema + "_plataforma_" + TipoPlataforma(x, y), false));
                            break;
                        case '^':
                            tmPerigos.SetTile(cel, espinho);
                            break;
                        case '~':
                            tmLiquido.SetTile(cel, liquidoTopo);
                            break;
                        case '%':
                            tmLiquido.SetTile(cel, liquidoFundo);
                            break;
                    }
                }
            }

            // colisores juntando células vizinhas iguais (uma caixa por trecho)
            for (int y = 0; y < altura; y++)
            {
                int x = 0;
                while (x < largura)
                {
                    char c = mapa[x, y];
                    if (c != '=' && c != '^' && c != '~')
                    {
                        x++;
                        continue;
                    }
                    int ini = x;
                    while (x < largura && mapa[x, y] == c)
                        x++;
                    int n = x - ini;
                    if (c == '=')
                        ColisorPlataforma(colisores, ini, y, n);
                    else if (c == '^')
                        ColisorEspinho(colisores, ini, y, n);
                    else
                        ColisorLiquido(colisores, ini, y, n);
                }
            }

            // paredes invisíveis nas laterais
            Parede(colisores, -0.5f);
            Parede(colisores, largura + 0.5f);

            DecorarAutomaticamente();
            return gridGo.transform;
        }

        string TipoPlataforma(int x, int y)
        {
            bool esq = Em(x - 1, y) == '=';
            bool dir = Em(x + 1, y) == '=';
            if (esq && dir) return "m";
            if (dir) return "e";
            if (esq) return "d";
            return "u";
        }

        void ColisorPlataforma(Transform pai, int x, int y, int n)
        {
            var go = new GameObject("Plataforma atravessavel");
            go.layer = Camadas.Plataforma;
            go.transform.SetParent(pai, false);
            go.transform.position = new Vector3(x + n * 0.5f, y + 1f, 0f);
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(n, 0.25f);
            col.offset = new Vector2(0f, -0.125f);
            col.usedByEffector = true;
            var ef = go.AddComponent<PlatformEffector2D>();
            ef.useOneWay = true;
            ef.surfaceArc = 160f;
        }

        void ColisorEspinho(Transform pai, int x, int y, int n)
        {
            var go = new GameObject("Espinhos");
            go.layer = Camadas.Perigo;
            go.transform.SetParent(pai, false);
            go.transform.position = new Vector3(x + n * 0.5f, y, 0f);
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(n - 0.2f, 0.55f);
            col.offset = new Vector2(0f, 0.275f);
        }

        void ColisorLiquido(Transform pai, int x, int y, int n)
        {
            var go = new GameObject(tema == Tema.Mata ? "Rio" : "Brasas");
            go.layer = Camadas.Perigo;
            go.transform.SetParent(pai, false);
            go.transform.position = new Vector3(x + n * 0.5f, y, 0f);
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(n, 0.6f);
            col.offset = new Vector2(0f, 0.3f);
            go.AddComponent<ZonaLiquida>().brasas = tema == Tema.Queimada;
        }

        void Parede(Transform pai, float x)
        {
            var go = new GameObject("Parede invisivel");
            go.layer = Camadas.Chao;
            go.transform.SetParent(pai, false);
            go.transform.position = new Vector3(x, altura * 0.5f + 5f, 0f);
            go.AddComponent<BoxCollider2D>().size = new Vector2(1f, altura + 20f);
        }

        static readonly string[] DecoMata =
        {
            "mata_deco_tufo_0", "mata_deco_tufo_1", "mata_deco_tufo_2", "mata_deco_flor_0", "mata_deco_flor_1",
            "mata_deco_flor_2", "mata_deco_bromelia", "mata_deco_samambaia", "mata_deco_cogumelo", "mata_deco_pedra",
        };

        static readonly string[] DecoQueimada =
        {
            "queimada_deco_capim_0", "queimada_deco_capim_1", "queimada_deco_cinzas", "queimada_deco_toco",
            "queimada_deco_galho", "queimada_deco_pedra",
        };

        static readonly string[] Flores = { "mata_deco_flor_0", "mata_deco_flor_1", "mata_deco_flor_2", "mata_deco_bromelia" };

        void DecorarAutomaticamente()
        {
            string[] lista = tema == Tema.Mata ? DecoMata : DecoQueimada;
            for (int y = 0; y < altura - 1; y++)
            {
                for (int x = 0; x < largura; x++)
                {
                    if (mapa[x, y] != '#' || mapa[x, y + 1] != '.')
                        continue;
                    int h = Hash(x + 7, y * 3 + numeroFase);
                    if (h % 100 < 38)
                        tmDeco.SetTile(new Vector3Int(x, y + 1, 0), Arte.Tile(lista[(h / 100) % lista.Length], false));
                }
            }
        }

        // ------------------------------------------------------------ entidades
        Vector3 CriarEntidades()
        {
            Vector3 inicio = new Vector3(2.5f, altura * 0.5f, 0f);
            var pais = new Dictionary<string, Transform>();
            System.Func<string, Transform> pai = nome =>
            {
                Transform t;
                if (!pais.TryGetValue(nome, out t))
                {
                    t = new GameObject(nome).transform;
                    pais[nome] = t;
                }
                return t;
            };

            for (int y = 0; y < altura; y++)
            {
                for (int x = 0; x < largura; x++)
                {
                    char c = mapa[x, y];
                    Vector3 pe = new Vector3(x + 0.5f, y, 0f);
                    Vector3 centro = new Vector3(x + 0.5f, y + 0.5f, 0f);
                    switch (c)
                    {
                        case 'P':
                            inicio = pe;
                            break;
                        case 'K':
                            Totem.Criar(pe).transform.SetParent(pai("Totens"), true);
                            break;
                        case '*':
                            Coletavel s = Coletavel.Criar(TipoColetavel.Semente, centro, modeloLuzSemente);
                            s.transform.SetParent(pai("Sementes"), true);
                            sementes.Add(s);
                            break;
                        case 'o':
                            Coletavel.Criar(TipoColetavel.Acai, centro, null).transform.SetParent(pai("Frutas"), true);
                            break;
                        case 'c':
                            Coletavel.Criar(TipoColetavel.Caju, centro, null).transform.SetParent(pai("Frutas"), true);
                            break;
                        case 'g':
                            Coletavel.Criar(TipoColetavel.Guarana, centro, null).transform.SetParent(pai("Frutas"), true);
                            break;
                        case 'h':
                            Coletavel.Criar(TipoColetavel.Coracao, centro, null).transform.SetParent(pai("Frutas"), true);
                            break;
                        case 's':
                            Inimigo.Criar<SerraSerra>(pe).transform.SetParent(pai("Inimigos"), true);
                            break;
                        case 'd':
                            Inimigo.Criar<Drone>(centro).transform.SetParent(pai("Inimigos"), true);
                            break;
                        case 'f':
                            Inimigo.Criar<Fagulha>(pe).transform.SetParent(pai("Inimigos"), true);
                            break;
                        case 'M':
                            CriarPlataformaMovel(x, y).transform.SetParent(pai("Plataformas moveis"), true);
                            break;
                        case 'T':
                            Arvore(tema == Tema.Mata ? "arvore_ipe" : "arvore_queimada_0", pe, pai("Arvores"));
                            break;
                        case 'Y':
                            Arvore(tema == Tema.Mata ? "arvore_copa" : "arvore_queimada_1", pe, pai("Arvores"));
                            break;
                        case 'B':
                            Arvore(tema == Tema.Mata ? "arvore_palmeira" : "toco_grande", pe, pai("Arvores"));
                            break;
                        case 'b':
                            Arvore(tema == Tema.Mata ? "arvore_bananeira" : "toco_grande", pe, pai("Arvores"));
                            break;
                        case 'a':
                            Arvore(tema == Tema.Mata ? "arbusto" : "toco_grande", pe, pai("Arvores"));
                            break;
                    }
                }
            }
            return inicio;
        }

        void Arvore(string sprite, Vector3 pe, Transform pai)
        {
            SpriteRenderer sr = Efeitos.NovoSprite(sprite, pai, Arte.Sprite(sprite), Ordem.ArvoresGrandes);
            sr.transform.position = pe;
        }

        PlataformaMovel CriarPlataformaMovel(int x, int y)
        {
            int esq = x;
            int dir = x;
            for (int i = x - 1; i >= Mathf.Max(0, x - 30); i--)
                if (mapa[i, y] == '<') { esq = i; break; }
            for (int i = x + 1; i < Mathf.Min(largura, x + 30); i++)
                if (mapa[i, y] == '>') { dir = i; break; }
            Vector2 a = new Vector2(esq + 1.5f, y + 0.5f);
            Vector2 b = new Vector2(dir - 0.5f, y + 0.5f);
            PlataformaMovel p = PlataformaMovel.Criar(new Vector2(x + 1.5f, y + 0.5f), b, 3, prefixoTema);
            p.pontoA = a;
            return p;
        }

        // ------------------------------------------------------------ a mata renasce
        /// <summary>
        /// Na fase queimada, o chão em volta da semente recuperada volta a ficar verde
        /// (em ondas). Na mata, nascem flores. Retorna a rotina para ser iniciada.
        /// </summary>
        public IEnumerator Renascer(Vector2 centro, float raio)
        {
            int cx = Mathf.FloorToInt(centro.x);
            int cy = Mathf.FloorToInt(centro.y);
            int r = Mathf.CeilToInt(raio);
            for (int anel = 0; anel <= r; anel++)
            {
                for (int y = cy - anel; y <= cy + anel; y++)
                {
                    for (int x = cx - anel; x <= cx + anel; x++)
                    {
                        if (Mathf.Max(Mathf.Abs(x - cx), Mathf.Abs(y - cy)) != anel)
                            continue;
                        if ((x - centro.x) * (x - centro.x) + (y - centro.y) * (y - centro.y) > raio * raio)
                            continue;
                        RenascerCelula(x, y);
                    }
                }
                yield return new WaitForSeconds(0.05f);
            }
        }

        void RenascerCelula(int x, int y)
        {
            if (x < 0 || x >= largura || y < 0 || y >= altura)
                return;
            var pos = new Vector2Int(x, y);
            char c = mapa[x, y];
            if (tema == Tema.Queimada && !renascidos.Contains(pos))
            {
                if (c == '#')
                {
                    renascidos.Add(pos);
                    tmChao.SetTile(new Vector3Int(x, y, 0), Arte.Tile(NomeTerra("mata", x, y), true));
                }
                else if (c == '=')
                {
                    renascidos.Add(pos);
                    tmPlataformas.SetTile(new Vector3Int(x, y, 0), Arte.Tile("mata_plataforma_" + TipoPlataforma(x, y), false));
                }
            }
            // flores sobre o chão
            if (c == '#' && Em(x, y + 1) == '.')
            {
                int h = Hash(x, y + 99);
                var acima = new Vector3Int(x, y + 1, 0);
                if (tema == Tema.Queimada || h % 3 == 0)
                {
                    string nome = h % 2 == 0 ? Flores[(h / 2) % Flores.Length] : DecoMata[(h / 7) % DecoMata.Length];
                    tmDeco.SetTile(acima, Arte.Tile(nome, false));
                    if (h % 4 == 0)
                        Efeitos.Animacao("brilho", new Vector3(x + 0.5f, y + 1.4f, 0f), 12f, 1f, true);
                }
            }
        }

        /// <summary>Faz a fase toda renascer (final da fase 2).</summary>
        public IEnumerator RenascerTudo(Vector2 centro)
        {
            float raio = Mathf.Max(largura, altura) + 2f;
            for (float r = 2f; r < raio; r += 3f)
            {
                StartCoroutine(Renascer(centro, r));
                yield return new WaitForSeconds(0.12f);
            }
        }

#if UNITY_EDITOR
        /// <summary>Mostra o formato da fase na aba Scene, mesmo sem dar Play.</summary>
        void OnDrawGizmos()
        {
            if (Application.isPlaying)
                return;
            TextAsset arquivo = Resources.Load<TextAsset>("Fases/" + arquivoFase);
            if (arquivo == null)
                return;
            var linhas = new List<string>();
            foreach (string bruta in arquivo.text.Split('\n'))
            {
                string l = bruta.TrimEnd('\r');
                if (!l.StartsWith("//"))
                    linhas.Add(l);
            }
            while (linhas.Count > 0 && linhas[linhas.Count - 1].Trim().Length == 0)
                linhas.RemoveAt(linhas.Count - 1);
            int h = linhas.Count;
            for (int li = 0; li < h; li++)
            {
                int y = h - 1 - li;
                for (int x = 0; x < linhas[li].Length; x++)
                {
                    char c = linhas[li][x];
                    Color cor;
                    switch (c)
                    {
                        case '#': cor = tema == Tema.Mata ? new Color(0.45f, 0.3f, 0.2f, 0.8f) : new Color(0.3f, 0.25f, 0.25f, 0.8f); break;
                        case '=': cor = new Color(0.8f, 0.6f, 0.3f, 0.8f); break;
                        case '^': cor = new Color(1f, 0.2f, 0.2f, 0.8f); break;
                        case '~': case '%': cor = new Color(0.2f, 0.5f, 1f, 0.6f); break;
                        case '*': cor = new Color(1f, 0.9f, 0.2f, 1f); break;
                        case 's': case 'd': case 'f': cor = new Color(1f, 0.4f, 0f, 1f); break;
                        case 'P': cor = new Color(0.2f, 1f, 0.3f, 1f); break;
                        case 'K': cor = new Color(0.9f, 0.5f, 1f, 1f); break;
                        default: continue;
                    }
                    Gizmos.color = cor;
                    float tam = (c == '#' || c == '=' || c == '~' || c == '%' || c == '^') ? 1f : 0.6f;
                    Gizmos.DrawCube(new Vector3(x + 0.5f, y + 0.5f, 0f), new Vector3(tam, tam, 0.1f));
                }
            }
        }
#endif
    }
}
