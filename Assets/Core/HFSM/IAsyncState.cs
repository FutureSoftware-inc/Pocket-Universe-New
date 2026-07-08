using System.Threading;
using System.Threading.Tasks;

namespace Crystal.HFSM
{
    public interface IAsyncState<TContext> where TContext : class
    {
        Task EntryAsync(TContext context, CancellationToken token);
        Task ExitAsync(TContext context, CancellationToken token);
        void Update(TContext context);
        void FixedUpdate(TContext context);
        void LateUpdate(TContext context);
    }
}