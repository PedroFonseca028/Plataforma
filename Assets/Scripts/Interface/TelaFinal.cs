using UnityEngine;
using UnityEngine.UI;

namespace Curupira
{
    /// <summary>
    /// Cena "FimDeJogo": informa VITÓRIA ou DERROTA, mostra a pontuação e
    /// tem os botões para jogar de novo ou voltar ao menu.
    /// </summary>
    public class TelaFinal : MonoBehaviour
    {
        [Header("Materiais do URP 2D")]
        public Material materialAceso;
        public Material materialBrilho;

        Button jogarDeNovo;

        void Start()
        {
            Materiais2D.Definir(materialAceso, materialBrilho);
            Time.timeScale = 1f;
            bool venceu = Sessao.Resultado == ResultadoJogo.Vitoria;

            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = CameraSeguir.TamanhoOrtografico;
                cam.transform.position = new Vector3(0f, 0f, -10f);
                Fundo.Criar(venceu ? Tema.Mata : Tema.Queimada, cam.transform, 0.6f);
                Personagens(venceu);
            }

            FabricaUI.GarantirEventSystem();
            Canvas canvas = FabricaUI.CriarCanvas("Fim de jogo", 10);
            Transform t = canvas.transform;
            Vector2 c = new Vector2(0.5f, 0.5f);

            string titulo;
            string frase;
            Color cor;
            if (Sessao.Resultado == ResultadoJogo.Vitoria)
            {
                titulo = "VITÓRIA!";
                frase = "VOCÊ RECUPEROU TODAS AS SEMENTES.\nA MATA ESTÁ SALVA, CURUPIRA!";
                cor = FabricaUI.CorDestaque;
            }
            else if (Sessao.Resultado == ResultadoJogo.Derrota)
            {
                titulo = "DERROTA...";
                frase = "SUAS VIDAS ACABARAM E AS MÁQUINAS VENCERAM.\nA FLORESTA AINDA PRECISA DE VOCÊ!";
                cor = FabricaUI.CorVermelha;
            }
            else
            {
                titulo = "FIM DE JOGO";
                frase = "JOGUE A PARTIR DO MENU PARA VER O RESULTADO.";
                cor = FabricaUI.CorTexto;
            }

            FabricaUI.Texto(t, titulo, c, new Vector2(0f, 175f), 8f, cor, TextAnchor.MiddleCenter, new Vector2(900f, 110f)).contorno = true;
            FabricaUI.Texto(t, frase, c, new Vector2(0f, 90f), 2f, FabricaUI.CorTexto, TextAnchor.MiddleCenter, new Vector2(900f, 60f)).contorno = true;

            FabricaUI.Painel(t, c, new Vector2(0f, -5f), new Vector2(420f, 110f));
            FabricaUI.Texto(t, "PONTOS: " + Sessao.Pontos, c, new Vector2(0f, 20f), 3f, FabricaUI.CorTexto);
            FabricaUI.Texto(t, "SEMENTES: " + Sessao.SementesColetadas + "/" + Sessao.TotalSementesDoJogo, c, new Vector2(0f, -15f), 2f, FabricaUI.CorDestaque);
            string recorde = Sessao.NovoRecorde ? "NOVO RECORDE!" : "RECORDE: " + Sessao.Recorde;
            FabricaUI.Texto(t, recorde, c, new Vector2(0f, -40f), 2f, Sessao.NovoRecorde ? FabricaUI.CorVerde : new Color(1f, 1f, 1f, 0.8f));

            jogarDeNovo = FabricaUI.Botao(t, "JOGAR NOVAMENTE", c, new Vector2(0f, -100f), new Vector2(300f, 48f), JogarNovamente);
            Button menu = FabricaUI.Botao(t, "MENU PRINCIPAL", c, new Vector2(0f, -160f), new Vector2(300f, 48f), () => Transicao.Carregar(Cenas.Menu));
            FabricaUI.NavegacaoVertical(jogarDeNovo, menu);
            FabricaUI.Selecionar(jogarDeNovo);

            SomJogo.Musica(venceu ? Trilha.Vitoria : Trilha.Derrota, false);
            Transicao.AoEntrarNaCena();
        }

        void Update()
        {
            if (Entrada.MudoApertado)
                SomJogo.AlternarMudo();
        }

        void Personagens(bool venceu)
        {
            float y = -CameraSeguir.TamanhoOrtografico + 1.2f;
            SpriteRenderer cur = Efeitos.NovoSprite("Curupira", null, null, Ordem.Jogador);
            cur.transform.position = new Vector3(-7.5f, y, 0f);
            cur.transform.localScale = Vector3.one * 2f;
            var an = cur.gameObject.AddComponent<AnimadorSprite>();
            an.alvo = cur;
            an.Tocar(venceu ? "curupira_vitoria" : "curupira_dano", venceu ? 4f : 1f);

            SpriteRenderer outro = Efeitos.NovoSprite(venceu ? "Broto" : "Serra", null, null, Ordem.Inimigos);
            outro.transform.position = new Vector3(8.5f, y, 0f);
            outro.transform.localScale = Vector3.one * (venceu ? 1.5f : 2f);
            var an2 = outro.gameObject.AddComponent<AnimadorSprite>();
            an2.alvo = outro;
            if (venceu)
                an2.Tocar(Arte.Anim("arvore_ipe"), 1f);
            else
            {
                an2.Tocar("serra_andar", 8f);
                outro.flipX = true;
            }
        }

        void JogarNovamente()
        {
            Sessao.NovoJogo();
            Transicao.Carregar(Cenas.Fase1);
        }
    }
}
