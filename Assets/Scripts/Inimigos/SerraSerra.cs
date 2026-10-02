using UnityEngine;

namespace Curupira
{
    /// <summary>
    /// Serra-Serra: motosserra-robô da madeireira. Patrulha o chão e dá meia-volta
    /// quando encontra uma parede ou a beirada da plataforma.
    /// </summary>
    public class SerraSerra : Inimigo
    {
        const float Velocidade = 2.2f;

        protected override string Prefixo
        {
            get { return "serra"; }
        }
        protected override string AnimNormal
        {
            get { return "serra_andar"; }
        }
        protected override Vector2 TamanhoColisor
        {
            get { return new Vector2(1.1f, 0.9f); }
        }

        protected override void Mover(float dt)
        {
            Vector2 pos = rb.position;
            Vector2 frente = pos + new Vector2(direcao * 0.65f, 0f);
            bool parede = TemParedeEm(frente + new Vector2(0f, 0.5f), new Vector2(0.1f, 0.5f));
            bool beirada = !TemChaoEm(frente + new Vector2(0f, 0.1f), 0.35f);
            if (parede || beirada)
                direcao = -direcao;
            rb.MovePosition(pos + new Vector2(direcao * Velocidade * dt, 0f));
        }
    }
}
