using BarPedidosFuncionarios.Models;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;

namespace BarPedidosFuncionarios.Services
{
    public class FirebaseService
    {
        private readonly FirebaseClient _firebaseClient;

        public FirebaseService()
        {
            _firebaseClient = new FirebaseClient("https://barpedidos-default-rtdb.firebaseio.com/");
        }

        public async Task<List<Pedido>> ObterPedidosAsync()
        {
            var listaPedidos = new List<Pedido>();

            try
            {
                var pedidos = await _firebaseClient
                    .Child("pedidos")
                    .OnceAsync<Dictionary<string, object>>();

                if (pedidos == null || pedidos.Count == 0)
                {
                    return listaPedidos;
                }

                foreach (var item in pedidos)
                {
                    try
                    {
                        if (item?.Object == null) continue;

                        var obj = item.Object;
                        var pedido = new Pedido
                        {
                            Id = item.Key ?? Guid.NewGuid().ToString(),
                            Itens = new List<ItemPedido>()
                        };

                        // Converter dados básicos
                        pedido.NumeroMesa = obj.ContainsKey("NumeroMesa") ? Convert.ToInt32(obj["NumeroMesa"]) : 0;
                        pedido.DeviceId = obj.ContainsKey("DeviceId") ? obj["DeviceId"]?.ToString() ?? "" : "";
                        pedido.Total = obj.ContainsKey("Total") ? Convert.ToDecimal(obj["Total"]) : 0;
                        pedido.Observacoes = obj.ContainsKey("Observacoes") ? obj["Observacoes"]?.ToString() ?? "" : "";

                        // Converter data
                        if (obj.ContainsKey("DataHora") && obj["DataHora"] != null)
                        {
                            pedido.DataHora = DateTime.Parse(obj["DataHora"].ToString());
                        }
                        else
                        {
                            pedido.DataHora = DateTime.Now;
                        }

                        // Converter status - pode vir como int ou long do Firebase
                        if (obj.ContainsKey("Status"))
                        {
                            var statusValue = obj["Status"];
                            if (statusValue is long || statusValue is int)
                            {
                                pedido.Status = (StatusPedido)Convert.ToInt32(statusValue);
                            }
                            else
                            {
                                pedido.Status = StatusPedido.Pendente;
                            }
                        }

                        // Datas de pagamento e cancelamento
                        if (obj.ContainsKey("DataPagamento") && obj["DataPagamento"] != null)
                        {
                            pedido.DataPagamento = DateTime.Parse(obj["DataPagamento"].ToString());
                        }

                        if (obj.ContainsKey("DataCancelamento") && obj["DataCancelamento"] != null)
                        {
                            pedido.DataCancelamento = DateTime.Parse(obj["DataCancelamento"].ToString());
                        }

                        // Processar itens do pedido
                        // NOTA: Firebase pode retornar em formatos diferentes (array, lista, dicionário)
                        if (obj.ContainsKey("Itens") && obj["Itens"] != null)
                        {
                            var itensObj = obj["Itens"];

                            // Verificar tipo e processar adequadamente
                            if (itensObj is JArray jArray)
                            {
                                foreach (var itemToken in jArray)
                                {
                                    if (itemToken is JObject jObject)
                                    {
                                        var itemDict = jObject.ToObject<Dictionary<string, object>>();
                                        var itemPedido = ProcessarItem(itemDict);
                                        if (itemPedido != null)
                                        {
                                            pedido.Itens.Add(itemPedido);
                                        }
                                    }
                                }
                            }
                            else if (itensObj is List<object> itensList)
                            {
                                foreach (var itemObj in itensList)
                                {
                                    if (itemObj is Dictionary<string, object> itemDict)
                                    {
                                        var itemPedido = ProcessarItem(itemDict);
                                        if (itemPedido != null)
                                        {
                                            pedido.Itens.Add(itemPedido);
                                        }
                                    }
                                }
                            }
                            else if (itensObj is Dictionary<string, object> itensDict)
                            {
                                foreach (var kvp in itensDict)
                                {
                                    if (kvp.Value is Dictionary<string, object> itemDict)
                                    {
                                        var itemPedido = ProcessarItem(itemDict);
                                        if (itemPedido != null)
                                        {
                                            pedido.Itens.Add(itemPedido);
                                        }
                                    }
                                }
                            }
                        }

                        listaPedidos.Add(pedido);
                    }
                    catch (Exception ex)
                    {
                        // Ignorar pedidos com erro e continuar
                        System.Diagnostics.Debug.WriteLine($"Erro ao processar pedido: {ex.Message}");
                    }
                }

                return listaPedidos.OrderByDescending(p => p.DataHora).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao obter pedidos: {ex.Message}");
                return listaPedidos;
            }
        }

        public async Task<bool> AtualizarStatusPedidoAsync(string pedidoId, StatusPedido novoStatus)
        {
            try
            {
                await _firebaseClient
                    .Child("pedidos")
                    .Child(pedidoId)
                    .Child("Status")
                    .PutAsync((int)novoStatus);

                // Registar data de pagamento se necessário
                if (novoStatus == StatusPedido.Pago)
                {
                    await _firebaseClient
                        .Child("pedidos")
                        .Child(pedidoId)
                        .Child("DataPagamento")
                        .PutAsync(DateTime.Now);
                }

                // Registar data de cancelamento se necessário
                if (novoStatus == StatusPedido.Cancelado)
                {
                    await _firebaseClient
                        .Child("pedidos")
                        .Child(pedidoId)
                        .Child("DataCancelamento")
                        .PutAsync(DateTime.Now);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar status: {ex.Message}");
                return false;
            }
        }

        private ItemPedido ProcessarItem(Dictionary<string, object> itemDict)
        {
            try
            {
                var itemPedido = new ItemPedido();

                if (itemDict.ContainsKey("Quantidade"))
                {
                    itemPedido.Quantidade = Convert.ToInt32(itemDict["Quantidade"]);
                }

                if (itemDict.ContainsKey("Subtotal"))
                {
                    itemPedido.Subtotal = Convert.ToDecimal(itemDict["Subtotal"]);
                }

                // Processar dados do produto
                if (itemDict.ContainsKey("Produto"))
                {
                    var produtoObj = itemDict["Produto"];
                    Dictionary<string, object> produtoDict = null;

                    if (produtoObj is Dictionary<string, object> dict)
                    {
                        produtoDict = dict;
                    }
                    else if (produtoObj is JObject jObj)
                    {
                        produtoDict = jObj.ToObject<Dictionary<string, object>>();
                    }

                    if (produtoDict != null)
                    {
                        itemPedido.Produto = new BarPedidosFuncionarios.Models.Produto
                        {
                            Id = produtoDict.ContainsKey("Id") ? produtoDict["Id"]?.ToString() : "",
                            Nome = produtoDict.ContainsKey("Nome") ? produtoDict["Nome"]?.ToString() : "",
                            Descricao = produtoDict.ContainsKey("Descricao") ? produtoDict["Descricao"]?.ToString() : "",
                            Preco = produtoDict.ContainsKey("Preco") ? Convert.ToDecimal(produtoDict["Preco"]) : 0,
                            Categoria = produtoDict.ContainsKey("Categoria") ? produtoDict["Categoria"]?.ToString() : "",
                            Disponivel = produtoDict.ContainsKey("Disponivel") ? Convert.ToBoolean(produtoDict["Disponivel"]) : true,
                            ImagemUrl = produtoDict.ContainsKey("ImagemUrl") ? produtoDict["ImagemUrl"]?.ToString() : ""
                        };
                    }
                }

                return itemPedido;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao processar item: {ex.Message}");
                return null;
            }
        }
    }
}
