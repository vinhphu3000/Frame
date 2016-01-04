using UnityEngine;
using System.Collections;
using CS.CSUI;

namespace CS
{
    public class TestNGUI : MonoBehaviour
    {

        void Awake()
        {
            //UIWindowConfig.Init();
            //UIManager.Instance.Init();
        }


        void OnGUI()
        {

            if (GUI.Button(new Rect(0, 0, 70, 30), "Show"))
            {
                UIManager.Instance.ShowWnd(UIType.BackpackWnd);
            }


            if (GUI.Button(new Rect(0, 30, 70, 30), "Hide"))
            {
                UIManager.Instance.HideWnd(UIType.BackpackWnd);
            }


        }

    }
}


