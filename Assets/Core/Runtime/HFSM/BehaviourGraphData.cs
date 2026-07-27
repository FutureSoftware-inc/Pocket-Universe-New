using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalEngine.HFSM
{
    /// <summary>
    /// Ассет на базе ScriptableObject для сохранения и хранения структуры графа поведения (HFSM).
    /// Объединяет визуальные метаданные для редактора GraphView и исполняемые данные состояний для рантайма.
    /// <br/><br/>
    /// A ScriptableObject asset for saving and storing the behavior graph structure (HFSM).
    /// Combines visual metadata for the GraphView editor and executable state data for runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "Crystal/HFSM/Behaviour Graph Asset", fileName = "NewBehaviourGraph")]
    public sealed class BehaviourGraphData : ScriptableObject
    {
        /// <summary>
        /// Список данных узлов, используемый исключительно для отображения и позиционирования элементов в окне редактора.
        /// <br/><br/>
        /// A list of node data used exclusively for displaying and positioning elements within the editor window.
        /// </summary>
        [Header("Editor Elements")]
        [SerializeField] private List<GraphNodeData> _editorNodes = new();

        /// <summary>
        /// Список рантайм-состояний машины, настроенных через селектор типов и готовых к исполнению в игре.
        /// <br/><br/>
        /// A list of runtime state machine states configured via the type selector and ready for in-game execution.
        /// </summary>
        [Header("Runtime State Machine Graph")]
        [SerializeReferenceSelector]
        [SerializeReference] private List<IState<IBlackboardProvider>> _runtimeStates = new();

        /// <summary>
        /// Возвращает доступный только для чтения список узлов редактора графа.
        /// <br/><br/>
        /// Gets the read-only list of graph editor nodes.
        /// </summary>
        public IReadOnlyList<GraphNodeData> EditorNodes => _editorNodes;

        /// <summary>
        /// Возвращает доступный только для чтения список рантайм-состояний машины.
        /// <br/><br/>
        /// Gets the read-only list of runtime state machine states.
        /// </summary>
        public IReadOnlyList<IState<IBlackboardProvider>> RuntimeStates => _runtimeStates;

        /// <summary>
        /// Сохраняет переданную структуру графа, обновляет данные ассета и помечает его как измененный (Dirty) в редакторе Unity.
        /// <br/><br/>
        /// Saves the provided graph structure, updates the asset data, and marks it as dirty within the Unity Editor.
        /// </summary>
        /// <param name="editorNodes">Список узлов с метаданными редактора. Не может быть null.<br/><br/>The list of nodes with editor metadata. Cannot be null.</param>
        /// <param name="runtimeStates">Список исполняемых состояний для рантайма. Не может быть null.<br/><br/>The list of executable states for runtime. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Вызывается, если один из переданных списков равен null.<br/><br/>Thrown when one of the specified lists is null.</exception>
        public void SaveGraph(List<GraphNodeData> editorNodes, List<IState<IBlackboardProvider>> runtimeStates)
        {
            _editorNodes = editorNodes ?? throw new ArgumentNullException(nameof(editorNodes));
            _runtimeStates = runtimeStates ?? throw new ArgumentNullException(nameof(runtimeStates));

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
