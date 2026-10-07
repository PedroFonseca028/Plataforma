using System;
using System.Collections.Generic;
using UnityEngine;

namespace RumoAoHexa
{
    /// <summary>
    /// Pequeno sintetizador "8 bits": cria todos os efeitos sonoros e as músicas
    /// do jogo por código (ondas quadrada, triangular, senoidal e ruído).
    /// Nenhum arquivo de áudio externo é necessário.
    /// </summary>
    public static class Sintetizador
    {
        public const int Frequencia = 22050;

        public enum Onda
        {
            Quadrada,
            QuadradaFina,
            Triangulo,
            Seno,
            Ruido,
        }

        static System.Random aleatorio = new System.Random(1234);

        // ------------------------------------------------------------ utilidades
        public static float FreqMidi(int nota)
        {
            return 440f * Mathf.Pow(2f, (nota - 69) / 12f);
        }

        /// <summary>Converte "C4", "F#3", "Bb5" em número MIDI (-1 se inválido).</summary>
        public static int Midi(string nome)
        {
            if (string.IsNullOrEmpty(nome))
                return -1;
            int[] semitons = { 9, 11, 0, 2, 4, 5, 7 }; // A B C D E F G
            char letra = char.ToUpperInvariant(nome[0]);
            if (letra < 'A' || letra > 'G')
                return -1;
            int n = semitons[letra - 'A'];
            int i = 1;
            if (i < nome.Length && nome[i] == '#') { n++; i++; }
            else if (i < nome.Length && nome[i] == 'b') { n--; i++; }
            int oitava;
            if (!int.TryParse(nome.Substring(i), out oitava))
                return -1;
            return (oitava + 1) * 12 + n;
        }

        static float Amostra(Onda onda, float fase)
        {
            fase -= Mathf.Floor(fase);
            switch (onda)
            {
                case Onda.Quadrada: return fase < 0.5f ? 0.8f : -0.8f;
                case Onda.QuadradaFina: return fase < 0.25f ? 0.8f : -0.8f;
                case Onda.Triangulo: return fase < 0.5f ? (fase * 4f - 1f) : (3f - fase * 4f);
                case Onda.Seno: return Mathf.Sin(fase * 2f * Mathf.PI);
                default: return (float)(aleatorio.NextDouble() * 2.0 - 1.0);
            }
        }

        /// <summary>Soma um tom (com glissando e envelope) dentro de um buffer.</summary>
        public static void Tom(float[] buffer, int inicio, float duracao, float freqInicial, float freqFinal,
            Onda onda, float volume, float ataque = 0.005f, float liberacao = 0.05f, float vibrato = 0f)
        {
            int total = Mathf.Max(1, (int)(duracao * Frequencia));
            float fase = 0f;
            for (int i = 0; i < total; i++)
            {
                int idx = inicio + i;
                if (idx < 0 || idx >= buffer.Length)
                    break;
                float t = i / (float)Frequencia;
                float p = i / (float)total;
                float f = Mathf.Lerp(freqInicial, freqFinal, p);
                if (vibrato > 0f)
                    f *= 1f + Mathf.Sin(t * 2f * Mathf.PI * 6f) * vibrato;
                fase += f / Frequencia;
                float env = 1f;
                if (t < ataque)
                    env = t / ataque;
                float resto = duracao - t;
                if (resto < liberacao)
                    env *= Mathf.Max(0f, resto / liberacao);
                buffer[idx] += Amostra(onda, fase) * volume * env;
            }
        }

        static AudioClip Clip(string nome, float[] dados, bool normalizar = true)
        {
            if (normalizar)
            {
                float max = 0.001f;
                for (int i = 0; i < dados.Length; i++)
                    max = Mathf.Max(max, Mathf.Abs(dados[i]));
                float ganho = max > 0.95f ? 0.95f / max : 1f;
                for (int i = 0; i < dados.Length; i++)
                    dados[i] *= ganho;
            }
            AudioClip clip = AudioClip.Create(nome, dados.Length, 1, Frequencia, false);
            clip.SetData(dados, 0);
            return clip;
        }

        static float[] Buffer(float segundos)
        {
            return new float[Mathf.Max(1, (int)(segundos * Frequencia))];
        }

        static int S(float segundos)
        {
            return (int)(segundos * Frequencia);
        }

        // ------------------------------------------------------------ efeitos sonoros
        public static AudioClip CriarEfeito(Som som)
        {
            float[] b;
            switch (som)
            {
                case Som.Pulo:
                    b = Buffer(0.18f);
                    Tom(b, 0, 0.18f, 280f, 620f, Onda.QuadradaFina, 0.35f);
                    break;
                case Som.Moeda:
                    b = Buffer(0.16f);
                    Tom(b, 0, 0.06f, 1318f, 1318f, Onda.Quadrada, 0.3f);
                    Tom(b, S(0.05f), 0.11f, 1976f, 1976f, Onda.Quadrada, 0.3f, 0.005f, 0.08f);
                    break;
                case Som.ItemObrigatorio:
                    b = Buffer(0.8f);
                    {
                        int[] notas = { 72, 76, 79, 84, 88, 91 };
                        for (int i = 0; i < notas.Length; i++)
                            Tom(b, S(i * 0.07f), 0.3f, FreqMidi(notas[i]), FreqMidi(notas[i]), Onda.Triangulo, 0.45f, 0.005f, 0.25f);
                        Tom(b, S(0.42f), 0.35f, FreqMidi(96), FreqMidi(96), Onda.Seno, 0.2f, 0.01f, 0.3f);
                    }
                    break;
                case Som.Vida:
                    b = Buffer(0.6f);
                    {
                        int[] notas = { 67, 71, 74, 79, 83, 86 };
                        for (int i = 0; i < notas.Length; i++)
                            Tom(b, S(i * 0.08f), 0.12f, FreqMidi(notas[i]), FreqMidi(notas[i]), Onda.Quadrada, 0.25f);
                    }
                    break;
                case Som.Dano:
                    b = Buffer(0.4f);
                    Tom(b, 0, 0.35f, 520f, 90f, Onda.Quadrada, 0.35f);
                    Tom(b, 0, 0.2f, 1f, 1f, Onda.Ruido, 0.25f);
                    break;
                case Som.Pisao:
                    // "boing" de quem pula na cabeça
                    b = Buffer(0.3f);
                    Tom(b, 0, 0.1f, 160f, 60f, Onda.Seno, 0.8f);
                    Tom(b, S(0.03f), 0.25f, 300f, 700f, Onda.Triangulo, 0.3f, 0.005f, 0.1f, 0.03f);
                    break;
                case Som.Chute:
                    // pancada seca no couro da bola
                    b = Buffer(0.2f);
                    Tom(b, 0, 0.09f, 220f, 70f, Onda.Seno, 0.9f, 0.001f, 0.06f);
                    Tom(b, 0, 0.04f, 1f, 1f, Onda.Ruido, 0.45f, 0.001f, 0.03f);
                    Tom(b, S(0.02f), 0.12f, 900f, 500f, Onda.QuadradaFina, 0.12f);
                    break;
                case Som.Cavadinha:
                    b = Buffer(0.25f);
                    Tom(b, 0, 0.07f, 260f, 120f, Onda.Seno, 0.7f, 0.001f, 0.05f);
                    Tom(b, S(0.03f), 0.2f, 500f, 1100f, Onda.Triangulo, 0.25f, 0.005f, 0.08f);
                    break;
                case Som.ChuteForte:
                    // a folha seca: estouro + assovio da bola cortando o ar
                    b = Buffer(0.7f);
                    Tom(b, 0, 0.16f, 180f, 40f, Onda.Seno, 1f, 0.001f, 0.1f);
                    Tom(b, 0, 0.12f, 1f, 1f, Onda.Ruido, 0.55f, 0.001f, 0.1f);
                    Tom(b, S(0.04f), 0.6f, 2400f, 700f, Onda.Seno, 0.18f, 0.02f, 0.3f, 0.02f);
                    break;
                case Som.CargaCheia:
                    b = Buffer(0.25f);
                    Tom(b, 0, 0.08f, FreqMidi(84), FreqMidi(84), Onda.QuadradaFina, 0.3f);
                    Tom(b, S(0.07f), 0.15f, FreqMidi(91), FreqMidi(91), Onda.QuadradaFina, 0.3f, 0.005f, 0.1f);
                    break;
                case Som.Quique:
                    b = Buffer(0.08f);
                    Tom(b, 0, 0.06f, 180f, 90f, Onda.Seno, 0.7f, 0.001f, 0.04f);
                    break;
                case Som.BolaVolta:
                    b = Buffer(0.3f);
                    Tom(b, 0, 0.28f, 1f, 1f, Onda.Ruido, 0.15f, 0.08f, 0.15f);
                    Tom(b, 0, 0.28f, 400f, 900f, Onda.Seno, 0.15f, 0.05f, 0.1f);
                    break;
                case Som.Expulso:
                    // cartão vermelho: "zap" descendo + estalo
                    b = Buffer(0.45f);
                    Tom(b, 0, 0.06f, 1f, 1f, Onda.Ruido, 0.4f, 0.001f, 0.05f);
                    Tom(b, S(0.02f), 0.35f, 1400f, 180f, Onda.Quadrada, 0.25f, 0.005f, 0.1f);
                    Tom(b, S(0.05f), 0.3f, 700f, 90f, Onda.Triangulo, 0.3f, 0.005f, 0.1f);
                    break;
                case Som.Combo:
                    b = Buffer(0.6f);
                    {
                        int[] notas = { 72, 79, 84, 88, 91, 96 };
                        for (int i = 0; i < notas.Length; i++)
                            Tom(b, S(i * 0.05f), 0.16f, FreqMidi(notas[i]), FreqMidi(notas[i]), Onda.Quadrada, 0.22f);
                    }
                    break;
                case Som.Bandeirinha:
                    b = Buffer(0.6f);
                    Tom(b, 0, 0.15f, FreqMidi(72), FreqMidi(72), Onda.Triangulo, 0.4f);
                    Tom(b, S(0.12f), 0.15f, FreqMidi(79), FreqMidi(79), Onda.Triangulo, 0.4f);
                    Tom(b, S(0.24f), 0.35f, FreqMidi(84), FreqMidi(84), Onda.Triangulo, 0.4f, 0.005f, 0.25f);
                    break;
                case Som.Clique:
                    b = Buffer(0.08f);
                    Tom(b, 0, 0.06f, 900f, 1200f, Onda.QuadradaFina, 0.25f);
                    break;
                case Som.Foco:
                    b = Buffer(0.05f);
                    Tom(b, 0, 0.04f, 660f, 660f, Onda.Triangulo, 0.2f);
                    break;
                case Som.Splash:
                    b = Buffer(0.6f);
                    Tom(b, 0, 0.55f, 1f, 1f, Onda.Ruido, 0.35f, 0.01f, 0.4f);
                    Tom(b, 0, 0.3f, 400f, 120f, Onda.Seno, 0.4f);
                    break;
                case Som.Queda:
                    b = Buffer(0.7f);
                    Tom(b, 0, 0.65f, 900f, 120f, Onda.Triangulo, 0.4f);
                    break;
                default:
                    b = Buffer(0.05f);
                    break;
            }
            return Clip("sfx_" + som, b);
        }

        // ------------------------------------------------------------ músicas
        /// <summary>
        /// Cada "passo" da partitura é uma semicolcheia (1/16).
        /// Tokens: "C4" = nota, "-" = segura a nota anterior, "." = pausa.
        /// </summary>
        static void Voz(float[] b, string partitura, float passo, Onda onda, float volume, int transpor = 0,
            float liberacao = 0.04f, float vibrato = 0f)
        {
            string[] t = partitura.Split(new[] { ' ', '\n', '\r', '\t', '|' }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            while (i < t.Length)
            {
                int midi = Midi(t[i]);
                if (midi < 0)
                {
                    i++;
                    continue;
                }
                int dur = 1;
                while (i + dur < t.Length && t[i + dur] == "-")
                    dur++;
                float f = FreqMidi(midi + transpor);
                Tom(b, S(i * passo), dur * passo * 0.95f, f, f, onda, volume, 0.004f, liberacao, vibrato);
                i += dur;
            }
        }

        /// <summary>Percussão: 'x' = batida forte, 'o' = batida fraca, '.' = nada.</summary>
        static void Bateria(float[] b, string padrao, int repeticoes, float passo, string tipo, float volume)
        {
            for (int r = 0; r < repeticoes; r++)
            {
                for (int i = 0; i < padrao.Length; i++)
                {
                    char c = padrao[i];
                    if (c != 'x' && c != 'o')
                        continue;
                    float v = c == 'x' ? volume : volume * 0.45f;
                    int ini = S((r * padrao.Length + i) * passo);
                    switch (tipo)
                    {
                        case "zabumba":
                            Tom(b, ini, 0.16f, 130f, 45f, Onda.Seno, v, 0.002f, 0.1f);
                            break;
                        case "caixa":
                            Tom(b, ini, 0.09f, 1f, 1f, Onda.Ruido, v * 0.6f, 0.001f, 0.08f);
                            Tom(b, ini, 0.06f, 260f, 180f, Onda.Triangulo, v * 0.5f);
                            break;
                        case "triangulo":
                            Tom(b, ini, 0.05f, 5200f, 5200f, Onda.Seno, v * 0.18f, 0.001f, 0.045f);
                            break;
                        case "chimbal":
                            Tom(b, ini, 0.03f, 1f, 1f, Onda.Ruido, v * 0.25f, 0.001f, 0.025f);
                            break;
                    }
                }
            }
        }

        public static AudioClip CriarMusica(Trilha trilha)
        {
            switch (trilha)
            {
                case Trilha.Menu: return MusicaMenu();
                case Trilha.Fase1: return MusicaFase1();
                case Trilha.Fase2: return MusicaFase2();
                case Trilha.Vitoria: return MusicaVitoria();
                case Trilha.Derrota: return MusicaDerrota();
            }
            return null;
        }

        /// <summary>Fase 1 (praia): baião em Sol mixolídio (zabumba, triângulo e baixo).</summary>
        static AudioClip MusicaFase1()
        {
            const float passo = 60f / 124f / 4f;
            const int compassos = 8;
            float[] b = Buffer(passo * 16 * compassos);
            string melodia =
                "D5 - - B4 - - G4 - A4 - B4 - C5 - B4 - " +
                "A4 - - F4 - - G4 - - - - - . . . . " +
                "C5 - - A4 - - F4 - G4 - A4 - C5 - A4 - " +
                "B4 - - G4 - - D4 - - - - - . . D5 E5 " +
                "F5 - - E5 - - C5 - E5 - G5 - E5 - C5 - " +
                "D5 - - B4 - - G4 - - - B4 - D5 - - - " +
                "A4 - - F4 - - D4 - F4 - A4 - C5 - B4 - " +
                "G4 - - - - - D4 - G4 - - - . . . . ";
            string baixo =
                "G2 - - D3 - - G2 - G2 - - D3 - - G3 - " +
                "G2 - - D3 - - G2 - G2 - - D3 - - G3 - " +
                "F2 - - C3 - - F2 - F2 - - C3 - - F3 - " +
                "G2 - - D3 - - G2 - G2 - - D3 - - G3 - " +
                "C3 - - G2 - - C3 - C3 - - G2 - - C3 - " +
                "G2 - - D3 - - G2 - G2 - - D3 - - G3 - " +
                "D3 - - A2 - - D3 - D3 - - A2 - - D3 - " +
                "G2 - - D3 - - G2 - G2 - - D3 - - G2 - ";
            string harmonia =
                "B3 - - - - - - - D4 - - - - - - - " +
                "B3 - - - - - - - D4 - - - - - - - " +
                "A3 - - - - - - - C4 - - - - - - - " +
                "B3 - - - - - - - D4 - - - - - - - " +
                "E4 - - - - - - - G4 - - - - - - - " +
                "D4 - - - - - - - B3 - - - - - - - " +
                "F4 - - - - - - - A3 - - - - - - - " +
                "B3 - - - - - - - D4 - - - - - - - ";
            Voz(b, melodia, passo, Onda.QuadradaFina, 0.22f, 0, 0.05f, 0.004f);
            Voz(b, baixo, passo, Onda.Triangulo, 0.45f);
            Voz(b, harmonia, passo, Onda.Triangulo, 0.12f, 0, 0.2f);
            Bateria(b, "x..x....x..x....", compassos, passo, "zabumba", 0.55f);
            Bateria(b, "....x.......x..o", compassos, passo, "caixa", 0.35f);
            Bateria(b, "xoxoxoxoxoxoxoxo", compassos, passo, "triangulo", 1f);
            return Clip("musica_fase1", b);
        }

        /// <summary>Fase 2 (a final): tensão da noite de decisão, em Ré menor (dórico), com arpejos.</summary>
        static AudioClip MusicaFase2()
        {
            const float passo = 60f / 100f / 4f;
            const int compassos = 8;
            float[] b = Buffer(passo * 16 * compassos);
            string arpejo =
                "D4 F4 A4 D5 A4 F4 D4 F4 A4 D5 A4 F4 D4 F4 A4 F4 " +
                "Bb3 D4 F4 Bb4 F4 D4 Bb3 D4 F4 Bb4 F4 D4 Bb3 D4 F4 D4 " +
                "C4 E4 G4 C5 G4 E4 C4 E4 G4 C5 G4 E4 C4 E4 G4 E4 " +
                "A3 C4 E4 A4 E4 C4 A3 C4 E4 A4 E4 C4 A3 C#4 E4 A4 " +
                "D4 F4 A4 D5 A4 F4 D4 F4 A4 D5 A4 F4 D4 F4 A4 F4 " +
                "Bb3 D4 F4 Bb4 F4 D4 Bb3 D4 F4 Bb4 F4 D4 Bb3 D4 F4 D4 " +
                "G3 Bb3 D4 G4 D4 Bb3 G3 Bb3 D4 G4 D4 Bb3 G3 Bb3 D4 Bb3 " +
                "A3 C#4 E4 A4 E4 C#4 A3 C#4 E4 A4 E4 C#4 A3 C#4 E4 G4 ";
            string melodia =
                "A4 - - - - - - - F4 - - - E4 - D4 - " +
                "F4 - - - - - - - . . . . D4 - F4 - " +
                "G4 - - - - - - - E4 - - - C4 - E4 - " +
                "A4 - - - - - - - . . . . . . . . " +
                "D5 - - - - - - - C5 - - - A4 - F4 - " +
                "Bb4 - - - - - - - A4 - - - F4 - D4 - " +
                "G4 - - - - - - - Bb4 - - - A4 - G4 - " +
                "A4 - - - - - - - - - - - . . . . ";
            string baixo =
                "D2 - - - - - - - D2 - - - - - - - " +
                "Bb1 - - - - - - - Bb1 - - - - - - - " +
                "C2 - - - - - - - C2 - - - - - - - " +
                "A1 - - - - - - - A1 - - - - - - - " +
                "D2 - - - - - - - D2 - - - - - - - " +
                "Bb1 - - - - - - - Bb1 - - - - - - - " +
                "G1 - - - - - - - G1 - - - - - - - " +
                "A1 - - - - - - - A1 - - - - - - - ";
            Voz(b, arpejo, passo, Onda.Triangulo, 0.16f, 0, 0.06f);
            Voz(b, melodia, passo, Onda.Quadrada, 0.12f, 0, 0.25f, 0.006f);
            Voz(b, baixo, passo, Onda.Triangulo, 0.4f, 12, 0.3f);
            Bateria(b, "x.......x.....o.", compassos, passo, "zabumba", 0.5f);
            Bateria(b, "..o...o...o...o.", compassos, passo, "chimbal", 0.8f);
            return Clip("musica_fase2", b);
        }

        /// <summary>Menu: tema calmo em Dó maior pentatônico.</summary>
        static AudioClip MusicaMenu()
        {
            const float passo = 60f / 88f / 4f;
            const int compassos = 8;
            float[] b = Buffer(passo * 16 * compassos);
            string melodia =
                "E5 - - - D5 - C5 - - - G4 - A4 - C5 - " +
                "D5 - - - - - - - E5 - D5 - C5 - - - " +
                "A4 - - - C5 - D5 - - - E5 - G5 - E5 - " +
                "D5 - - - - - - - . . . . . . . . " +
                "E5 - - - G5 - A5 - - - G5 - E5 - D5 - " +
                "C5 - - - - - - - D5 - E5 - D5 - - - " +
                "A4 - - - C5 - D5 - - - E5 - D5 - C5 - " +
                "C5 - - - - - - - . . . . . . . . ";
            string acordes =
                "C3 G3 C4 E4 G3 C4 E4 G3 C3 G3 C4 E4 G3 C4 E4 G3 " +
                "G2 D3 G3 B3 D3 G3 B3 D3 G2 D3 G3 B3 D3 G3 B3 D3 " +
                "A2 E3 A3 C4 E3 A3 C4 E3 A2 E3 A3 C4 E3 A3 C4 E3 " +
                "G2 D3 G3 B3 D3 G3 B3 D3 G2 D3 G3 B3 D3 G3 B3 D3 " +
                "F2 C3 F3 A3 C3 F3 A3 C3 F2 C3 F3 A3 C3 F3 A3 C3 " +
                "C3 G3 C4 E4 G3 C4 E4 G3 C3 G3 C4 E4 G3 C4 E4 G3 " +
                "F2 C3 F3 A3 C3 F3 A3 C3 G2 D3 G3 B3 D3 G3 B3 D3 " +
                "C3 G3 C4 E4 G3 C4 E4 G3 C3 G3 C4 E4 G3 C4 E4 G3 ";
            Voz(b, melodia, passo, Onda.Triangulo, 0.3f, 0, 0.15f, 0.005f);
            Voz(b, acordes, passo, Onda.Seno, 0.14f, 0, 0.08f);
            Bateria(b, "x.......o.......", compassos, passo, "zabumba", 0.35f);
            Bateria(b, "..o...o...o...o.", compassos, passo, "triangulo", 0.8f);
            return Clip("musica_menu", b);
        }

        static AudioClip MusicaVitoria()
        {
            const float passo = 60f / 132f / 4f;
            float[] b = Buffer(passo * 16 * 4);
            string melodia =
                "G4 - C5 - E5 - G5 - - - E5 - G5 - - - " +
                "A5 - - - G5 - - - E5 - - - C5 - D5 - " +
                "E5 - - - D5 - - - C5 - - - D5 - E5 - " +
                "C5 - - - - - - - - - - - . . . . ";
            string baixo =
                "C3 - - - G2 - - - C3 - - - G2 - - - " +
                "F2 - - - C3 - - - F2 - - - G2 - - - " +
                "A2 - - - E3 - - - F2 - - - G2 - - - " +
                "C3 - - - - - - - - - - - . . . . ";
            Voz(b, melodia, passo, Onda.QuadradaFina, 0.25f, 0, 0.1f, 0.004f);
            Voz(b, baixo, passo, Onda.Triangulo, 0.4f);
            Bateria(b, "x..x....x..x....", 3, passo, "zabumba", 0.5f);
            Bateria(b, "xoxoxoxoxoxoxoxo", 3, passo, "triangulo", 1f);
            return Clip("musica_vitoria", b);
        }

        static AudioClip MusicaDerrota()
        {
            const float passo = 60f / 80f / 4f;
            float[] b = Buffer(passo * 16 * 3);
            string melodia =
                "A4 - - - G4 - - - F4 - - - E4 - - - " +
                "D4 - - - F4 - - - E4 - - - C#4 - - - " +
                "D4 - - - - - - - - - - - . . . . ";
            string baixo =
                "D3 - - - - - - - Bb2 - - - - - - - " +
                "G2 - - - - - - - A2 - - - - - - - " +
                "D2 - - - - - - - - - - - . . . . ";
            Voz(b, melodia, passo, Onda.Triangulo, 0.35f, 0, 0.2f, 0.01f);
            Voz(b, baixo, passo, Onda.Triangulo, 0.35f, 0, 0.3f);
            return Clip("musica_derrota", b);
        }
    }

    public enum Som
    {
        Pulo,
        Moeda,
        ItemObrigatorio,
        Vida,
        Dano,
        Pisao,
        Chute,
        Cavadinha,
        ChuteForte,
        CargaCheia,
        Quique,
        BolaVolta,
        Expulso,
        Combo,
        Bandeirinha,
        Clique,
        Foco,
        Splash,
        Queda,
    }

    public enum Trilha
    {
        Nenhuma,
        Menu,
        Fase1,
        Fase2,
        Vitoria,
        Derrota,
    }
}
