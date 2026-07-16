using Crystal.Common;
using Crystal.HFSM;
using System.Collections.Generic;
using UnityEngine;

public class UnitTest : MonoBehaviour
{
    [SerializeReference]
    public List<Condition<UnitTest>> ConditionsList = new List<Condition<UnitTest>>();
    
}
