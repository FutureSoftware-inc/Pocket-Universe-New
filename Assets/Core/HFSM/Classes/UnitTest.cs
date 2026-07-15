using Crystal.Common;
using Crystal.HFSM;
using System.Collections.Generic;
using UnityEngine;

public class UnitTest : MonoBehaviour
{
    public AnyNumber number;
    public BoolCondition<UnitTest> condition;
    public NumericCondition<UnitTest> numericCondition;
    [SerializeReference] public List<UnitTest> conditions;
    public int a;
    public byte b;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
