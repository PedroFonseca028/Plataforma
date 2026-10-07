using NUnit.Framework;

namespace RumoAoHexa.Testes
{
    public class TestesChutes
    {
        [Test]
        public void ToqueRapido_ChuteRasteiro()
        {
            var c = new ControleChute();
            c.Apertar();
            c.Atualizar(0.05f);
            Assert.That(c.Soltar(false), Is.EqualTo(TipoChute.Rasteiro));
            Assert.That(c.Segurando, Is.False);
        }

        [Test]
        public void ToqueRapidoComCima_Cavadinha()
        {
            var c = new ControleChute();
            c.Apertar();
            c.Atualizar(0.05f);
            Assert.That(c.Soltar(true), Is.EqualTo(TipoChute.Cavadinha));
        }

        [Test]
        public void SegurarAteEncher_FolhaSeca()
        {
            var c = new ControleChute();
            c.Apertar();
            for (int i = 0; i < 60; i++)
                c.Atualizar(1f / 60f);
            Assert.That(c.CargaCheia, Is.True);
            Assert.That(c.Soltar(false), Is.EqualTo(TipoChute.FolhaSeca));
        }

        [Test]
        public void SoltarAntesDeEncher_NaoEhFolhaSeca()
        {
            var c = new ControleChute();
            c.Apertar();
            c.Atualizar(ControleChute.TempoCargaCompleta * 0.7f);
            Assert.That(c.Carregando, Is.True);
            Assert.That(c.Carga, Is.LessThan(1f));
            Assert.That(c.Soltar(false), Is.EqualTo(TipoChute.Rasteiro));
        }

        [Test]
        public void Carga_CresceDeZeroAUm()
        {
            var c = new ControleChute();
            Assert.That(c.Carga, Is.EqualTo(0f));
            c.Apertar();
            c.Atualizar(ControleChute.TempoInicioCarga * 0.5f);
            Assert.That(c.Carga, Is.EqualTo(0f));
            Assert.That(c.Carregando, Is.False);
            c.Atualizar(10f);
            Assert.That(c.Carga, Is.EqualTo(1f));
        }

        [Test]
        public void Cancelar_ZeraACarga()
        {
            var c = new ControleChute();
            c.Apertar();
            c.Atualizar(1f);
            c.Cancelar();
            Assert.That(c.Segurando, Is.False);
            Assert.That(c.Carga, Is.EqualTo(0f));
        }

        [Test]
        public void SoFolhaSecaAtravessa()
        {
            Assert.That(Chutes.Parametros(TipoChute.Rasteiro).Atravessa, Is.False);
            Assert.That(Chutes.Parametros(TipoChute.Cavadinha).Atravessa, Is.False);
            Assert.That(Chutes.Parametros(TipoChute.FolhaSeca).Atravessa, Is.True);
        }

        [Test]
        public void Cavadinha_SobeMaisQueORasteiro_EFolhaSecaEhAMaisRapida()
        {
            ParametrosChute r = Chutes.Parametros(TipoChute.Rasteiro);
            ParametrosChute c = Chutes.Parametros(TipoChute.Cavadinha);
            ParametrosChute f = Chutes.Parametros(TipoChute.FolhaSeca);
            Assert.That(c.VelocidadeY, Is.GreaterThan(r.VelocidadeY));
            Assert.That(f.VelocidadeX, Is.GreaterThan(r.VelocidadeX));
            Assert.That(r.VelocidadeX, Is.GreaterThan(c.VelocidadeX));
            // a cavadinha passa por cima de um argentino (~2 tiles de altura)
            float g = 9.81f * c.Gravidade;
            float alturaMaxima = c.VelocidadeY * c.VelocidadeY / (2f * g);
            Assert.That(alturaMaxima, Is.GreaterThan(3f));
        }

        [Test]
        public void VelocidadeInicial_RespeitaADirecao()
        {
            float vx, vy;
            Chutes.VelocidadeInicial(TipoChute.Rasteiro, 1, 0f, out vx, out vy);
            Assert.That(vx, Is.GreaterThan(0f));
            Assert.That(vy, Is.GreaterThan(0f));
            Chutes.VelocidadeInicial(TipoChute.Rasteiro, -1, 0f, out vx, out vy);
            Assert.That(vx, Is.LessThan(0f));
        }

        [Test]
        public void VelocidadeInicial_CorrendoParaOMesmoLadoChutaMaisForte()
        {
            float parado, correndo, contra, vy;
            Chutes.VelocidadeInicial(TipoChute.Rasteiro, 1, 0f, out parado, out vy);
            Chutes.VelocidadeInicial(TipoChute.Rasteiro, 1, 7f, out correndo, out vy);
            Chutes.VelocidadeInicial(TipoChute.Rasteiro, 1, -7f, out contra, out vy);
            Assert.That(correndo, Is.GreaterThan(parado));
            Assert.That(contra, Is.EqualTo(parado).Within(0.0001f), "correr para trás não enfraquece o chute");
        }

        [Test]
        public void Combo_DobleteHatTrickGoleada()
        {
            var combo = new ContadorCombo();
            Assert.That(combo.RegistrarAcerto(), Is.EqualTo(0));
            Assert.That(combo.Nome, Is.Null);
            Assert.That(combo.RegistrarAcerto(), Is.EqualTo(100));
            Assert.That(combo.Nome, Is.EqualTo("DOBLETE!"));
            Assert.That(combo.RegistrarAcerto(), Is.EqualTo(200));
            Assert.That(combo.Nome, Is.EqualTo("HAT-TRICK!"));
            Assert.That(combo.RegistrarAcerto(), Is.EqualTo(200));
            Assert.That(combo.Nome, Is.EqualTo("GOLEADA!"));
            Assert.That(100 + 200 + 200, Is.EqualTo(Regras.BonusCombo(4)));
            combo.Reiniciar();
            Assert.That(combo.Acertos, Is.EqualTo(0));
        }

        [Test]
        public void BonusCombo_NuncaDiminui()
        {
            for (int n = 1; n < 10; n++)
                Assert.That(Regras.BonusCombo(n + 1), Is.GreaterThanOrEqualTo(Regras.BonusCombo(n)));
        }
    }
}
