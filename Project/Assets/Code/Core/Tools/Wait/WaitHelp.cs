using UnityEngine;
using System.Collections;

public class WaitHelp : MonoBehaviour {


    private static WaitHelp instance;
    public static WaitHelp Instance 
    {
        get 
        { 
            if (instance == null)
            {
                instance = new GameObject("Wait Help").AddComponent<WaitHelp>();
            }

            return instance;
        } 
    }


    new public Coroutine StartCoroutine(IEnumerator cor)
    {
        return base.StartCoroutine(cor);
    }


}
