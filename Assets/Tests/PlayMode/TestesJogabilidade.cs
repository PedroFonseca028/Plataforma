using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RumoAoHexa.Testes
{
    /// <summary>
    /// Testes jogando de verdade (Play Mode): carregam as cenas, chutam a bola,
    /// pisam nos argentinos, pegam os itens e conferem vitória, derrota e o
    /// botão de reiniciar. Rodam em Window > General > Test Runner > PlayMode.
    /// </summary>
    public class TestesJogabilidade
    {
        [UnitySetUp]
        public IEnumerator Preparar()
        {
            Time.timeScale = 1f;
            // espera uma troca de cena que tenha sobrado de outro teste
            float limite = 5f;
            while (Transicao.Carregando && limite > 0f)
            {
                limite -= Time.unscaledDeltaTime;
                yield return null;
            }
        }

        [TearDown]
        public void Finalizar()
        {
            Time.timeScale = 1f;
        }

        static IEnumerator CarregarFase(string cena)
        {
            Sessao.NovoJogo();
            yield return SceneManager.LoadSceneAsync(cena);
            yield return null;
            yield return new WaitForFixedUpdate();
            Assert.That(GerenciadorFase.Instancia, Is.Not.Null, "a fase não montou o GerenciadorFase");
            Assert.That(Jogador.Atual, Is.Not.Null, "a fase não criou o jogador");
        }

        static void TirarTodosOsArgentinos()
        {
            foreach (Inimigo i in new List<Inimigo>(Inimigo.Todos))
                Object.DestroyImmediate(i.gameObject);
        }

        static T ArgentinoNaFrente<T>(float distancia) where T : Inimigo
        {
            Jogador j = Jogador.Atual;
            Vector3 pos = j.transform.position + new Vector3(j.Direcao * distancia, 0f, 0f);
            return Inimigo.Criar<T>(pos);
        }

        static IEnumerator EsperarAte(System.Func<bool> condicao, float segundos)
        {
            float t = 0f;
            while (!condicao() && t < segundos)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator Fase1_MontaCraqueBolaArgentinosEEstrelas()
        {
            yield return CarregarFase(Cenas.Fase1);
            Jogador j = Jogador.Atual;
            Assert.That(j.Bola, Is.Not.Null);
            Assert.That(j.Bola.NoPe, Is.True, "a bola começa no pé");
            Assert.That(Inimigo.Todos.Count, Is.GreaterThan(5));
            Assert.That(GerenciadorFase.Instancia.TotalItens, Is.EqualTo(5));
            Assert.That(GerenciadorFase.Instancia.Tema, Is.EqualTo(Tema.Praia));
        }

        [UnityTest]
        public IEnumerator Jogador_FicaEmCimaDoChao()
        {
            yield return CarregarFase(Cenas.Fase1);
            float y = Jogador.Atual.transform.position.y;
            yield return new WaitForSeconds(1f);
            Assert.That(Jogador.Atual.NoChao, Is.True, "o craque precisa estar apoiado no chão");
            Assert.That(Jogador.Atual.transform.position.y, Is.EqualTo(y).Within(0.1f), "o craque não pode atravessar o chão");
        }

        [UnityTest]
        public IEnumerator ChuteRasteiro_ExpulsaOArgentino_EEleSome()
        {
            yield return CarregarFase(Cenas.Fase1);
            TirarTodosOsArgentinos();
            Zagueiro z = ArgentinoNaFrente<Zagueiro>(3f);
            yield return new WaitForFixedUpdate();
            int pontos = Sessao.Pontos;

            Assert.That(Jogador.Atual.Chutar(TipoChute.Rasteiro), Is.True);
            yield return EsperarAte(() => z == null, 2f);

            Assert.That(z == null, Is.True, "o argentino atingido pela bola precisa sumir");
            Assert.That(Sessao.Expulsos, Is.EqualTo(1));
            Assert.That(Sessao.Pontos, Is.EqualTo(pontos + Regras.PontosExpulsao));
        }

        [UnityTest]
        public IEnumerator FolhaSeca_AtravessaDoisArgentinos_ComBonusDeDoblete()
        {
            yield return CarregarFase(Cenas.Fase1);
            TirarTodosOsArgentinos();
            Zagueiro a = ArgentinoNaFrente<Zagueiro>(2.5f);
            Zagueiro b = ArgentinoNaFrente<Zagueiro>(4.5f);
            yield return new WaitForFixedUpdate();
            int pontos = Sessao.Pontos;

            Assert.That(Jogador.Atual.Chutar(TipoChute.FolhaSeca), Is.True);
            yield return EsperarAte(() => a == null && b == null, 2f);

            Assert.That(a == null && b == null, Is.True, "a folha seca atravessa e expulsa os dois");
            Assert.That(Sessao.Expulsos, Is.EqualTo(2));
            Assert.That(Sessao.Pontos, Is.EqualTo(pontos + 2 * Regras.PontosExpulsao + Regras.BonusCombo(2)));
        }

        [UnityTest]
        public IEnumerator BolaChutada_VoltaSozinhaParaOPe()
        {
            yield return CarregarFase(Cenas.Fase1);
            TirarTodosOsArgentinos();
            Bola bola = Jogador.Atual.Bola;
            Assert.That(Jogador.Atual.Chutar(TipoChute.Cavadinha), Is.True);
            Assert.That(bola.NoPe, Is.False);
            Assert.That(Jogador.Atual.Chutar(TipoChute.Rasteiro), Is.False, "não dá para chutar com a bola longe");
            yield return EsperarAte(() => bola.NoPe, 6f);
            Assert.That(bola.NoPe, Is.True, "a bola precisa voltar para o pé");
        }

        [UnityTest]
        public IEnumerator ChamarABola_FazElaVoltarNaHora()
        {
            yield return CarregarFase(Cenas.Fase1);
            TirarTodosOsArgentinos();
            Bola bola = Jogador.Atual.Bola;
            Jogador.Atual.Chutar(TipoChute.Rasteiro);
            yield return new WaitForSeconds(0.3f);
            bola.Chamar();
            Assert.That(bola.Estado.Situacao, Is.EqualTo(SituacaoBola.Voltando));
            yield return EsperarAte(() => bola.NoPe, 1.5f);
            Assert.That(bola.NoPe, Is.True);
        }

        [UnityTest]
        public IEnumerator PularNaCabeca_SoDeixaTonto_NaoExpulsa()
        {
            yield return CarregarFase(Cenas.Fase1);
            TirarTodosOsArgentinos();
            Zagueiro z = ArgentinoNaFrente<Zagueiro>(4f);
            yield return new WaitForFixedUpdate();
            // coloca o craque caindo bem em cima da cabeça do zagueiro
            Jogador.Atual.transform.position = z.transform.position + new Vector3(0f, 1.9f, 0f);
            Jogador.Atual.GetComponent<Rigidbody2D>().position = Jogador.Atual.transform.position;
            Jogador.Atual.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0f, -4f);
            yield return EsperarAte(() => z != null && z.EstaTonto, 1f);

            Assert.That(z != null, Is.True, "o pisão não pode expulsar");
            Assert.That(z.EstaTonto, Is.True, "o pisão deixa o argentino tonto");
            Assert.That(z.EstaExpulso, Is.False);
            Assert.That(Sessao.Vidas, Is.EqualTo(Regras.VidasIniciais), "pisar na cabeça não machuca");
        }

        [UnityTest]
        public IEnumerator EncostarNoArgentino_TiraUmaVida()
        {
            yield return CarregarFase(Cenas.Fase1);
            TirarTodosOsArgentinos();
            Inimigo.Criar<Zagueiro>(Jogador.Atual.transform.position + new Vector3(0.3f, 0f, 0f));
            yield return EsperarAte(() => Sessao.Vidas < Regras.VidasIniciais, 1f);
            Assert.That(Sessao.Vidas, Is.EqualTo(Regras.VidasIniciais - 1));
            Assert.That(GerenciadorFase.Instancia.Placar.Argentina, Is.EqualTo(1), "vida perdida vira gol da Argentina");
        }

        [UnityTest]
        public IEnumerator Moeda_SomaPontos()
        {
            yield return CarregarFase(Cenas.Fase1);
            Coletavel moeda = null;
            GameObject moedas = GameObject.Find("Moedas");
            Assert.That(moedas, Is.Not.Null, "a fase agrupa as moedas no objeto 'Moedas'");
            foreach (Coletavel c in moedas.GetComponentsInChildren<Coletavel>())
                if (c.tipo == TipoColetavel.Moeda) { moeda = c; break; }
            Assert.That(moeda, Is.Not.Null);
            moeda.Coletar();
            Assert.That(Sessao.Moedas, Is.EqualTo(1));
            Assert.That(Sessao.Pontos, Is.EqualTo(Regras.PontosMoeda));
        }

        static IEnumerator ColetarItensObrigatorios()
        {
            foreach (Coletavel c in new List<Coletavel>(GerenciadorFase.Instancia.ItensObrigatorios))
            {
                if (c == null || c.Coletado)
                    continue;
                c.Coletar();
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator PegarAsCincoEstrelas_LevaParaAFase2()
        {
            yield return CarregarFase(Cenas.Fase1);
            TirarTodosOsArgentinos();
            yield return ColetarItensObrigatorios();
            Assert.That(GerenciadorFase.Instancia.ItensColetados, Is.EqualTo(5));
            Assert.That(GerenciadorFase.Instancia.Terminou, Is.True);
            yield return EsperarAte(() => SceneManager.GetActiveScene().name == Cenas.Fase2 && !Transicao.Carregando, 10f);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(Cenas.Fase2));
            Assert.That(Sessao.ItensColetados, Is.EqualTo(5), "as estrelas continuam contando na fase 2");
        }

        /// <summary>Algum texto da tela (Canvas) é exatamente <paramref name="texto"/>?</summary>
        static bool TemTexto(Canvas tela, string texto)
        {
            foreach (TextoPixel t in tela.GetComponentsInChildren<TextoPixel>())
                if (t.Texto == texto)
                    return true;
            return false;
        }

        static IEnumerator EsperarTelaFinal()
        {
            yield return EsperarAte(() => SceneManager.GetActiveScene().name == Cenas.FimDeJogo && !Transicao.Carregando, 12f);
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(Cenas.FimDeJogo), "a cena final precisa aparecer");
        }

        [UnityTest]
        public IEnumerator PegarHEXA_NaFase2_EhVitoria_ComBotaoReiniciar()
        {
            yield return CarregarFase(Cenas.Fase2);
            TirarTodosOsArgentinos();
            Assert.That(GerenciadorFase.Instancia.TotalItens, Is.EqualTo(4));
            yield return ColetarItensObrigatorios();
            yield return EsperarTelaFinal();
            Assert.That(Sessao.Resultado, Is.EqualTo(ResultadoJogo.Vitoria));

            TelaFinal tela = Object.FindAnyObjectByType<TelaFinal>();
            Assert.That(tela, Is.Not.Null);
            Assert.That(TemTexto(tela.Tela, Textos.VitoriaTitulo), Is.True, "a cena final informa VITÓRIA");
            Assert.That(TemTexto(tela.Tela, Textos.VitoriaFrase), Is.True, "e explica que todos os itens foram coletados");
            Assert.That(tela.BotaoReiniciar, Is.Not.Null, "a cena final tem o botão REINICIAR");
        }

        [UnityTest]
        public IEnumerator PerderTodasAsVidas_EhDerrota_EReiniciarRecomeca()
        {
            yield return CarregarFase(Cenas.Fase1);
            TirarTodosOsArgentinos();
            for (int i = 0; i < Regras.VidasIniciais; i++)
                GerenciadorFase.Instancia.JogadorFerido();
            Assert.That(Sessao.Vidas, Is.EqualTo(0));
            yield return EsperarTelaFinal();
            Assert.That(Sessao.Resultado, Is.EqualTo(ResultadoJogo.Derrota));

            TelaFinal tela = Object.FindAnyObjectByType<TelaFinal>();
            Assert.That(tela, Is.Not.Null);
            Assert.That(TemTexto(tela.Tela, Textos.DerrotaTitulo), Is.True, "a cena final informa DERROTA");
            Assert.That(TemTexto(tela.Tela, Textos.DerrotaFrase), Is.True, "e explica que as vidas chegaram a zero");
            Assert.That(tela.BotaoReiniciar, Is.Not.Null, "a cena final tem o botão REINICIAR");
            tela.BotaoReiniciar.onClick.Invoke();
            yield return EsperarAte(() => SceneManager.GetActiveScene().name == Cenas.Fase1 && !Transicao.Carregando, 10f);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(Cenas.Fase1), "REINICIAR volta para a Fase 1");
            Assert.That(Sessao.Vidas, Is.EqualTo(Regras.VidasIniciais), "reiniciar devolve as vidas");
            Assert.That(Sessao.Pontos, Is.EqualTo(0), "reiniciar zera os pontos");
            Assert.That(Sessao.ItensColetados, Is.EqualTo(0), "reiniciar zera os itens");
        }

        [UnityTest]
        public IEnumerator ArgentinosMachucandoAteAsVidasZerarem_EhDerrota()
        {
            yield return CarregarFase(Cenas.Fase1);
            TirarTodosOsArgentinos();
            // um argentino aparece em cima do craque sempre que ele pode ser machucado de novo
            float limite = 15f;
            while (Sessao.Vidas > 0 && limite > 0f)
            {
                Jogador j = Jogador.Atual;
                if (j != null && j.Vivo && !j.Invulneravel)
                    Inimigo.Criar<Zagueiro>(j.transform.position);
                limite -= Time.unscaledDeltaTime;
                yield return null;
            }
            Assert.That(Sessao.Vidas, Is.EqualTo(0), "os argentinos tiram uma vida a cada encostada");
            yield return EsperarTelaFinal();
            Assert.That(Sessao.Resultado, Is.EqualTo(ResultadoJogo.Derrota));
            Assert.That(TemTexto(Object.FindAnyObjectByType<TelaFinal>().Tela, Textos.DerrotaTitulo), Is.True);
        }

        [UnityTest]
        public IEnumerator DarPlayNaFaseSemPassarPeloMenu_AbreATelaInicial()
        {
            Sessao.Encerrar();
            yield return SceneManager.LoadSceneAsync(Cenas.Fase1);
            yield return EsperarAte(() => SceneManager.GetActiveScene().name == Cenas.Menu, 5f);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(Cenas.Menu), "o jogo sempre começa pela tela inicial");
        }

        [UnityTest]
        public IEnumerator CairNoMar_TiraVida_EVoltaParaOInicio()
        {
            yield return CarregarFase(Cenas.Fase1);
            TirarTodosOsArgentinos();
            Vector3 inicio = Jogador.Atual.transform.position;
            GerenciadorFase.Instancia.JogadorCaiuNoAbismo();
            yield return null;
            Assert.That(Sessao.Vidas, Is.EqualTo(Regras.VidasIniciais - 1));
            Assert.That(Jogador.Atual.transform.position.x, Is.EqualTo(inicio.x).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator Estadio_PlacarMarcaGolDoBrasilNaExpulsao()
        {
            yield return CarregarFase(Cenas.Fase2);
            TirarTodosOsArgentinos();
            Assert.That(GerenciadorFase.Instancia.Tema, Is.EqualTo(Tema.Estadio));
            Placar placar = Object.FindAnyObjectByType<Placar>();
            Assert.That(placar, Is.Not.Null, "o estádio tem placar ao vivo");
            Zagueiro z = ArgentinoNaFrente<Zagueiro>(3f);
            yield return new WaitForFixedUpdate();
            Jogador.Atual.Chutar(TipoChute.Rasteiro);
            yield return EsperarAte(() => z == null, 2f);
            yield return null;
            Assert.That(GerenciadorFase.Instancia.Placar.Brasil, Is.EqualTo(1));
            Assert.That(placar.Brasil, Is.EqualTo(1), "o telão mostra o gol");
        }

        [UnityTest]
        public IEnumerator TelaInicial_TemOrientacoesDeComoJogar_EBotaoJogarComecaAFase1()
        {
            yield return SceneManager.LoadSceneAsync(Cenas.Menu);
            yield return null;
            MenuPrincipal menu = Object.FindAnyObjectByType<MenuPrincipal>();
            Assert.That(menu, Is.Not.Null);
            Assert.That(menu.Tela, Is.Not.Null);
            Assert.That(menu.BotaoJogar, Is.Not.Null);
            Assert.That(TemTexto(menu.Tela, Textos.ComoJogarTitulo), Is.True, "COMO JOGAR");
            for (int i = 0; i < Textos.Controles.GetLength(0); i++)
            {
                Assert.That(TemTexto(menu.Tela, Textos.Controles[i, 0]), Is.True, "tecla " + Textos.Controles[i, 0]);
                Assert.That(TemTexto(menu.Tela, Textos.Controles[i, 1]), Is.True, "ação " + Textos.Controles[i, 1]);
            }
            Assert.That(TemTexto(menu.Tela, Textos.ObjetivoTitulo), Is.True, "OBJETIVO");
            Assert.That(TemTexto(menu.Tela, Textos.ComoGanharTitulo), Is.True, "COMO GANHAR");
            Assert.That(TemTexto(menu.Tela, Textos.ComoPerderTitulo), Is.True, "COMO PERDER");
            Assert.That(TemTexto(menu.Tela, Textos.ComoPerderTexto), Is.True, "explica que sem vidas é derrota");

            menu.BotaoJogar.onClick.Invoke();
            yield return EsperarAte(() => SceneManager.GetActiveScene().name == Cenas.Fase1 && !Transicao.Carregando, 10f);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(Cenas.Fase1));
        }

        [UnityTest]
        public IEnumerator Pausa_ParaOTempo()
        {
            yield return CarregarFase(Cenas.Fase1);
            GerenciadorFase.Instancia.AlternarPausa();
            Assert.That(Time.timeScale, Is.EqualTo(0f));
            GerenciadorFase.Instancia.AlternarPausa();
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }
    }
}
