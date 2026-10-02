using UnityEngine;

namespace Curupira
{
    /// <summary>Totem de madeira: ponto de controle (checkpoint). Acende quando o Curupira passa.</summary>
    public class Totem : MonoBehaviour
    {
        public bool Ativo { get; private set; }
        AnimadorSprite anim;

        public static Totem Criar(Vector3 posicao)
        {
            var go = new GameObject("Totem (checkpoint)");
            go.layer = Camadas.Item;
            go.transform.position = posicao;
            return go.AddComponent<Totem>();
        }

        void Awake()
        {
            var col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 2f);
            col.offset = new Vector2(0f, 1f);
            SpriteRenderer sr = Efeitos.NovoSprite("Sprite", transform, Arte.Sprite("totem_desligado"), Ordem.Totem);
            anim = sr.gameObject.AddComponent<AnimadorSprite>();
            anim.alvo = sr;
        }

        void OnTriggerEnter2D(Collider2D outro)
        {
            if (Ativo || outro.GetComponent<Jogador>() == null)
                return;
            Ativo = true;
            anim.Tocar("totem_ligado", 6f);
            Materiais2D.Aplicar(anim.alvo, true);
            SomJogo.Tocar(Som.Checkpoint);
            Efeitos.Particulas("brilho", transform.position + Vector3.up * 1.8f, 8, new Color(1f, 0.9f, 0.4f), 1.5f, 3.5f, 0.6f, 1f, true);
            if (GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.CheckpointAtivado(this);
        }
    }
}
