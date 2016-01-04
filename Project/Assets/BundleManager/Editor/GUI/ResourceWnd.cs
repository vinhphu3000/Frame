using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class ResourceWnd : EditorWindow
{

    [MenuItem("Resource Tools/Resource Window &R", false, 1)]
    static void ShowWnd()
    {
        ResourceWnd window = (ResourceWnd)EditorWindow.GetWindow(typeof(ResourceWnd));
        window.Show();
    }


    bool isPlaying;

    //  Select Info
    int selectIndex = 0;
    readonly string[] SelectInfoArray = new string[] { 
        "Ref Wnd",
        "Chunk Wnd",
        "LoadDown Wnd",
        };


    //  ref data
    Vector2 refScrollPos;

    //  chunk data
    Vector2 chunkScrollPos;

    //  load task
    Vector2 loadTaskScrollPos;


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

            DrawController();

            switch (selectIndex)
            {
                case 0: DrawCache(); break;
                case 1: DrawChunkData(); break;
                case 2: DrawLoadTask(); break;
                default:
                    break;
            }

            GUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("No Playing ! ", MessageType.Info);
        }
    }


    void DrawLoadTask()
    {
        var CacheData = CSCommonEditor.GetField<List<MultiForWww>>(ResourcesManager.Instance, "mLoadTask");

        for (int i = 0; i < CacheData.Count; i++)
        {
            loadTaskScrollPos = GUILayout.BeginScrollView(loadTaskScrollPos);

            if (CSCommonEditor.DrawHeader((i + 1).ToString(), "loadTask" + i.ToString()))
            {
                MultiForWww data = CacheData[i];

                CSCommonEditor.BeginContents();

                if (data.ResList.Count == 0)
                    EditorGUILayout.LabelField("Current Load", "无");
                else
                    EditorGUILayout.LabelField("Current Load", data.ResList[data.Index]);
                EditorGUILayout.LabelField("Parallel Load", data.ParallelCount.ToString());
                EditorGUILayout.LabelField("Is Error", data.IsError.ToString());


                for (int k = 0; k < data.WwwList.Count; k++)
                {
                    EditorGUILayout.LabelField("Url", data.WwwList[k].Url);
                    EditorGUILayout.LabelField("State", data.WwwList[k].mState.ToString());
                    if (data.WwwList[k].Www != null)
                    {
                        EditorGUILayout.LabelField("Progress", data.WwwList[i].Www.progress.ToString());
                    }
                }

                GUILayout.Space(5f);
                EditorGUILayout.LabelField("Res Stack");
                for (int j = 0; j < data.ResList.Count; j++)
                {
                    EditorGUILayout.LabelField(j.ToString(), data.ResList[j]);
                }

                CSCommonEditor.EndContents();
            }

            GUILayout.EndScrollView();
        }


    }


    void DrawChunkData()
    {
        var CacheData = CSCommonEditor.GetField<Dictionary<string, ResourcesChunkData>>(ResourcesManager.Instance, "mChunkData");

        if (CacheData != null)
        {
            chunkScrollPos = GUILayout.BeginScrollView(chunkScrollPos);
            foreach (var item in CacheData)
            {
                if (CSCommonEditor.DrawHeader(item.Key, item.Key))
                {
                    CSCommonEditor.BeginContents();
                    foreach (var subChunk in item.Value.resHash)
                    {
                        GUILayout.Label(subChunk);
                    }
                    CSCommonEditor.EndContents();
                }
            }
            GUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("ResourcesManager.Instance.mChunkData can't find! ", MessageType.Info);
        }
    }


    void DrawCache()
    {
        var CacheData = CSCommonEditor.GetField<Dictionary<string, ResourcesData>>(ResourcesManager.Instance, "mCacheData");

        if (CacheData != null)
        {
            //  Titile
            GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
            GUI.contentColor = Color.green;
            GUILayout.Label("AssetName", GUILayout.MinWidth(100f));
            GUILayout.Label("Ref", GUILayout.Width(70f));
            GUILayout.Label("Load", GUILayout.Width(70f));
            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();


            refScrollPos = GUILayout.BeginScrollView(refScrollPos);
            foreach (var item in CacheData)
            {
                GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
                EditorGUILayout.LabelField(item.Key, GUILayout.MinWidth(100f));
                GUILayout.Label(item.Value.ReferenceCount.ToString(), GUILayout.Width(70f));
                GUILayout.Label(item.Value.IsLoad ? "true" : "false", GUILayout.Width(70f));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("ResourcesManager.Instance.mCacheData can't find! ", MessageType.Info);
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




    string ctlResName = "", ctlChunkName = "";
    string ctlUnChunkName = "", ctlUnResName = "";
    private bool isInstance;
    void DrawController()
    {
        CSCommonEditor.BeginContents();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Res AssetName", GUILayout.Width(70));
        ctlResName = GUILayout.TextArea(ctlResName, GUILayout.MinWidth(120));
        GUILayout.Label("Chunk AssetName", GUILayout.Width(70));
        ctlChunkName = GUILayout.TextField(ctlChunkName, GUILayout.MinWidth(120));
        isInstance = GUILayout.Toggle(isInstance, "IsInstance", GUILayout.MinWidth(120));
        if (GUILayout.Button("Load", GUILayout.Width(70)))
        {
            ResourcesManager.Instance.Load(ctlResName, ctlChunkName, () =>
            {
                if (isInstance)
                {
                    GameObject obj = ResourcesManager.Instance.GetResources<GameObject>(ctlResName);
                    Object.Instantiate(obj);
                }
            });
        }
        GUILayout.EndHorizontal();

        if (CSCommonEditor.DrawTextButton("Chunk AssetName", ref ctlUnChunkName, "UnLoad"))
        {
            ResourcesManager.Instance.Unload(ctlUnChunkName);
        }

        if (CSCommonEditor.DrawTextButton("Res AssetName", ref ctlUnResName, "UnLoad"))
        {
            ResourcesManager.Instance.Editor_UnloadRes(ctlUnResName);
        }

        CSCommonEditor.EndContents();

        GUILayout.Space(10f);
    }

}
