using System;

namespace RumoAoHexa
{
    public enum ResultadoJogo
    {
        Nenhum,
        Vitoria,
        Derrota,
    }

    /// <summary>
    /// Dados da partida (pontos, vidas, moedas, itens e expulsões) e as regras
    /// para mudá-los. A classe <c>Sessao</c> guarda uma instância que atravessa
    /// as cenas; aqui não há nada do Unity, para dar para testar fora dele.
    /// </summary>
    public class EstadoPartida
    {
        public int Pontos { get; private set; }
        public int Vidas { get; private set; }
        public int Moedas { get; private set; }
        /// <summary>Estrelas + letras coletadas no jogo todo.</summary>
        public int ItensColetados { get; private set; }
        /// <summary>Argentinos expulsos no jogo todo.</summary>
        public int Expulsos { get; private set; }
        public int FaseAtual { get; private set; }
        public ResultadoJogo Resultado { get; private set; }
        public bool NovoRecorde { get; private set; }

        // valores do começo da fase (usados em "Reiniciar fase")
        int pontosInicio;
        int vidasInicio;
        int moedasInicio;
        int itensInicio;
        int expulsosInicio;

        public EstadoPartida()
        {
            NovoJogo();
        }

        public void NovoJogo()
        {
            Pontos = 0;
            Vidas = Regras.VidasIniciais;
            Moedas = 0;
            ItensColetados = 0;
            Expulsos = 0;
            Resultado = ResultadoJogo.Nenhum;
            NovoRecorde = false;
            MarcarInicioFase(1);
        }

        public void MarcarInicioFase(int fase)
        {
            FaseAtual = fase;
            pontosInicio = Pontos;
            vidasInicio = Vidas;
            moedasInicio = Moedas;
            itensInicio = ItensColetados;
            expulsosInicio = Expulsos;
        }

        /// <summary>Volta tudo para como estava no começo da fase (com pelo menos 1 vida).</summary>
        public void RestaurarInicioFase()
        {
            Pontos = pontosInicio;
            Vidas = Math.Max(1, vidasInicio);
            Moedas = moedasInicio;
            ItensColetados = itensInicio;
            Expulsos = expulsosInicio;
            Resultado = ResultadoJogo.Nenhum;
        }

        public void SomarPontos(int valor)
        {
            Pontos = Math.Max(0, Pontos + valor);
        }

        public void ColetarMoeda()
        {
            Moedas++;
            SomarPontos(Regras.PontosMoeda);
        }

        public void ColetarItemObrigatorio()
        {
            ItensColetados++;
            SomarPontos(Regras.PontosItemObrigatorio);
        }

        /// <summary>Ganha uma vida. Se já estiver no máximo, ganha pontos e devolve false.</summary>
        public bool ColetarCoracao()
        {
            if (Vidas < Regras.VidasMaximas)
            {
                Vidas++;
                return true;
            }
            SomarPontos(Regras.PontosCoracaoExtra);
            return false;
        }

        /// <summary>Tira uma vida. Devolve true se as vidas acabaram.</summary>
        public bool PerderVida()
        {
            if (Vidas > 0)
                Vidas--;
            return Vidas <= 0;
        }

        public void ZerarVidas()
        {
            Vidas = 0;
        }

        /// <summary>Um argentino foi expulso pela bola (o bônus de combo é somado à parte).</summary>
        public void RegistrarExpulsao()
        {
            Expulsos++;
            SomarPontos(Regras.PontosExpulsao);
        }

        public void BonusDeFase()
        {
            SomarPontos(Regras.BonusFase);
        }

        /// <summary>Encerra a partida. Devolve true se bateu o recorde anterior.</summary>
        public bool Finalizar(ResultadoJogo resultado, int recordeAnterior)
        {
            Resultado = resultado;
            NovoRecorde = Pontos > recordeAnterior;
            return NovoRecorde;
        }
    }
}
