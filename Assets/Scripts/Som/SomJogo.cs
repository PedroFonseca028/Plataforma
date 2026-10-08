using System.Collections.Generic;
using UnityEngine;

namespace RumoAoHexa
{
    /// <summary>
    /// Toca músicas e efeitos sonoros. É criado automaticamente na primeira vez
    /// que for usado e sobrevive à troca de cenas (DontDestroyOnLoad).
    /// </summary>
    public class SomJogo : MonoBehaviour
    {
        static SomJogo instancia;
        static bool mudo;

        AudioSource fonteMusica;
        AudioSource fonteEfeitos;
        readonly Dictionary<Som, AudioClip> efeitos = new Dictionary<Som, AudioClip>();
        readonly Dictionary<Trilha, AudioClip> musicas = new Dictionary<Trilha, AudioClip>();
        Trilha trilhaAtual = Trilha.Nenhuma;
        float volumeMusica = 0.45f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ZerarEstaticos()
        {
            instancia = null;
            mudo = false;
        }

        static SomJogo Instancia
        {
            get
            {
                if (instancia == null)
                {
                    var go = new GameObject("SomJogo");
                    DontDestroyOnLoad(go);
                    instancia = go.AddComponent<SomJogo>();
                }
                return instancia;
            }
        }

        public static bool Mudo
        {
            get { return mudo; }
        }

        void Awake()
        {
            fonteMusica = gameObject.AddComponent<AudioSource>();
            fonteMusica.loop = true;
            fonteMusica.playOnAwake = false;
            fonteMusica.spatialBlend = 0f;
            fonteMusica.volume = volumeMusica;

            fonteEfeitos = gameObject.AddComponent<AudioSource>();
            fonteEfeitos.loop = false;
            fonteEfeitos.playOnAwake = false;
            fonteEfeitos.spatialBlend = 0f;
            fonteEfeitos.volume = 0.8f;
            AplicarMudo();
        }

        void OnDestroy()
        {
            // libera os áudios criados por código
            foreach (AudioClip c in efeitos.Values)
                if (c != null) Destroy(c);
            foreach (AudioClip c in musicas.Values)
                if (c != null) Destroy(c);
            if (instancia == this)
                instancia = null;
        }

        public static void Tocar(Som som, float volume = 1f)
        {
            SomJogo s = Instancia;
            AudioClip clip;
            if (!s.efeitos.TryGetValue(som, out clip) || clip == null)
            {
                clip = Sintetizador.CriarEfeito(som);
                s.efeitos[som] = clip;
            }
            s.fonteEfeitos.PlayOneShot(clip, volume);
        }

        /// <summary>Troca a música (não reinicia se já estiver tocando a mesma).</summary>
        public static void Musica(Trilha trilha, bool repetir = true)
        {
            SomJogo s = Instancia;
            if (s.trilhaAtual == trilha && s.fonteMusica.isPlaying)
                return;
            s.trilhaAtual = trilha;
            s.fonteMusica.Stop();
            if (trilha == Trilha.Nenhuma)
                return;
            AudioClip clip;
            if (!s.musicas.TryGetValue(trilha, out clip) || clip == null)
            {
                clip = Sintetizador.CriarMusica(trilha);
                s.musicas[trilha] = clip;
            }
            s.fonteMusica.clip = clip;
            s.fonteMusica.loop = repetir;
            s.fonteMusica.Play();
        }

        /// <summary>Deixa a música mais baixa (ex.: no menu de pausa).</summary>
        public static void AbafarMusica(bool abafar)
        {
            SomJogo s = Instancia;
            s.fonteMusica.volume = abafar ? s.volumeMusica * 0.35f : s.volumeMusica;
        }

        public static void AlternarMudo()
        {
            mudo = !mudo;
            Instancia.AplicarMudo();
        }

        void AplicarMudo()
        {
            fonteMusica.mute = mudo;
            fonteEfeitos.mute = mudo;
        }
    }
}
