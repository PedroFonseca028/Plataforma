using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Curupira.EditorTools
{
    /// <summary>
    /// Ao abrir o projeto pela primeira vez, abre a cena do Menu automaticamente.
    /// Também cria o menu "Curupira" na barra do Unity com atalhos para as cenas.
    /// </summary>
    [InitializeOnLoad]
    static class AtalhosCurupira
    {
        const string Menu = "Assets/Scenes/Menu.unity";
        const string Chave = "Curupira.MenuAbertoNaSessao";

        static AtalhosCurupira()
        {
            EditorApplication.delayCall += AbrirMenuSeNecessario;
        }

        static void AbrirMenuSeNecessario()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SessionState.GetBool(Chave, false))
                return;
            SessionState.SetBool(Chave, true);
            Scene ativa = SceneManager.GetActiveScene();
            bool vazia = string.IsNullOrEmpty(ativa.path);
            if (vazia && !ativa.isDirty && File.Exists(Menu))
                EditorSceneManager.OpenScene(Menu);
        }

        [MenuItem("Curupira/Abrir cena Menu", false, 1)]
        static void AbrirMenu() { Abrir(Menu); }

        [MenuItem("Curupira/Abrir cena Fase 1", false, 2)]
        static void AbrirFase1() { Abrir("Assets/Scenes/Fase1.unity"); }

        [MenuItem("Curupira/Abrir cena Fase 2", false, 3)]
        static void AbrirFase2() { Abrir("Assets/Scenes/Fase2.unity"); }

        [MenuItem("Curupira/Abrir cena Fim de Jogo", false, 4)]
        static void AbrirFim() { Abrir("Assets/Scenes/FimDeJogo.unity"); }

        [MenuItem("Curupira/Editar mapa da Fase 1 (texto)", false, 20)]
        static void MapaFase1() { EditorUtility.OpenWithDefaultApp("Assets/Resources/Fases/Fase1.txt"); }

        [MenuItem("Curupira/Editar mapa da Fase 2 (texto)", false, 21)]
        static void MapaFase2() { EditorUtility.OpenWithDefaultApp("Assets/Resources/Fases/Fase2.txt"); }

        static void Abrir(string caminho)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(caminho);
        }
    }
}
