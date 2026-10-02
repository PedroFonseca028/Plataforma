using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Curupira
{
    /// <summary>
    /// Regras da fase: conta as sementes, soma pontos, controla vidas,
    /// checkpoints, pausa e decide vitória ou derrota.
    /// </summary>
    public class GerenciadorFase : MonoBehaviour
    {
        public static GerenciadorFase Instancia { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            Instancia = null;
        }

        public const int PontosSemente = 100;
        public const int BonusFase = 500;

        public int TotalSementes { get; private set; }
        public int SementesColetadas { get; private set; }
        public float LimiteQueda
        {
            get { return -3f; }
        }
        public bool Terminou
        {
            get { return terminou; }
        }

        ConstrutorFase construtor;
        Jogador jogador;
        Vector3 pontoRenascer;
        List<Coletavel> sementes;
        string proximaCena;
        int numeroFase;
        Tema tema;
        Hud hud;
        MenuPausa pausa;
        SpriteRenderer seta;
        bool terminou;

        public void Iniciar(ConstrutorFase c, Jogador j, Vector3 inicio, List<Coletavel> listaSementes, int fase,
            string nome, string proxima, Tema t)
        {
            Instancia = this;
            construtor = c;
            jogador = j;
            pontoRenascer = inicio;
            sementes = listaSementes;
            TotalSementes = listaSementes.Count;
            proximaCena = proxima;
            numeroFase = fase;
            tema = t;

            FabricaUI.GarantirEventSystem();
            hud = Hud.Criar(TotalSementes);
            hud.Banner(nome, "RECUPERE AS " + TotalSementes + " SEMENTES ENCANTADAS!");
            pausa = MenuPausa.Criar(this);

            seta = Efeitos.NovoSprite("Seta da semente", null, Arte.Sprite("seta"), Ordem.TextoFlutuante, true);
            seta.enabled = false;

            SomJogo.Musica(tema == Tema.Mata ? Trilha.Fase1 : Trilha.Fase2);
            Transicao.AoEntrarNaCena();
        }

        void OnDestroy()
        {
            if (Instancia == this)
                Instancia = null;
            Time.timeScale = 1f;
        }

        void Update()
        {
            if (Entrada.MudoApertado)
                SomJogo.AlternarMudo();
            if (!terminou && !Transicao.Carregando && Entrada.PausaApertada)
                AlternarPausa();
            if (jogador != null && hud != null)
                hud.Atualizar(SementesColetadas, jogador.RecargaRestante / Jogador.RecargaAssobio);
            AtualizarSeta();
        }

        public void AlternarPausa()
        {
            bool pausar = !pausa.Aberto;
            pausa.Mostrar(pausar);
            Time.timeScale = pausar ? 0f : 1f;
            SomJogo.AbafarMusica(pausar);
        }

        public void ReiniciarFase()
        {
            Time.timeScale = 1f;
            Sessao.RestaurarInicioFase();
            Transicao.Carregar(SceneManager.GetActiveScene().name);
        }

        public void IrParaMenu()
        {
            Time.timeScale = 1f;
            Transicao.Carregar(Cenas.Menu);
        }

        /// <summary>A setinha dourada aponta para a semente mais próxima quando ela está longe.</summary>
        void AtualizarSeta()
        {
            if (seta == null || jogador == null)
                return;
            Coletavel maisPerto = null;
            float melhor = float.MaxValue;
            Vector3 p = jogador.transform.position + Vector3.up * 0.8f;
            foreach (Coletavel s in sementes)
            {
                if (s == null || s.Coletado)
                    continue;
                float d = (s.transform.position - p).sqrMagnitude;
                if (d < melhor)
                {
                    melhor = d;
                    maisPerto = s;
                }
            }
            bool mostrar = maisPerto != null && melhor > 10f * 10f && !terminou && jogador.Vivo;
            seta.enabled = mostrar;
            if (!mostrar)
                return;
            Vector3 dir = (maisPerto.transform.position - p).normalized;
            float pulso = 1.4f + Mathf.Sin(Time.time * 5f) * 0.1f;
            seta.transform.position = p + dir * pulso;
            seta.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            seta.color = new Color(1f, 1f, 1f, 0.85f);
        }

        // ------------------------------------------------------------ eventos do jogo
        public void ItemColetado(Coletavel item)
        {
            Vector3 pos = item.transform.position;
            switch (item.tipo)
            {
                case TipoColetavel.Semente:
                    SementesColetadas++;
                    Sessao.SementesColetadas++;
                    Sessao.SomarPontos(PontosSemente);
                    SomJogo.Tocar(Som.Semente);
                    Efeitos.Particulas("brilho", pos, 10, new Color(1f, 0.9f, 0.4f), 2f, 5f, 0.7f, 2f, true);
                    Efeitos.TextoSubindo("+" + PontosSemente, pos + Vector3.up * 0.6f, FabricaUI.CorDestaque);
                    Efeitos.Animacao("broto", new Vector3(pos.x, Mathf.Floor(pos.y) + 0.5f, 0f), 6f, 1f, false, Ordem.Decoracao + 1);
                    StartCoroutine(construtor.Renascer(pos, tema == Tema.Queimada ? 5.5f : 3.5f));
                    if (tema == Tema.Queimada)
                        SomJogo.Tocar(Som.Renascer, 0.6f);
                    int faltam = TotalSementes - SementesColetadas;
                    if (faltam <= 0)
                        StartCoroutine(Vitoria());
                    else
                        Mensagem(faltam == 1 ? "FALTA 1 SEMENTE!" : "FALTAM " + faltam + " SEMENTES", FabricaUI.CorDestaque, 1.4f);
                    break;
                case TipoColetavel.Acai:
                    Pontuar(10, pos, Som.Acai);
                    break;
                case TipoColetavel.Caju:
                    Pontuar(25, pos, Som.Fruta);
                    break;
                case TipoColetavel.Guarana:
                    Pontuar(50, pos, Som.Fruta);
                    break;
                case TipoColetavel.Coracao:
                    if (Sessao.Vidas < Sessao.VidasMaximas)
                    {
                        Sessao.Vidas++;
                        Mensagem("+1 VIDA!", FabricaUI.CorVermelha, 1.2f);
                        SomJogo.Tocar(Som.Vida);
                    }
                    else
                    {
                        Pontuar(200, pos, Som.Vida);
                    }
                    Efeitos.Particulas("brilho", pos, 8, FabricaUI.CorVermelha, 2f, 4f, 0.6f, 2f, true);
                    break;
            }
            hud.AtualizarTudo();
        }

        void Pontuar(int valor, Vector3 pos, Som som)
        {
            Sessao.SomarPontos(valor);
            SomJogo.Tocar(som, 0.8f);
            Efeitos.Animacao("brilho", pos, 16f, 0.8f, true);
            Efeitos.TextoSubindo("+" + valor, pos + Vector3.up * 0.5f, Color.white);
        }

        public void CheckpointAtivado(Totem t)
        {
            pontoRenascer = t.transform.position + new Vector3(0.9f, 0f, 0f);
            Mensagem("TOTEM ATIVADO!", FabricaUI.CorVerde, 1.2f);
        }

        public void Mensagem(string texto, Color cor, float duracao)
        {
            if (hud != null)
                hud.Mensagem(texto, cor, duracao);
        }

        public void JogadorFerido()
        {
            if (terminou)
                return;
            Sessao.Vidas--;
            hud.AtualizarTudo();
            hud.PiscarVidas();
            if (Sessao.Vidas <= 0)
                StartCoroutine(Derrota());
        }

        public void JogadorCaiuNoAbismo()
        {
            if (terminou || jogador == null || !jogador.Vivo)
                return;
            Sessao.Vidas--;
            hud.AtualizarTudo();
            hud.PiscarVidas();
            SomJogo.Tocar(Som.Queda);
            if (Sessao.Vidas <= 0)
                StartCoroutine(Derrota());
            else
            {
                jogador.Renascer(pontoRenascer);
                Mensagem("CUIDADO!", FabricaUI.CorVermelha, 1f);
            }
        }

        IEnumerator Derrota()
        {
            terminou = true;
            Sessao.Vidas = 0;
            jogador.Morrer();
            SomJogo.Musica(Trilha.Nenhuma);
            Mensagem("AS MÁQUINAS VENCERAM...", FabricaUI.CorVermelha, 3f);
            yield return new WaitForSeconds(2f);
            Sessao.Finalizar(ResultadoJogo.Derrota);
            Transicao.Carregar(Cenas.FimDeJogo);
        }

        IEnumerator Vitoria()
        {
            terminou = true;
            jogador.Comemorar();
            SomJogo.Musica(Trilha.Vitoria, false);
            Sessao.SomarPontos(BonusFase);
            Mensagem("A MATA AGRADECE!  +" + BonusFase, FabricaUI.CorVerde, 3.5f);
            StartCoroutine(construtor.RenascerTudo(jogador.transform.position));
            yield return new WaitForSeconds(3.5f);
            if (!string.IsNullOrEmpty(proximaCena))
            {
                Transicao.Carregar(proximaCena);
            }
            else
            {
                Sessao.Finalizar(ResultadoJogo.Vitoria);
                Transicao.Carregar(Cenas.FimDeJogo);
            }
        }
    }
}
