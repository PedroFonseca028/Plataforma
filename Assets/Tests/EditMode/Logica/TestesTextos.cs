using NUnit.Framework;

namespace RumoAoHexa.Testes
{
    public class TestesTextos
    {
        [Test]
        public void TodosOsTextos_TemLetraNaFontePixelada()
        {
            foreach (string texto in Textos.TodosOsTextos())
            {
                char faltando;
                Assert.That(GlifosFonte.TextoSuportado(texto, out faltando), Is.True, "a fonte não tem o caractere '" + faltando + "' usado em: " + texto);
            }
        }

        [Test]
        public void RotulosDosItens_AnosELetras()
        {
            Assert.That(Regras.RotuloItem(1, 0), Is.EqualTo("1958"));
            Assert.That(Regras.RotuloItem(1, 4), Is.EqualTo("2002"));
            Assert.That(Regras.RotuloItem(2, 0), Is.EqualTo("H"));
            Assert.That(Regras.RotuloItem(2, 3), Is.EqualTo("A"));
            Assert.That(Regras.RotuloItem(2, 9), Is.EqualTo(""));
            Assert.That(Regras.TotalItensDoJogo, Is.EqualTo(9));
        }

        [Test]
        public void FrasesDeItem_TemAnoELetra()
        {
            Assert.That(Narrador.FraseItem(1, "1970", 2), Is.EqualTo("ESTRELA DE 1970 RECUPERADA!  FALTAM 2"));
            Assert.That(Narrador.FraseItem(2, "X", 1), Is.EqualTo("LETRA X!  FALTA 1"));
            Assert.That(Narrador.FraseItem(2, "A", 0), Is.EqualTo("LETRA A!"));
        }

        [Test]
        public void Narrador_EscolheFraseDoTipoDeChute()
        {
            for (int s = -20; s < 20; s++)
            {
                Assert.That(Narrador.FrasesCavadinha, Has.Member(Narrador.FraseExpulsao(TipoChute.Cavadinha, s, false)));
                Assert.That(Narrador.FrasesFolhaSeca, Has.Member(Narrador.FraseExpulsao(TipoChute.FolhaSeca, s, false)));
                Assert.That(Narrador.FrasesRasteiro, Has.Member(Narrador.FraseExpulsao(TipoChute.Rasteiro, s, false)));
            }
        }

        [Test]
        public void Narrador_NoEstadioTambemGritaGol()
        {
            bool gritouGol = false;
            for (int s = 0; s < 10; s++)
                if (System.Array.IndexOf(Narrador.FrasesEstadio, Narrador.FraseExpulsao(TipoChute.Rasteiro, s, true)) >= 0)
                    gritouGol = true;
            Assert.That(gritouGol, Is.True);
        }

        [Test]
        public void Placar_ContaGolsDosDoisLados()
        {
            var p = new PlacarFase();
            Assert.That(p.Texto, Is.EqualTo("BRA 0 X 0 ARG"));
            p.GolBrasil();
            p.GolBrasil();
            p.GolArgentina();
            Assert.That(p.Brasil, Is.EqualTo(2));
            Assert.That(p.Argentina, Is.EqualTo(1));
            Assert.That(p.Texto, Is.EqualTo("BRA 2 X 1 ARG"));
        }

        [Test]
        public void Glifos_NormalizaMinusculasEAcentos()
        {
            Assert.That(GlifosFonte.Normalizar('a'), Is.EqualTo('A'));
            Assert.That(GlifosFonte.Normalizar('ç'), Is.EqualTo('Ç'));
            Assert.That(GlifosFonte.Normalizar('è'), Is.EqualTo('É'));
            Assert.That(GlifosFonte.Normalizar('€'), Is.EqualTo('?'));
        }

        [Test]
        public void Glifos_TodosTemDezLinhas()
        {
            foreach (var par in GlifosFonte.Todos())
                Assert.That(par.Value.Length, Is.EqualTo(GlifosFonte.AlturaGlifo), "glifo " + par.Key);
        }
    }
}
