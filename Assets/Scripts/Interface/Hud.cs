using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RumoAoHexa
{
    /// <summary>
    /// Informações na tela durante a fase: vidas, pontos, moedas, os itens
    /// obrigatórios (estrelas ou letras H-E-X-A) e a barra da folha seca.
    /// </summary>
    public class Hud : MonoBehaviour
    {
        Image[] coracoes;
        TextoPixel pontos;
        TextoPixel moedas;
        Image[] slots;
        float[] pulsoSlot;
        Image iconeBola;
        Image fundoBarra;
        Image barraCarga;
        TextoPixel titulo;
        TextoPixel subtitulo;
        TextoPixel mensagem;
        int fase;
        int total;
        int ultimosPontos = -1;
        int ultimasVidas = -1;
        int ultimasMoedas = -1;
        Coroutine rotinaMensagem;
        float piscar;

        public static Hud Criar(int fase, int totalItens)
        {
            Canvas canvas = FabricaUI.CriarCanvas("HUD", 10);
            var hud = canvas.gameObject.AddComponent<Hud>();
            hud.fase = fase;
            hud.total = totalItens;
            hud.Montar(canvas.transform);
            return hud;
        }

        void Montar(Transform raiz)
        {
            Vector2 cimaEsq = new Vector2(0f, 1f);
            Vector2 cimaDir = new Vector2(1f, 1f);
            Vector2 baixoEsq = new Vector2(0f, 0f);
            Vector2 centro = new Vector2(0.5f, 0.5f);

            coracoes = new Image[Regras.VidasMaximas];
            for (int i = 0; i < coracoes.Length; i++)
                coracoes[i] = FabricaUI.Imagem(raiz, Arte.Sprite("ui_coracao_cheio"), cimaEsq, new Vector2(34f + i * 36f, -32f), new Vector2(32f, 32f));

            pontos = FabricaUI.Texto(raiz, "PONTOS 0", cimaEsq, new Vector2(218f, -70f), 2f, FabricaUI.CorTexto, TextAnchor.MiddleLeft, new Vector2(400f, 30f));
            FabricaUI.Imagem(raiz, Arte.Sprite("moeda_0"), cimaEsq, new Vector2(34f, -100f), new Vector2(24f, 24f));
            moedas = FabricaUI.Texto(raiz, "X 0", cimaEsq, new Vector2(252f, -100f), 2f, FabricaUI.CorDestaque, TextAnchor.MiddleLeft, new Vector2(400f, 30f));

            // itens obrigatórios: começam apagados e acendem quando são pegos
            slots = new Image[total];
            pulsoSlot = new float[total];
            float passo = fase == 1 ? 42f : 40f;
            for (int i = 0; i < total; i++)
            {
                string sprite = fase == 1 ? "estrela_0" : "letra_" + Regras.RotuloItem(fase, i).ToLowerInvariant() + "_0";
                float x = -30f - (total - 1 - i) * passo;
                slots[i] = FabricaUI.Imagem(raiz, Arte.Sprite(sprite), cimaDir, new Vector2(x, -34f), new Vector2(32f, 32f),
                    new Color(0.25f, 0.27f, 0.35f, 0.75f));
                if (fase == 1)
                    FabricaUI.Texto(raiz, Regras.RotuloItem(fase, i), cimaDir, new Vector2(x, -64f), 1f, FabricaUI.CorTexto,
                        TextAnchor.MiddleCenter, new Vector2(60f, 16f));
            }

            // bola + barra da folha seca
            iconeBola = FabricaUI.Imagem(raiz, Arte.Sprite("bola_ui"), baixoEsq, new Vector2(40f, 40f), new Vector2(44f, 44f));
            fundoBarra = FabricaUI.Imagem(raiz, Arte.Sprite("ui_branco"), baixoEsq, new Vector2(118f, 40f), new Vector2(104f, 16f),
                new Color(0.05f, 0.05f, 0.12f, 0.75f));
            barraCarga = FabricaUI.Imagem(raiz, Arte.Sprite("ui_branco"), baixoEsq, new Vector2(118f, 40f), new Vector2(100f, 12f),
                FabricaUI.CorDestaque);
            barraCarga.type = Image.Type.Filled;
            barraCarga.fillMethod = Image.FillMethod.Horizontal;
            barraCarga.fillOrigin = (int)Image.OriginHorizontal.Left;
            barraCarga.fillAmount = 0f;
            FabricaUI.Texto(raiz, Textos.HudControles, baixoEsq, new Vector2(186f, 40f), 2f, new Color(1f, 1f, 1f, 0.8f),
                TextAnchor.MiddleLeft, new Vector2(760f, 30f)).rectTransform.pivot = new Vector2(0f, 0.5f);

            titulo = FabricaUI.Texto(raiz, "", centro, new Vector2(0f, 150f), 4f, FabricaUI.CorDestaque);
            titulo.contorno = true;
            subtitulo = FabricaUI.Texto(raiz, "", centro, new Vector2(0f, 100f), 2f, FabricaUI.CorTexto);
            subtitulo.contorno = true;
            mensagem = FabricaUI.Texto(raiz, "", centro, new Vector2(0f, 40f), 3f, FabricaUI.CorTexto);
            mensagem.contorno = true;
            AtualizarTudo();
        }

        public void AtualizarTudo()
        {
            ultimosPontos = -1;
            ultimasVidas = -1;
            ultimasMoedas = -1;
        }

        /// <param name="carga">Barra da folha seca (0 a 1).</param>
        /// <param name="bolaNoPe">Com a bola longe, o ícone fica apagado.</param>
        public void Atualizar(int coletados, float carga, bool bolaNoPe)
        {
            if (Sessao.Pontos != ultimosPontos)
            {
                ultimosPontos = Sessao.Pontos;
                pontos.Texto = "PONTOS " + Sessao.Pontos.ToString("000000");
            }
            if (Sessao.Moedas != ultimasMoedas)
            {
                ultimasMoedas = Sessao.Moedas;
                moedas.Texto = "X " + Sessao.Moedas;
            }
            if (Sessao.Vidas != ultimasVidas)
            {
                ultimasVidas = Sessao.Vidas;
                for (int i = 0; i < coracoes.Length; i++)
                {
                    coracoes[i].gameObject.SetActive(i < Mathf.Max(Regras.VidasIniciais, Sessao.Vidas));
                    coracoes[i].sprite = Arte.Sprite(i < Sessao.Vidas ? "ui_coracao_cheio" : "ui_coracao_vazio");
                }
            }

            barraCarga.fillAmount = Mathf.Clamp01(carga);
            bool cheia = carga >= 1f;
            barraCarga.color = cheia && Mathf.Repeat(Time.unscaledTime * 8f, 1f) < 0.5f ? Color.white : FabricaUI.CorDestaque;
            iconeBola.color = bolaNoPe ? Color.white : new Color(1f, 1f, 1f, 0.35f);
            iconeBola.rectTransform.localRotation = Quaternion.Euler(0f, 0f, bolaNoPe ? 0f : -Time.unscaledTime * 360f);

            for (int i = 0; i < slots.Length; i++)
            {
                if (pulsoSlot[i] <= 0f)
                    continue;
                pulsoSlot[i] -= Time.unscaledDeltaTime;
                float s = 1f + Mathf.Max(0f, pulsoSlot[i]) * 1.2f;
                slots[i].rectTransform.localScale = new Vector3(s, s, 1f);
            }

            if (piscar > 0f)
            {
                piscar -= Time.unscaledDeltaTime;
                float s = 1f + Mathf.Sin(piscar * 30f) * 0.15f;
                for (int i = 0; i < coracoes.Length; i++)
                    coracoes[i].rectTransform.localScale = new Vector3(s, s, 1f);
            }
        }

        /// <summary>Acende o item pego no canto da tela.</summary>
        public void MarcarItem(int indice)
        {
            if (indice < 0 || indice >= slots.Length)
                return;
            slots[indice].color = Color.white;
            pulsoSlot[indice] = 0.5f;
        }

        public bool ItemMarcado(int indice)
        {
            return indice >= 0 && indice < slots.Length && slots[indice].color == Color.white;
        }

        public void PiscarVidas()
        {
            piscar = 0.5f;
        }

        public void Banner(string textoTitulo, string textoSubtitulo)
        {
            StartCoroutine(RotinaBanner(textoTitulo, textoSubtitulo));
        }

        IEnumerator RotinaBanner(string t, string s)
        {
            titulo.Texto = t;
            subtitulo.Texto = s;
            float tempo = 0f;
            while (tempo < 3.5f)
            {
                tempo += Time.deltaTime;
                float a = tempo < 2.5f ? 1f : 1f - (tempo - 2.5f);
                titulo.color = new Color(titulo.color.r, titulo.color.g, titulo.color.b, a);
                subtitulo.color = new Color(subtitulo.color.r, subtitulo.color.g, subtitulo.color.b, a);
                yield return null;
            }
            titulo.Texto = "";
            subtitulo.Texto = "";
        }

        public void Mensagem(string texto, Color cor, float duracao)
        {
            if (rotinaMensagem != null)
                StopCoroutine(rotinaMensagem);
            rotinaMensagem = StartCoroutine(RotinaMensagem(texto, cor, duracao));
        }

        IEnumerator RotinaMensagem(string texto, Color cor, float duracao)
        {
            mensagem.Texto = texto;
            float t = 0f;
            while (t < duracao)
            {
                t += Time.deltaTime;
                float a = Mathf.Clamp01((duracao - t) * 3f);
                mensagem.color = new Color(cor.r, cor.g, cor.b, a);
                float s = 1f + Mathf.Max(0f, 0.3f - t) * 1.5f;
                mensagem.rectTransform.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            mensagem.Texto = "";
        }
    }
}
