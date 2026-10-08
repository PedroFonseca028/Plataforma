using NUnit.Framework;

namespace RumoAoHexa.Testes
{
    public class TestesEstadoPartida
    {
        [Test]
        public void NovoJogo_ComecaComTresVidasESemPontos()
        {
            var e = new EstadoPartida();
            Assert.That(e.Vidas, Is.EqualTo(Regras.VidasIniciais));
            Assert.That(e.Pontos, Is.EqualTo(0));
            Assert.That(e.Moedas, Is.EqualTo(0));
            Assert.That(e.ItensColetados, Is.EqualTo(0));
            Assert.That(e.FaseAtual, Is.EqualTo(1));
            Assert.That(e.Resultado, Is.EqualTo(ResultadoJogo.Nenhum));
        }

        [Test]
        public void Moeda_SomaDezPontos()
        {
            var e = new EstadoPartida();
            e.ColetarMoeda();
            e.ColetarMoeda();
            Assert.That(e.Moedas, Is.EqualTo(2));
            Assert.That(e.Pontos, Is.EqualTo(2 * Regras.PontosMoeda));
        }

        [Test]
        public void ItemObrigatorio_ContaEPontua()
        {
            var e = new EstadoPartida();
            e.ColetarItemObrigatorio();
            Assert.That(e.ItensColetados, Is.EqualTo(1));
            Assert.That(e.Pontos, Is.EqualTo(Regras.PontosItemObrigatorio));
        }

        [Test]
        public void Expulsao_ContaEPontua()
        {
            var e = new EstadoPartida();
            e.RegistrarExpulsao();
            Assert.That(e.Expulsos, Is.EqualTo(1));
            Assert.That(e.Pontos, Is.EqualTo(Regras.PontosExpulsao));
        }

        [Test]
        public void Coracao_DaVidaAteOMaximo_DepoisViraPontos()
        {
            var e = new EstadoPartida();
            Assert.That(e.ColetarCoracao(), Is.True);
            Assert.That(e.ColetarCoracao(), Is.True);
            Assert.That(e.Vidas, Is.EqualTo(Regras.VidasMaximas));
            Assert.That(e.ColetarCoracao(), Is.False);
            Assert.That(e.Vidas, Is.EqualTo(Regras.VidasMaximas));
            Assert.That(e.Pontos, Is.EqualTo(Regras.PontosCoracaoExtra));
        }

        [Test]
        public void PerderVida_AvisaQuandoAcaba()
        {
            var e = new EstadoPartida();
            Assert.That(e.PerderVida(), Is.False);
            Assert.That(e.PerderVida(), Is.False);
            Assert.That(e.PerderVida(), Is.True);
            Assert.That(e.Vidas, Is.EqualTo(0));
            Assert.That(e.PerderVida(), Is.True, "continua 'acabou' sem ficar negativo");
            Assert.That(e.Vidas, Is.EqualTo(0));
        }

        [Test]
        public void Pontos_NuncaFicamNegativos()
        {
            var e = new EstadoPartida();
            e.SomarPontos(-50);
            Assert.That(e.Pontos, Is.EqualTo(0));
        }

        [Test]
        public void ReiniciarFase_VoltaAoComecoDaFase()
        {
            var e = new EstadoPartida();
            e.ColetarMoeda();
            e.MarcarInicioFase(2);
            int pontos = e.Pontos;
            e.ColetarMoeda();
            e.ColetarItemObrigatorio();
            e.RegistrarExpulsao();
            e.PerderVida();
            e.RestaurarInicioFase();
            Assert.That(e.FaseAtual, Is.EqualTo(2));
            Assert.That(e.Pontos, Is.EqualTo(pontos));
            Assert.That(e.Moedas, Is.EqualTo(1));
            Assert.That(e.ItensColetados, Is.EqualTo(0));
            Assert.That(e.Expulsos, Is.EqualTo(0));
            Assert.That(e.Vidas, Is.EqualTo(Regras.VidasIniciais));
        }

        [Test]
        public void ReiniciarFase_GaranteAoMenosUmaVida()
        {
            var e = new EstadoPartida();
            e.ZerarVidas();
            e.MarcarInicioFase(1);
            e.RestaurarInicioFase();
            Assert.That(e.Vidas, Is.EqualTo(1));
        }

        [Test]
        public void Finalizar_DetectaNovoRecorde()
        {
            var e = new EstadoPartida();
            e.SomarPontos(500);
            Assert.That(e.Finalizar(ResultadoJogo.Vitoria, 400), Is.True);
            Assert.That(e.Resultado, Is.EqualTo(ResultadoJogo.Vitoria));
            Assert.That(e.NovoRecorde, Is.True);

            var outro = new EstadoPartida();
            outro.SomarPontos(300);
            Assert.That(outro.Finalizar(ResultadoJogo.Derrota, 400), Is.False);
            Assert.That(outro.Resultado, Is.EqualTo(ResultadoJogo.Derrota));
        }

        [Test]
        public void NovoJogo_ApagaOResultadoAnterior()
        {
            var e = new EstadoPartida();
            e.SomarPontos(10);
            e.Finalizar(ResultadoJogo.Derrota, 0);
            e.NovoJogo();
            Assert.That(e.Resultado, Is.EqualTo(ResultadoJogo.Nenhum));
            Assert.That(e.NovoRecorde, Is.False);
            Assert.That(e.Pontos, Is.EqualTo(0));
        }
    }
}
