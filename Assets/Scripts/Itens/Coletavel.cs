using UnityEngine;

namespace RumoAoHexa
{
    public enum TipoColetavel
    {
        /// <summary>+10 pontos.</summary>
        Moeda,
        /// <summary>+1 vida.</summary>
        Coracao,
        /// <summary>Estrela (fase 1) ou letra (fase 2): pegar todas vence a fase.</summary>
        ItemObrigatorio,
    }

    /// <summary>Item que o craque pega ao encostar.</summary>
    public class Coletavel : MonoBehaviour
    {
        public TipoColetavel tipo;
        /// <summary>Ordem do item obrigatório (0 = o mais à esquerda do mapa).</summary>
        public int indice;
        /// <summary>Ano da estrela ou letra.</summary>
        public string rotulo;
        public bool Coletado { get; private set; }

        Transform visual;
        float fase;

        /// <param name="animacao">Animação do item obrigatório ("estrela", "letra_h"...). Ignorada nos outros tipos.</param>
        public static Coletavel Criar(TipoColetavel tipo, Vector3 posicao, int indice, string rotulo, string animacao,
            GameObject modeloLuz)
        {
            var go = new GameObject(tipo == TipoColetavel.ItemObrigatorio ? "Item " + rotulo : "Item " + tipo);
            go.layer = Camadas.Item;
            go.transform.position = posicao;
            var c = go.AddComponent<Coletavel>();
            c.tipo = tipo;
            c.indice = indice;
            c.rotulo = rotulo;
            c.Montar(animacao, modeloLuz);
            return c;
        }

        void Montar(string animacao, GameObject modeloLuz)
        {
            bool obrigatorio = tipo == TipoColetavel.ItemObrigatorio;
            var col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = obrigatorio ? 0.5f : 0.38f;

            visual = new GameObject("Visual").transform;
            visual.SetParent(transform, false);

            if (obrigatorio)
            {
                // brilho atrás do item
                SpriteRenderer halo = Efeitos.NovoSprite("Halo", visual, Arte.Sprite("luz_suave"), Ordem.Itens - 1, true);
                halo.color = new Color(1f, 0.85f, 0.3f, 0.5f);
                halo.transform.localScale = Vector3.one * 1.4f;
                if (modeloLuz != null)
                {
                    GameObject luz = Instantiate(modeloLuz, transform);
                    luz.transform.localPosition = Vector3.zero;
                    luz.SetActive(true);
                }
                // o ano aparece embaixo da estrela
                if (animacao == "estrela" && !string.IsNullOrEmpty(rotulo))
                    Efeitos.TextoNoCenario(rotulo, transform.position + new Vector3(0f, -0.8f, 0f), FabricaUI.CorDestaque,
                        visual, Ordem.Itens + 1);
            }

            SpriteRenderer sr = Efeitos.NovoSprite("Sprite", visual, null, Ordem.Itens, obrigatorio);
            var anim = visual.gameObject.AddComponent<AnimadorSprite>();
            anim.alvo = sr;
            switch (tipo)
            {
                case TipoColetavel.Moeda: anim.Tocar("moeda", 10f); break;
                case TipoColetavel.Coracao: anim.Tocar("coracao", 6f); break;
                default: anim.Tocar(animacao, 8f); break;
            }
            anim.Dessincronizar();
            fase = transform.position.x * 0.9f;
        }

        void Update()
        {
            if (visual != null && !Coletado)
            {
                float altura = tipo == TipoColetavel.ItemObrigatorio ? 0.12f : 0.08f;
                visual.localPosition = new Vector3(0f, Mathf.Sin(Time.time * 3f + fase) * altura, 0f);
            }
        }

        void OnTriggerEnter2D(Collider2D outro)
        {
            if (Coletado)
                return;
            Jogador j = outro.GetComponent<Jogador>();
            if (j == null || !j.Vivo)
                return;
            Coletar();
        }

        /// <summary>Pega o item (também usado pelos testes).</summary>
        public void Coletar()
        {
            if (Coletado)
                return;
            Coletado = true;
            if (GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.ItemColetado(this);
            Destroy(gameObject);
        }
    }
}
