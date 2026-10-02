using UnityEngine;

namespace Curupira
{
    public enum TipoColetavel
    {
        Semente,   // obrigatória para vencer a fase
        Acai,      // +10
        Caju,      // +25
        Guarana,   // +50
        Coracao,   // +1 vida
    }

    /// <summary>Item que o Curupira pega ao encostar.</summary>
    public class Coletavel : MonoBehaviour
    {
        public TipoColetavel tipo;
        public bool Coletado { get; private set; }

        Transform visual;
        float fase;

        public static Coletavel Criar(TipoColetavel tipo, Vector3 posicao, GameObject modeloLuz)
        {
            var go = new GameObject("Item " + tipo);
            go.layer = Camadas.Item;
            go.transform.position = posicao;
            var c = go.AddComponent<Coletavel>();
            c.tipo = tipo;
            c.Montar(modeloLuz);
            return c;
        }

        void Montar(GameObject modeloLuz)
        {
            var col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = tipo == TipoColetavel.Semente ? 0.45f : 0.38f;

            visual = new GameObject("Visual").transform;
            visual.SetParent(transform, false);
            bool brilha = tipo == TipoColetavel.Semente;

            if (brilha)
            {
                // halo suave atrás da semente
                SpriteRenderer halo = Efeitos.NovoSprite("Halo", visual, Arte.Sprite("luz_suave"), Ordem.Itens - 1, true);
                halo.color = new Color(1f, 0.85f, 0.3f, 0.45f);
                halo.transform.localScale = Vector3.one * 1.3f;
                if (modeloLuz != null)
                {
                    GameObject luz = Instantiate(modeloLuz, transform);
                    luz.transform.localPosition = Vector3.zero;
                    luz.SetActive(true);
                }
            }

            SpriteRenderer sr = Efeitos.NovoSprite("Sprite", visual, null, Ordem.Itens, brilha);
            var anim = visual.gameObject.AddComponent<AnimadorSprite>();
            anim.alvo = sr;
            switch (tipo)
            {
                case TipoColetavel.Semente: anim.Tocar("semente", 8f); break;
                case TipoColetavel.Acai: anim.Tocar("acai", 5f); break;
                case TipoColetavel.Caju: anim.Tocar("caju", 5f); break;
                case TipoColetavel.Guarana: anim.Tocar("guarana", 5f); break;
                case TipoColetavel.Coracao: anim.Tocar("coracao", 6f); break;
            }
            anim.Dessincronizar();
            fase = transform.position.x * 0.9f;
        }

        void Update()
        {
            if (visual != null && !Coletado)
                visual.localPosition = new Vector3(0f, Mathf.Sin(Time.time * 3f + fase) * 0.08f, 0f);
        }

        void OnTriggerEnter2D(Collider2D outro)
        {
            if (Coletado)
                return;
            Jogador j = outro.GetComponent<Jogador>();
            if (j == null || !j.Vivo)
                return;
            Coletado = true;
            if (GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.ItemColetado(this);
            Destroy(gameObject);
        }
    }
}
