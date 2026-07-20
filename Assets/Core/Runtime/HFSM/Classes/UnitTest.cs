using Crystal.Common;
using Crystal.HFSM;
using System.Collections.Generic;
using UnityEngine;

public class UnitTest : MonoBehaviour
{
    [SerializeReferenceSelector]
    [SerializeReference]
    public List<Condition<UnitTest>> ConditionsList = new List<Condition<UnitTest>>();
    [SerializeReferenceSelector]
    [SerializeReference]
    public List<MonoBehaviour> BehaviourList = new List<MonoBehaviour>();
    
}
