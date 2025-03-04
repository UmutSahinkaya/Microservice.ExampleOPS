using Coordinator.Models;
using Coordinator.Models.Contexts;
using Coordinator.Services.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace Coordinator.Services.Concrete
{
    public class TransactionService(IHttpClientFactory _httpClientFactory, TwoPhaseCommitContext _context) : ITransactionService
    {
        HttpClient _orderClient = _httpClientFactory.CreateClient("OrderAPI");
        HttpClient _stockClient = _httpClientFactory.CreateClient("StockAPI");
        HttpClient _paymentClient = _httpClientFactory.CreateClient("PaymentAPI");
        public async Task<Guid> CreateTransactionAsync()
        {
            Guid transactionId = Guid.NewGuid();

            var nodes = await _context.Nodes.ToListAsync();
            nodes.ForEach(n => n.NodeStates = new List<NodeState>()
            {
                new(transactionId)
                {
                    IsReady=Enums.ReadyType.Pending,
                    TransactionState=Enums.TransactionState.Pending,
                }
            });
            await _context.SaveChangesAsync();
            return transactionId;
        }
        public async Task PrepareServicesAsync(Guid transactionId)
        {
            var transactionNodes = await _context.NodeStates.Include(n => n.Node).Where(x => x.TransactionId == transactionId).ToListAsync();
            foreach (var transactionNode in transactionNodes)
            {
                try
                {
                    var response = await (transactionNode.Node.Name switch
                    {
                        "Order.API" => _orderClient.GetAsync("ready"),
                        "Stock.API" => _stockClient.GetAsync("ready"),
                        "Payment.API" => _paymentClient.GetAsync("ready"),
                    });
                    var result = bool.Parse(await response.Content.ReadAsStringAsync());
                    transactionNode.IsReady = result ? Enums.ReadyType.Ready : Enums.ReadyType.Unready;
                }
                catch (Exception)
                {
                    transactionNode.IsReady = Enums.ReadyType.Unready;
                }
            }
            await _context.SaveChangesAsync();
        }
        public async Task<bool> CheckReadyServicesAsync(Guid transactionId)
            => (await _context.NodeStates.Where(ns => ns.TransactionId == transactionId)
                .ToListAsync()).TrueForAll(ns => ns.IsReady == Enums.ReadyType.Ready);

        public async Task CommitAsync(Guid transactionId)
        {
            var transactionNodes = await _context.NodeStates
                .Include(ns => ns.Node)
                .Where(ns => ns.TransactionId == transactionId)
                .ToListAsync();

            foreach (var transactionNode in transactionNodes)
            {
                try
                {
                    var response = await (transactionNode.Node.Name switch
                    {
                        "Order.API" => _orderClient.GetAsync("commit"),
                        "Stock.API" => _stockClient.GetAsync("commit"),
                        "Payment.API" => _paymentClient.GetAsync("commit"),
                    });
                    var result = bool.Parse(await response.Content.ReadAsStringAsync());
                    transactionNode.TransactionState = result ? Enums.TransactionState.Done : Enums.TransactionState.Abort;
                }
                catch
                {
                    transactionNode.TransactionState = Enums.TransactionState.Abort;
                }
            }
            await _context.SaveChangesAsync();
        }
        public async Task<bool> CheckTransactionStateServicesAsync(Guid transactionId)
        => (await _context.NodeStates.Where(ns => ns.TransactionId == transactionId).ToListAsync()).TrueForAll(ns => ns.TransactionState == Enums.TransactionState.Done);
        public async Task RollBackAsync(Guid transactionId)
        {
            var transactionNodes = await _context.NodeStates.Include(ns => ns.Node).Where(ns => ns.TransactionId == transactionId).ToListAsync();
            foreach (var transactionNode in transactionNodes)
            {
                try
                {
                    if (transactionNode.TransactionState == Enums.TransactionState.Done)
                    {
                        _ = await (transactionNode.Node.Name switch
                        {
                            "Order.API" => _orderClient.GetAsync("rollback"),
                            "Stock.API" => _stockClient.GetAsync("rollback"),
                            "Payment.API" => _paymentClient.GetAsync("rollback")
                        });
                    }
                    transactionNode.TransactionState = Enums.TransactionState.Abort;
                }
                catch
                {
                    transactionNode.TransactionState = Enums.TransactionState.Abort;
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
