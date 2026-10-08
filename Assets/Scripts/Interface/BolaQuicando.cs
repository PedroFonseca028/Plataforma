using UnityEngine;

namespace RumoAoHexa
{
    /// <summary>Bola das telas de menu: sobe e desce no pé do craque (embaixadinha decorativa).</summary>
    public class BolaQuicando : MonoBehaviour
    {
        public Vector3 pe;
        public float altura = 1.5f;
        public float periodo = 0.6f;

        float tempo;

        void Update()
        {
            tempo += Time.unscaledDeltaTime;
            float f = Mathf.Repeat(tempo / periodo, 1f);
            transform.position = pe + Vector3.up * (Mathf.Sin(f * Mathf.PI) * altura);
            transform.rotation = Quaternion.Euler(0f, 0f, -tempo * 360f);
        }
    }
}
