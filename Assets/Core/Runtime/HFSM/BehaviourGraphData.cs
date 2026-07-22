using Crystal.Common;
using Crystal.Common.Editor;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Crystal.HFSM
{
    [CreateAssetMenu(menuName = "Crystal/HFSM/Behaviour Graph Asset", fileName = "NewBehaviourGraph")]
    public sealed class BehaviourGraphData : ScriptableObject
    {
        [Header("Editor Elements")]
        [SerializeField] private List<GraphNodeData> _editorNodes = new();

        [Header("Runtime State Machine Graph")]
        [SerializeReferenceSelector]
        [SerializeField] private List<IState<IBlackboardProvider>> _runtimeStates = new();

        public IReadOnlyList<GraphNodeData> EditorNodes => _editorNodes;
        public IReadOnlyList<IState<IBlackboardProvider>> RuntimeStates => _runtimeStates;

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
