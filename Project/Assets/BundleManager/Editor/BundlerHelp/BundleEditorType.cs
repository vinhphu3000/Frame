using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections;


public interface IListElementData
{
    void Draw(ListElement element);
    void OnInit(ListElement element);
}


public interface IListElementOwner
{
    void OnTriggerSelect(ListElement element);

    void OnTriggerDeSelect(ListElement element);

    void OnAddNewElement(ListElement element);
}




/// <summary>
/// tree元素
/// </summary>


[SerializeField]
public class ListElement
{

    public float Space { get; protected set; }

    public IListElementData ElementData { get; set; }

    public IListElementOwner ElementOwner { get; private set; }

    public List<ListElement> ChildElements { get; protected set; }

    public ListElement ParentElement { get; protected set; }

    public bool State { get; set; }

    public bool Select { get; private set; }

    public bool DisableFold { get; set; }

    public bool IsGetFile { get; set; }

    public bool IsFolder { get; set; }

    public int InstanceID { get; private set; }


    private string mPath;
    public string Path {
        get { return mPath; }
        protected set { mPath = value; }
    }

    private const int verticalfixedspace = 0;
    private const int hierarchyfixedspace = 10;
    private const int toggleoffset = 5;
    private const int emptyoffset = hierarchyfixedspace + toggleoffset;


    public ListElement(IListElementData data, IListElementOwner owner, string path, bool isFolder)
    {
        ElementData = data;
        ElementOwner = owner;
        ChildElements = new List<ListElement>();
        DisableFold = !isFolder;
        IsGetFile = false;

        this.IsFolder = isFolder;
        this.Path = path;

        InstanceID = Animator.StringToHash(path);
        data.OnInit(this);

        ElementOwner.OnAddNewElement(this);
    }


    virtual public void Draw()
    {



        EditorGUILayout.BeginHorizontal(Select ? "LODSliderRangeSelected" : "LODSliderText", GUILayout.MinHeight(20f));

        //GUI.backgroundColor = Select ? Color.blue : Color.white;
        //EditorGUILayout.BeginHorizontal( "ProjectBrowserTopBarBg",GUILayout.MaxHeight(20f));
        //GUI.backgroundColor = Color.white;

        GUILayout.Space(Space);
        if (IsFolder && !DisableFold)
        {
            DrawToggle();
        }
        else
        {
            GUILayout.Space(emptyoffset);
        }
        ElementData.Draw(this);
        EditorGUILayout.EndHorizontal();

        if (State)
            for (int i = 0; i < ChildElements.Count; i++)
            {
                ChildElements[i].Draw();
            }
    }


    public void AddChild(ListElement element)
    {
        if (ChildElements.Contains(element) == false)
        {
            element.ParentElement = this;
            ChildElements.Add(element);
        }
    }


    public void RemoveChild(ListElement element)
    {
        ChildElements.Remove(element);
    }


    public void SetSpace(float space)
    {
        Space = space;
        for (int i = 0; i < ChildElements.Count; i++)
        {
            ChildElements[i].SetSpace(Space + hierarchyfixedspace);
        }
    }


    public void ResetSpace()
    {
        for (int i = 0; i < ChildElements.Count; i++)
        {
            ChildElements[i].SetSpace(Space + hierarchyfixedspace);
        }
    }



    public void OnSelected()
    {
        Select = true;
        ElementOwner.OnTriggerSelect(this);
    }

    public void OnDeSelected()
    {
        Select = false;
        ElementOwner.OnTriggerDeSelect(this);
    }


    protected void DrawToggle()
    {
        if (!GUILayout.Toggle(true, State ? "\u25BC " : "\u25BA ", "label", GUILayout.MaxWidth(15f)))
        {
            if (!IsGetFile)
            {
                GetChildFile();
                IsGetFile = true;
            }
            State = !State;
        }
        GUILayout.Space(-toggleoffset);
    }


    public void TryUpdateFile()
    {
        if (IsGetFile == false)
        {
            GetChildFile();
        }
    }

    protected void GetChildFile()
    {
        string[] files = Directory.GetFiles(Path);
        for (int i = 0; i < files.Length; i++)
        {
            if (BundleDataManager.Instance.IsFilter(ref files[i]))
                continue;

            string assetFile = BundleHelp.FullPath2AssetPath(ref files[i]);
            ListElement childElement = new ListElement(new HierarchyListWindow.HierarchyElementData(), ElementOwner, assetFile, false);
            AddChild(childElement);
        }

        //Debug.Log(string.Format("Folder Name [{0}] Get Child File Count [{1}]", BundleHelp.GetFolderName(ref mPath), files.Length));

        ResetSpace();
    }

}




/// <summary>
/// 自适应区域
/// </summary>


[SerializeField]
public class AreaBox
{
    private Rect mWindowRect;
    private Rect mArea;
    private bool mFixed = false;
    AreaPoint areaX, areaY, areaWidth, areaHeight;

    [SerializeField]
    public enum AreaType
    {
        FiexdRelativeLeft,
        FiexdRelativeRight,
        FiexdRelativeTop,
        FiexdRelativeBottom,
        RatioRelativeLeft,
        RatioRelativeRight,
        RatioRelativeTop,
        RatioRelativeBottom,
    }

    [SerializeField]
    public struct AreaPoint
    {
        public float value;
        public AreaType style;

        public float Calc(float standard)
        {
            switch (style)
            {
                case AreaType.FiexdRelativeBottom:
                    return standard - value;
                case AreaType.FiexdRelativeLeft:
                    return value;
                case AreaType.FiexdRelativeRight:
                    return standard - value;
                case AreaType.FiexdRelativeTop:
                    return value;
                case AreaType.RatioRelativeBottom:
                    return standard - (standard * value);
                case AreaType.RatioRelativeLeft:
                    return standard * value;
                case AreaType.RatioRelativeRight:
                    return standard - (standard * value);
                case AreaType.RatioRelativeTop:
                    return standard * value;
            }
            return value;
        }
    }


    public AreaBox(float windowWidth, float windowHeight, float ratioX, float ratioY, float ratioW, float ratioH)
    {
        mWindowRect = new Rect(0, 0, windowWidth, windowHeight);

        SetX(ratioX, AreaType.RatioRelativeLeft);
        SetY(ratioY, AreaType.RatioRelativeTop);
        SetWidth(ratioW, AreaType.RatioRelativeRight);
        SetHeight(ratioH, AreaType.RatioRelativeBottom);
    }


    public AreaBox(float windowWidth, float windowHeight)
    {
        mWindowRect = new Rect(0, 0, windowWidth, windowHeight);

        areaX.style = AreaType.RatioRelativeLeft;
        areaY.style = AreaType.RatioRelativeTop;
        areaWidth.style = AreaType.RatioRelativeRight;
        areaHeight.style = AreaType.RatioRelativeBottom;
    }


    public void CalcArea()
    {
        mArea.x = areaX.Calc(mWindowRect.width);
        mArea.y = areaY.Calc(mWindowRect.height);
        mArea.width = areaWidth.Calc(mWindowRect.width);
        mArea.height = areaHeight.Calc(mWindowRect.height);
    }


    public void BegainArea()
    {
        GUILayout.BeginArea(mArea);
    }


    public void BegainArea(float windowWidth, float windowHeight, string style = "")
    {
        mWindowRect.width = windowWidth;
        mWindowRect.height = windowHeight;
        CalcArea();
        GUILayout.BeginArea(mArea, "", string.IsNullOrEmpty(style) ? "Wizard Box" : style);
    }


    public void SetX(float value, AreaType t)
    {
        areaX.value = value;
        areaX.style = t;

        mArea.x = areaX.Calc(mWindowRect.width);
    }


    public void SetY(float value, AreaType t)
    {
        areaY.value = value;
        areaY.style = t;

        mArea.y = areaY.Calc(mWindowRect.height);
    }


    public void SetWidth(float value, AreaType t)
    {
        areaWidth.value = value;
        areaWidth.style = t;

        mArea.width = areaWidth.Calc(mWindowRect.width);
    }


    public void SetHeight(float value, AreaType t)
    {
        areaHeight.value = value;
        areaHeight.style = t;

        mArea.height = areaHeight.Calc(mWindowRect.height);
    }


    public void EndArea()
    {
        GUILayout.EndArea();
    }
}


/// <summary>
/// 自窗口控件,使用AreaBox做自适应
/// </summary>

[SerializeField]
public class SubWindow 
{
    protected Rect mPosition;
    protected AreaBox mAreaBox;
    protected EditorWindow mOwnerWindow;


    public SubWindow(EditorWindow wnd)
    {
        mOwnerWindow = wnd;
        mAreaBox = new AreaBox(wnd.position.width, wnd.position.height);
    }


    virtual public void Update()
    {

    }


    virtual public void Draw()
    {

    }

}