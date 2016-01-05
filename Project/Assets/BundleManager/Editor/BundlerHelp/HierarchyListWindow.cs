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

    //private Dictionary<int, ListElement> mAllElements; 


    public class HierarchyElementData : IListElementData
    {
        private static Texture directoryIcon;
        private bool isExistDataMng;

        public string AssetFilePath
        {
            get { return mListElement.Path; }
        }

        public bool IsFolder
        {
            get { return mListElement.IsFolder; }
        }

        private InspectorElement mElementData;
        public InspectorElement ElementData 
        { 
            get { return mElementData; }
            protected set { mElementData = value; }
        }

        private ListElement mListElement;
        public ListElement ListElement
        {
            get { return mListElement; }
        }


        //public HierarchyElementData(string assetfilePath, bool isFolder)
        //{
        //    //if (BundleDataManager.Instance.GetInspectorElement())
        //    //ElementData = BundleDataManager.Instance.CreateInspectorElement(ref assetfilePath, IsFolder);
        //}

        static HierarchyElementData()
        {
            directoryIcon = (Texture)EditorGUIUtility.LoadRequired("folder.jpg");
        }

       

        public void OnInit(ListElement element)
        {
            mListElement = element;
            ElementData = BundleDataManager.Instance.GetInspectorElement(mListElement.InstanceID);
            if (ElementData == null)
            {
                ElementData = BundleDataManager.Instance.CreateInspectorElement(mListElement.Path, mListElement.IsFolder);
            }
            else
            {
                ElementData.IsFolder = mListElement.IsFolder;
                ElementData.AssetFilePath = mListElement.Path;
                ElementData.AssetName = Path.GetFileNameWithoutExtension(mListElement.Path);
            }
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
        //mAllElements = new Dictionary<int, ListElement>();
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
        //BundleDataManager.Instance.AddResource(fileName, IsFolder);
        //var data = new HierarchyElementData(fileName, isFolder);
        var element = new ListElement(new HierarchyElementData(), this, fileName, isFolder);
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
            string[] fileStrings = Directory.GetDirectories(fileName);
            for (int i = 0; i < fileStrings.Length; i++)
            {
                //if (BundleDataManager.Instance.IsFilter(ref fileStrings[i]) == false)
                    AddElement(element, fileStrings[i], Directory.Exists(fileStrings[i]));
            }
        }

        //return data;
    }

    public void Init(List<string> rootFilePath)
    {
        for (int j = 0; j < rootFilePath.Count; j++)
        {
            bool isFolder = Directory.Exists(rootFilePath[j]);
            ListElement rootElement = new ListElement(new HierarchyElementData(), this, rootFilePath[j], isFolder);
            string[] fileStrings = Directory.GetDirectories(rootFilePath[j]);

            for (int i = 0; i < fileStrings.Length; i++)
            {
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


    public void OnAddNewElement(ListElement element)
    {
        BundleDataManager.Instance.OnAddNewElement(element);
    }


    #endregion




   
}
