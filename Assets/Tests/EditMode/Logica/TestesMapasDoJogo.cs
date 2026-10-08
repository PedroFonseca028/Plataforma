using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace RumoAoHexa.Testes
{
    /// <summary>Confere os mapas de verdade (Assets/Resources/Fases): formato, itens e se dá para alcançar tudo.</summary>
    public class TestesMapasDoJogo
    {
        /// <summary>Acha a pasta do projeto subindo a partir da pasta atual (funciona no Unity e no dotnet test).</summary>
        public static string PastaDoProjeto()
        {
            // RUMOAOHEXA_PROJETO (opcional) aponta a pasta do projeto quando os testes rodam de outro lugar
            string informada = Environment.GetEnvironmentVariable("RUMOAOHEXA_PROJETO");
            foreach (string inicio in new[] { informada, Directory.GetCurrentDirectory(), AppDomain.CurrentDomain.BaseDirectory })
            {
                if (string.IsNullOrEmpty(inicio))
                    continue;
                DirectoryInfo d = new DirectoryInfo(inicio);
                while (d != null)
                {
                    if (Directory.Exists(Path.Combine(d.FullName, "Assets", "Resources", "Fases")))
                        return d.FullName;
                    d = d.Parent;
                }
            }
            throw new DirectoryNotFoundException("Pasta Assets/Resources/Fases não encontrada.");
        }

        static MapaFase Carregar(string nome)
        {
            string caminho = Path.Combine(PastaDoProjeto(), "Assets", "Resources", "Fases", nome + ".txt");
            return MapaFase.Ler(File.ReadAllText(caminho));
        }

        static string Listar(List<Celula> celulas)
        {
            var partes = new List<string>();
            foreach (Celula c in celulas)
                partes.Add(c.ToString());
            return string.Join(", ", partes.ToArray());
        }

        [TestCase("Fase1", 1)]
        [TestCase("Fase2", 2)]
        public void Mapa_EhValido(string nome, int fase)
        {
            MapaFase m = Carregar(nome);
            List<string> problemas = m.Validar(Regras.ItensObrigatorios(fase));
            Assert.That(problemas, Is.Empty, string.Join("\n", problemas.ToArray()));
        }

        [TestCase("Fase1", 1)]
        [TestCase("Fase2", 2)]
        public void Mapa_TemOsItensObrigatoriosCertos(string nome, int fase)
        {
            MapaFase m = Carregar(nome);
            Assert.That(m.ItensObrigatoriosEmOrdem().Count, Is.EqualTo(Regras.ItensObrigatorios(fase)));
        }

        [TestCase("Fase1")]
        [TestCase("Fase2")]
        public void TodosOsItensObrigatorios_SaoAlcancaveis(string nome)
        {
            MapaFase m = Carregar(nome);
            List<Celula> fora = AnalisadorMapa.Inalcancaveis(m, Legenda.ItemObrigatorio);
            Assert.That(fora, Is.Empty, "itens fora do alcance: " + Listar(fora));
        }

        [TestCase("Fase1")]
        [TestCase("Fase2")]
        public void CoracoesBandeirinhasEMoedas_SaoAlcancaveis(string nome)
        {
            MapaFase m = Carregar(nome);
            foreach (char tipo in new[] { Legenda.Coracao, Legenda.Bandeirinha, Legenda.Moeda })
            {
                List<Celula> fora = AnalisadorMapa.Inalcancaveis(m, tipo);
                Assert.That(fora, Is.Empty, "fora do alcance: " + Listar(fora));
            }
        }

        [TestCase("Fase1")]
        [TestCase("Fase2")]
        public void ODaFase_EhAlcancavel(string nome)
        {
            // o final da fase (última coluna com chão) precisa ser alcançável
            MapaFase m = Carregar(nome);
            HashSet<long> alc = AnalisadorMapa.PosicoesAlcancaveis(m);
            bool chegou = false;
            for (int y = 0; y < m.Altura && !chegou; y++)
                chegou = AnalisadorMapa.AlcancaCelula(m, alc, m.Largura - 3, y) && AnalisadorMapa.PodeFicar(m, m.Largura - 3, y);
            Assert.That(chegou, Is.True, "o jogador não consegue chegar ao fim da fase");
        }

        [TestCase("Fase1")]
        [TestCase("Fase2")]
        public void Inimigos_EstaoEmCimaDoChao(string nome)
        {
            MapaFase m = Carregar(nome);
            List<Celula> soltos = AnalisadorMapa.InimigosFlutuando(m);
            Assert.That(soltos, Is.Empty, "inimigos sem chão: " + Listar(soltos));
        }

        [TestCase("Fase1")]
        [TestCase("Fase2")]
        public void Fase_TemOsTresTiposDeArgentino_EPeloMenosUmCoracaoEUmaBandeirinha(string nome)
        {
            MapaFase m = Carregar(nome);
            Assert.That(m.Contar(Legenda.Zagueiro), Is.GreaterThan(0));
            Assert.That(m.Contar(Legenda.Lateral), Is.GreaterThan(0));
            Assert.That(m.Contar(Legenda.Cabeceador), Is.GreaterThan(0));
            Assert.That(m.Contar(Legenda.Coracao), Is.GreaterThan(0));
            Assert.That(m.Contar(Legenda.Bandeirinha), Is.GreaterThan(0));
            Assert.That(m.Contar(Legenda.Moeda), Is.GreaterThanOrEqualTo(30), "a fase precisa de bastante moeda");
        }

        [TestCase("Fase1")]
        [TestCase("Fase2")]
        public void NenhumInimigoComecaPertoDoInicio(string nome)
        {
            MapaFase m = Carregar(nome);
            Celula p = m.Encontrar(Legenda.Inicio)[0];
            for (int y = 0; y < m.Altura; y++)
                for (int x = 0; x < m.Largura; x++)
                    if (Legenda.EhInimigo(m.Em(x, y)))
                        Assert.That(Math.Abs(x - p.X), Is.GreaterThan(8), "inimigo muito perto do início em (" + x + ", " + y + ")");
        }

        [Test]
        public void Fase2_EhMaiorQueAFase1()
        {
            Assert.That(Carregar("Fase2").Largura, Is.GreaterThan(Carregar("Fase1").Largura));
        }
    }
}
