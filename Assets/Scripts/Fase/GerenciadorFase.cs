using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RumoAoHexa
{
    /// <summary>
    /// Regras da fase: conta os itens obrigatórios, soma pontos e combos,
    /// controla vidas, bandeirinhas, placar, pausa e decide vitória ou derrota.
    /// </summary>
    public class GerenciadorFase : MonoBehaviour
    {
        public static GerenciadorFase Instancia { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            Instancia = null;
        }

        /// <summary>Abaixo disso o jogador (ou a bola) caiu do mapa.</summary>
        public float LimiteQueda
        {
            get { return -3f; }
        }
        public int TotalItens { get; private set; }
        public int ItensColetados { get; private set; }
        public int NumeroFase { get; private set; }
        public Tema Tema { get; private set; }
        public PlacarFase Placar { get; private set; }
        public bool Terminou
        {
            get { return terminou; }
        }
        public Jogador Jogador
        {
            get { return jogador; }
        }
        /// <summary>Estrelas (fase 1) ou letras (fase 2) da fase, na ordem do mapa.</summary>
        public IReadOnlyList<Coletavel> ItensObrigatorios
        {
            get { return itens; }
        }

        Jogador jogador;
        Vector3 pontoRenascer;
        List<Coletavel> itens;
        string proximaCena;
        Hud hud;
        MenuPausa pausa;
        SpriteRenderer seta;
        bool terminou;
        int sorteioNarrador;

        public void Iniciar(Jogador j, Vector3 inicio, List<Coletavel> itensObrigatorios, int fase, string nome,
            string objetivo, string proxima, Tema t)
        {
            Instancia = this;
            jogador = j;
            pontoRenascer = inicio;
            itens = itensObrigatorios;
            TotalItens = itensObrigatorios.Count;
            proximaCena = proxima;
            NumeroFase = fase;
            Tema = t;
            Placar = new PlacarFase();

            FabricaUI.GarantirEventSystem();
            hud = Hud.Criar(fase, TotalItens);
            hud.Banner(nome, objetivo);
            pausa = MenuPausa.Criar(this);

            seta = Efeitos.NovoSprite("Seta do item", null, Arte.Sprite("seta"), Ordem.TextoFlutuante, true);
            seta.enabled = false;

            SomJogo.Musica(t == Tema.Praia ? Trilha.Fase1 : Trilha.Fase2);
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
                hud.Atualizar(ItensColetados, jogador.CargaChute, jogador.Bola != null && jogador.Bola.NoPe);
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

        /// <summary>A setinha amarela aponta para o item obrigatório mais próximo quando ele está longe.</summary>
        void AtualizarSeta()
        {
            if (seta == null || jogador == null)
                return;
            Coletavel maisPerto = null;
            float melhor = float.MaxValue;
            Vector3 p = jogador.transform.position + Vector3.up * 0.8f;
            foreach (Coletavel s in itens)
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
            EstadoPartida e = Sessao.Estado;
            switch (item.tipo)
            {
                case TipoColetavel.ItemObrigatorio:
                    ItensColetados++;
                    e.ColetarItemObrigatorio();
                    SomJogo.Tocar(Som.ItemObrigatorio);
                    Efeitos.Particulas("brilho", pos, 10, new Color(1f, 0.9f, 0.4f), 2f, 5f, 0.7f, 2f, true);
                    Efeitos.Confete(pos, 14);
                    Efeitos.TextoSubindo("+" + Regras.PontosItemObrigatorio, pos + Vector3.up * 0.6f, FabricaUI.CorDestaque);
                    hud.MarcarItem(item.indice);
                    int faltam = TotalItens - ItensColetados;
                    Mensagem(Narrador.FraseItem(NumeroFase, item.rotulo, faltam), FabricaUI.CorDestaque, 1.8f);
                    if (faltam <= 0)
                        StartCoroutine(Vitoria());
                    break;
                case TipoColetavel.Moeda:
                    e.ColetarMoeda();
                    SomJogo.Tocar(Som.Moeda, 0.7f);
                    Efeitos.Animacao("brilho", pos, 16f, 0.8f, true);
                    break;
                case TipoColetavel.Coracao:
                    if (e.ColetarCoracao())
                    {
                        Mensagem(Textos.MaisUmaVida, FabricaUI.CorVermelha, 1.2f);
                        SomJogo.Tocar(Som.Vida);
                    }
                    else
                    {
                        SomJogo.Tocar(Som.Vida, 0.6f);
                        Efeitos.TextoSubindo("+" + Regras.PontosCoracaoExtra, pos + Vector3.up * 0.5f, Color.white);
                    }
                    Efeitos.Particulas("brilho", pos, 8, FabricaUI.CorVermelha, 2f, 4f, 0.6f, 2f, true);
                    break;
            }
            hud.AtualizarTudo();
        }

        /// <summary>
        /// A bola expulsou um argentino. <paramref name="acertos"/> é quantos a mesma bola já
        /// expulsou (2 = doblete...) e <paramref name="bonusCombo"/> o bônus a somar agora.
        /// </summary>
        public void ArgentinoExpulso(Inimigo ini, TipoChute tipo, int acertos, int bonusCombo)
        {
            EstadoPartida e = Sessao.Estado;
            e.RegistrarExpulsao();
            Placar.GolBrasil();
            Vector3 pos = (Vector3)ini.Centro + Vector3.up * (ini.Altura * 0.5f + 1.4f);
            Efeitos.TextoSubindo("+" + Regras.PontosExpulsao, pos, Color.white);
            if (bonusCombo > 0)
            {
                e.SomarPontos(bonusCombo);
                Mensagem(Regras.NomeCombo(acertos) + "  +" + Regras.BonusCombo(acertos), FabricaUI.CorDestaque, 1.6f);
                SomJogo.Tocar(Som.Combo);
                Efeitos.Confete(pos, 10);
            }
            else if (acertos <= 1)
            {
                Mensagem(Narrador.FraseExpulsao(tipo, sorteioNarrador++, Tema == Tema.Estadio), FabricaUI.CorVerde, 1.2f);
            }
            if (Tema == Tema.Estadio)
                Efeitos.Confete(pos, 6);
            hud.AtualizarTudo();
        }

        public void ArgentinoAtordoado(Inimigo ini)
        {
            Efeitos.TextoSubindo(Textos.Tonto, (Vector3)ini.Centro + Vector3.up * (ini.Altura * 0.5f + 1.2f), FabricaUI.CorTexto);
        }

        public void BandeirinhaAtivada(Bandeirinha b)
        {
            pontoRenascer = b.transform.position + new Vector3(0.9f, 0f, 0f);
            Mensagem(Textos.BandeirinhaAtivada, FabricaUI.CorVerde, 1.2f);
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
            PerderVida();
        }

        public void JogadorCaiuNoAbismo()
        {
            if (terminou || jogador == null || !jogador.Vivo)
                return;
            SomJogo.Tocar(Som.Queda);
            if (!PerderVida())
            {
                jogador.Renascer(pontoRenascer);
                Mensagem(Textos.Cuidado, FabricaUI.CorVermelha, 1f);
            }
        }

        /// <summary>Tira uma vida (vira gol da Argentina no placar). Devolve true se acabou o jogo.</summary>
        bool PerderVida()
        {
            bool acabou = Sessao.Estado.PerderVida();
            Placar.GolArgentina();
            hud.AtualizarTudo();
            hud.PiscarVidas();
            if (acabou)
                StartCoroutine(Derrota());
            return acabou;
        }

        IEnumerator Derrota()
        {
            terminou = true;
            Sessao.Estado.ZerarVidas();
            jogador.Morrer();
            SomJogo.Musica(Trilha.Nenhuma);
            Mensagem(Textos.PerdeuTudo, FabricaUI.CorVermelha, 3f);
            yield return new WaitForSeconds(2f);
            Sessao.Finalizar(ResultadoJogo.Derrota);
            Transicao.Carregar(Cenas.FimDeJogo);
        }

        IEnumerator Vitoria()
        {
            terminou = true;
            jogador.Comemorar();
            SomJogo.Musica(Trilha.Vitoria, false);
            Sessao.Estado.BonusDeFase();
            bool ultima = string.IsNullOrEmpty(proximaCena);
            Mensagem((ultima ? Textos.EHexa : Textos.FaseConcluida) + "  +" + Regras.BonusFase, FabricaUI.CorVerde, 3.5f);
            for (int i = 0; i < 6; i++)
            {
                Efeitos.Confete(jogador.transform.position + new Vector3(Random.Range(-6f, 6f), 7f, 0f), 18);
                yield return new WaitForSeconds(0.45f);
            }
            yield return new WaitForSeconds(0.8f);
            if (!ultima)
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
