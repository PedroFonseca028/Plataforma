using UnityEngine;

namespace RumoAoHexa
{
    /// <summary>
    /// Cabeceador argentino: pula sem parar, como se fosse cabecear. No alto
    /// do pulo um chute rasteiro passa por baixo; a cavadinha pega ele no ar.
    /// </summary>
    public class Cabeceador : Inimigo
    {
        const float Gravidade = 28f;
        const float Impulso = 11.5f;     // sobe ~2,4 tiles
        const float Espera = 1.1f;
        const float Agachar = 0.3f;

        float baseY;
        float vy;
        float espera;
        bool iniciado;
        bool noAr;

        protected override string Prefixo
        {
            get { return "cabeceador"; }
        }
        protected override float QpsNormal
        {
            get { return 3f; }
        }

        protected override void Mover(float dt)
        {
            if (!iniciado)
            {
                baseY = rb.position.y;
                espera = Random.Range(0.2f, Espera);
                iniciado = true;
            }
            Vector2 pos = rb.position;
            if (!noAr)
            {
                espera -= dt;
                anim.Tocar(espera < Agachar ? "cabeceador_pulo_0" : AnimNormal, QpsNormal);
                if (espera <= 0f)
                {
                    vy = Impulso;
                    noAr = true;
                }
            }
            else
            {
                vy -= Gravidade * dt;
                pos.y += vy * dt;
                anim.Tocar(vy > 0f ? "cabeceador_pulo_1" : "cabeceador_pulo_2", 1f);
                if (pos.y <= baseY)
                {
                    pos.y = baseY;
                    vy = 0f;
                    noAr = false;
                    espera = Espera;
                }
            }
            if (Jogador.Atual != null)
                direcao = Jogador.Atual.transform.position.x < pos.x ? -1 : 1;
            rb.MovePosition(pos);
        }

        protected override void AoFicarTonto()
        {
            // cai de volta no chão na hora
            noAr = false;
            vy = 0f;
            espera = Espera;
            if (iniciado)
                rb.MovePosition(new Vector2(rb.position.x, baseY));
        }
    }
}
