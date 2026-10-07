using NUnit.Framework;

namespace RumoAoHexa.Testes
{
    public class TestesEstadoBola
    {
        const float Dt = 1f / 50f;

        [Test]
        public void ComecaNoPe_EPodeChutar()
        {
            var b = new EstadoBola();
            Assert.That(b.Situacao, Is.EqualTo(SituacaoBola.NoPe));
            Assert.That(b.PodeChutar, Is.True);
            Assert.That(b.PodeAcertar, Is.False);
        }

        [Test]
        public void Chutar_ColocaEmVoo_ENaoDeixaChutarDeNovo()
        {
            var b = new EstadoBola();
            Assert.That(b.Chutar(TipoChute.Rasteiro), Is.True);
            Assert.That(b.Situacao, Is.EqualTo(SituacaoBola.Voando));
            Assert.That(b.PodeAcertar, Is.True);
            Assert.That(b.Chutar(TipoChute.Cavadinha), Is.False);
            Assert.That(b.UltimoChute, Is.EqualTo(TipoChute.Rasteiro));
        }

        [Test]
        public void VoltaSozinha_DepoisDoTempoMaximo()
        {
            var b = new EstadoBola();
            b.Chutar(TipoChute.FolhaSeca);
            float limite = Chutes.Parametros(TipoChute.FolhaSeca).TempoMaximo;
            float t = 0f;
            while (t < limite - Dt)
            {
                b.AtualizarVoo(Dt, 15f, 10f, false);
                t += Dt;
                Assert.That(b.Situacao, Is.EqualTo(SituacaoBola.Voando));
            }
            b.AtualizarVoo(Dt * 2f, 15f, 10f, false);
            Assert.That(b.Situacao, Is.EqualTo(SituacaoBola.Voltando));
            Assert.That(b.PodeAcertar, Is.False, "voltando a bola não expulsa ninguém");
        }

        [Test]
        public void VoltaSozinha_QuandoParaNoChao()
        {
            var b = new EstadoBola();
            b.Chutar(TipoChute.Rasteiro);
            for (int i = 0; i < 30; i++)
                b.AtualizarVoo(Dt, 0.1f, 10f, false);
            Assert.That(b.Situacao, Is.EqualTo(SituacaoBola.Voltando));
        }

        [Test]
        public void VoltaSozinha_QuandoCaiDoMapa()
        {
            var b = new EstadoBola();
            b.Chutar(TipoChute.Cavadinha);
            b.AtualizarVoo(Dt, 10f, 10f, true);
            Assert.That(b.Situacao, Is.EqualTo(SituacaoBola.Voltando));
        }

        [Test]
        public void Chamar_FazVoltar()
        {
            var b = new EstadoBola();
            b.Chutar(TipoChute.Rasteiro);
            b.Chamar();
            Assert.That(b.Situacao, Is.EqualTo(SituacaoBola.Voltando));
        }

        [Test]
        public void ChamarComBolaNoPe_NaoFazNada()
        {
            var b = new EstadoBola();
            b.Chamar();
            Assert.That(b.Situacao, Is.EqualTo(SituacaoBola.NoPe));
        }

        [Test]
        public void Voltando_EhDominadaAoChegarNoPe()
        {
            var b = new EstadoBola();
            b.Chutar(TipoChute.Rasteiro);
            b.Chamar();
            b.AtualizarVolta(Dt, 3f);
            Assert.That(b.Situacao, Is.EqualTo(SituacaoBola.Voltando));
            b.AtualizarVolta(Dt, EstadoBola.DistanciaChegada * 0.5f);
            Assert.That(b.Situacao, Is.EqualTo(SituacaoBola.NoPe));
            Assert.That(b.PodeChutar, Is.True);
        }

        [Test]
        public void BolaLentaPertoDoPe_EhDominada()
        {
            var b = new EstadoBola();
            b.Chutar(TipoChute.Rasteiro);
            b.AtualizarVoo(EstadoBola.TempoMinimoParaDominar + Dt, 2f, 0.3f, false);
            Assert.That(b.Situacao, Is.EqualTo(SituacaoBola.NoPe));
        }

        [Test]
        public void LogoDepoisDoChute_NaoDominaSemQuerer()
        {
            var b = new EstadoBola();
            b.Chutar(TipoChute.Rasteiro);
            b.AtualizarVoo(Dt, 2f, 0.3f, false);
            Assert.That(b.Situacao, Is.EqualTo(SituacaoBola.Voando));
        }

        [Test]
        public void BolaRapidaPertoDoPe_NaoEhDominada()
        {
            var b = new EstadoBola();
            b.Chutar(TipoChute.Rasteiro);
            b.AtualizarVoo(EstadoBola.TempoMinimoParaDominar + Dt, 12f, 0.3f, false);
            Assert.That(b.Situacao, Is.EqualTo(SituacaoBola.Voando));
        }

        [Test]
        public void NovoChute_ZeraOCombo()
        {
            var b = new EstadoBola();
            b.Chutar(TipoChute.FolhaSeca);
            b.Combo.RegistrarAcerto();
            b.Combo.RegistrarAcerto();
            b.Dominar();
            b.Chutar(TipoChute.Rasteiro);
            Assert.That(b.Combo.Acertos, Is.EqualTo(0));
        }

        [Test]
        public void Atravessa_SoNaFolhaSecaEmVoo()
        {
            var b = new EstadoBola();
            b.Chutar(TipoChute.FolhaSeca);
            Assert.That(b.Atravessa, Is.True);
            b.Chamar();
            Assert.That(b.Atravessa, Is.False);
        }

        [Test]
        public void VelocidadeDeVolta_AumentaComOTempo_ComLimite()
        {
            Assert.That(EstadoBola.VelocidadeVolta(0.5f), Is.GreaterThan(EstadoBola.VelocidadeVolta(0f)));
            Assert.That(EstadoBola.VelocidadeVolta(100f), Is.EqualTo(34f));
        }
    }
}
