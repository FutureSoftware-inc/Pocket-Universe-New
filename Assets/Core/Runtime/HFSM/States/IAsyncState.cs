using System.Threading;
using Cysharp.Threading.Tasks;

namespace CrystalEngine.HFSM
{
    /// <summary>
    /// Определяет интерфейс асинхронного состояния в иерархической машине состояний (HFSM).
    /// Поддерживает асинхронные операции при входе и выходе из состояния с возможностью отмены через CancellationToken.
    /// <br/><br/>
    /// Defines an asynchronous state interface within the hierarchical finite state machine (HFSM).
    /// Supports asynchronous operations when entering or exiting the state with cancellation support via a CancellationToken.
    /// </summary>
    /// <typeparam name="TContext">Тип класса контекста данных для состояний.<br/><br/>The type of the data context class for the states.</typeparam>
    public interface IAsyncState<TContext> : IState<TContext> where TContext : class
    {
        /// <summary>
        /// Асинхронно вызывается один раз при переходе в (активации) это состояние.
        /// <br/><br/>
        /// Asynchronously called once when entering (activating) this state.
        /// </summary>
        /// <param name="context">Контекст данных текущей машины состояний.<br/><br/>The data context of the current state machine.</param>
        /// <param name="token">Токен отмены асинхронной операции.<br/><br/>The cancellation token for the asynchronous operation.</param>
        /// <returns>Задача, представляющая процесс входа в состояние.<br/><br/>A task representing the state entry process.</returns>
        UniTask EntryAsync(TContext context, CancellationToken token);

        /// <summary>
        /// Асинхронно вызывается один раз при выходе из (деактивации) этого состояния.
        /// <br/><br/>
        /// Asynchronously called once when exiting (deactivating) this state.
        /// </summary>
        /// <param name="context">Контекст данных текущей машины состояний.<br/><br/>The data context of the current state machine.</param>
        /// <param name="token">Токен отмены асинхронной операции.<br/><br/>The cancellation token for the asynchronous operation.</param>
        /// <returns>Задача, представляющая процесс выхода из состояния.<br/><br/>A task representing the state exit process.</returns>
        UniTask ExitAsync(TContext context, CancellationToken token);
    }
}
