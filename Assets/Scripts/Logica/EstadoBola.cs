using System;

namespace RumoAoHexa
{
    public enum SituacaoBola
    {
        /// <summary>Dominada no pé do jogador (pronta para chutar).</summary>
        NoPe,
        /// <summary>Chutada, quicando pelo cenário.</summary>
        Voando,
        /// <summary>Voltando sozinha para o pé (não acerta ninguém).</summary>
        Voltando,
    }

    /// <summary>
    /// Regras da bola: quando pode ser chutada, quando volta para o pé e
    /// quando o jogador consegue dominá-la de novo. O componente <c>Bola</c>
    /// cuida da física e do desenho; aqui fica só a decisão.
    /// </summary>
    public class EstadoBola
    {
        /// <summary>Abaixo dessa velocidade a bola é considerada parada.</summary>
        public const float VelocidadeParada = 0.8f;
        /// <summary>Parada por esse tempo, a bola volta sozinha.</summary>
        public const float TempoParadaParaVoltar = 0.35f;
        /// <summary>Logo depois do chute o jogador não "pega" a bola de novo sem querer.</summary>
        public const float TempoMinimoParaDominar = 0.3f;
        /// <summary>Bola mais lenta que isso pode ser dominada encostando nela.</summary>
        public const float VelocidadeMaximaParaDominar = 4.5f;
        public const float DistanciaParaDominar = 0.75f;
        /// <summary>Voltando, a bola é dominada quando chega a essa distância do pé.</summary>
        public const float DistanciaChegada = 0.45f;

        public SituacaoBola Situacao { get; private set; }
        public TipoChute UltimoChute { get; private set; }
        public float TempoVoo { get; private set; }
        public float TempoVoltando { get; private set; }
        public ContadorCombo Combo { get; private set; }

        float tempoMaximo;
        float tempoParada;

        public EstadoBola()
        {
            Situacao = SituacaoBola.NoPe;
            Combo = new ContadorCombo();
        }

        public bool PodeChutar
        {
            get { return Situacao == SituacaoBola.NoPe; }
        }

        /// <summary>Só a bola em voo expulsa argentinos.</summary>
        public bool PodeAcertar
        {
            get { return Situacao == SituacaoBola.Voando; }
        }

        public bool Atravessa
        {
            get { return Situacao == SituacaoBola.Voando && Chutes.Parametros(UltimoChute).Atravessa; }
        }

        public bool Chutar(TipoChute tipo)
        {
            if (!PodeChutar)
                return false;
            Situacao = SituacaoBola.Voando;
            UltimoChute = tipo;
            TempoVoo = 0f;
            TempoVoltando = 0f;
            tempoParada = 0f;
            tempoMaximo = Chutes.Parametros(tipo).TempoMaximo;
            Combo.Reiniciar();
            return true;
        }

        /// <summary>
        /// Avança o voo. <paramref name="velocidade"/> é o módulo da velocidade da bola e
        /// <paramref name="distanciaAoPe"/> a distância até o pé do jogador.
        /// </summary>
        public void AtualizarVoo(float dt, float velocidade, float distanciaAoPe, bool caiuDoMapa)
        {
            if (Situacao != SituacaoBola.Voando)
                return;
            TempoVoo += dt;
            tempoParada = velocidade < VelocidadeParada ? tempoParada + dt : 0f;

            if (TempoVoo >= TempoMinimoParaDominar && velocidade <= VelocidadeMaximaParaDominar &&
                distanciaAoPe <= DistanciaParaDominar)
            {
                Dominar();
                return;
            }
            if (caiuDoMapa || TempoVoo >= tempoMaximo || tempoParada >= TempoParadaParaVoltar)
                Chamar();
        }

        /// <summary>Faz a bola voltar para o pé (apertar J com a bola longe também chama).</summary>
        public void Chamar()
        {
            if (Situacao != SituacaoBola.Voando)
                return;
            Situacao = SituacaoBola.Voltando;
            TempoVoltando = 0f;
        }

        public void AtualizarVolta(float dt, float distanciaAoPe)
        {
            if (Situacao != SituacaoBola.Voltando)
                return;
            TempoVoltando += dt;
            if (distanciaAoPe <= DistanciaChegada)
                Dominar();
        }

        public void Dominar()
        {
            Situacao = SituacaoBola.NoPe;
            TempoVoo = 0f;
            TempoVoltando = 0f;
            tempoParada = 0f;
        }

        /// <summary>A bola volta cada vez mais rápido (assim nunca fica para trás).</summary>
        public static float VelocidadeVolta(float tempoVoltando)
        {
            return Math.Min(14f + tempoVoltando * 30f, 34f);
        }
    }
}
