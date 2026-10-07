using UnityEngine;

namespace RumoAoHexa
{
    /// <summary>
    /// Bandeirinha de escanteio: ponto de controle. Quando o craque passa, a
    /// bandeira verde e amarela sobe e tremula; se ele cair, volta para cá.
    /// </summary>
    public class Bandeirinha : MonoBehaviour
    {
        public bool Ativa { get; private set; }
        AnimadorSprite anim;

        public static Bandeirinha Criar(Vector3 posicao)
        {
            var go = new GameObject("Bandeirinha (ponto de controle)");
            go.layer = Camadas.Item;
            go.transform.position = posicao;
            return go.AddComponent<Bandeirinha>();
        }

        void Awake()
        {
            var col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 2f);
            col.offset = new Vector2(0f, 1f);
            SpriteRenderer sr = Efeitos.NovoSprite("Sprite", transform, Arte.Sprite("bandeirinha_desligada"), Ordem.Bandeirinha);
            anim = sr.gameObject.AddComponent<AnimadorSprite>();
            anim.alvo = sr;
        }

        void OnTriggerEnter2D(Collider2D outro)
        {
            if (Ativa || outro.GetComponent<Jogador>() == null)
                return;
            Ativar();
        }

        public void Ativar()
        {
            if (Ativa)
                return;
            Ativa = true;
            anim.Tocar("bandeirinha_ligada", 8f);
            SomJogo.Tocar(Som.Bandeirinha);
            Efeitos.Particulas("brilho", transform.position + Vector3.up * 1.7f, 8, new Color(1f, 0.9f, 0.4f), 1.5f, 3.5f, 0.6f, 1f, true);
            if (GerenciadorFase.Instancia != null)
                GerenciadorFase.Instancia.BandeirinhaAtivada(this);
        }
    }
}
