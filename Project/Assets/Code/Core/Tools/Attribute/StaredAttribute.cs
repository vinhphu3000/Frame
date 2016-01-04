using System;
using UnityEngine;
using System.Collections;


public enum StaredType
{
    First,
    Data,
    Instance,
}


public class StaredAttribute : Attribute
{


    
    private StaredType type;
    private string functionName;
    private bool isEnumerator = false;

    public StaredType Type
    {
        get { return type; }
        set { type = value; }
    }

    public string FunctionName
    {
        get { return functionName; }
        set { functionName = value; }
    }

    public bool IsEnumerator
    {
        get { return isEnumerator; }
        set { isEnumerator = value; }
    }

    public StaredAttribute(StaredType t)
    {
        type = t;
    }

}
