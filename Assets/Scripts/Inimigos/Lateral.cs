using UnityEngine;

namespace RumoAoHexa
{
    /// <summary>
    /// Lateral argentino: anda devagar, mas quando vê o craque na mesma altura
    /// dá uma arrancada na direção dele. Depois precisa de um tempo para recuperar o fôlego.
    /// </summary>
    public class Lateral : Inimigo
    {
        const float VelocidadeAndando = 1.5f;
        const float VelocidadeArrancada = 6.5f;
        const float DistanciaVisao = 7f;
        const float DuracaoArrancada = 1.1f;
        const float Folego = 1.4f;

        float tempoArrancada;
        float tempoFolego;

        public bool EmArrancada
        {
            get { return tempoArrancada > 0f; }
        }

        protected override string Prefixo
        {
            get { return "lateral"; }
        }

        protected override void Mover(float dt)
        {
            if (tempoArrancada > 0f)
            {
                tempoArrancada -= dt;
                if (Patrulhar(VelocidadeArrancada, dt) || tempoArrancada <= 0f)
                    TerminarArrancada();
                return;
            }
            tempoFolego -= dt;
            if (tempoFolego <= 0f && VendoOCraque())
            {
                tempoArrancada = DuracaoArrancada;
                anim.Tocar("lateral_correr", 14f);
                Efeitos.Animacao("poeira", transform.position + new Vector3(-direcao * 0.4f, 0.2f, 0f), 16f, 0.8f, false,
                    Ordem.Efeitos, direcao > 0);
                return;
            }
            Patrulhar(VelocidadeAndando, dt);
        }

        bool VendoOCraque()
        {
            Jogador j = Jogador.Atual;
            if (j == null || !j.Vivo)
                return false;
            Vector2 d = (Vector2)j.transform.position - rb.position;
            if (Mathf.Abs(d.x) > DistanciaVisao || Mathf.Abs(d.y) > 1.2f)
                return false;
            direcao = d.x < 0f ? -1 : 1;
            return true;
        }

        void TerminarArrancada()
        {
            tempoArrancada = 0f;
            tempoFolego = Folego;
            anim.Tocar(AnimNormal, QpsNormal);
        }

        protected override void AoFicarTonto()
        {
            tempoArrancada = 0f;
            tempoFolego = Folego;
        }
    }
}
