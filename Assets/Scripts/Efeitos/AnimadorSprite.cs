using UnityEngine;
using UnityEngine.UI;

namespace Curupira
{
    /// <summary>
    /// Anima um SpriteRenderer (ou uma Image de UI) trocando os quadros de uma
    /// animação da <see cref="Arte"/> numa velocidade fixa (quadros por segundo).
    /// </summary>
    public class AnimadorSprite : MonoBehaviour
    {
        public SpriteRenderer alvo;
        public Image alvoUI;
        public bool usarTempoReal;
        public bool pausado;

        Sprite[] quadros;
        float qps = 10f;
        bool repetir = true;
        float tempo;
        int indiceAtual = -1;
        string nomeAtual;

        public bool Terminou { get; private set; }
        public string Atual
        {
            get { return nomeAtual; }
        }

        /// <summary>Toca uma animação pelo prefixo (ex.: "curupira_correr").</summary>
        public void Tocar(string nome, float quadrosPorSegundo, bool repetirAnimacao = true, bool reiniciar = false)
        {
            if (!reiniciar && nome == nomeAtual)
            {
                qps = quadrosPorSegundo;
                return;
            }
            Tocar(Arte.Anim(nome), quadrosPorSegundo, repetirAnimacao);
            nomeAtual = nome;
        }

        public void Tocar(Sprite[] novosQuadros, float quadrosPorSegundo, bool repetirAnimacao = true)
        {
            quadros = novosQuadros;
            qps = quadrosPorSegundo;
            repetir = repetirAnimacao;
            tempo = 0f;
            indiceAtual = -1;
            Terminou = false;
            nomeAtual = null;
            Aplicar(0);
        }

        /// <summary>Começa a animação num ponto aleatório (para vários itens não piscarem juntos).</summary>
        public void Dessincronizar()
        {
            tempo = Random.Range(0f, 2f);
        }

        void Update()
        {
            if (pausado || quadros == null || quadros.Length == 0 || Terminou)
                return;
            tempo += usarTempoReal ? Time.unscaledDeltaTime : Time.deltaTime;
            int i = Mathf.FloorToInt(tempo * qps);
            if (repetir)
            {
                i %= quadros.Length;
            }
            else if (i >= quadros.Length)
            {
                i = quadros.Length - 1;
                Terminou = true;
            }
            Aplicar(i);
        }

        void Aplicar(int i)
        {
            if (quadros == null || quadros.Length == 0 || i == indiceAtual)
                return;
            indiceAtual = i;
            if (alvo != null)
                alvo.sprite = quadros[i];
            if (alvoUI != null)
                alvoUI.sprite = quadros[i];
        }
    }
}
