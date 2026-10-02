using System.Collections;
using UnityEngine;

namespace Curupira
{
    /// <summary>Drone Madeireiro: voa de um lado para o outro, balançando.</summary>
    public class Drone : Inimigo
    {
        const float Alcance = 3f;
        const float Velocidade = 1.2f;

        Vector2 inicio;
        float tempo;
        bool iniciado;

        protected override string Prefixo
        {
            get { return "drone"; }
        }
        protected override string AnimNormal
        {
            get { return "drone_voar"; }
        }
        protected override float QpsNormal
        {
            get { return 16f; }
        }
        protected override int PontosPisao
        {
            get { return 150; }
        }
        protected override Vector2 TamanhoColisor
        {
            get { return new Vector2(1.1f, 0.7f); }
        }

        protected override void Awake()
        {
            base.Awake();
            // o sprite tem o corpo no meio da célula: desce o desenho para alinhar ao colisor
            visual.localPosition = new Vector3(0f, -0.35f, 0f);
        }

        protected override void Mover(float dt)
        {
            if (!iniciado)
            {
                inicio = rb.position;
                tempo = Random.Range(0f, 6f);
                iniciado = true;
            }
            tempo += dt;
            float x = inicio.x + Mathf.Sin(tempo * Velocidade) * Alcance;
            float y = inicio.y + Mathf.Sin(tempo * 2.6f) * 0.3f;
            direcao = Mathf.Cos(tempo * Velocidade) >= 0f ? 1 : -1;
            rb.MovePosition(new Vector2(x, y));
        }

        protected override IEnumerator Sumir()
        {
            // o drone quebrado cai girando
            float vy = 2f;
            float t = 0f;
            while (t < 1.2f)
            {
                t += Time.deltaTime;
                vy -= 25f * Time.deltaTime;
                transform.position += new Vector3(0f, vy * Time.deltaTime, 0f);
                visual.Rotate(0f, 0f, 400f * Time.deltaTime);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
