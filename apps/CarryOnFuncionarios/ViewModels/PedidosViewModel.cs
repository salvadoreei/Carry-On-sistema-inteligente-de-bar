using BarPedidosFuncionarios.Models;
using BarPedidosFuncionarios.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BarPedidosFuncionarios.ViewModels
{
    // ViewModel da página de gestão de pedidos (barman)
    // Mostra TODOS os pedidos de todos os dispositivos (sem filtro por DeviceId)
    // Permite visualizar detalhes e alterar status dos pedidos
    public partial class PedidosViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;

        // Lista completa de todos os pedidos ativos
        [ObservableProperty]
        private ObservableCollection<Pedido> pedidos;

        // Lista filtrada exibida na interface (exclui pedidos pagos e cancelados)
        [ObservableProperty]
        private ObservableCollection<Pedido> pedidosFiltrados;

        // Indica se está a atualizar pedidos (pull-to-refresh)
        [ObservableProperty]
        private bool isRefreshing;

        // Contador de pedidos ativos (exibido no header)
        [ObservableProperty]
        private int totalPedidos;

        public PedidosViewModel(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
            Pedidos = new ObservableCollection<Pedido>();
            PedidosFiltrados = new ObservableCollection<Pedido>();
        }

        // Carrega todos os pedidos do Firebase
        // Filtra apenas pedidos ativos (exclui Pago e Cancelado)
        // Ordena por data/hora (mais antigos primeiro)
        [RelayCommand]
        private async Task CarregarPedidos()
        {
            try
            {
                // Obtém TODOS os pedidos (sem filtro de DeviceId)
                var todosPedidos = await _firebaseService.ObterPedidosAsync();

                // Filtra apenas pedidos ativos (Pendente, EmPreparacao, Pronto, Entregue)
                // Pedidos Pago e Cancelado são removidos automaticamente após 1 minuto
                var pedidosAtivos = todosPedidos
                    .Where(p => p.Status != StatusPedido.Pago && p.Status != StatusPedido.Cancelado)
                    .OrderBy(p => p.DataHora)  // Mais antigos primeiro
                    .ToList();

                Pedidos.Clear();
                foreach (var pedido in pedidosAtivos)
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
        private async Task RefreshPedidos()
        {
            IsRefreshing = true;
            await CarregarPedidos();
            IsRefreshing = false;
        }

        // Aplica filtros aos pedidos (atualmente sem filtros, mostra todos os ativos)
        // Atualiza contador de pedidos
        private void FiltrarPedidos()
        {
            PedidosFiltrados.Clear();
            foreach (var pedido in Pedidos)
            {
                PedidosFiltrados.Add(pedido);
            }

            TotalPedidos = PedidosFiltrados.Count;
        }

        // Mostra detalhes completos do pedido em popup
        // Inclui mesa, status, hora, itens, total e observações
        [RelayCommand]
        private async Task VerDetalhes(Pedido pedido)
        {
            if (pedido == null) return;

            // Monta lista de itens com quantidade e subtotal
            var itens = pedido.Itens?.Count > 0
                ? string.Join(Environment.NewLine, pedido.Itens.Select(i => $"• {i.Produto?.Nome ?? "Item"} x{i.Quantidade} - €{i.Subtotal:F2}"))
                : "Nenhum item";

            // Monta mensagem com informações do pedido
            var mensagem = $"Mesa: {pedido.NumeroMesa}{Environment.NewLine}" +
                          $"Status: {ObterTextoStatus(pedido.Status)}{Environment.NewLine}" +
                          $"Hora: {pedido.DataHora:HH:mm}{Environment.NewLine}{Environment.NewLine}" +
                          $"Itens:{Environment.NewLine}{itens}{Environment.NewLine}{Environment.NewLine}" +
                          $"Total: €{pedido.Total:F2}";

            // Adiciona observações se existirem
            if (!string.IsNullOrWhiteSpace(pedido.Observacoes))
            {
                mensagem += $"{Environment.NewLine}{Environment.NewLine}Observações:{Environment.NewLine}{pedido.Observacoes}";
            }

            await Shell.Current.DisplayAlert($"Pedido #{pedido.Id.Substring(0, 8)}", mensagem, "OK");
        }

        // Altera o status de um pedido
        // Mostra menu com opções: Pendente, Em Preparação, Pronto, Entregue, Pago, Cancelar
        // Pede confirmação adicional para cancelamento
        [RelayCommand]
        private async Task AlterarStatus(Pedido pedido)
        {
            if (pedido == null) return;

            var opcoes = new[] { "Pendente", "Em Preparação", "Pronto", "Entregue", "Pago", "Cancelar Pedido" };

            var resultado = await Shell.Current.DisplayActionSheet(
                "Alterar Status do Pedido",
                "Fechar",
                null,
                opcoes);

            if (resultado == null || resultado == "Fechar") return;

            // Converte texto selecionado para enum StatusPedido
            StatusPedido novoStatus = resultado switch
            {
                "Pendente" => StatusPedido.Pendente,
                "Em Preparação" => StatusPedido.EmPreparacao,
                "Pronto" => StatusPedido.Pronto,
                "Entregue" => StatusPedido.Entregue,
                "Pago" => StatusPedido.Pago,
                "Cancelar Pedido" => StatusPedido.Cancelado,
                _ => pedido.Status
            };

            // Pede confirmação extra para cancelamento
            if (novoStatus == StatusPedido.Cancelado)
            {
                bool confirmar = await Shell.Current.DisplayAlert(
                    "Confirmar Cancelamento",
                    $"Tem certeza que deseja cancelar o pedido da mesa {pedido.NumeroMesa}?",
                    "Sim, cancelar",
                    "Não");

                if (!confirmar) return;
            }
            // Pede confirmação extra para pagamento
            if (novoStatus == StatusPedido.Pago)
            {
                bool confirmar = await Shell.Current.DisplayAlert(
                    "Confirmar Pagamento",
                    $"Tem certeza que deseja terminar o pedido da mesa {pedido.NumeroMesa}?",
                    "Sim, pago",
                    "Não");

                if (!confirmar) return;
            }

            // Atualiza status no Firebase
            bool sucesso = await _firebaseService.AtualizarStatusPedidoAsync(pedido.Id, novoStatus);

            if (sucesso)
            {
                // Recarrega lista para refletir mudanças
                await CarregarPedidos();
            }
        }

        // Método auxiliar para converter StatusPedido enum em texto legível
        private string ObterTextoStatus(StatusPedido status)
        {
            return status switch
            {
                StatusPedido.Pendente => "Pendente",
                StatusPedido.EmPreparacao => "Em Preparação",
                StatusPedido.Pronto => "Pronto",
                StatusPedido.Entregue => "Entregue",
                StatusPedido.Pago => "Pago",
                StatusPedido.Cancelado => "Cancelado",
                _ => status.ToString()
            };
        }
    }
}
