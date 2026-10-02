using UnityEngine;
using UnityEngine.UI;

namespace Curupira
{
    /// <summary>Cena "Menu": título, fundo animado, botões e a tela "Como jogar".</summary>
    public class MenuPrincipal : MonoBehaviour
    {
        [Header("Materiais do URP 2D")]
        public Material materialAceso;
        public Material materialBrilho;

        [Header("Créditos (edite com os nomes da dupla)")]
        [TextArea] public string creditos = "ARTE, MÚSICA E CÓDIGO ORIGINAIS - FEITO COM UNITY 6";

        GameObject telaInicial;
        GameObject telaComoJogar;
        Button botaoJogar;
        Button botaoVoltar;

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
                Fundo.Criar(Tema.Mata, cam.transform, 2.5f);
                CriarChaoCorrendo();
            }

            FabricaUI.GarantirEventSystem();
            Canvas canvas = FabricaUI.CriarCanvas("Menu", 10);
            Vector2 c = new Vector2(0.5f, 0.5f);

            telaInicial = new GameObject("Tela inicial", typeof(RectTransform));
            telaInicial.transform.SetParent(canvas.transform, false);
            FabricaUI.Esticar((RectTransform)telaInicial.transform);
            Transform t = telaInicial.transform;

            TextoPixel titulo = FabricaUI.Texto(t, "CURUPIRA", c, new Vector2(0f, 175f), 9f, FabricaUI.CorDestaque, TextAnchor.MiddleCenter, new Vector2(900f, 120f));
            titulo.contorno = true;
            TextoPixel sub = FabricaUI.Texto(t, "GUARDIÃO DA MATA", c, new Vector2(0f, 100f), 3f, FabricaUI.CorTexto);
            sub.contorno = true;

            botaoJogar = FabricaUI.Botao(t, "JOGAR", c, new Vector2(0f, 20f), new Vector2(260f, 48f), Jogar);
            Button comoJogar = FabricaUI.Botao(t, "COMO JOGAR", c, new Vector2(0f, -40f), new Vector2(260f, 48f), () => MostrarComoJogar(true));
            Button sair = FabricaUI.Botao(t, "SAIR", c, new Vector2(0f, -100f), new Vector2(260f, 48f), Sair);
            FabricaUI.NavegacaoVertical(botaoJogar, comoJogar, sair);

            FabricaUI.Texto(t, "RECORDE: " + Sessao.Recorde, c, new Vector2(0f, -160f), 2f, FabricaUI.CorVerde).contorno = true;
            FabricaUI.Texto(t, creditos, new Vector2(0.5f, 0f), new Vector2(0f, 20f), 2f, new Color(1f, 1f, 1f, 0.75f)).contorno = true;

            MontarComoJogar(canvas.transform);
            MostrarComoJogar(false);

            SomJogo.Musica(Trilha.Menu);
            Transicao.AoEntrarNaCena();
        }

        void Update()
        {
            if (Entrada.MudoApertado)
                SomJogo.AlternarMudo();
            if (telaComoJogar != null && telaComoJogar.activeSelf && Entrada.VoltarApertado)
                MostrarComoJogar(false);
        }

        /// <summary>Chão e Curupira correndo "no lugar" enquanto o fundo rola.</summary>
        void CriarChaoCorrendo()
        {
            var raiz = new GameObject("Chao do menu").transform;
            raiz.gameObject.AddComponent<RolarChao>();
            float y0 = -CameraSeguir.TamanhoOrtografico + 0.5f;
            for (int x = -16; x <= 16; x++)
            {
                for (int k = 0; k < 2; k++)
                {
                    string nome = k == 0 ? "mata_terra_14" : "mata_terra_15";
                    SpriteRenderer sr = Efeitos.NovoSprite("tile", raiz, Arte.Sprite(nome), Ordem.Chao);
                    sr.transform.localPosition = new Vector3(x, y0 + 1f - k, 0f);
                }
            }
            SpriteRenderer cur = Efeitos.NovoSprite("Curupira", null, null, Ordem.Jogador);
            cur.transform.position = new Vector3(-6f, y0 + 1.5f, 0f);
            cur.transform.localScale = Vector3.one * 1.5f;
            var an = cur.gameObject.AddComponent<AnimadorSprite>();
            an.alvo = cur;
            an.usarTempoReal = true;
            an.Tocar("curupira_correr", 12f);

            SpriteRenderer serra = Efeitos.NovoSprite("Serra perseguindo", null, null, Ordem.Inimigos);
            serra.transform.position = new Vector3(-10.5f, y0 + 1.5f, 0f);
            serra.transform.localScale = Vector3.one * 1.5f;
            var an2 = serra.gameObject.AddComponent<AnimadorSprite>();
            an2.alvo = serra;
            an2.Tocar("serra_andar", 10f);
        }

        void MontarComoJogar(Transform canvas)
        {
            Vector2 c = new Vector2(0.5f, 0.5f);
            Image cortina = FabricaUI.Cortina(canvas, new Color(0.03f, 0.02f, 0.06f, 0.55f));
            telaComoJogar = cortina.gameObject;
            Transform t = cortina.transform;
            FabricaUI.Painel(t, c, Vector2.zero, new Vector2(820f, 480f));
            FabricaUI.Texto(t, "COMO JOGAR", c, new Vector2(0f, 200f), 4f, FabricaUI.CorDestaque).contorno = true;

            string[,] linhas =
            {
                { "A D  OU  ← →", "ANDAR" },
                { "ESPAÇO / W / ↑", "PULAR (SEGURE PARA IR MAIS ALTO)" },
                { "J  OU  X", "ASSOBIO MÁGICO: DEIXA AS MÁQUINAS TONTAS" },
                { "S + ESPAÇO", "DESCER DA PLATAFORMA DE MADEIRA" },
                { "ESC / P", "PAUSAR      M: SOM" },
            };
            for (int i = 0; i < linhas.GetLength(0); i++)
            {
                float y = 140f - i * 34f;
                FabricaUI.Texto(t, linhas[i, 0], c, new Vector2(-190f, y), 2f, FabricaUI.CorVerde, TextAnchor.MiddleRight, new Vector2(300f, 30f));
                FabricaUI.Texto(t, linhas[i, 1], c, new Vector2(195f, y), 2f, FabricaUI.CorTexto, TextAnchor.MiddleLeft, new Vector2(400f, 30f));
            }

            string historia =
                "AS MÁQUINAS DA MADEIREIRA SERRA-TUDO INVADIRAM A MATA E\n" +
                "ESPALHARAM AS SEMENTES ENCANTADAS QUE MANTÊM A FLORESTA VIVA.\n" +
                "RECUPERE TODAS AS SEMENTES DE CADA FASE PARA VENCER!\n" +
                "PULE NA CABEÇA DAS MÁQUINAS PARA DESLIGÁ-LAS.\n" +
                "A FAGULHA QUEIMA: ASSOBIE ANTES DE PISAR NELA!\n" +
                "TOTENS SALVAM O CAMINHO. FRUTAS DÃO PONTOS E O CORAÇÃO DÁ VIDA.";
            FabricaUI.Texto(t, historia, c, new Vector2(0f, -85f), 2f, FabricaUI.CorTexto, TextAnchor.MiddleCenter, new Vector2(780f, 170f));

            botaoVoltar = FabricaUI.Botao(t, "VOLTAR", c, new Vector2(0f, -200f), new Vector2(220f, 44f), () => MostrarComoJogar(false));
        }

        void MostrarComoJogar(bool mostrar)
        {
            telaComoJogar.SetActive(mostrar);
            telaInicial.SetActive(!mostrar);
            FabricaUI.Selecionar(mostrar ? botaoVoltar : botaoJogar);
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
