using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Collections;

public class BundleInfoWindow : SubWindow
{

    private InspectorElement cacheBundleObject;
    private InspectorElementPrefab cachePrefabObject;
    private InspectorElementFolder cacheFolderObject;
    private Vector2 scrollVector2;
    private Rect lastRect;
    private Rect tempRect;
    //private Vector2[] start;
    private Vector2[] startTitle;


    private Object mainObject;
    private List<Object> cacheObjects = new List<Object>();


    private ListElement mSelectNodel;
    private string[] strategieStrings;
    //private int SelectIndex;


    public BundleInfoWindow(EditorWindow wnd)
        :base(wnd)
    {
        mAreaBox.SetX(0.31f, AreaBox.AreaType.RatioRelativeLeft);
        mAreaBox.SetY(35f, AreaBox.AreaType.FiexdRelativeTop);
        mAreaBox.SetWidth(0.68f, AreaBox.AreaType.RatioRelativeLeft);
        mAreaBox.SetHeight(45, AreaBox.AreaType.FiexdRelativeBottom);

        //start = new[] {new Vector2(15f, 30f), new Vector2(90f, 200f)};
        startTitle = new[] {new Vector2(5f, 70f), new Vector2(135f, 70f)};

        ReadAgainStrategie();


        
    }



    public void Refresh(InspectorElement element)
    {
        mSelectNodel = ((BundleListWindow)mOwnerWindow).SelectNodel;
        cacheBundleObject = element;
        scrollVector2 = Vector2.zero;

        cacheFolderObject = cacheBundleObject as InspectorElementFolder;
        cachePrefabObject = cacheBundleObject as InspectorElementPrefab;
        if (cachePrefabObject != null)
        {
            LoadAsset(cachePrefabObject.Depends);
        }
    }

    public void ReadAgainStrategie()
    {
        var strategies = BundleDataManager.Instance.BuildStrategies;
        strategieStrings = new string[strategies.Count];
        for (int i = 0; i < strategies.Count; i++)
        {
            strategieStrings[i] = strategies[i].Name;
        }
        //SelectIndex = 0;
        
    }

    private void LoadAsset(InspectorElementPrefab.InspectorElementPrefabDepend[] filePath)
    {
        cacheObjects.Clear();

        if (filePath == null)
            return;

        for (int i = 0; i < filePath.Length; i++)
        {
            cacheObjects.Add(AssetDatabase.LoadAssetAtPath<Object>(filePath[i].assetsFilePath));
        }
    }


    public override void Draw()
    {
        mAreaBox.BegainArea(mOwnerWindow.position.width, mOwnerWindow.position.height);
        if (cacheBundleObject != null)
        {
            DrawCommon();

            if (cacheFolderObject != null)
            {
                DrawFolder();
            }
            else if (cachePrefabObject != null)
            {
                DrawPrefab();
            }
        }

        mAreaBox.EndArea();
    }


    private void DrawCommon()
    {
        EditorGUILayout.LabelField("AssetName", cacheBundleObject.AssetName);
        EditorGUILayout.LabelField("Asset Path", cacheBundleObject.AssetFilePath);
        cacheBundleObject.isIgnore = EditorGUILayout.Toggle("Ignore Current", cacheBundleObject.isIgnore);
        int index = EditorGUILayout.Popup("Strategie", cacheBundleObject.StrategieIndex, strategieStrings, GUILayout.MaxWidth(350f));
        if (index != cacheBundleObject.StrategieIndex)
        {
            if (cachePrefabObject != null)
                BuildManager.ApplyStrategyGroup(cachePrefabObject, index);
            else if (cacheFolderObject != null)
                BuildManager.ApplyStrategyGroup(mSelectNodel, index);
            cacheBundleObject.StrategieIndex = index;
        }
    }


    private void DrawFolder()
    {
        cacheFolderObject.IsIgnoreAll = EditorGUILayout.Toggle("Ignore All", cacheFolderObject.IsIgnoreAll);

//        int index = EditorGUILayout.Popup("Strategie", cacheFolderObject.StrategieIndex, strategieStrings, GUILayout.MaxWidth(350f));
//// ReSharper disable once RedundantCheckBeforeAssignment
//        if (index != cacheFolderObject.StrategieIndex)
//        {
//            cacheFolderObject.StrategieIndex = index;
//        }
    }


    private void DrawPrefab()
    {
        GUILayout.Space(20f);
        scrollVector2 = EditorGUILayout.BeginScrollView(scrollVector2, "Wizard Box");
        if (cachePrefabObject.Depends == null || cachePrefabObject.Depends.Length == 0)
        {
            GUILayout.Space(5f);
            GUILayout.Label("Depends Count is Zero!");
        }
        else
        {
            //DrawTitle();
            GUILayout.Space(5f);
            for (int i = 0; i < cachePrefabObject.Depends.Length; i++)
            {
                lastRect.Set(startTitle[0].x, i * 18 + 25f, 200, 16);
                //lastRect = GUILayoutUtility.GetLastRect();
                DrawDepend(cachePrefabObject.Depends[i], i);
            }

           
        }
        EditorGUILayout.EndScrollView();
        
    }


    private void DrawDepend(InspectorElementPrefab.InspectorElementPrefabDepend depend, int index )
    {
        EditorGUILayout.BeginHorizontal();
        depend.groupNumber = (short)EditorGUILayout.IntField(depend.groupNumber, GUILayout.MaxWidth(50f));
        EditorGUILayout.ObjectField(string.Empty, cacheObjects[index], typeof (Object), false, GUILayout.MaxWidth(200f));
        //lastRect.width = start[0].y;
        //depend.groupNumber = (byte)EditorGUI.IntField(lastRect, depend.groupNumber);
        //lastRect.x += start[1].x;
        //lastRect.width = start[1].y;
        //EditorGUI.ObjectField(lastRect, cacheObjects[index], typeof (Object));
        EditorGUILayout.EndHorizontal();
    }


    private void DrawTitle()
    {
        EditorGUILayout.BeginHorizontal();
        lastRect.Set(startTitle[0].x, tempRect.y + 58f, startTitle[0].y, 16);
        EditorGUI.LabelField(lastRect, "Grounp");
        lastRect.x = startTitle[1].x;
        lastRect.width = startTitle[1].x;
        EditorGUI.LabelField(lastRect, "Ref");
        EditorGUILayout.EndHorizontal();
    }


    private void GetBeforRect()
    {
        tempRect = GUILayoutUtility.GetLastRect();
    }
}
