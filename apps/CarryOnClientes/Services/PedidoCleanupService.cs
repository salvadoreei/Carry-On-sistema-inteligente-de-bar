using BarPedidos.Models;
using System.Timers;

namespace BarPedidos.Services
{
    // Serviço de limpeza automática de pedidos
    // Remove automaticamente pedidos pagos ou cancelados após 1 minuto
    public class PedidoCleanupService
    {
        // Serviço Firebase para comunicar com a base de dados
        private readonly FirebaseService _firebaseService;

        // Timer que dispara periodicamente para verificar pedidos antigos
        private System.Timers.Timer _timer;

        // Flag que indica se o serviço está a funcionar
        private bool _isRunning = false;

        // Construtor: recebe o FirebaseService por injeção de dependências
        public PedidoCleanupService(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        // Inicia o serviço de limpeza automática
        public void Start()
        {
            // Se já está a funcionar, não faz nada (evita duplicação)
            if (_isRunning) return;

            // Cria timer que dispara a cada 15 segundos
            _timer = new System.Timers.Timer(15000);

            // Define o método que será executado quando o timer disparar
            _timer.Elapsed += OnTimerElapsed;

            // AutoReset = true: timer dispara continuamente (não apenas uma vez)
            _timer.AutoReset = true;

            // Inicia o timer
            _timer.Start();

            // Marca que o serviço está ativo
            _isRunning = true;
        }

        // Para o serviço de limpeza
        // É chamado quando a app vai para segundo plano (OnSleep)
        // Liberta recursos do timer
        public void Stop()
        {
            // Para o timer (se existir)
            _timer?.Stop();

            // Liberta os recursos do timer da memória
            _timer?.Dispose();

            // Marca que o serviço está inativo
            _isRunning = false;
        }

        // Método executado automaticamente a cada 15 segundos
        private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                // Chama o método que faz a limpeza dos pedidos
                await LimparPedidosPagosAntigos();
            }
            catch (Exception ex)
            {
                // Se der erro, escreve no log mas não para o serviço
                // O timer vai continuar a tentar na próxima execução
                System.Diagnostics.Debug.WriteLine($"Erro ao limpar pedidos: {ex.Message}");
            }
        }

        // Método que faz a limpeza efetiva dos pedidos antigos
        // Verifica todos os pedidos e remove os que já passaram do tempo limite
        // Pedidos pagos ou cancelados: removidos após 1 minuto
        private async Task LimparPedidosPagosAntigos()
        {
            // Obtém TODOS os pedidos da base de dados (sem filtro de dispositivo)
            var pedidos = await _firebaseService.ObterTodosPedidosAsync();

            // Guarda a data/hora atual para comparar com os pedidos
            var agora = DateTime.Now;

            // Percorre todos os pedidos um por um
            foreach (var pedido in pedidos)
            {
                // Flags para controlar se deve remover e porquê
                bool deveRemover = false;
                string motivo = "";

                // REGRA 1: Pedidos pagos há mais de 1 minuto
                // Verifica se está pago E se tem data de pagamento registada
                if (pedido.Status == StatusPedido.Pago &&
                    pedido.DataPagamento.HasValue &&
                    (agora - pedido.DataPagamento.Value).TotalMinutes >= 1)
                {
                    deveRemover = true;
                    // Calcula há quantos minutos foi pago (para o log)
                    motivo = $"pago há {(agora - pedido.DataPagamento.Value).TotalMinutes:F1} minutos";
                }

                // REGRA 2: Pedidos cancelados há mais de 1 minuto
                // Verifica se está cancelado E se tem data de cancelamento registada
                if (pedido.Status == StatusPedido.Cancelado &&
                    pedido.DataCancelamento.HasValue &&
                    (agora - pedido.DataCancelamento.Value).TotalMinutes >= 1)
                {
                    deveRemover = true;
                    // Calcula há quantos minutos foi cancelado (para o log)
                    motivo = $"cancelado há {(agora - pedido.DataCancelamento.Value).TotalMinutes:F1} minutos";
                }

                // Se cumpriu alguma das regras acima, remove o pedido
                if (deveRemover)
                {
                    // Remove o pedido da base de dados Firebase
                    await _firebaseService.RemoverPedidoAsync(pedido.Id);

                    // Escreve no log para debug
                    // Mostra o ID do pedido e o motivo da remoção
                    System.Diagnostics.Debug.WriteLine($"✅ Pedido {pedido.Id} removido automaticamente ({motivo})");
                }
            }
        }
    }
}
