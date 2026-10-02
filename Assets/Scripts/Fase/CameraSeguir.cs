using UnityEngine;

namespace Curupira
{
    /// <summary>
    /// Câmera que segue o Curupira com suavidade, olha um pouco à frente,
    /// respeita os limites da fase e treme quando algo forte acontece.
    /// </summary>
    [DefaultExecutionOrder(50)]
    public class CameraSeguir : MonoBehaviour
    {
        public const float TamanhoOrtografico = 6.75f; // 13,5 tiles de altura (1080p = 5 px por pixel da arte)

        static CameraSeguir atual;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            atual = null;
        }

        public Transform alvo;
        public Rect limites;

        Camera cam;
        float baseY;
        float olhar;
        Vector3 velocidade;
        float tremorForca;
        float tremorTempo;
        bool teleportar = true;

        void Awake()
        {
            atual = this;
            cam = GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = TamanhoOrtografico;
        }

        public static void Tremer(float forca, float duracao)
        {
            if (atual == null)
                return;
            atual.tremorForca = Mathf.Max(atual.tremorForca, forca);
            atual.tremorTempo = Mathf.Max(atual.tremorTempo, duracao);
        }

        public static void Teleportar()
        {
            if (atual != null)
                atual.teleportar = true;
        }

        void LateUpdate()
        {
            if (alvo == null)
                return;
            Vector3 p = alvo.position;
            Jogador j = alvo.GetComponent<Jogador>();

            // só acompanha a altura quando o jogador pisa em algo (evita "pular junto")
            if (teleportar || (j != null && j.NoChao) || p.y < baseY - 1.5f || (j != null && !j.Vivo))
                baseY = p.y;
            if (p.y > baseY + 4f)
                baseY = p.y - 4f;

            float h = Entrada.Horizontal;
            if (Mathf.Abs(h) > 0.1f)
                olhar = Mathf.MoveTowards(olhar, Mathf.Sign(h) * 1.8f, 3f * Time.deltaTime);

            Vector3 destino = new Vector3(p.x + olhar, baseY + 1.6f, transform.position.z);
            destino = Limitar(destino);

            if (teleportar)
            {
                transform.position = destino;
                velocidade = Vector3.zero;
                teleportar = false;
            }
            else
            {
                Vector3 pos = transform.position;
                pos.x = Mathf.SmoothDamp(pos.x, destino.x, ref velocidade.x, 0.12f);
                pos.y = Mathf.SmoothDamp(pos.y, destino.y, ref velocidade.y, 0.2f);
                transform.position = Limitar(pos);
            }

            if (tremorTempo > 0f)
            {
                tremorTempo -= Time.deltaTime;
                Vector2 d = Random.insideUnitCircle * tremorForca;
                transform.position += new Vector3(d.x, d.y, 0f);
                if (tremorTempo <= 0f)
                    tremorForca = 0f;
            }
        }

        Vector3 Limitar(Vector3 pos)
        {
            float meiaAltura = cam.orthographicSize;
            float meiaLargura = meiaAltura * cam.aspect;
            if (limites.width <= meiaLargura * 2f)
                pos.x = limites.center.x;
            else
                pos.x = Mathf.Clamp(pos.x, limites.xMin + meiaLargura, limites.xMax - meiaLargura);
            if (limites.height <= meiaAltura * 2f)
                pos.y = limites.yMin + meiaAltura;
            else
                pos.y = Mathf.Clamp(pos.y, limites.yMin + meiaAltura, limites.yMax - meiaAltura);
            return pos;
        }
    }
}
