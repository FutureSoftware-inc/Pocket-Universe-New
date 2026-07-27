using CrystalEngine;
using CrystalEngine.HFSM; // Подключаем рантайм-интерфейсы состояний
using UnityEngine;
using UnityEngine.UIElements;

namespace CrystalEditor
{
    /// <summary>
    /// Специализированный узел HFSM-состояния. Управляет визуальным отображением и хранит 
    /// ссылку на полиморфный экземпляр рантайм-логики IState.
    /// </summary>
    public sealed class StateNode : GridNode
    {
        /// <summary>
        /// Живой экземпляр рантайм-логики состояния, привязанный к этому визуальному узлу.
        /// </summary>
        public ISyncState<IBlackboardProvider> UnderlyingState { get; private set; }

        public StateNode(GraphNodeData nodeData) : base(nodeData)
        {
            // Устанавливаем дефолтный цвет шапки при создании
            RefreshLifecycleStyling();
        }

        /// <summary>
        /// Инжектирует рантайм-класс логики в визуальный узел и обновляет его внешний вид.
        /// </summary>
        public void BindRuntimeState(ISyncState<IBlackboardProvider> stateInstance)
        {
            UnderlyingState = stateInstance;
            RefreshLifecycleStyling();
        }

        /// <summary>
        /// Динамически перекрашивает шапку узла на основе полиморфного типа рантайм-состояния.
        /// </summary>
        private void RefreshLifecycleStyling()
        {
            if (titleContainer == null) return;

            // Если состояние реализует интерфейс асинхронного выполнения
            if (UnderlyingState is IAsyncState<IBlackboardProvider>)
            {
                titleContainer.style.backgroundColor = new StyleColor(new Color(0.12f, 0.43f, 0.52f, 0.95f)); // Бирюзовый для Async
            }
            else
            {
                titleContainer.style.backgroundColor = new StyleColor(new Color(0.18f, 0.38f, 0.22f, 0.95f)); // Приглушенный зеленый для Sync
            }
        }

        protected override void OnContextBound()
        {
            // Сюда мы позже добавим вывод имени Action-скрипта в интерфейс ноды
        }
    }
}
