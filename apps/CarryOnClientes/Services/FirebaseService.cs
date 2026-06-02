using BarPedidos.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace BarPedidos.Services
{
    // Serviço responsável por toda a comunicação com a Firebase Realtime Database
    public class FirebaseService
    {
        // Cliente Firebase que faz a ligação à base de dados online
        private readonly FirebaseClient _firebaseClient;

        private readonly DeviceService _deviceService;

        // Construtor: recebe o DeviceService por injeção de dependências
        // Inicializa a ligação ao Firebase com a URL da base de dados
        public FirebaseService(DeviceService deviceService)
        {
            _deviceService = deviceService;
            // URL do Firebase Realtime Database do projeto
            _firebaseClient = new FirebaseClient("https://barpedidos-default-rtdb.firebaseio.com/");
        }

        #region Produtos
        // Obtém todos os produtos disponíveis na base de dados
        // Retorna uma lista de produtos para mostrar no catálogo
        public async Task<List<Produto>> ObterProdutosAsync()
        {
            try
            {
                // Vai à "pasta" produtos do Firebase e obtém todos os dados uma vez
                // OnceAsync: lê os dados uma única vez (mudanças futuras não são monitorizar)
                var produtos = await _firebaseClient
                    .Child("produtos")
                    .OnceAsync<Produto>();

                // Transforma os dados do Firebase em objetos Produto
                // p.Key: ID único gerado pelo Firebase
                // p.Object: dados do produto (nome, preço, etc.)
                return produtos.Select(p => new Produto
                {
                    Id = p.Key,
                    Nome = p.Object.Nome,
                    Descricao = p.Object.Descricao,
                    Preco = p.Object.Preco,
                    Categoria = p.Object.Categoria,
                    ImagemUrl = p.Object.ImagemUrl
                }).ToList();
            }
            catch (Exception ex)
            {
                // Se der erro (sem internet, Firebase offline, etc.), escreve no log
                // Debug.WriteLine: só aparece durante desenvolvimento, não afeta o utilizador
                System.Diagnostics.Debug.WriteLine($"Erro ao obter produtos: {ex.Message}");
                // Retorna lista vazia em vez de crashar a app
                return new List<Produto>();
            }
        }

        // Adiciona um novo produto à base de dados
        // Usado pelo administrador para criar produtos no catálogo
        // Retorna true se teve sucesso, false se falhou
        public async Task<bool> AdicionarProdutoAsync(Produto produto)
        {
            try
            {
                // PostAsync: cria um novo registo no Firebase com ID automático
                await _firebaseClient
                    .Child("produtos")
                    .PostAsync(produto);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao adicionar produto: {ex.Message}");
                return false;
            }
        }
        #endregion

        #region Pedidos
        // Cria um novo pedido na base de dados
        // Adiciona automaticamente o DeviceId, data/hora e status inicial
        // Retorna true se conseguiu criar, false se falhou
        public async Task<bool> CriarPedidoAsync(Pedido pedido)
        {
            try
            {
                // Adiciona automaticamente o ID do dispositivo ao pedido
                pedido.DeviceId = _deviceService.GetDeviceId();

                // Regista a data e hora exata em que o pedido foi criado
                pedido.DataHora = DateTime.Now;

                // Todos os pedidos começam com status "Pendente"
                pedido.Status = StatusPedido.Pendente;

                // Envia o pedido para o Firebase
                await _firebaseClient
                    .Child("pedidos")
                    .PostAsync(pedido);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao criar pedido: {ex.Message}");
                return false;
            }
        }

        // Obtém pedidos filtrados por dispositivo (apenas deste smartphone)
        // O cliente só vê os pedidos que ele próprio fez
        // Retorna lista ordenada por data (mais recentes primeiro)
        public async Task<List<Pedido>> ObterPedidosAsync()
        {
            try
            {
                // Busca todos os pedidos do Firebase
                var pedidos = await _firebaseClient
                    .Child("pedidos")
                    .OnceAsync<Pedido>();

                // Obtém o ID único deste smartphone
                string meuDeviceId = _deviceService.GetDeviceId();

                // Transforma os dados e aplica filtros
                return pedidos
                    .Select(p => new Pedido
                    {
                        Id = p.Key,
                        DeviceId = p.Object.DeviceId,
                        NumeroMesa = p.Object.NumeroMesa,
                        Itens = p.Object.Itens,
                        Total = p.Object.Total,
                        DataHora = p.Object.DataHora,
                        Status = p.Object.Status,
                        Observacoes = p.Object.Observacoes,
                        DataPagamento = p.Object.DataPagamento,
                        DataCancelamento = p.Object.DataCancelamento
                    })
                    // FILTRO IMPORTANTE: só mostra pedidos deste dispositivo
                    .Where(p => p.DeviceId == meuDeviceId)
                    // Ordena por data decrescente (mais recentes no topo)
                    .OrderByDescending(p => p.DataHora)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao obter pedidos: {ex.Message}");
                return new List<Pedido>();
            }
        }

        // Obtém TODOS os pedidos sem filtrar por dispositivo
        // Usado pelo PedidoCleanupService para limpeza automática
        public async Task<List<Pedido>> ObterTodosPedidosAsync()
        {
            try
            {
                // Busca todos os pedidos
                var pedidos = await _firebaseClient
                    .Child("pedidos")
                    .OnceAsync<Pedido>();

                // Converte para lista de objetos Pedido
                // ATENÇÃO: não filtra por DeviceId - retorna TODOS os pedidos
                return pedidos.Select(p => new Pedido
                {
                    Id = p.Key,
                    DeviceId = p.Object.DeviceId,
                    NumeroMesa = p.Object.NumeroMesa,
                    Itens = p.Object.Itens,
                    Total = p.Object.Total,
                    DataHora = p.Object.DataHora,
                    Status = p.Object.Status,
                    Observacoes = p.Object.Observacoes,
                    DataPagamento = p.Object.DataPagamento,
                    DataCancelamento = p.Object.DataCancelamento
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao obter todos pedidos: {ex.Message}");
                return new List<Pedido>();
            }
        }

        // Atualiza o status de um pedido específico
        // Se for marcado como Pago ou Cancelado, guarda a data/hora
        public async Task<bool> AtualizarStatusPedidoAsync(string pedidoId, StatusPedido novoStatus)
        {
            try
            {
                // Cria dicionário com os campos a atualizar
                // Só atualiza o status
                var updates = new Dictionary<string, object>
                {
                    { "Status", (int)novoStatus }
                };

                // Se mudou para Pago, regista a data/hora do pagamento
                // Isto para que o PedidoCleanupService saiba quando remover
                if (novoStatus == StatusPedido.Pago)
                {
                    updates.Add("DataPagamento", DateTime.Now);
                }

                // Se mudou para Cancelado, regista a data/hora do cancelamento
                // Pedidos cancelados são removidos após 1 minuto
                if (novoStatus == StatusPedido.Cancelado)
                {
                    updates.Add("DataCancelamento", DateTime.Now);
                }

                // PatchAsync: atualiza apenas os campos especificados
                // Não substitui o pedido todo, só os campos do dicionário
                await _firebaseClient
                    .Child("pedidos")
                    .Child(pedidoId)
                    .PatchAsync(updates);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar status: {ex.Message}");
                return false;
            }
        }

        // Remove um pedido completamente da base de dados
        // Pedidos pagos ou cancelados: removidos após 1 minuto
        public async Task<bool> RemoverPedidoAsync(string pedidoId)
        {
            try
            {
                // DeleteAsync: apaga o pedido permanentemente
                await _firebaseClient
                    .Child("pedidos")
                    .Child(pedidoId)
                    .DeleteAsync();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao remover pedido: {ex.Message}");
                return false;
            }
        }
        #endregion
    }
}
