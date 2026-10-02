using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Curupira
{
    /// <summary>Informações na tela durante a fase: vidas, pontos, sementes e assobio.</summary>
    public class Hud : MonoBehaviour
    {
        Image[] coracoes;
        TextoPixel pontos;
        TextoPixel sementes;
        Image recargaAssobio;
        Image iconeAssobio;
        TextoPixel titulo;
        TextoPixel subtitulo;
        TextoPixel mensagem;
        int total;
        int ultimasSementes = -1;
        int ultimosPontos = -1;
        int ultimasVidas = -1;
        Coroutine rotinaMensagem;
        float piscar;

        public static Hud Criar(int totalSementes)
        {
            Canvas canvas = FabricaUI.CriarCanvas("HUD", 10);
            var hud = canvas.gameObject.AddComponent<Hud>();
            hud.total = totalSementes;
            hud.Montar(canvas.transform);
            return hud;
        }

        void Montar(Transform raiz)
        {
            Vector2 cimaEsq = new Vector2(0f, 1f);
            Vector2 cimaDir = new Vector2(1f, 1f);
            Vector2 baixoEsq = new Vector2(0f, 0f);
            Vector2 centro = new Vector2(0.5f, 0.5f);

            coracoes = new Image[Sessao.VidasMaximas];
            for (int i = 0; i < coracoes.Length; i++)
                coracoes[i] = FabricaUI.Imagem(raiz, Arte.Sprite("ui_coracao_cheio"), cimaEsq, new Vector2(34f + i * 36f, -32f), new Vector2(32f, 32f));

            pontos = FabricaUI.Texto(raiz, "PONTOS 0", cimaEsq, new Vector2(218f, -72f), 2f, FabricaUI.CorTexto, TextAnchor.MiddleLeft, new Vector2(400f, 30f));

            FabricaUI.Imagem(raiz, Arte.Sprite("ui_semente"), cimaDir, new Vector2(-150f, -34f), new Vector2(32f, 32f));
            sementes = FabricaUI.Texto(raiz, "0/0", cimaDir, new Vector2(-70f, -34f), 3f, FabricaUI.CorDestaque, TextAnchor.MiddleLeft, new Vector2(120f, 40f));
            sementes.rectTransform.anchoredPosition = new Vector2(-66f, -34f);

            iconeAssobio = FabricaUI.Imagem(raiz, Arte.Sprite("ui_assobio"), baixoEsq, new Vector2(40f, 40f), new Vector2(48f, 48f));
            recargaAssobio = FabricaUI.Imagem(raiz, Arte.Sprite("ui_branco"), baixoEsq, new Vector2(40f, 40f), new Vector2(48f, 48f),
                new Color(0.05f, 0.03f, 0.08f, 0.7f));
            recargaAssobio.type = Image.Type.Filled;
            recargaAssobio.fillMethod = Image.FillMethod.Vertical;
            recargaAssobio.fillOrigin = (int)Image.OriginVertical.Top;
            FabricaUI.Texto(raiz, "J / X: ASSOBIO", baixoEsq, new Vector2(170f, 40f), 2f, new Color(1f, 1f, 1f, 0.8f), TextAnchor.MiddleLeft, new Vector2(200f, 30f));

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
            ultimasSementes = -1;
            ultimosPontos = -1;
            ultimasVidas = -1;
        }

        public void Atualizar(int coletadas, float recargaFracao)
        {
            if (coletadas != ultimasSementes)
            {
                ultimasSementes = coletadas;
                sementes.Texto = coletadas + "/" + total;
            }
            if (Sessao.Pontos != ultimosPontos)
            {
                ultimosPontos = Sessao.Pontos;
                pontos.Texto = "PONTOS " + Sessao.Pontos.ToString("000000");
            }
            if (Sessao.Vidas != ultimasVidas)
            {
                ultimasVidas = Sessao.Vidas;
                for (int i = 0; i < coracoes.Length; i++)
                {
                    coracoes[i].gameObject.SetActive(i < Mathf.Max(Sessao.VidasIniciais, Sessao.Vidas));
                    coracoes[i].sprite = Arte.Sprite(i < Sessao.Vidas ? "ui_coracao_cheio" : "ui_coracao_vazio");
                }
            }
            recargaAssobio.fillAmount = Mathf.Clamp01(recargaFracao);
            iconeAssobio.color = recargaFracao <= 0f ? Color.white : new Color(1f, 1f, 1f, 0.6f);

            if (piscar > 0f)
            {
                piscar -= Time.unscaledDeltaTime;
                float s = 1f + Mathf.Sin(piscar * 30f) * 0.15f;
                for (int i = 0; i < coracoes.Length; i++)
                    coracoes[i].rectTransform.localScale = new Vector3(s, s, 1f);
            }
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
