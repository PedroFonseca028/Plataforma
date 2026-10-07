using UnityEngine;

namespace RumoAoHexa
{
    /// <summary>
    /// Faz um objeto da cena (ex.: a luz 2D "Luz do Craque" da Fase 2) seguir o jogador.
    /// </summary>
    public class SeguirJogador : MonoBehaviour
    {
        public Vector3 deslocamento = new Vector3(0f, 1.5f, 0f);

        void LateUpdate()
        {
            if (Jogador.Atual != null)
                transform.position = Jogador.Atual.transform.position + deslocamento;
        }
    }
}
