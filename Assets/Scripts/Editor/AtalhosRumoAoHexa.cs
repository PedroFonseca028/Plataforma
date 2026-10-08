using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace RumoAoHexa.EditorTools
{
    /// <summary>
    /// Ao abrir o projeto: garante o modo 2D do editor (o jogo é 2D), deixa a
    /// aba Scene em 2D e abre a cena do Menu. Também cria o menu "Rumo ao Hexa"
    /// na barra do Unity com atalhos para as cenas e para testar uma fase direto.
    /// </summary>
    [InitializeOnLoad]
    static class AtalhosRumoAoHexa
    {
        const string Menu = "Assets/Scenes/Menu.unity";
        const string Chave = "RumoAoHexa.MenuAbertoNaSessao";

        static AtalhosRumoAoHexa()
        {
            EditorApplication.delayCall += AoAbrirOProjeto;
            EditorApplication.playModeStateChanged += AoMudarModo;
        }

        /// <summary>Ao sair do Play, o jogo volta a começar sempre pela tela inicial.</summary>
        static void AoMudarModo(PlayModeStateChange modo)
        {
            if (modo == PlayModeStateChange.EnteredEditMode)
                SessionState.SetBool(Partida.ChavePularTelaInicial, false);
        }

        static void AoAbrirOProjeto()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SessionState.GetBool(Chave, false))
                return;
            SessionState.SetBool(Chave, true);

            // projeto 2D: sprites importados como Sprite e cenas novas em 2D
            if (EditorSettings.defaultBehaviorMode != EditorBehaviorMode.Mode2D)
                EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;

            Scene ativa = SceneManager.GetActiveScene();
            bool vazia = string.IsNullOrEmpty(ativa.path);
            if (vazia && !ativa.isDirty && File.Exists(Menu))
                EditorSceneManager.OpenScene(Menu);
            AbaSceneEm2D();
        }

        /// <summary>Coloca todas as abas Scene abertas no modo 2D.</summary>
        [MenuItem("Rumo ao Hexa/Aba Scene em 2D", false, 30)]
        static void AbaSceneEm2D()
        {
            foreach (object item in SceneView.sceneViews)
            {
                SceneView sv = item as SceneView;
                if (sv == null || sv.in2DMode)
                    continue;
                sv.in2DMode = true;
                sv.Repaint();
            }
        }

        [MenuItem("Rumo ao Hexa/Abrir cena Menu", false, 1)]
        static void AbrirMenu() { Abrir(Menu); }

        [MenuItem("Rumo ao Hexa/Abrir cena Fase 1 (Praia)", false, 2)]
        static void AbrirFase1() { Abrir("Assets/Scenes/Fase1.unity"); }

        [MenuItem("Rumo ao Hexa/Abrir cena Fase 2 (Maracanã)", false, 3)]
        static void AbrirFase2() { Abrir("Assets/Scenes/Fase2.unity"); }

        [MenuItem("Rumo ao Hexa/Abrir cena Fim de Jogo", false, 4)]
        static void AbrirFim() { Abrir("Assets/Scenes/FimDeJogo.unity"); }

        [MenuItem("Rumo ao Hexa/Testar Fase 1 direto (pula a tela inicial)", false, 10)]
        static void TestarFase1() { TestarDireto("Assets/Scenes/Fase1.unity"); }

        [MenuItem("Rumo ao Hexa/Testar Fase 2 direto (pula a tela inicial)", false, 11)]
        static void TestarFase2() { TestarDireto("Assets/Scenes/Fase2.unity"); }

        /// <summary>
        /// Dar Play numa fase normalmente leva para a tela inicial; este atalho
        /// abre a fase e dá Play direto nela (só para testes no Editor).
        /// </summary>
        static void TestarDireto(string cena)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            EditorSceneManager.OpenScene(cena);
            SessionState.SetBool(Partida.ChavePularTelaInicial, true);
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Rumo ao Hexa/Editar mapa da Fase 1 (texto)", false, 20)]
        static void MapaFase1() { EditorUtility.OpenWithDefaultApp("Assets/Resources/Fases/Fase1.txt"); }

        [MenuItem("Rumo ao Hexa/Editar mapa da Fase 2 (texto)", false, 21)]
        static void MapaFase2() { EditorUtility.OpenWithDefaultApp("Assets/Resources/Fases/Fase2.txt"); }

        [MenuItem("Rumo ao Hexa/Rodar os testes (Test Runner)", false, 40)]
        static void Testes() { EditorApplication.ExecuteMenuItem("Window/General/Test Runner"); }

        static void Abrir(string caminho)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            EditorSceneManager.OpenScene(caminho);
            AbaSceneEm2D();
        }
    }
}
