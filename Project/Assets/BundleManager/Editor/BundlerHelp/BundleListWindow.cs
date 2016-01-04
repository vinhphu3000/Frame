using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;



public class BundleListWindow : EditorWindow
{

    BundleInfoWindow infoWindow;
    HierarchyListWindow listWindow;
    static BundleListWindow SelfWindow;



    private InspectorElement mSelectElement;
    private ListElement mSelectNodel;

    public ListElement SelectNodel
    {
        get { return mSelectNodel; }
    }

    [MenuItem("Resource Tools/Bundle List")]
    static void OpenWindow()
    {
        SelfWindow = GetWindow<BundleListWindow>();
    }

    [MenuItem("Tools/Test")]
    static void Test()
    {
        //BundleDataManager.Instance.FolderList.AddRange(new string[] { @"E:\Unity Project\Frame\Project\Assets\LocalResources"});
        //BundleDataManager.Instance.SaveFolderList();
        //BundleDataManager.Instance.FilterSuffix.AddRange(new string[] { ".cs", ".meta" });
        //BundleDataManager.Instance.SaveFilterSuffix();
    }


    void OnEnable()
    {
        SelfWindow = this;
        InitLiseWindow();
        InitInfoWindow();
    }

    public void OnDisable()
    {
        BundleDataManager.Instance.SaveAll();
    }

    //void On

    void OnGUI()
    {
        Toolbar();
        listWindow.Draw();
        infoWindow.Draw();
    }


    void InitLiseWindow()
    {
        listWindow = new HierarchyListWindow(this);
        listWindow.Init(BundleDataManager.Instance.FolderList);
        listWindow.OnSelectChangeAction += OnListSelectChange;
    }

    void InitInfoWindow()
    {
        infoWindow = new BundleInfoWindow(this);
    }


    void OnListSelectChange(ListElement data)
    {
        mSelectNodel = data;
        //var hierarchyData = (HierarchyListWindow.HierarchyElementData) data.ElementData;

        mSelectElement = InspectorElement.GetInspectorElement(data);

        if (mSelectElement != null)
        {
            infoWindow.Refresh(mSelectElement);
        }
    }


    void Toolbar()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Build Select", GUILayout.MaxWidth(120f)))
        {
            OnBuildSelect();
        }

        if (GUILayout.Button("Read Again", GUILayout.MaxWidth(120f)))
        {
            BundleDataManager.Instance.ReadAgainBuildStrategies();
            infoWindow.ReadAgainStrategie();
            //OnBuildSelect();
        }

        GUILayout.EndHorizontal();
    }



    void OnBuildSelect()
    {
        if (mSelectElement != null)
        {
            if (mSelectElement is InspectorElementFolder)
            {
                BuildManager.BuildSingleByElementFolder((InspectorElementFolder)mSelectElement, mSelectNodel);
            }
            else if (mSelectElement is InspectorElementPrefab)
            {
                BuildManager.BuildByElementPrefab((InspectorElementPrefab)mSelectElement);
            }
        }
    }

}