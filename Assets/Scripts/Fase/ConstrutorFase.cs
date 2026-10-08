using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace RumoAoHexa
{
    /// <summary>
    /// Monta a fase inteira a partir de um mapa em texto (Resources/Fases/*.txt):
    /// Grid + Tilemaps (chão com Tilemap Collider 2D + Composite Collider 2D +
    /// Rigidbody 2D Static), plataformas, perigos, argentinos, itens, fundo com
    /// parallax, câmera e HUD. Veja a legenda no começo de cada arquivo de fase.
    /// </summary>
    public class ConstrutorFase : MonoBehaviour
    {
        [Header("Fase")]
        public string arquivoFase = "Fase1";
        public int numeroFase = 1;
        public string nomeFase = Textos.Fase1Nome;
        public Tema tema = Tema.Praia;
        [Tooltip("Cena carregada ao pegar todos os itens. Vazio = fim do jogo (vitória).")]
        public string proximaCena = Cenas.Fase2;

        [Header("Materiais do URP 2D")]
        public Material materialAceso;
        public Material materialBrilho;

        [Header("Luzes 2D opcionais (modelo desativado na cena)")]
        public GameObject modeloLuzItem;

        MapaFase mapa;
        string prefixo;
        Tilemap tmChao;
        Tilemap tmDeco;
        Tilemap tmPlataformas;
        readonly List<Coletavel> itensObrigatorios = new List<Coletavel>();

        public MapaFase Mapa
        {
            get { return mapa; }
        }
        public int Largura
        {
            get { return mapa != null ? mapa.Largura : 0; }
        }
        public int Altura
        {
            get { return mapa != null ? mapa.Altura : 0; }
        }

        void Awake()
        {
            // o jogo sempre começa pela tela inicial (com as orientações de como jogar):
            // dar Play direto numa fase leva primeiro para o Menu
            if (!Sessao.Iniciada && !Partida.PodePularTelaInicial())
            {
                enabled = false;
                SceneManager.LoadScene(Cenas.Menu);
                return;
            }

            Materiais2D.Definir(materialAceso, materialBrilho);
            Camadas.ConfigurarColisoes();
            Sessao.GarantirIniciada();
            Sessao.MarcarInicioFase(numeroFase);
            prefixo = Temas.Prefixo(tema);

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
            seguir.limites = new Rect(0f, 0f, mapa.Largura, mapa.Altura + 4f);
            // posiciona a câmera antes de criar o fundo (o parallax usa a posição inicial)
            cam.transform.position = new Vector3(inicio.x, Mathf.Max(inicio.y + 1.6f, CameraSeguir.TamanhoOrtografico), -10f);
            Fundo.Criar(tema, cam.transform, 0f, true);

            var g = gameObject.AddComponent<GerenciadorFase>();
            string objetivo = numeroFase == 1 ? Textos.Fase1Objetivo : Textos.Fase2Objetivo;
            g.Iniciar(jogador, inicio, itensObrigatorios, numeroFase, nomeFase, objetivo, proximaCena, tema);
            grade.name = "Grid (fase " + numeroFase + ")";
        }

        // ------------------------------------------------------------ leitura do mapa
        bool LerMapa()
        {
            TextAsset arquivo = Resources.Load<TextAsset>("Fases/" + arquivoFase);
            if (arquivo == null)
            {
                Debug.LogError("[RumoAoHexa] Mapa não encontrado: Resources/Fases/" + arquivoFase + ".txt");
                return false;
            }
            mapa = MapaFase.Ler(arquivo.text);
            foreach (string problema in mapa.Validar(Regras.ItensObrigatorios(numeroFase)))
                Debug.LogWarning("[RumoAoHexa] " + arquivoFase + ": " + problema);
            return mapa.Largura > 0;
        }

        string NomeChao(int x, int y)
        {
            int m = mapa.Mascara(x, y);
            // no estádio, o que fica acima do gramado é a arquibancada de concreto
            string tipo = tema == Tema.Estadio && y >= 4 ? "_degrau_" : "_chao_";
            string nome = prefixo + tipo + m;
            if (m == 15)
            {
                int v = MapaFase.Hash(x, y) % 6;
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
            Tilemap tmPerigos = NovoTilemap(gridGo.transform, "Perigos (Tilemap)", Ordem.Perigos, Camadas.Perigo, tema == Tema.Estadio);
            Tilemap tmLiquido = NovoTilemap(gridGo.transform, "Liquido (Tilemap)", Ordem.Liquido, 0, false);

            var colisores = new GameObject("Colisores extras").transform;
            colisores.SetParent(gridGo.transform, false);

            TileBase perigo = Arte.TileAnimado(prefixo + "_espinho", tema == Tema.Praia ? 2f : 9f);
            TileBase liquidoTopo = Arte.TileAnimado(prefixo + "_liquido_topo", 5f);
            TileBase liquidoFundo = Arte.TileAnimado(prefixo + "_liquido_fundo", 3f);

            for (int y = 0; y < mapa.Altura; y++)
            {
                for (int x = 0; x < mapa.Largura; x++)
                {
                    var cel = new Vector3Int(x, y, 0);
                    switch (mapa.Em(x, y))
                    {
                        case Legenda.Chao:
                            tmChao.SetTile(cel, Arte.Tile(NomeChao(x, y), true));
                            break;
                        case Legenda.Plataforma:
                            tmPlataformas.SetTile(cel, Arte.Tile(prefixo + "_plataforma_" + mapa.TipoPlataforma(x, y), false));
                            break;
                        case Legenda.Espinho:
                            tmPerigos.SetTile(cel, perigo);
                            break;
                        case Legenda.LiquidoTopo:
                            tmLiquido.SetTile(cel, liquidoTopo);
                            break;
                        case Legenda.LiquidoFundo:
                            tmLiquido.SetTile(cel, liquidoFundo);
                            break;
                    }
                }
            }

            // colisores juntando células vizinhas iguais (uma caixa por trecho)
            for (int y = 0; y < mapa.Altura; y++)
            {
                int x = 0;
                while (x < mapa.Largura)
                {
                    char c = mapa.Em(x, y);
                    if (c != Legenda.Plataforma && c != Legenda.Espinho && c != Legenda.LiquidoTopo)
                    {
                        x++;
                        continue;
                    }
                    int ini = x;
                    while (x < mapa.Largura && mapa.Em(x, y) == c)
                        x++;
                    int n = x - ini;
                    if (c == Legenda.Plataforma)
                        ColisorPlataforma(colisores, ini, y, n);
                    else if (c == Legenda.Espinho)
                        ColisorPerigo(colisores, ini, y, n);
                    else
                        ColisorLiquido(colisores, ini, y, n);
                }
            }

            // paredes invisíveis nas laterais
            Parede(colisores, -0.5f);
            Parede(colisores, mapa.Largura + 0.5f);

            DecorarAutomaticamente();
            return gridGo.transform;
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

        void ColisorPerigo(Transform pai, int x, int y, int n)
        {
            var go = new GameObject(tema == Tema.Praia ? "Ouricos" : "Sinalizadores");
            go.layer = Camadas.Perigo;
            go.transform.SetParent(pai, false);
            go.transform.position = new Vector3(x + n * 0.5f, y, 0f);
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(n - 0.2f, 0.55f);
            col.offset = new Vector2(0f, 0.275f);
        }

        void ColisorLiquido(Transform pai, int x, int y, int n)
        {
            var go = new GameObject(tema == Tema.Praia ? "Mar" : "Fosso");
            go.layer = Camadas.Perigo;
            go.transform.SetParent(pai, false);
            go.transform.position = new Vector3(x + n * 0.5f, y, 0f);
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(n, 0.6f);
            col.offset = new Vector2(0f, 0.3f);
            go.AddComponent<ZonaLiquida>();
        }

        void Parede(Transform pai, float x)
        {
            var go = new GameObject("Parede invisivel");
            go.layer = Camadas.Chao;
            go.transform.SetParent(pai, false);
            go.transform.position = new Vector3(x, mapa.Altura * 0.5f + 5f, 0f);
            go.AddComponent<BoxCollider2D>().size = new Vector2(1f, mapa.Altura + 20f);
        }

        public static readonly string[] DecoPraia =
        {
            "praia_deco_concha", "praia_deco_estrelamar", "praia_deco_balde", "praia_deco_chinelo",
            "praia_deco_coco", "praia_deco_capim", "praia_deco_capim", "praia_deco_pedra",
        };

        public static readonly string[] DecoEstadio =
        {
            "estadio_deco_tufoa", "estadio_deco_tufob", "estadio_deco_tufoa", "estadio_deco_cone",
            "estadio_deco_garrafa", "estadio_deco_copo", "estadio_deco_papel",
        };

        void DecorarAutomaticamente()
        {
            string[] lista = tema == Tema.Praia ? DecoPraia : DecoEstadio;
            for (int y = 0; y < mapa.Altura - 1; y++)
            {
                for (int x = 0; x < mapa.Largura; x++)
                {
                    if (mapa.Em(x, y) != Legenda.Chao || mapa.Em(x, y + 1) != Legenda.Vazio)
                        continue;
                    // a arquibancada já tem as cadeiras: decoração só no gramado
                    if (tema == Tema.Estadio && y >= 4)
                        continue;
                    int h = MapaFase.Hash(x + 7, y * 3 + numeroFase);
                    if (h % 100 < 30)
                        tmDeco.SetTile(new Vector3Int(x, y + 1, 0), Arte.Tile(lista[(h / 100) % lista.Length], false));
                }
            }
        }

        // ------------------------------------------------------------ entidades
        /// <summary>Sprite da decoração grande de cada letra do mapa (T Y B b a).</summary>
        public static string DecoracaoGrande(Tema tema, char c)
        {
            bool praia = tema == Tema.Praia;
            switch (c)
            {
                case 'T': return praia ? "praia_coqueiro" : "estadio_trave";
                case 'Y': return praia ? "praia_guarda_sol" : "estadio_placa";
                case 'B': return praia ? "praia_trave" : "estadio_banco";
                case 'b': return praia ? "praia_quiosque" : "estadio_camera";
                case 'a': return praia ? "praia_castelo" : "estadio_sacola";
            }
            return null;
        }

        Vector3 CriarEntidades()
        {
            Vector3 inicio = new Vector3(2.5f, mapa.Altura * 0.5f, 0f);
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

            // itens obrigatórios: o ano/letra segue a ordem da esquerda para a direita
            List<Celula> ordem = mapa.ItensObrigatoriosEmOrdem();
            for (int i = 0; i < ordem.Count; i++)
            {
                Celula c = ordem[i];
                string rotulo = Regras.RotuloItem(numeroFase, i);
                string anim = numeroFase == 1 ? "estrela" : "letra_" + rotulo.ToLowerInvariant();
                Coletavel item = Coletavel.Criar(TipoColetavel.ItemObrigatorio, new Vector3(c.X + 0.5f, c.Y + 0.5f, 0f), i,
                    rotulo, anim, modeloLuzItem);
                item.transform.SetParent(pai(numeroFase == 1 ? "Estrelas" : "Letras"), true);
                itensObrigatorios.Add(item);
            }

            for (int y = 0; y < mapa.Altura; y++)
            {
                for (int x = 0; x < mapa.Largura; x++)
                {
                    char c = mapa.Em(x, y);
                    Vector3 pe = new Vector3(x + 0.5f, y, 0f);
                    Vector3 centro = new Vector3(x + 0.5f, y + 0.5f, 0f);
                    switch (c)
                    {
                        case Legenda.Inicio:
                            inicio = pe;
                            break;
                        case Legenda.Bandeirinha:
                            Bandeirinha.Criar(pe).transform.SetParent(pai("Bandeirinhas"), true);
                            break;
                        case Legenda.Moeda:
                            Coletavel.Criar(TipoColetavel.Moeda, centro, 0, null, null, null).transform.SetParent(pai("Moedas"), true);
                            break;
                        case Legenda.Coracao:
                            Coletavel.Criar(TipoColetavel.Coracao, centro, 0, null, null, null).transform.SetParent(pai("Coracoes"), true);
                            break;
                        case Legenda.Zagueiro:
                            Inimigo.Criar<Zagueiro>(pe).transform.SetParent(pai("Argentinos"), true);
                            break;
                        case Legenda.Lateral:
                            Inimigo.Criar<Lateral>(pe).transform.SetParent(pai("Argentinos"), true);
                            break;
                        case Legenda.Cabeceador:
                            Inimigo.Criar<Cabeceador>(pe).transform.SetParent(pai("Argentinos"), true);
                            break;
                        case Legenda.PlataformaMovel:
                            CriarPlataformaMovel(x, y).transform.SetParent(pai("Plataformas moveis"), true);
                            break;
                        default:
                            string deco = DecoracaoGrande(tema, c);
                            if (deco != null)
                            {
                                SpriteRenderer sr = Efeitos.NovoSprite(deco, pai("Decoracao"), Arte.Sprite(deco), Ordem.DecoracaoGrande);
                                sr.transform.position = pe;
                            }
                            break;
                    }
                }
            }
            return inicio;
        }

        PlataformaMovel CriarPlataformaMovel(int x, int y)
        {
            int esq, dir;
            mapa.LimitesPlataformaMovel(x, y, out esq, out dir);
            Vector2 a = new Vector2(esq + 1.5f, y + 0.5f);
            Vector2 b = new Vector2(dir - 0.5f, y + 0.5f);
            PlataformaMovel p = PlataformaMovel.Criar(new Vector2(x + 1.5f, y + 0.5f), b, 3, prefixo);
            p.pontoA = a;
            return p;
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
            MapaFase m = MapaFase.Ler(arquivo.text);
            for (int y = 0; y < m.Altura; y++)
            {
                for (int x = 0; x < m.Largura; x++)
                {
                    char c = m.Em(x, y);
                    Color cor;
                    switch (c)
                    {
                        case '#': cor = tema == Tema.Praia ? new Color(0.95f, 0.8f, 0.5f, 0.8f) : new Color(0.3f, 0.6f, 0.3f, 0.8f); break;
                        case '=': cor = new Color(0.9f, 0.9f, 0.9f, 0.8f); break;
                        case '^': cor = new Color(1f, 0.2f, 0.2f, 0.8f); break;
                        case '~': case '%': cor = new Color(0.2f, 0.5f, 1f, 0.6f); break;
                        case '*': cor = new Color(1f, 0.9f, 0.2f, 1f); break;
                        case 'z': case 'r': case 'c': cor = new Color(0.4f, 0.75f, 0.95f, 1f); break;
                        case 'P': cor = new Color(0.2f, 1f, 0.3f, 1f); break;
                        case 'K': cor = new Color(0.1f, 0.6f, 0.3f, 1f); break;
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
