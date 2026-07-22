using System.Threading;
using System.Threading.Tasks;

namespace CrystalEngine.HFSM
{
    /// <summary>
    /// Определяет интерфейс асинхронного состояния в иерархической машине состояний (HFSM).
    /// Поддерживает асинхронные операции при входе и выходе из состояния с возможностью отмены через CancellationToken.
    /// <br/><br/>
    /// Defines an asynchronous state interface within the hierarchical finite state machine (HFSM).
    /// Supports asynchronous operations when entering or exiting the state with cancellation support via a CancellationToken.
    /// </summary>
    /// <typeparam name="TContext">Тип класса контекста данных для состояний. / The type of the data context class for the states.</typeparam>
    public interface IAsyncState<TContext> where TContext : class
    {
        /// <summary>
        /// Асинхронно вызывается один раз при переходе в (активации) это состояние.
        /// <br/><br/>
        /// Asynchronously called once when entering (activating) this state.
        /// </summary>
        /// <param name="context">Контекст данных текущей машины состояний. / The data context of the current state machine.</param>
        /// <param name="token">Токен отмены асинхронной операции. / The cancellation token for the asynchronous operation.</param>
        /// <returns>Задача, представляющая процесс входа в состояние. / A task representing the state entry process.</returns>
        Task EntryAsync(TContext context, CancellationToken token);

        /// <summary>
        /// Асинхронно вызывается один раз при выходе из (деактивации) этого состояния.
        /// <br/><br/>
        /// Asynchronously called once when exiting (deactivating) this state.
        /// </summary>
        /// <param name="context">Контекст данных текущей машины состояний. / The data context of the current state machine.</param>
        /// <param name="token">Токен отмены асинхронной операции. / The cancellation token for the asynchronous operation.</param>
        /// <returns>Задача, представляющая процесс выхода из состояния. / A task representing the state exit process.</returns>
        Task ExitAsync(TContext context, CancellationToken token);

        /// <summary>
        /// Вызывается каждый кадр игрового цикла (Update) пока состояние активно.
        /// <br/><br/>
        /// Called every frame of the game loop (Update) while the state is active.
        /// </summary>
        /// <param name="context">Контекст данных текущей машины состояний. / The data context of the current state machine.</param>
        void Update(TContext context);

        /// <summary>
        /// Вызывается каждый кадр физического цикла (FixedUpdate) пока состояние активно.
        /// <br/><br/>
        /// Called every physics frame (FixedUpdate) while the state is active.
        /// </summary>
        /// <param name="context">Контекст данных текущей машины состояний. / The data context of the current state machine.</param>
        void FixedUpdate(TContext context);

        /// <summary>
        /// Вызывается в конце кадра игрового цикла (LateUpdate) пока состояние активно.
        /// <br/><br/>
        /// Called at the end of every frame loop (LateUpdate) while the state is active.
        /// </summary>
        /// <param name="context">Контекст данных текущей машины состояний. / The data context of the current state machine.</param>
        void LateUpdate(TContext context);
    }
}
