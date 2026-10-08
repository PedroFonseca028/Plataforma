using System;

namespace RumoAoHexa
{
    public enum TipoChute
    {
        /// <summary>J: chute rasteiro, quica no chão e ricocheteia nas paredes.</summary>
        Rasteiro,
        /// <summary>↑+J ou K: bola alta, por cima dos obstáculos.</summary>
        Cavadinha,
        /// <summary>Segurar J até encher a barra: chute forte que atravessa vários argentinos.</summary>
        FolhaSeca,
    }

    /// <summary>Como a bola sai do pé em cada tipo de chute.</summary>
    public struct ParametrosChute
    {
        public float VelocidadeX;
        public float VelocidadeY;
        /// <summary>gravityScale da bola durante o voo.</summary>
        public float Gravidade;
        /// <summary>Quanto a velocidade vertical cai por segundo (a "folha seca" mergulha no fim).</summary>
        public float Curva;
        /// <summary>Passa por vários adversários em vez de ricochetear no primeiro.</summary>
        public bool Atravessa;
        /// <summary>Segundos de voo antes de a bola voltar sozinha para o pé.</summary>
        public float TempoMaximo;
    }

    public static class Chutes
    {
        /// <summary>Quanto da velocidade do jogador passa para a bola.</summary>
        public const float HerancaVelocidade = 0.35f;

        public static ParametrosChute Parametros(TipoChute tipo)
        {
            var p = new ParametrosChute();
            switch (tipo)
            {
                case TipoChute.Cavadinha:
                    p.VelocidadeX = 7.5f;
                    p.VelocidadeY = 14f;
                    p.Gravidade = 2.2f;
                    p.Atravessa = false;
                    p.TempoMaximo = 2.2f;
                    break;
                case TipoChute.FolhaSeca:
                    p.VelocidadeX = 21f;
                    p.VelocidadeY = 2.6f;
                    p.Gravidade = 0f;
                    p.Curva = 7f;
                    p.Atravessa = true;
                    p.TempoMaximo = 1.1f;
                    break;
                default:
                    p.VelocidadeX = 15f;
                    p.VelocidadeY = 3.5f;
                    p.Gravidade = 2.5f;
                    p.Atravessa = false;
                    p.TempoMaximo = 1.8f;
                    break;
            }
            return p;
        }

        /// <summary>
        /// Velocidade inicial da bola. <paramref name="direcao"/> é 1 (direita) ou -1 (esquerda).
        /// O jogador correndo para o mesmo lado deixa o chute mais forte.
        /// </summary>
        public static void VelocidadeInicial(TipoChute tipo, int direcao, float velocidadeJogadorX,
            out float vx, out float vy)
        {
            ParametrosChute p = Parametros(tipo);
            int d = direcao >= 0 ? 1 : -1;
            float heranca = velocidadeJogadorX * d > 0f ? velocidadeJogadorX * HerancaVelocidade : 0f;
            vx = p.VelocidadeX * d + heranca;
            vy = p.VelocidadeY;
        }
    }

    /// <summary>
    /// Decide o tipo de chute pelo tempo que o botão ficou apertado:
    /// toque rápido = rasteiro (ou cavadinha com ↑), segurar até a barra
    /// encher = folha seca.
    /// </summary>
    public class ControleChute
    {
        /// <summary>Segurar mais que isso já começa a encher a barra.</summary>
        public const float TempoInicioCarga = 0.2f;
        /// <summary>Segurando por esse tempo a barra fica cheia.</summary>
        public const float TempoCargaCompleta = 0.85f;

        public bool Segurando { get; private set; }
        public float TempoSegurado { get; private set; }

        /// <summary>0 a 1: quanto da barra da folha seca está cheia.</summary>
        public float Carga
        {
            get
            {
                if (!Segurando)
                    return 0f;
                float c = (TempoSegurado - TempoInicioCarga) / (TempoCargaCompleta - TempoInicioCarga);
                return Math.Max(0f, Math.Min(1f, c));
            }
        }

        public bool Carregando
        {
            get { return Segurando && TempoSegurado >= TempoInicioCarga; }
        }

        public bool CargaCheia
        {
            get { return Carga >= 1f; }
        }

        public void Apertar()
        {
            Segurando = true;
            TempoSegurado = 0f;
        }

        public void Atualizar(float dt)
        {
            if (Segurando)
                TempoSegurado += dt;
        }

        /// <summary>Soltou o botão: devolve o chute que deve sair.</summary>
        public TipoChute Soltar(bool cimaSegurado)
        {
            bool cheia = CargaCheia;
            Cancelar();
            if (cheia)
                return TipoChute.FolhaSeca;
            return cimaSegurado ? TipoChute.Cavadinha : TipoChute.Rasteiro;
        }

        public void Cancelar()
        {
            Segurando = false;
            TempoSegurado = 0f;
        }
    }

    /// <summary>Conta quantos argentinos uma mesma bola expulsou e calcula o bônus.</summary>
    public class ContadorCombo
    {
        public int Acertos { get; private set; }

        public string Nome
        {
            get { return Regras.NomeCombo(Acertos); }
        }

        public void Reiniciar()
        {
            Acertos = 0;
        }

        /// <summary>Registra mais um acerto e devolve o bônus a somar agora (pode ser 0).</summary>
        public int RegistrarAcerto()
        {
            int antes = Regras.BonusCombo(Acertos);
            Acertos++;
            return Regras.BonusCombo(Acertos) - antes;
        }
    }
}
