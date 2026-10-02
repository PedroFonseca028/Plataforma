using UnityEngine;

namespace Curupira
{
    /// <summary>
    /// Uma camada do fundo com efeito parallax. "fator" diz o quanto a camada
    /// acompanha a câmera: 1 = presa na câmera (infinitamente longe),
    /// 0 = parada no mundo (mesma distância do jogador). A imagem se repete
    /// na horizontal (SpriteRenderer em modo Tiled), então nunca acaba.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class CamadaParallax : MonoBehaviour
    {
        public Transform cam;
        public float fatorX = 0.5f;
        public float fatorY = 0.8f;
        public float velocidadeAutomatica;   // nuvens andando sozinhas
        public float deslocamentoY;
        public bool usarTempoReal;

        float largura;
        float deslocAuto;
        float camYInicial;

        public static CamadaParallax Criar(string sprite, Transform cam, int ordem, float fatorX, float fatorY,
            float deslocamentoY, float velocidadeAutomatica = 0f)
        {
            Sprite s = Arte.Sprite(sprite);
            if (s == null)
                return null;
            SpriteRenderer sr = Efeitos.NovoSprite("Parallax " + sprite, null, s, ordem, true);
            sr.drawMode = SpriteDrawMode.Tiled;
            float w = s.bounds.size.x;
            sr.size = new Vector2(w * 3f, s.bounds.size.y);
            var c = sr.gameObject.AddComponent<CamadaParallax>();
            c.cam = cam;
            c.fatorX = fatorX;
            c.fatorY = fatorY;
            c.deslocamentoY = deslocamentoY;
            c.velocidadeAutomatica = velocidadeAutomatica;
            c.largura = w;
            c.camYInicial = cam.position.y;
            c.LateUpdate();
            return c;
        }

        void LateUpdate()
        {
            if (cam == null)
                return;
            deslocAuto += velocidadeAutomatica * (usarTempoReal ? Time.unscaledDeltaTime : Time.deltaTime);
            Vector3 c = cam.position;
            // posição do "conteúdo" da camada; como a imagem se repete a cada "largura",
            // basta posicionar a cópia mais próxima da câmera
            float conteudoX = c.x * fatorX + deslocAuto;
            float x = conteudoX + largura * Mathf.Round((c.x - conteudoX) / largura);
            float y = camYInicial + (c.y - camYInicial) * fatorY + deslocamentoY;
            transform.position = new Vector3(x, y, 10f);
        }
    }
}
