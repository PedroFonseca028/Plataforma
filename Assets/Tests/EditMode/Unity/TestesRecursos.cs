using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace RumoAoHexa.Testes
{
    /// <summary>
    /// Confere, dentro do Unity, se tudo o que o jogo carrega existe: sprites,
    /// animações, mapas, cenas na Build, camadas de física, sons e textos.
    /// </summary>
    public class TestesRecursos
    {
        static readonly string[] Temas = { "praia", "estadio" };

        static IEnumerable<string> SpritesUsadosPeloJogo()
        {
            var nomes = new List<string>
            {
                "bola", "bola_ui", "rastro", "confete", "cartao", "seta", "anel", "luz_suave", "taca", "placar",
                "bandeirinha_desligada", "tontura_0", "tontura_1", "moeda_0",
                "ui_coracao_cheio", "ui_coracao_vazio", "ui_botao", "ui_painel", "ui_branco", "praia_prancha",
                "cabeceador_pulo_0", "cabeceador_pulo_1", "cabeceador_pulo_2",
                "fundo_praia_ceu", "fundo_praia_nuvens", "fundo_praia_morros", "fundo_praia_mar", "fundo_praia_orla",
                "fundo_estadio_ceu", "fundo_estadio_refletores", "fundo_estadio_torcida_0", "fundo_estadio_torcida_1",
                "fundo_estadio_placas",
            };
            for (int d = 0; d <= 9; d++)
                nomes.Add("led_" + d);
            foreach (string tema in Temas)
            {
                for (int m = 0; m < 16; m++)
                    nomes.Add(tema + "_chao_" + m);
                nomes.Add(tema + "_chao_15b");
                nomes.Add(tema + "_chao_15c");
                foreach (string p in new[] { "e", "m", "d", "u" })
                    nomes.Add(tema + "_plataforma_" + p);
            }
            for (int m = 0; m < 16; m++)
                nomes.Add("estadio_degrau_" + m);
            nomes.Add("estadio_degrau_15b");
            nomes.Add("estadio_degrau_15c");
            nomes.AddRange(ConstrutorFase.DecoPraia);
            nomes.AddRange(ConstrutorFase.DecoEstadio);
            foreach (char c in Legenda.Decoracoes)
            {
                nomes.Add(ConstrutorFase.DecoracaoGrande(Tema.Praia, c));
                nomes.Add(ConstrutorFase.DecoracaoGrande(Tema.Estadio, c));
            }
            return nomes;
        }

        [Test]
        public void TodosOsSpritesUsados_Existem()
        {
            var faltando = new List<string>();
            foreach (string nome in SpritesUsadosPeloJogo())
                if (!Arte.Existe(nome))
                    faltando.Add(nome);
            Assert.That(faltando, Is.Empty, "sprites faltando: " + string.Join(", ", faltando.ToArray()));
        }

        [TestCase("craque_parado", 4)]
        [TestCase("craque_correr", 6)]
        [TestCase("craque_pulo", 2)]
        [TestCase("craque_queda", 2)]
        [TestCase("craque_chute", 3)]
        [TestCase("craque_carregar", 2)]
        [TestCase("craque_embaixadinha", 4)]
        [TestCase("craque_dano", 1)]
        [TestCase("craque_vitoria", 2)]
        [TestCase("zagueiro_andar", 4)]
        [TestCase("zagueiro_tonto", 2)]
        [TestCase("zagueiro_expulso", 1)]
        [TestCase("lateral_andar", 4)]
        [TestCase("lateral_correr", 4)]
        [TestCase("lateral_tonto", 2)]
        [TestCase("lateral_expulso", 1)]
        [TestCase("cabeceador_andar", 4)]
        [TestCase("cabeceador_pulo", 3)]
        [TestCase("cabeceador_tonto", 2)]
        [TestCase("cabeceador_expulso", 1)]
        [TestCase("moeda", 6)]
        [TestCase("estrela", 6)]
        [TestCase("letra_h", 4)]
        [TestCase("letra_e", 4)]
        [TestCase("letra_x", 4)]
        [TestCase("letra_a", 4)]
        [TestCase("coracao", 4)]
        [TestCase("bandeirinha_ligada", 3)]
        [TestCase("brilho", 4)]
        [TestCase("poeira", 4)]
        [TestCase("fumaca", 4)]
        [TestCase("praia_espinho", 2)]
        [TestCase("estadio_espinho", 3)]
        [TestCase("praia_liquido_topo", 4)]
        [TestCase("praia_liquido_fundo", 4)]
        [TestCase("estadio_liquido_topo", 4)]
        [TestCase("estadio_liquido_fundo", 4)]
        [TestCase("fundo_estadio_torcida", 2)]
        public void Animacao_TemTodosOsQuadros(string nome, int quadros)
        {
            Sprite[] q = Arte.Anim(nome);
            Assert.That(q.Length, Is.EqualTo(quadros), "animação " + nome);
            foreach (Sprite s in q)
                Assert.That(s, Is.Not.Null);
        }

        [Test]
        public void Personagens_Tem32PixelsE16PixelsPorUnidade()
        {
            Sprite s = Arte.Sprite("craque_parado_0");
            Assert.That(s.rect.width, Is.EqualTo(32f));
            Assert.That(s.rect.height, Is.EqualTo(32f));
            Assert.That(s.bounds.size.y, Is.EqualTo(2f).Within(0.001f), "32 px a 16 px por unidade = 2 unidades");
        }

        [TestCase("Fase1", 1)]
        [TestCase("Fase2", 2)]
        public void MapaCarregadoPeloResources_EhValido(string arquivo, int fase)
        {
            TextAsset t = Resources.Load<TextAsset>("Fases/" + arquivo);
            Assert.That(t, Is.Not.Null, "Resources/Fases/" + arquivo + ".txt");
            MapaFase m = MapaFase.Ler(t.text);
            Assert.That(m.Validar(Regras.ItensObrigatorios(fase)), Is.Empty);
        }

        [Test]
        public void AsQuatroCenas_EstaoNaBuild_NaOrdemCerta()
        {
            var esperado = new[] { "Menu", "Fase1", "Fase2", "FimDeJogo" };
            EditorBuildSettingsScene[] cenas = EditorBuildSettings.scenes;
            Assert.That(cenas.Length, Is.GreaterThanOrEqualTo(esperado.Length));
            for (int i = 0; i < esperado.Length; i++)
            {
                Assert.That(cenas[i].path, Is.EqualTo("Assets/Scenes/" + esperado[i] + ".unity"));
                Assert.That(cenas[i].enabled, Is.True, esperado[i] + " precisa estar marcada na Build");
            }
        }

        [Test]
        public void CamadasDeFisica_TemOsNomesCertos()
        {
            Assert.That(LayerMask.LayerToName(Camadas.Chao), Is.EqualTo("Chao"));
            Assert.That(LayerMask.LayerToName(Camadas.Jogador), Is.EqualTo("Jogador"));
            Assert.That(LayerMask.LayerToName(Camadas.Inimigo), Is.EqualTo("Inimigo"));
            Assert.That(LayerMask.LayerToName(Camadas.Item), Is.EqualTo("Item"));
            Assert.That(LayerMask.LayerToName(Camadas.Perigo), Is.EqualTo("Perigo"));
            Assert.That(LayerMask.LayerToName(Camadas.Plataforma), Is.EqualTo("Plataforma"));
            Assert.That(LayerMask.LayerToName(Camadas.Bola), Is.EqualTo("Bola"));
        }

        [Test]
        public void Bola_QuicaNoChao_MasNaoNoJogador()
        {
            Camadas.ConfigurarColisoes();
            Assert.That(Physics2D.GetIgnoreLayerCollision(Camadas.Bola, Camadas.Chao), Is.False);
            Assert.That(Physics2D.GetIgnoreLayerCollision(Camadas.Bola, Camadas.Inimigo), Is.False);
            Assert.That(Physics2D.GetIgnoreLayerCollision(Camadas.Bola, Camadas.Jogador), Is.True);
            Assert.That(Physics2D.GetIgnoreLayerCollision(Camadas.Bola, Camadas.Item), Is.True);
        }

        [TestCase("Fase1", "arquivoFase: Fase1", "tema: 0", "proximaCena: Fase2")]
        [TestCase("Fase2", "arquivoFase: Fase2", "tema: 1", "proximaCena: \n")]
        public void CenaDaFase_ConfiguraOConstrutor(string cena, string arquivo, string tema, string proxima)
        {
            string texto = File.ReadAllText("Assets/Scenes/" + cena + ".unity").Replace("\r\n", "\n");
            Assert.That(texto, Does.Contain(arquivo));
            Assert.That(texto, Does.Contain(tema));
            Assert.That(texto, Does.Contain(proxima));
        }

        [Test]
        public void TodosOsSons_SaoGerados()
        {
            foreach (Som s in System.Enum.GetValues(typeof(Som)))
            {
                AudioClip c = Sintetizador.CriarEfeito(s);
                Assert.That(c, Is.Not.Null, s.ToString());
                Assert.That(c.length, Is.GreaterThan(0f), s.ToString());
                Object.DestroyImmediate(c);
            }
        }

        [Test]
        public void TodasAsMusicas_SaoGeradas()
        {
            foreach (Trilha t in System.Enum.GetValues(typeof(Trilha)))
            {
                if (t == Trilha.Nenhuma)
                    continue;
                AudioClip c = Sintetizador.CriarMusica(t);
                Assert.That(c, Is.Not.Null, t.ToString());
                Assert.That(c.length, Is.GreaterThan(1f), t.ToString());
                Object.DestroyImmediate(c);
            }
        }

        [Test]
        public void FontePixel_TemTodasAsLetrasDosTextos()
        {
            foreach (string texto in Textos.TodosOsTextos())
            {
                foreach (char c in texto)
                {
                    if (c == ' ' || c == '\n')
                        continue;
                    FontePixel.Glifo g;
                    Assert.That(FontePixel.TentarGlifo(FontePixel.Normalizar(c), out g), Is.True, "sem glifo para '" + c + "' em " + texto);
                }
            }
        }
    }
}
