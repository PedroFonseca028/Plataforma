using UnityEngine;
using UnityEngine.UI;

namespace RumoAoHexa
{
    /// <summary>
    /// Cena "Menu" (tela inicial): título, a história em duas linhas e as
    /// orientações de como jogar direto na tela, com os botões JOGAR e SAIR.
    /// No fundo, a praia rola e o craque faz embaixadinhas.
    /// </summary>
    public class MenuPrincipal : MonoBehaviour
    {
        [Header("Materiais do URP 2D")]
        public Material materialAceso;
        public Material materialBrilho;

        [Header("Créditos (edite com os nomes da dupla)")]
        [TextArea] public string creditos = "ARTE, MÚSICA E CÓDIGO ORIGINAIS - FEITO COM UNITY 6";

        Button botaoJogar;
        Canvas tela;

        public Button BotaoJogar
        {
            get { return botaoJogar; }
        }

        /// <summary>O Canvas da tela inicial (com o título e as instruções).</summary>
        public Canvas Tela
        {
            get { return tela; }
        }

        void Start()
        {
            Materiais2D.Definir(materialAceso, materialBrilho);
            Time.timeScale = 1f;

            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = CameraSeguir.TamanhoOrtografico;
                cam.transform.position = new Vector3(0f, 0f, -10f);
                Fundo.Criar(Tema.Praia, cam.transform, 1.2f);
                CriarPersonagens();
            }

            FabricaUI.GarantirEventSystem();
            Canvas canvas = FabricaUI.CriarCanvas("Menu", 10);
            tela = canvas;
            Transform t = canvas.transform;
            Vector2 c = new Vector2(0.5f, 0.5f);

            TextoPixel titulo = FabricaUI.Texto(t, Textos.Titulo, c, new Vector2(0f, 222f), 6f, FabricaUI.CorDestaque,
                TextAnchor.MiddleCenter, new Vector2(900f, 70f));
            titulo.contorno = true;
            titulo.corContorno = new Color(0.07f, 0.35f, 0.16f, 1f);
            FabricaUI.Texto(t, Textos.Subtitulo, c, new Vector2(0f, 172f), 2f, FabricaUI.CorTexto).contorno = true;

            // orientações de como jogar, direto na tela inicial
            FabricaUI.Painel(t, c, new Vector2(0f, -12f), new Vector2(900f, 312f));
            FabricaUI.Texto(t, Textos.Historia, c, new Vector2(0f, 110f), 2f, FabricaUI.CorDestaque,
                TextAnchor.MiddleCenter, new Vector2(860f, 50f));
            for (int i = 0; i < Textos.Controles.GetLength(0); i++)
            {
                float y = 74f - i * 23f;
                FabricaUI.Texto(t, Textos.Controles[i, 0], c, new Vector2(-150f, y), 2f, FabricaUI.CorVerde,
                    TextAnchor.MiddleRight, new Vector2(300f, 24f));
                FabricaUI.Texto(t, Textos.Controles[i, 1], c, new Vector2(232f, y), 2f, FabricaUI.CorTexto,
                    TextAnchor.MiddleLeft, new Vector2(420f, 24f));
            }
            FabricaUI.Texto(t, Textos.Dicas, c, new Vector2(0f, -130f), 2f, new Color(1f, 1f, 1f, 0.85f),
                TextAnchor.MiddleCenter, new Vector2(880f, 50f));

            botaoJogar = FabricaUI.Botao(t, "JOGAR", c, new Vector2(-130f, -210f), new Vector2(220f, 44f), Jogar);
            Button sair = FabricaUI.Botao(t, "SAIR", c, new Vector2(130f, -210f), new Vector2(220f, 44f), Sair);
            FabricaUI.NavegacaoHorizontal(botaoJogar, sair);
            FabricaUI.Selecionar(botaoJogar);

            FabricaUI.Texto(t, "RECORDE: " + Sessao.Recorde, c, new Vector2(0f, -246f), 2f, FabricaUI.CorDestaque).contorno = true;
            FabricaUI.Texto(t, creditos, new Vector2(0.5f, 0f), new Vector2(0f, 8f), 1f, new Color(1f, 1f, 1f, 0.8f),
                TextAnchor.MiddleCenter, new Vector2(900f, 14f)).contorno = true;

            SomJogo.Musica(Trilha.Menu);
            Transicao.AoEntrarNaCena();
        }

        void Update()
        {
            if (Entrada.MudoApertado)
                SomJogo.AlternarMudo();
        }

        /// <summary>O craque fazendo embaixadinha ao lado do título e um argentino de olho.</summary>
        void CriarPersonagens()
        {
            float y = 3.65f;
            SpriteRenderer craque = Efeitos.NovoSprite("Craque", null, null, Ordem.Jogador);
            craque.transform.position = new Vector3(-8.6f, y, 0f);
            craque.transform.localScale = Vector3.one * 1.5f;
            var an = craque.gameObject.AddComponent<AnimadorSprite>();
            an.alvo = craque;
            an.usarTempoReal = true;
            an.Tocar("craque_embaixadinha", 4f / Jogador.CicloEmbaixadinha);

            SpriteRenderer bola = Efeitos.NovoSprite("Bola", null, Arte.Sprite("bola"), Ordem.Bola);
            bola.transform.localScale = Vector3.one * 1.5f;
            var q = bola.gameObject.AddComponent<BolaQuicando>();
            q.pe = new Vector3(-8.6f + 0.47f, y + 1.03f, 0f);
            q.altura = 1.6f;
            q.periodo = Jogador.CicloEmbaixadinha;

            SpriteRenderer argentino = Efeitos.NovoSprite("Argentino", null, null, Ordem.Inimigos);
            argentino.transform.position = new Vector3(8.6f, y, 0f);
            argentino.transform.localScale = Vector3.one * 1.5f;
            argentino.flipX = true;
            var an2 = argentino.gameObject.AddComponent<AnimadorSprite>();
            an2.alvo = argentino;
            an2.usarTempoReal = true;
            an2.Tocar("cabeceador_andar", 4f);
        }

        void Jogar()
        {
            Sessao.NovoJogo();
            Transicao.Carregar(Cenas.Fase1);
        }

        void Sair()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
