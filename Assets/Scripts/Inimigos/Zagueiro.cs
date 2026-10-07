using UnityEngine;

namespace RumoAoHexa
{
    /// <summary>Zagueiro argentino: forte e barbudo, patrulha a área e vira na parede ou na beirada.</summary>
    public class Zagueiro : Inimigo
    {
        const float Velocidade = 2f;

        protected override string Prefixo
        {
            get { return "zagueiro"; }
        }
        protected override Vector2 TamanhoColisor
        {
            get { return new Vector2(0.8f, 1.5f); }
        }

        protected override void Mover(float dt)
        {
            Patrulhar(Velocidade, dt);
        }
    }
}
