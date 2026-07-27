using CrystalEngine;
using CrystalEngine.HFSM;
using System.Collections.Generic;
using UnityEngine;

public class UnitTest : MonoBehaviour, IBlackboardProvider
{
    [SerializeReferenceSelector]
    [SerializeReference]
    public IState<UnitTest> State;

    public Blackboard Blackboard => new Blackboard();
}
