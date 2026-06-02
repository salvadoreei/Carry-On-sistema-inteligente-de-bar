using BarPedidos.Models;
using BarPedidos.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BarPedidos.ViewModels
{
    // ViewModel da página de pedidos (histórico do cliente)
    // Mostra apenas os pedidos deste dispositivo (filtrados por DeviceId)
    // Permite ver detalhes e cancelar pedidos pendentes
    public partial class PedidosViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;

        // Lista completa de pedidos deste dispositivo
        [ObservableProperty]
        private ObservableCollection<Pedido> pedidos;

        // Lista filtrada exibida na interface
        [ObservableProperty]
        private ObservableCollection<Pedido> pedidosFiltrados;

        // Indica se está a atualizar pedidos (pull-to-refresh)
        [ObservableProperty]
        private bool isRefreshing;

        public PedidosViewModel(DeviceService deviceService, FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
            Pedidos = new ObservableCollection<Pedido>();
            PedidosFiltrados = new ObservableCollection<Pedido>();
        }

        // Carrega pedidos do Firebase
        // FirebaseService já filtra automaticamente por DeviceId
        [RelayCommand]
        public async Task CarregarPedidosAsync()
        {
            try
            {
                // ObterPedidosAsync retorna apenas pedidos deste dispositivo
                var pedidosFirebase = await _firebaseService.ObterPedidosAsync();

                Pedidos.Clear();
                foreach (var pedido in pedidosFirebase)
                {
                    Pedidos.Add(pedido);
                }

                FiltrarPedidos();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar pedidos: {ex.Message}", "OK");
            }
        }

        // Pull-to-refresh: atualiza lista de pedidos
        [RelayCommand]
        public async Task RefreshPedidosAsync()
        {
            IsRefreshing = true;
            await CarregarPedidosAsync();
            IsRefreshing = false;
        }

        // Navega de volta para a página principal (catálogo)
        [RelayCommand]
        public async Task IrParaCardapio()
        {
            await Shell.Current.GoToAsync("//MainPage");
        }

        // Atualiza o status de um pedido manualmente
        // Cliente pode ver e alterar status
        [RelayCommand]
        public async Task AtualizarStatusAsync(Pedido pedido)
        {
            var statusOpcoes = new[] { "Pendente", "Em Preparação", "Pronto", "Entregue", "Pago", "Cancelado" };

            var resultado = await Shell.Current.DisplayActionSheet(
                "Atualizar Status",
                "Cancelar",
                null,
                statusOpcoes);

            if (resultado != null && resultado != "Cancelar")
            {
                // Converte texto selecionado para enum StatusPedido
                StatusPedido novoStatus = resultado switch
                {
                    "Pendente" => StatusPedido.Pendente,
                    "Em Preparação" => StatusPedido.EmPreparacao,
                    "Pronto" => StatusPedido.Pronto,
                    "Entregue" => StatusPedido.Entregue,
                    "Pago" => StatusPedido.Pago,
                    "Cancelado" => StatusPedido.Cancelado,
                    _ => pedido.Status
                };

                bool sucesso = await _firebaseService.AtualizarStatusPedidoAsync(pedido.Id, novoStatus);

                if (sucesso)
                {
                    pedido.Status = novoStatus;
                    await CarregarPedidosAsync();
                }
            }
        }

        // Aplica filtros aos pedidos (atualmente sem filtros, mostra todos)
        private void FiltrarPedidos()
        {
            PedidosFiltrados.Clear();

            foreach (var pedido in Pedidos)
            {
                PedidosFiltrados.Add(pedido);
            }
        }

        // Mostra detalhes completos do pedido em popup
        // Inclui itens, total, observações e tempo para remoção automática
        [RelayCommand]
        public async Task VerDetalhesPedidoAsync(Pedido pedido)
        {
            if (pedido == null) return;

            // Monta lista de itens com quantidade e subtotal
            var itens = string.Join("\n", pedido.Itens.Select(i =>
                $"• {i.Produto.Nome} x{i.Quantidade} - €{i.Subtotal:F2}"));

            // Monta mensagem principal com informações do pedido
            var mensagem = $"Mesa: {pedido.NumeroMesa}\n" +
                          $"Status: {pedido.Status}\n" +
                          $"Data: {pedido.DataHora:dd/MM/yyyy HH:mm}\n\n" +
                          $"Itens:\n{itens}\n\n" +
                          $"Total: €{pedido.Total:F2}";

            // Adiciona observações se existirem
            if (!string.IsNullOrWhiteSpace(pedido.Observacoes))
            {
                mensagem += $"\n\nObservações: \n{pedido.Observacoes}";
            }

            // Se pedido foi pago, mostra tempo restante até remoção automática (1 minuto)
            if (pedido.Status == StatusPedido.Pago && pedido.DataPagamento.HasValue)
            {
                var tempoDecorrido = DateTime.Now - pedido.DataPagamento.Value;
                var segundosRestantes = 60 - (int)tempoDecorrido.TotalSeconds;
                if (segundosRestantes > 0)
                {
                    mensagem += $"\n\nSerá removido em ~{segundosRestantes} segundo(s)";
                }
            }

            // Se pedido foi cancelado, mostra tempo restante até remoção automática (1 minuto)
            if (pedido.Status == StatusPedido.Cancelado && pedido.DataCancelamento.HasValue)
            {
                var tempoDecorrido = DateTime.Now - pedido.DataCancelamento.Value;
                var segundosRestantes = 60 - (int)tempoDecorrido.TotalSeconds;
                if (segundosRestantes > 0)
                {
                    mensagem += $"\n\nSerá removido em ~{segundosRestantes} segundo(s)";
                }
            }

            await Shell.Current.DisplayAlert($"Pedido #{pedido.Id.Substring(0, 8)}", mensagem, "OK");
        }

        // Cancela um pedido pendente
        // Apenas pedidos com status Pendente podem ser cancelados pelo cliente
        [RelayCommand]
        public async Task CancelarPedidoAsync(Pedido pedido)
        {
            // Valida se o pedido pode ser cancelado
            if (pedido == null || pedido.Status != StatusPedido.Pendente)
            {
                await Shell.Current.DisplayAlert("Aviso", "Apenas pedidos pendentes podem ser cancelados.", "OK");
                return;
            }

            // Pede confirmação ao cliente
            bool confirmar = await Shell.Current.DisplayAlert(
                "Cancelar Pedido",
                $"Tem certeza que deseja cancelar o pedido da mesa {pedido.NumeroMesa}?",
                "Sim, cancelar",
                "Não"
            );

            if (confirmar)
            {
                // Atualiza status para Cancelado no Firebase
                bool sucesso = await _firebaseService.AtualizarStatusPedidoAsync(pedido.Id, StatusPedido.Cancelado);

                if (sucesso)
                {
                    // Recarrega lista para refletir mudança
                    // PedidoCleanupService removerá após 1 minuto
                    await CarregarPedidosAsync();
                }
            }
        }
    }
}
