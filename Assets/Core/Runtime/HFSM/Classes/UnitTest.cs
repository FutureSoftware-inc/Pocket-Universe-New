using Crystal.Common;
using Crystal.HFSM;
using System.Collections.Generic;
using UnityEngine;

public class UnitTest : MonoBehaviour, IBlackboardProvider
{
    [SerializeReferenceSelector]
    [SerializeReference]
    public List<Condition<UnitTest>> ConditionsList = new List<Condition<UnitTest>>();
    [SerializeReferenceSelector]
    [SerializeReference]
    public List<Transition<UnitTest>> TransitionsList = new List<Transition<UnitTest>>();

    [SerializeReferenceSelector]
    [SerializeReference]
    public IState<UnitTest> State;

    public Blackboard Blackboard => new Blackboard();
}
