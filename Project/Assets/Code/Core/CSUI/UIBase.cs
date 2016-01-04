using UnityEngine;
using System.Collections;

public class UIBase 
{
    public GameObject mObject { get; protected set; }

    /// <summary>
    /// Setup 1
    /// </summary>
    virtual public void OnInit() { }

    /// <summary>
    /// Setup 2
    /// </summary>
    /// <param name="param"></param>
    virtual public void OnParam(object[] param) { }

    /// <summary>
    /// Setup 3
    /// </summary>
    virtual public void OnEnter() { }

    /// <summary>
    /// Setup 4
    /// </summary>
    virtual public void OnUpdate() { }

    /// <summary>
    /// Setup 5
    /// </summary>
    virtual public void OnLeave() { }
}
