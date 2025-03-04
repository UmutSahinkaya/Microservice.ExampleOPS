using Coordinator.Models;
using Coordinator.Models.Contexts;
using Coordinator.Services.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace Coordinator.Services.Concrete
{
    public class TransactionService(IHttpClientFactory _httpClientFactory,TwoPhaseCommitContext _context) : ITransactionService
    {
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
        public Task PrepareServicesAsync(Guid transactionId)
        {
            throw new NotImplementedException();
        }
        public Task<bool> CheckReadyServicesAsync(Guid transactionId)
        {
            throw new NotImplementedException();
        }
        public Task CommitAsync(Guid transactionId)
        {
            throw new NotImplementedException();
        }
        public Task<bool> CheckTransactionStateServicesAsync(Guid transactionId)
        {
            throw new NotImplementedException();
        }
        public Task RollBackAsync(Guid transactionId)
        {
            throw new NotImplementedException();
        }
    }
}
