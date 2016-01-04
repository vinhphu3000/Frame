using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using CS.CSUI;

namespace CS
{
    public class CSWindowEditor : EditorWindow
    {



        [MenuItem("Tools/CSUI Window")]
        static void ShowWnd()
        {
            CSWindowEditor window = (CSWindowEditor)EditorWindow.GetWindow(typeof(CSWindowEditor));
            window.Show();
        }

        bool isPlaying;

        //  Controller
        UIType ctlSelectShowType;
        UIType ctlSelectHideType;
        string controllerSearchFilter;

        enum CtlSortWnd
        {
            Order,
            Depth,
        }
        CtlSortWnd controllerSortType = CtlSortWnd.Order;

        Vector2 ctlScrollPos1 = Vector2.zero;

        //  Select Info
        int selectIndex = 0;
        readonly string[] SelectInfoArray = new string[] { 
        "Controller Wnd",
        "Cache Wnd",
        "Chunk Wnd",
        };

        //  Cache
        UIType cacheSelectType;
        Vector2 cacheScrollPos = Vector2.zero;
        List<UIType> cacheUnloadType = new List<UIType>();

        //  Chunk
        UIWindowType chunkSelectType = 0;
        Vector2 chunkScrollPos = Vector2.zero;


        //  Instance cache



        void Awake()
        {
            isPlaying = Application.isPlaying;
        }


        void Update()
        {
            if (isPlaying != Application.isPlaying)
            {
                isPlaying = Application.isPlaying;
                OnChangePlaymode(isPlaying);
            }
        }


        void OnChangePlaymode(bool isPlaying)
        {

        }

        void OnInspectorUpdate()
        {
            this.Repaint();
        }

        void OnGUI()
        {
            if (isPlaying)
            {
                GUILayout.BeginVertical();

                DrawSelect();
                switch (selectIndex)
                {
                    case 0: DrawController(); break;
                    case 1: DrawCache(); break;
                    case 2: DrawStack(); break;
                    default:
                        EditorGUILayout.HelpBox(string.Format("Do not have this type ! {0}", selectIndex.ToString()), MessageType.Info);
                        break;
                }

                GUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("No Playing ! ", MessageType.Info);
            }
        }


        void DrawSelect()
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < SelectInfoArray.Length; i++)
            {
                if (GUILayout.Toggle(selectIndex == i, SelectInfoArray[i], "ButtonLeft"))
                {
                    selectIndex = i;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5f);
        }

        void DrawController()
        {
            CSCommonEditor.DrawHeader("Controller");

            CSCommonEditor.BeginContents();
            GUILayout.BeginHorizontal();
            ctlSelectShowType = (UIType)EditorGUILayout.EnumPopup("Show UI Type", ctlSelectShowType);
            if (GUILayout.Button("Show"))
            {
                UIManager.Instance.ShowWnd(ctlSelectShowType);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            ctlSelectHideType = (UIType)EditorGUILayout.EnumPopup("Hide UI Type", ctlSelectHideType);
            if (GUILayout.Button("Hide"))
            {
                UIManager.Instance.HideWnd(ctlSelectHideType);
            }
            GUILayout.EndHorizontal();
            CSCommonEditor.EndContents();

            DrawCtlTopInfo();
            DrawControllerListWnd();
        }

        void DrawControllerListWnd()
        {
            var stackMng = CSCommonEditor.GetField<StackManager>(UIManager.Instance, "mStackInstance");
            if (stackMng == null)
                return;
            var listWnd = CSCommonEditor.GetField<List<StackData>>(stackMng, "mListWindow");

            List<StackData> temp = new List<StackData>(listWnd);

            CSCommonEditor.DrawHeader("Window Info");

            //  Titile
            GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
            GUI.contentColor = Color.green;
            GUILayout.Label("Order", GUILayout.Width(50f));
            GUILayout.Label("Instance", GUILayout.MinWidth(100f));
            GUILayout.Label("S-Depth", GUILayout.Width(70f));
            GUILayout.Label("E-Depth", GUILayout.Width(70f));
            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();

            if (temp.Count == 0)
            {
                EditorGUILayout.HelpBox("No Windows ! ", MessageType.Info);
                return;
            }
            else
            {
                switch (controllerSortType)
                {
                    default:
                    case CtlSortWnd.Order: break;
                    case CtlSortWnd.Depth:
                        temp.Sort((StackData l, StackData r) => { return l.mStartDepth.CompareTo(r.mStartDepth); });
                        break;
                }

                GUILayout.Space(5f);

                controllerSortType = (CtlSortWnd)EditorGUILayout.EnumPopup(controllerSortType);

                GUILayout.Space(5f);

                StackData data = null;
                ctlScrollPos1 = GUILayout.BeginScrollView(ctlScrollPos1);
                for (int i = 0; i < temp.Count; i++)
                {
                    data = temp[i];

                    GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
                    GUILayout.Label(data.mOrder.ToString(), GUILayout.Width(50f));
                    EditorGUILayout.ObjectField(data.mObject, typeof(GameObject), GUILayout.MinWidth(100f));
                    EditorGUILayout.LabelField(data.mStartDepth.ToString(), GUILayout.Width(70f));
                    EditorGUILayout.LabelField(data.mEndDepth.ToString(), GUILayout.Width(70f));
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }

        }


        void DrawCtlTopInfo()
        {
            var stackMng = CSCommonEditor.GetField<StackManager>(UIManager.Instance, "mStackInstance");

            if (stackMng == null)
                return;

            var info = stackMng.GetTopLevel();

            CSCommonEditor.DrawHeader("TopLevel Window");
            if (info != null)
            {
                _DrawStackData(ref info);
            }
            else
            {
                EditorGUILayout.HelpBox("No Window", MessageType.Info);
            }
        }


        void DrawCache()
        {
            if (CSCommonEditor.DrawHeader("Cache", "CSUI-Cache"))
            {
                var info = CSCommonEditor.GetField<Dictionary<UIType, UICache>>(UIManager.Instance, "mWindowInstanceList");
                if (info != null)
                {
                    int index = 0;
                    bool isUnload = false;

                    cacheScrollPos = GUILayout.BeginScrollView(cacheScrollPos);

                    foreach (var item in info)
                    {
                        ++index;

                        bool highlight = cacheSelectType == item.Key;
                        GUI.backgroundColor = highlight ? Color.white : new Color(0.8f, 0.8f, 0.8f);
                        GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
                        GUI.backgroundColor = Color.white;
                        GUILayout.Label(index.ToString(), GUILayout.Width(24f));

                        if (GUILayout.Button(item.Key.ToString(), "OL TextField", GUILayout.Height(20f)))
                            cacheSelectType = item.Key;

                        if (cacheUnloadType.Contains(item.Key))
                        {
                            GUI.backgroundColor = Color.red;

                            if (GUILayout.Button("Delete", GUILayout.Width(60f)))
                            {
                                isUnload = true;
                            }
                            GUI.backgroundColor = Color.green;
                            if (GUILayout.Button("X", GUILayout.Width(22f)))
                            {
                                cacheUnloadType.Remove(item.Key);
                                isUnload = false;
                            }
                            GUI.backgroundColor = Color.white;
                        }
                        else
                        {
                            if (GUILayout.Button("X", GUILayout.Width(22f))) cacheUnloadType.Add(item.Key);
                        }

                        GUILayout.EndHorizontal();
                    }

                    GUILayout.EndScrollView();

                    if (isUnload)
                    {
                        string dBug = "unload item : ";
                        for (int i = 0; i < cacheUnloadType.Count; i++)
                        {
                            dBug += cacheUnloadType[i].ToString() + " ";
                        }
                        //Utility.Log(dBug);
                    }
                }
            }
        }


        void DrawStack()
        {
            if (CSCommonEditor.DrawHeader("Chunk Info", "CSUI-Stack"))
            {
                var stackMng = CSCommonEditor.GetField<StackManager>(UIManager.Instance, "mStackInstance");
                var listWnd = CSCommonEditor.GetField<List<StackData>>(stackMng, "mListWindow");
                var chunkWnd = CSCommonEditor.GetField<Dictionary<UIWindowType, StackChunk>>(stackMng, "mChunkWindow");

                GUILayout.Label(string.Format(" Stack Window Count {0} ", listWnd.Count.ToString()), GUILayout.Width(120));

                GUILayout.BeginHorizontal();
                foreach (var iter in chunkWnd)
                {
                    if (GUILayout.Toggle(chunkSelectType == iter.Key, iter.Key.ToString(), "ButtonLeft"))
                    {
                        chunkSelectType = iter.Key;
                    }
                }
                GUILayout.EndHorizontal();

                DrawStackChunkInfo(chunkWnd[chunkSelectType]);
            }
        }


        void DrawStackChunkInfo(StackChunk chunk)
        {
            EditorGUILayout.LabelField("Chunk Type ", chunk.mChunkType.ToString());
            EditorGUILayout.LabelField("Chunk Start Depth ", chunk.mStartDepth.ToString());
            EditorGUILayout.LabelField("Chunk Child Count ", chunk.mChunkList.Count.ToString());

            chunkScrollPos = GUILayout.BeginScrollView(chunkScrollPos);

            for (int i = 0; i < chunk.mChunkList.Count; i++)
            {
                var info = chunk.mChunkList[i];

                if (CSCommonEditor.DrawHeader(info.mInfo.Key.ToString(), info.mInfo.Key.ToString()))
                {
                    _DrawStackData(ref info);
                }


            }

            GUILayout.EndScrollView();

        }




        #region Draw Filed

        void _DrawStackData(ref StackData info)
        {
            CSCommonEditor.BeginContents();

            EditorGUILayout.ObjectField("Instances ", info.mObject, typeof(GameObject));
            EditorGUILayout.LabelField("Order ", info.mOrder.ToString());
            EditorGUILayout.LabelField("Start Depeth ", info.mStartDepth.ToString());
            EditorGUILayout.LabelField("End Depeth ", info.mEndDepth.ToString());
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("UI Type ", info.mInfo.Key.ToString());
            EditorGUILayout.LabelField("Owner Type ", CSCommonEditor.ToString(info.mInfo.Owner));
            EditorGUILayout.LabelField("UI Wnd Type ", info.mInfo.WinType.ToString());
            EditorGUILayout.LabelField("UI Effect Type ", info.mInfo.WinEffect.ToString());
            EditorGUILayout.LabelField("UI Resources AssetName ", info.mInfo.GetResourcePath());

            CSCommonEditor.EndContents();
        }
        #endregion
    }
}


