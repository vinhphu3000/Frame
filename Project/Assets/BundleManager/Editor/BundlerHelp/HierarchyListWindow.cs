using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections;

[SerializeField]
public class HierarchyListWindow : SubWindow, IListElementOwner
{

    private ListElement selectElement;
    private List<ListElement> listElement;
    private Vector2 scrollViewPos;

    public System.Action<ListElement> OnSelectChangeAction;


    public class HierarchyElementData : IListElementData
    {
        private static Texture directoryIcon;
        private bool isExistDataMng;

        public string AssetFilePath
        {
            get { return ElementData.AssetFilePath; }
        }

        public bool IsFolder
        {
            get { return ElementData.isFolder; }
        }

        public bool IsExistDataMng
        {
            get { return isExistDataMng; }
        }


        private InspectorElement mElementData;
        public InspectorElement ElementData 
        { 
            get { return mElementData; }
            set { mElementData = value; }
        }

        public HierarchyElementData(string assetfilePath, bool isFolder)
        {
            if (!BundleDataManager.Instance.ResourcElements.TryGetValue(assetfilePath, out mElementData))
            {
                mElementData = new InspectorElement(assetfilePath);
                mElementData.isFolder = isFolder;
                isExistDataMng = false;
            }
            else
            {
                isExistDataMng = true;
            }
        }


        static HierarchyElementData()
        {
            directoryIcon = (Texture)EditorGUIUtility.LoadRequired("folder.jpg");
        }

       

        public void OnInit(ListElement element)
        {
            element.DisableFold = !IsFolder;
        }

        #region IListElementData implementation

        private static Rect tempRect;
        private static Rect beforRect;

        public void Draw(ListElement element)
        {

            beforRect = GUILayoutUtility.GetLastRect();
            if (IsFolder)
            {
                tempRect.Set(beforRect.x + 2f, beforRect.y + 2f, 16, 16);
                GUI.DrawTexture(tempRect, directoryIcon);
                GUILayout.Space(23);
            }
            else
            {
                GUILayout.Space(13);
            }

            if (GUILayout.Button(ElementData.AssetName, "Label", GUILayout.MinWidth(150)))
            {
                element.OnSelected();
            }
        }

        #endregion

    }



    public HierarchyListWindow(EditorWindow wnd)
        : base(wnd)
    {
        listElement = new List<ListElement>();
        mAreaBox.SetX(5f, AreaBox.AreaType.FiexdRelativeLeft);
        mAreaBox.SetY(35f, AreaBox.AreaType.FiexdRelativeTop);
        mAreaBox.SetWidth(0.3f, AreaBox.AreaType.RatioRelativeLeft);
        mAreaBox.SetHeight(45, AreaBox.AreaType.FiexdRelativeBottom);
    }


    public override void Draw()
    {
        base.Draw();

        mAreaBox.BegainArea(mOwnerWindow.position.width, mOwnerWindow.position.height);
        scrollViewPos = GUILayout.BeginScrollView(scrollViewPos);
        for (int i = 0; i < listElement.Count; i++)
        {
            listElement[i].Draw();
        }

        GUILayout.EndScrollView();
        mAreaBox.EndArea();
    }


    public void AddElement(ListElement parent, string fileName, bool isFolder)
    {
        fileName = BundleHelp.FullPath2AssetPath(ref fileName);
        BundleDataManager.Instance.AddResource(fileName, isFolder);
        var data = new HierarchyElementData(fileName, isFolder);
        var element = new ListElement(data, this);
        if (parent != null)
        {
            parent.AddChild(element);
            parent.ResetSpace();
        }
        else
        {
            listElement.Add(element);
            listElement[listElement.Count - 1].SetSpace(0f);
        }

        if (isFolder)
        {
            string[] fileStrings = Directory.GetFileSystemEntries(fileName);
            for (int i = 0; i < fileStrings.Length; i++)
            {
                if (BundleDataManager.Instance.IsFilter(ref fileStrings[i]) == false)
                    AddElement(element, fileStrings[i], Directory.Exists(fileStrings[i]));
            }
        }

        //return data;
    }

    public void Init(List<string> rootFilePath)
    {
        bool isFolder = false;
        for (int j = 0; j < rootFilePath.Count; j++)
        {
            isFolder = Directory.Exists(rootFilePath[j]);
            BundleDataManager.Instance.AddResource(rootFilePath[j], isFolder);

            HierarchyElementData data = new HierarchyElementData(rootFilePath[j], isFolder);
            ListElement rootElement = new ListElement(data, this);
            string[] fileStrings = Directory.GetFileSystemEntries(rootFilePath[j]);

            for (int i = 0; i < fileStrings.Length; i++)
            {
                if (BundleDataManager.Instance.IsFilter(ref fileStrings[i]) == false)
                    AddElement(rootElement, fileStrings[i], Directory.Exists(fileStrings[i]));
            }
            
            rootElement.SetSpace(0f);
            listElement.Add(rootElement);
        }
        
    }


    #region IListElementOwner implementation

    public void OnTriggerSelect(ListElement element)
    {

        if (selectElement != element)
        {
            if (selectElement != null)
                selectElement.OnDeSelected();

            selectElement = element;

            if (OnSelectChangeAction != null)
                OnSelectChangeAction(element);
        }
    }

    public void OnTriggerDeSelect(ListElement element)
    {
    }

    #endregion


}
