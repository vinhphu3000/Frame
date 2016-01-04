using UnityEngine;
using System.Collections;

public static class UITools 
{

    /// <summary>
    /// 设置 NGUI Panel 深度
    /// </summary>
    /// <param name="obj">根节点</param>
    /// <param name="sDepth">起始深度</param>
    /// <returns></returns>

    public static int SetObjectDepth( GameObject obj, int sDepth)
    {
        if (obj == null)
            throw new System.NullReferenceException();

        int endDepth = sDepth;
        UIPanel[] panels = obj.GetComponentsInChildren<UIPanel>(true);
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].depth = sDepth + i;
        }

        endDepth = sDepth + panels.Length;
        return endDepth;
    }


    public static GameObject LoadWnd( string name, GameObject parent)
    {
        GameObject result = null;
        result = Resources.Load<GameObject>(name);

        do
        {
            if (result == null)
            {
                Debug.Log(string.Format("Resources.Load (...) Can't load prefab : [{0}] !", name));
                break;
            }

            result = NGUITools.AddChild(parent, result);

        } while (false);
       
        return result;
    }


    public static GameObject CreateObject ( string name, GameObject parent = null)
    {
        GameObject result = new GameObject(name);

        if (parent != null)
        {
            Transform ctrans = result.transform;
            ctrans.parent = parent.transform;
            ctrans.localPosition = Vector3.zero;
            ctrans.localScale = Vector3.one;
            ctrans.localRotation = Quaternion.identity;

            result.layer = parent.layer;
        }

        return result;
    }
}
