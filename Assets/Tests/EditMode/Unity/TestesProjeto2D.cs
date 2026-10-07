using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace RumoAoHexa.Testes
{
    /// <summary>
    /// Garante que o projeto é 2D de ponta a ponta (editor, renderização,
    /// câmeras, física e sprites) e que está configurado para o Unity 6000.5.10f1.
    /// </summary>
    public class TestesProjeto2D
    {
        static readonly string[] CenasDoJogo = { "Menu", "Fase1", "Fase2", "FimDeJogo" };
        const string GuidRenderer2D = "424799608f7334c24bf367e4bbfa7f9a";

        [Test]
        public void Editor_EstaEmModo2D()
        {
            Assert.That(EditorSettings.defaultBehaviorMode, Is.EqualTo(EditorBehaviorMode.Mode2D));
        }

        [Test]
        public void ProjetoEhDaVersao6000_5_10f1()
        {
            string versao = File.ReadAllText("ProjectSettings/ProjectVersion.txt");
            Assert.That(versao, Does.Contain("m_EditorVersion: 6000.5.10f1"));
        }

        [Test]
        public void Renderizacao_UsaURPComRenderer2D()
        {
            RenderPipelineAsset padrao = GraphicsSettings.defaultRenderPipeline;
            Assert.That(padrao, Is.Not.Null, "Project Settings > Graphics precisa do URP");
            Assert.That(padrao.GetType().Name, Is.EqualTo("UniversalRenderPipelineAsset"));
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                RenderPipelineAsset nivel = QualitySettings.GetRenderPipelineAssetAt(i);
                Assert.That(nivel, Is.Not.Null, "nível de qualidade " + QualitySettings.names[i] + " sem URP");
                Assert.That(nivel.GetType().Name, Is.EqualTo("UniversalRenderPipelineAsset"));
            }

            Object renderer = AssetDatabase.LoadMainAssetAtPath("Assets/Settings/Renderer2D.asset");
            Assert.That(renderer, Is.Not.Null);
            Assert.That(renderer.GetType().Name, Is.EqualTo("Renderer2DData"), "o renderer precisa ser o 2D Renderer");
            string urp = File.ReadAllText(AssetDatabase.GetAssetPath(padrao));
            Assert.That(urp, Does.Contain(GuidRenderer2D), "o URP precisa usar o Renderer2D");
        }

        [Test]
        public void VersaoDoURP_NoManifest_EhAMesmaDoEditor()
        {
            var info = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.unity.render-pipelines.universal");
            Assert.That(info, Is.Not.Null, "pacote do URP não encontrado");
            string manifest = File.ReadAllText("Packages/manifest.json");
            Match m = Regex.Match(manifest, "\"com.unity.render-pipelines.universal\"\\s*:\\s*\"([^\"]+)\"");
            Assert.That(m.Success, Is.True);
            Assert.That(m.Groups[1].Value, Is.EqualTo(info.version),
                "a versão do URP no manifest precisa ser a que vem com o editor");
        }

        [TestCaseSource(nameof(CenasDoJogo))]
        public void Cena_TemCameraOrtografica_ESoComponentes2D(string cena)
        {
            string texto = File.ReadAllText("Assets/Scenes/" + cena + ".unity");
            Assert.That(texto, Does.Contain("orthographic: 1"), "a câmera precisa ser ortográfica (2D)");
            Assert.That(texto, Does.Contain("m_ClearFlags: 2"), "fundo de cor sólida, sem skybox 3D");
            Assert.That(texto, Does.Contain("m_SkyboxMaterial: {fileID: 0}"));
            // IDs de classe do Unity para componentes 3D: Light, Rigidbody, BoxCollider, SphereCollider,
            // MeshCollider, CapsuleCollider, MeshRenderer, MeshFilter, Terrain
            foreach (string id in new[] { "108", "54", "65", "135", "64", "136", "23", "33", "218" })
                Assert.That(texto, Does.Not.Contain("--- !u!" + id + " "), "a cena " + cena + " tem um componente 3D (!u!" + id + ")");
        }

        [Test]
        public void Codigo_NaoUsaFisica3D()
        {
            var proibidos = new Regex(@"\b(Rigidbody|BoxCollider|SphereCollider|CapsuleCollider|MeshCollider|CharacterController|Collision|Collider)\b(?!2D)|\bPhysics\.");
            foreach (string arquivo in Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories))
            {
                string[] linhas = File.ReadAllLines(arquivo);
                for (int i = 0; i < linhas.Length; i++)
                {
                    string l = linhas[i].Trim();
                    if (l.StartsWith("//") || l.StartsWith("///"))
                        continue;
                    Assert.That(proibidos.IsMatch(l), Is.False, arquivo + ":" + (i + 1) + " usa física 3D: " + l);
                }
            }
        }

        [Test]
        public void Sprites_SaoImportadosComoSprite_PixelArt()
        {
            string[] pngs = Directory.GetFiles("Assets/Resources/Sprites", "*.png");
            Assert.That(pngs.Length, Is.GreaterThan(0));
            foreach (string png in pngs)
            {
                var imp = AssetImporter.GetAtPath(png.Replace('\\', '/')) as TextureImporter;
                Assert.That(imp, Is.Not.Null, png);
                Assert.That(imp.textureType, Is.EqualTo(TextureImporterType.Sprite), png);
                Assert.That(imp.filterMode, Is.EqualTo(FilterMode.Point), png);
                Assert.That(imp.spritePixelsPerUnit, Is.EqualTo(16f), png);
            }
        }
    }
}
