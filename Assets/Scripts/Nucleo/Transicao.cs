using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Curupira
{
    /// <summary>
    /// Troca de cena com "fade" (escurece, carrega a cena e clareia).
    /// Criado automaticamente e mantido entre as cenas.
    /// </summary>
    public class Transicao : MonoBehaviour
    {
        static Transicao instancia;

        Image cortina;
        bool carregando;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            instancia = null;
        }

        static Transicao Instancia
        {
            get
            {
                if (instancia == null)
                {
                    Canvas canvas = FabricaUI.CriarCanvas("Transicao", 1000);
                    DontDestroyOnLoad(canvas.gameObject);
                    instancia = canvas.gameObject.AddComponent<Transicao>();
                    instancia.cortina = FabricaUI.Cortina(canvas.transform, new Color(0.05f, 0.03f, 0.08f, 0f));
                    instancia.cortina.raycastTarget = false;
                }
                return instancia;
            }
        }

        public static bool Carregando
        {
            get { return instancia != null && instancia.carregando; }
        }

        /// <summary>Escurece a tela, carrega a cena e clareia de novo.</summary>
        public static void Carregar(string cena)
        {
            Transicao t = Instancia;
            if (t.carregando)
                return;
            t.StartCoroutine(t.RotinaCarregar(cena));
        }

        /// <summary>Clareia a tela ao entrar numa cena (se ela começou escura).</summary>
        public static void AoEntrarNaCena()
        {
            if (instancia != null && !instancia.carregando && instancia.cortina.color.a > 0f)
                instancia.StartCoroutine(instancia.Fade(0f, 0.35f));
        }

        IEnumerator RotinaCarregar(string cena)
        {
            carregando = true;
            cortina.raycastTarget = true;
            yield return Fade(1f, 0.35f);
            Time.timeScale = 1f;
            AsyncOperation op = SceneManager.LoadSceneAsync(cena);
            if (op != null)
            {
                while (!op.isDone)
                    yield return null;
            }
            else
            {
                Debug.LogError("[Curupira] Não foi possível carregar a cena '" + cena +
                               "'. Confira se ela está em File > Build Profiles > Scene List.");
            }
            yield return null;
            carregando = false;
            cortina.raycastTarget = false;
            yield return Fade(0f, 0.4f);
        }

        IEnumerator Fade(float alvo, float duracao)
        {
            Color c = cortina.color;
            float inicio = c.a;
            float t = 0f;
            while (t < duracao)
            {
                t += Time.unscaledDeltaTime;
                c.a = Mathf.Lerp(inicio, alvo, t / duracao);
                cortina.color = c;
                yield return null;
            }
            c.a = alvo;
            cortina.color = c;
        }
    }
}
