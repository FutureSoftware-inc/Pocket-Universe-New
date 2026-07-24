using CrystalEngine;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrystalEditor
{
    /// <summary>
    /// Специализированный узел HFSM-состояния. Базовый нативный стиль наследуется от GridNode.
    /// </summary>
    public sealed class StateNode : GridNode
    {
        public StateNode(GraphNodeData nodeData) : base(nodeData)
        {
            // Кастомизируем цвет шапки конкретно под HFSM-стейты (делаем чуть темнее/светлее)
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.2f, 0.25f, 0.3f, 0.95f));
        }

        protected override void OnContextBound()
        {
            // Сюда мы позже добавим вывод имени Action-скрипта в интерфейс ноды
        }
    }
}
