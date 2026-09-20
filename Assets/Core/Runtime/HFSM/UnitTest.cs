using CrystalEngine;
using System.Collections.Generic;
using UnityEngine;

public class UnitTest : MonoBehaviour, IBlackboardProvider
{
    [SerializeReferenceSelector]
    [SerializeReference]
    public Condition<UnitTest>[] Conditions;
    public Blackboard Blackboard => new Blackboard();
}
