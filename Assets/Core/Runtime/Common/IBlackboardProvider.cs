using UnityEditor.Experimental.GraphView;

namespace Crystal.HFSM
{
    public interface IBlackboardProvider
    {
        Blackboard Blackboard { get; }
    }
}
