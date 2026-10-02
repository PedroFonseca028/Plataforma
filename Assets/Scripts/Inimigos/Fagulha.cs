using UnityEngine;

namespace Curupira
{
    /// <summary>
    /// Fagulha: brasa viva que salta sem parar. Queima se for tocada, inclusive
    /// por cima — só pode ser apagada (pisada) depois de ficar tonta com o assobio.
    /// </summary>
    public class Fagulha : Inimigo
    {
        const float Gravidade = 30f;
        const float Impulso = 12f;       // sobe ~2,4 tiles
        const float Espera = 0.9f;

        float baseY;
        float vy;
        float espera;
        bool iniciado;
        float timerBrasa;

        protected override string Prefixo
        {
            get { return "fagulha"; }
        }
        protected override string AnimNormal
        {
            get { return "fagulha_viva"; }
        }
        protected override bool PodeSerPisado
        {
            get { return false; }
        }
        protected override int PontosPisao
        {
            get { return 200; }
        }
        protected override bool Brilha
        {
            get { return true; }
        }
        protected override Vector2 TamanhoColisor
        {
            get { return new Vector2(0.75f, 0.85f); }
        }

        protected override void Mover(float dt)
        {
            if (!iniciado)
            {
                baseY = rb.position.y;
                espera = Random.Range(0f, Espera);
                iniciado = true;
            }
            Vector2 pos = rb.position;
            if (espera > 0f)
            {
                espera -= dt;
                if (espera <= 0f)
                    vy = Impulso;
            }
            else
            {
                vy -= Gravidade * dt;
                pos.y += vy * dt;
                if (pos.y <= baseY)
                {
                    pos.y = baseY;
                    vy = 0f;
                    espera = Espera;
                }
            }
            if (Jogador.Atual != null)
                direcao = Jogador.Atual.transform.position.x < pos.x ? -1 : 1;
            rb.MovePosition(pos);

            timerBrasa -= dt;
            if (timerBrasa <= 0f)
            {
                timerBrasa = 0.25f;
                Efeitos.Particulas("brasa", (Vector3)pos + new Vector3(0f, 0.6f, 0f), 1, Color.white, 0.3f, 1f, 0.7f, -2f, true, 60f, 120f);
            }
        }
    }
}
