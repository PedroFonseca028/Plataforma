using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RumoAoHexa
{
    /// <summary>
    /// Cena "FimDeJogo": informa VITÓRIA (pegou todos os itens) ou DERROTA
    /// (os argentinos machucaram e as vidas chegaram a zero), mostra a pontuação
    /// e tem o botão REINICIAR (começa o jogo de novo) e o de voltar ao menu.
    /// </summary>
    public class TelaFinal : MonoBehaviour
    {
        [Header("Materiais do URP 2D")]
        public Material materialAceso;
        public Material materialBrilho;

        Button reiniciar;
        Canvas tela;
        bool venceu;
        float tempoConfete;

        /// <summary>O botão que reinicia o jogo (volta para a Fase 1 com tudo zerado).</summary>
        public Button BotaoReiniciar
        {
            get { return reiniciar; }
        }

        /// <summary>O Canvas da tela final (com VITÓRIA ou DERROTA).</summary>
        public Canvas Tela
        {
            get { return tela; }
        }

        void Start()
        {
            Time.timeScale = 1f;
            // aberta direto no editor, sem partida: o jogo começa pela tela inicial
            if (Sessao.Resultado == ResultadoJogo.Nenhum && !Sessao.Iniciada && !Partida.PodePularTelaInicial())
            {
                SceneManager.LoadScene(Cenas.Menu);
                return;
            }
            Materiais2D.Definir(materialAceso, materialBrilho);
            venceu = Sessao.Resultado == ResultadoJogo.Vitoria;

            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = CameraSeguir.TamanhoOrtografico;
                cam.transform.position = new Vector3(0f, 0f, -10f);
                Fundo.Criar(venceu ? Tema.Estadio : Tema.Praia, cam.transform, 0.4f);
                Personagens();
            }

            FabricaUI.GarantirEventSystem();
            Canvas canvas = FabricaUI.CriarCanvas("Fim de jogo", 10);
            tela = canvas;
            Transform t = canvas.transform;
            Vector2 c = new Vector2(0.5f, 0.5f);

            string titulo;
            string frase;
            Color cor;
            if (Sessao.Resultado == ResultadoJogo.Vitoria)
            {
                titulo = Textos.VitoriaTitulo;
                frase = Textos.VitoriaFrase;
                cor = FabricaUI.CorDestaque;
            }
            else if (Sessao.Resultado == ResultadoJogo.Derrota)
            {
                titulo = Textos.DerrotaTitulo;
                frase = Textos.DerrotaFrase;
                cor = FabricaUI.CorVermelha;
            }
            else
            {
                titulo = "FIM DE JOGO";
                frase = Textos.FimSemResultado;
                cor = FabricaUI.CorTexto;
            }

            FabricaUI.Texto(t, titulo, c, new Vector2(0f, 196f), 8f, cor, TextAnchor.MiddleCenter, new Vector2(900f, 110f)).contorno = true;
            FabricaUI.Texto(t, frase, c, new Vector2(0f, 118f), 2f, FabricaUI.CorTexto, TextAnchor.MiddleCenter, new Vector2(920f, 60f)).contorno = true;

            FabricaUI.Painel(t, c, new Vector2(0f, -8f), new Vector2(580f, 150f));
            FabricaUI.Texto(t, "PONTOS: " + Sessao.Pontos, c, new Vector2(0f, 38f), 3f, FabricaUI.CorTexto);
            FabricaUI.Texto(t, "ESTRELAS E LETRAS: " + Sessao.ItensColetados + "/" + Regras.TotalItensDoJogo, c,
                new Vector2(0f, 5f), 2f, FabricaUI.CorDestaque);
            FabricaUI.Texto(t, "MOEDAS: " + Sessao.Moedas + "    ARGENTINOS EXPULSOS: " + Sessao.Expulsos, c,
                new Vector2(0f, -20f), 2f, FabricaUI.CorTexto);
            string recorde = Sessao.NovoRecorde ? "NOVO RECORDE!" : "RECORDE: " + Sessao.Recorde;
            FabricaUI.Texto(t, recorde, c, new Vector2(0f, -48f), 2f, Sessao.NovoRecorde ? FabricaUI.CorVerde : new Color(1f, 1f, 1f, 0.8f));

            reiniciar = FabricaUI.Botao(t, Textos.BotaoReiniciar, c, new Vector2(0f, -122f), new Vector2(300f, 48f), Reiniciar);
            Button menu = FabricaUI.Botao(t, Textos.BotaoMenu, c, new Vector2(0f, -182f), new Vector2(300f, 48f), IrParaMenu);
            FabricaUI.NavegacaoVertical(reiniciar, menu);
            FabricaUI.Selecionar(reiniciar);
            FabricaUI.Texto(t, Textos.DicaReiniciar, c, new Vector2(0f, -226f), 2f, new Color(1f, 1f, 1f, 0.75f)).contorno = true;

            SomJogo.Musica(venceu ? Trilha.Vitoria : Trilha.Derrota, false);
            Transicao.AoEntrarNaCena();
        }

        void Update()
        {
            if (Entrada.MudoApertado)
                SomJogo.AlternarMudo();
            if (reiniciar != null && Entrada.ConfirmarApertado)
                Reiniciar();
            if (!venceu)
                return;
            // chuva de confete verde e amarelo
            tempoConfete -= Time.deltaTime;
            if (tempoConfete <= 0f)
            {
                tempoConfete = 0.35f;
                Efeitos.Confete(new Vector3(Random.Range(-11f, 11f), 7.5f, 0f), 8);
            }
        }

        void Personagens()
        {
            float y = -CameraSeguir.TamanhoOrtografico + 1.2f;
            SpriteRenderer craque = Efeitos.NovoSprite("Craque", null, null, Ordem.Jogador);
            craque.transform.position = new Vector3(-8.5f, y, 0f);
            craque.transform.localScale = Vector3.one * 2f;
            var an = craque.gameObject.AddComponent<AnimadorSprite>();
            an.alvo = craque;
            an.Tocar(venceu ? "craque_vitoria" : "craque_dano", venceu ? 4f : 1f);

            if (venceu)
            {
                // a taça erguida acima da cabeça
                SpriteRenderer taca = Efeitos.NovoSprite("Taça", null, Arte.Sprite("taca"), Ordem.Jogador + 1, true);
                taca.transform.position = new Vector3(-8.5f, y + 3.55f, 0f);
                taca.transform.localScale = Vector3.one * 2f;
                SpriteRenderer brilho = Efeitos.NovoSprite("Brilho", null, Arte.Sprite("luz_suave"), Ordem.Jogador, true);
                brilho.transform.position = taca.transform.position + new Vector3(0f, 0.9f, 0f);
                brilho.transform.localScale = Vector3.one * 3f;
                brilho.color = new Color(1f, 0.9f, 0.4f, 0.6f);
            }

            // do outro lado, um argentino: tonto na vitória, pulando de alegria na derrota
            SpriteRenderer outro = Efeitos.NovoSprite("Argentino", null, null, Ordem.Inimigos);
            outro.transform.position = new Vector3(8.5f, y, 0f);
            outro.transform.localScale = Vector3.one * 2f;
            outro.flipX = true;
            var an2 = outro.gameObject.AddComponent<AnimadorSprite>();
            an2.alvo = outro;
            if (venceu)
                an2.Tocar("zagueiro_tonto", 3f);
            else
                an2.Tocar("cabeceador_pulo", 4f);
        }

        /// <summary>Reinicia o jogo: pontos, vidas e itens zerados, de volta à Fase 1.</summary>
        public void Reiniciar()
        {
            if (Transicao.Carregando)
                return;
            Sessao.NovoJogo();
            Transicao.Carregar(Cenas.Fase1);
        }

        void IrParaMenu()
        {
            if (Transicao.Carregando)
                return;
            Sessao.Encerrar();
            Transicao.Carregar(Cenas.Menu);
        }
    }
}
