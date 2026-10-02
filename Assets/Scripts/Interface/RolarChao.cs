using UnityEngine;

namespace Curupira
{
    /// <summary>Faz o chão do menu rolar para a esquerda sem fim (dá a impressão de corrida).</summary>
    public class RolarChao : MonoBehaviour
    {
        public float velocidade = 2.5f;
        float desloc;

        void Update()
        {
            desloc = Mathf.Repeat(desloc + velocidade * Time.unscaledDeltaTime, 1f);
            transform.position = new Vector3(-desloc, 0f, 0f);
        }
    }
}
