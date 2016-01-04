using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace CS.CSUI
{
    public class UICache
    {

        public UICache( UIType t, GameObject obj, UIWindowInfo info, UIBase owner)
        {
            mType = t;
            mInfo = info;
            mOwner = owner;
            mInstance = obj;
        }

        public UIType mType;
        public UIBase mOwner;
        public GameObject mInstance;
        public UIWindowInfo mInfo;
    }

    public sealed class UIManager : Singleton<UIManager>, ISingleManager
    {

        public UIRoot NGUIRoot { get; private set; }
        
        public Camera NGUICamera { get; private set; }

        public GameObject WindowRoot { get; private set; }


        UIPanel mRootPanel;
        StackManager mStackInstance;
        Dictionary<UIType, UICache> mWindowInstanceList;

        bool isLoadDependObject;


        public GameObject mPrefabGoback { get; private set; }


        public UIManager ()
        {
            isLoadDependObject = false;
        }


        public IEnumerator Init()
        {
            mStackInstance = new StackManager();
            mWindowInstanceList = new Dictionary<UIType, UICache>();

            // TODO
            mPrefabGoback = Resources.Load<GameObject>("RootGobackWnd");

            LoadDependRoot();

            isInit = true;
            yield break;
        }


        public bool IsInitComplete()
        {
            return isInit;
        }

        private void LoadDependRoot()
        {
            mRootPanel = NGUITools.CreateUI(null, false, LayerMask.NameToLayer("NGUI"));

            if (UIRoot.list.Count > 0)
            {
                NGUIRoot = UIRoot.list[0];
                WindowRoot = UITools.CreateObject("WndRoot", NGUIRoot.gameObject);
            }

            NGUIRoot.tag = "NGUIRoot";

            NGUICamera = NGUIRoot.GetComponentInChildren<Camera>();
            NGUICamera.cullingMask = 1 << LayerMask.NameToLayer("NGUI");
            NGUICamera.clearFlags = CameraClearFlags.Depth;

            NGUIRoot.manualWidth = 960;
            NGUIRoot.manualHeight = 640;
            NGUIRoot.fitHeight = true;
            NGUIRoot.scalingStyle = UIRoot.Scaling.ConstrainedOnMobiles;

            isLoadDependObject = true;
        }


        public void ShowWnd(UIType t, object[] param = null)
        {
            UIWindowInfo info = null;

            if (mWindowInstanceList.ContainsKey(t))
            {
                UICache cache = mWindowInstanceList[t];
                mStackInstance.Push(cache.mInstance, cache.mInfo);
            }
            else
            {
                if (UIWindowConfig.WindowConfig.TryGetValue(t, out info))
                {
                    ResourcesManager.Instance.Load(t.ToString(), "UI", () =>
                    {
                        GameObject instan = ResourcesManager.Instance.GetResources<GameObject>(t.ToString());
                        instan = NGUITools.AddChild(WindowRoot, instan);
                        mStackInstance.Push(instan, info);
                        UIBase owner = (UIBase)Activator.CreateInstance(info.Owner);
                        mWindowInstanceList.Add(t, new UICache(t, instan, info, owner));

                        owner.OnInit();
                        owner.OnParam(param);
                        owner.OnEnter();
                    } );
                    //GameObject instan = UITools.LoadWnd( "Windows/" + info.GetResourcePath(), WindowRoot);//GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(info.GetResourcePath()));
                  
                }
                else
                {
                    Debug.Log(string.Format("WindowConfig Can't find type [{0}]", t.ToString()));
                }
            }
        }


        public void HideWnd(UIType t)
        {
            UICache cache = null;
            if (mWindowInstanceList.TryGetValue(t, out cache))
            {
                cache.mOwner.OnLeave();
                mStackInstance.Pop(t);

                Utility.LogColor("8080ff", string.Format("HideWnd {0}", t.ToString()));
            }
            else
            {
                Debug.Log(string.Format("mWindowInstanceList Can't find type [{0}]", t.ToString()));
            }
        }


        public void DestoryWnd(UIType t)
        {
            UICache cache = null;
            if (mWindowInstanceList.TryGetValue(t, out cache))
            {
                mStackInstance.Pop(t);
                mWindowInstanceList.Remove(t);

                //  TODO
                cache.mOwner = null;
                GameObject.Destroy(cache.mInstance);
                Utility.LogColor("8080ff", string.Format("DestoryWnd {0}", t.ToString()));
            }
            else
            {
                Debug.Log(string.Format("mWindowInstanceList Can't find type [{0}]", t.ToString()));
            }
        }


        public void DoUpdate()
        {
            foreach (var iter in mWindowInstanceList.Values)
            {
                iter.mOwner.OnUpdate();
            }
        }

    }

}
