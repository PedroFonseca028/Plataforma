using System.Collections.Generic;
using NUnit.Framework;

namespace RumoAoHexa.Testes
{
    public class TestesMapaFase
    {
        static MapaFase Mapa(params string[] linhas)
        {
            return MapaFase.Ler(string.Join("\n", linhas));
        }

        [Test]
        public void Ler_IgnoraComentarios_EAPrimeiraLinhaEhOTopo()
        {
            MapaFase m = Mapa(
                "// legenda",
                "..*.",
                ".P..",
                "####",
                "");
            Assert.That(m.Largura, Is.EqualTo(4));
            Assert.That(m.Altura, Is.EqualTo(3));
            Assert.That(m.Em(0, 0), Is.EqualTo('#'));
            Assert.That(m.Em(1, 1), Is.EqualTo('P'));
            Assert.That(m.Em(2, 2), Is.EqualTo('*'));
        }

        [Test]
        public void Ler_AceitaFinalDeLinhaDoWindows_ELinhasCurtas()
        {
            MapaFase m = MapaFase.Ler("..\r\n.P..\r\n####\r\n");
            Assert.That(m.Largura, Is.EqualTo(4));
            Assert.That(m.Em(3, 2), Is.EqualTo('.'), "linha curta é completada com vazio");
            Assert.That(m.Em(1, 1), Is.EqualTo('P'));
        }

        [Test]
        public void ForaDoMapa_EhVazio_MasOChaoContinuaNasBordas()
        {
            MapaFase m = Mapa("..", "##");
            Assert.That(m.Em(-1, 0), Is.EqualTo('.'));
            Assert.That(m.EhChao(-1, 0), Is.True, "lateral esquerda continua sendo chão");
            Assert.That(m.EhChao(0, -1), Is.True, "abaixo do mapa é chão");
            Assert.That(m.EhChao(0, 5), Is.False, "acima do mapa é ar");
        }

        [Test]
        public void Mascara_IndicaOsVizinhosComChao()
        {
            MapaFase m = Mapa(
                ".....",
                "..#..",
                ".###.",
                "#####");
            // o tile do topo (2, 2) só tem chão embaixo
            Assert.That(m.Mascara(2, 2), Is.EqualTo(4));
            // (2, 1): chão em cima, embaixo e dos dois lados
            Assert.That(m.Mascara(2, 1), Is.EqualTo(1 | 2 | 4 | 8));
            // (1, 1): sem chão em cima nem à esquerda
            Assert.That(m.Mascara(1, 1), Is.EqualTo(2 | 4));
        }

        [Test]
        public void TipoPlataforma_PontasEMeio()
        {
            MapaFase m = Mapa("===..=", "######");
            Assert.That(m.TipoPlataforma(0, 1), Is.EqualTo("e"));
            Assert.That(m.TipoPlataforma(1, 1), Is.EqualTo("m"));
            Assert.That(m.TipoPlataforma(2, 1), Is.EqualTo("d"));
            Assert.That(m.TipoPlataforma(5, 1), Is.EqualTo("u"));
        }

        [Test]
        public void Hash_EhSempreOMesmoENaoNegativo()
        {
            Assert.That(MapaFase.Hash(10, 3), Is.EqualTo(MapaFase.Hash(10, 3)));
            for (int x = -50; x < 50; x++)
                Assert.That(MapaFase.Hash(x, x * 7), Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void ItensEmOrdem_DaEsquerdaParaADireita()
        {
            MapaFase m = Mapa(
                "....*.",
                "*.P..*",
                "######");
            List<Celula> itens = m.ItensObrigatoriosEmOrdem();
            Assert.That(itens.Count, Is.EqualTo(3));
            Assert.That(itens[0].X, Is.EqualTo(0));
            Assert.That(itens[1].X, Is.EqualTo(4));
            Assert.That(itens[2].X, Is.EqualTo(5));
        }

        [Test]
        public void Validar_MapaCorreto_NaoTemProblemas()
        {
            MapaFase m = Mapa(
                "...*..",
                ".P..o.",
                "######");
            Assert.That(m.Validar(1), Is.Empty);
        }

        [Test]
        public void Validar_ApontaFaltaDeInicio_ItensErrados_ECaractereEstranho()
        {
            MapaFase m = Mapa(
                "..@...",
                "......",
                "######");
            List<string> problemas = m.Validar(2);
            Assert.That(problemas.Count, Is.EqualTo(3), string.Join(" | ", problemas.ToArray()));
        }

        [Test]
        public void Validar_PlataformaMovelSemLimites()
        {
            MapaFase m = Mapa(
                ".P....*",
                "###M###",
                "###%###");
            List<string> problemas = m.Validar(1);
            Assert.That(problemas.Count, Is.EqualTo(1), string.Join(" | ", problemas.ToArray()));
        }

        [Test]
        public void LimitesPlataformaMovel_AchaOsMarcadores()
        {
            MapaFase m = Mapa(".P..........", "##<M.....>##");
            int e, d;
            m.LimitesPlataformaMovel(3, 0, out e, out d);
            Assert.That(e, Is.EqualTo(2));
            Assert.That(d, Is.EqualTo(9));
        }

        [Test]
        public void Analisador_PuloDeTresTilesAlcanca_DeQuatroNao()
        {
            MapaFase alcanca = Mapa(
                ".....*",
                ".....#",
                ".....#",
                ".P...#",
                "######");
            Assert.That(AnalisadorMapa.Inalcancaveis(alcanca, Legenda.ItemObrigatorio), Is.Empty);

            MapaFase alto = Mapa(
                "...........*",
                "...........#",
                "...........#",
                "...........#",
                "...........#",
                "...........#",
                "...........#",
                ".P.........#",
                "############");
            Assert.That(AnalisadorMapa.Inalcancaveis(alto, Legenda.ItemObrigatorio).Count, Is.EqualTo(1));
        }

        [Test]
        public void Analisador_ItemQuatroTilesAcima_Alcanca_SeisNao()
        {
            MapaFase quatro = Mapa(
                ".....*....",
                "..........",
                "..........",
                "..........",
                ".P........",
                "##########");
            Assert.That(AnalisadorMapa.Inalcancaveis(quatro, Legenda.ItemObrigatorio), Is.Empty);

            MapaFase seis = Mapa(
                ".....*....",
                "..........",
                "..........",
                "..........",
                "..........",
                "..........",
                ".P........",
                "##########");
            Assert.That(AnalisadorMapa.Inalcancaveis(seis, Legenda.ItemObrigatorio).Count, Is.EqualTo(1));
        }

        [Test]
        public void Analisador_BuracoDeQuatroPula_DeSeisNao()
        {
            MapaFase quatro = Mapa(
                ".P..........*.",
                "#####....#####");
            Assert.That(AnalisadorMapa.Inalcancaveis(quatro, Legenda.ItemObrigatorio), Is.Empty);

            MapaFase seis = Mapa(
                ".P............*.",
                "#####......#####");
            Assert.That(AnalisadorMapa.Inalcancaveis(seis, Legenda.ItemObrigatorio).Count, Is.EqualTo(1));
        }

        [Test]
        public void Analisador_PlataformaMovelAtravessaOMarLargo()
        {
            MapaFase m = Mapa(
                ".P.................*.",
                "###<M............>###",
                "###~~~~~~~~~~~~~~~###",
                "#####################");
            Assert.That(AnalisadorMapa.Inalcancaveis(m, Legenda.ItemObrigatorio), Is.Empty);
        }

        [Test]
        public void Analisador_NaoAtravessaParede()
        {
            MapaFase m = Mapa(
                "....#...",
                "....#...",
                "....#...",
                "....#...",
                "....#...",
                ".P..#.*.",
                "########");
            Assert.That(AnalisadorMapa.Inalcancaveis(m, Legenda.ItemObrigatorio).Count, Is.EqualTo(1));
        }

        [Test]
        public void Analisador_DetectaInimigoFlutuando()
        {
            MapaFase m = Mapa(
                "..z...",
                ".P..z.",
                "######");
            List<Celula> soltos = AnalisadorMapa.InimigosFlutuando(m);
            Assert.That(soltos.Count, Is.EqualTo(1));
            Assert.That(soltos[0].X, Is.EqualTo(2));
        }

        [Test]
        public void Analisador_NaoFicaEmCimaDeEspinho()
        {
            MapaFase m = Mapa(
                "......",
                ".P....",
                "######");
            Assert.That(AnalisadorMapa.PodeFicar(m, 3, 1), Is.True);
            MapaFase comEspinho = Mapa(
                "......",
                "......",
                ".P.^..",
                "######");
            Assert.That(AnalisadorMapa.PodeFicar(comEspinho, 3, 1), Is.False, "dentro do espinho");
            Assert.That(AnalisadorMapa.PodeFicar(comEspinho, 3, 2), Is.False, "em cima do espinho");
        }
    }
}
