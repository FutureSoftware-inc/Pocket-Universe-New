using Crystal.Common;
using Crystal.HFSM;
using System.Collections.Generic;
using UnityEngine;

public class UnitTest : MonoBehaviour
{
    [SerializeReferenceDropdown]
    [SerializeReference]
    public List<Condition<UnitTest>> ConditionsList = new List<Condition<UnitTest>>();
    [SerializeReferenceDropdown]
    [SerializeReference]
    public List<MonoBehaviour> BehaviourList = new List<MonoBehaviour>();
    
}
