using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using System.Collections;





[SerializeField]
public class InspectorElement 
{
    public bool isIgnore;
    public bool isFolder;
    public string AssetName;
    public string AssetFilePath;

    public int StrategieIndex { get; set; }

    public InspectorElement(string assetFilePath)
    {
        AssetName = Path.GetFileNameWithoutExtension(assetFilePath);
        AssetFilePath = assetFilePath; //BundleHelp.FullPath2AssetPath(ref assetFilePath);
    }


    static public InspectorElement GetInspectorElement(ListElement element)
    {
        HierarchyListWindow.HierarchyElementData elementData =
            (HierarchyListWindow.HierarchyElementData) element.ElementData;
        return elementData.ElementData;
    }
}


[SerializeField]
public enum BuildResourceType
{
    None,
    Texture,
    Material,
    Sound,
    Prefab,
    Font,
    Fbx,
}


[SerializeField]
public class BuildStrategy
{
    public struct StrategyGroup
    {
        public BuildResourceType Type;
        public short GroupNumber;
    }

    public string Name { get; set; }

    public string[] Matching { get; set; }

    public StrategyGroup[] GroupData { get; set; }


    public bool IsMatching(ref string filePath)
    {
        for (int i = 0; i < Matching.Length; i++)
        {
            if (filePath.Contains(Matching[i]))
                return true;
        }
        return false;
    }


    public short GroupNumber(ref string filePath)
    {
        BuildResourceType type = BuildResourceType.None;
        string suffix = Path.GetExtension(filePath).ToLower();
        if (suffix == ".png" || suffix == ".jpg")
        {
            type = BuildResourceType.Texture;
        }
        else if (suffix == ".mat")
        {
            type = BuildResourceType.Material;
        }
        else if (suffix == ".ogg" || suffix == ".mp3")
        {
            type = BuildResourceType.Sound;
        }
        else if (suffix == ".prefab")
        {
            type = BuildResourceType.Prefab;
        }
        else if (suffix == ".ttf")
        {
            type = BuildResourceType.Font;
        }
        
        for (int i = 0; i < GroupData.Length; i++)
        {
            if (type == GroupData[i].Type)
                return GroupData[i].GroupNumber;
        }

        return 0;
    }
}


[SerializeField]
public class InspectorElementFolder : InspectorElement
{

    public bool IsIgnoreAll { get; set; }



    public string FolderName
    {
        get { return AssetName; }
    }

    public InspectorElementFolder(string assetFilePath)
        : base(assetFilePath)
    {
        isFolder = true;
        IsIgnoreAll = false;
    }
}


[SerializeField]
public class InspectorElementPrefab : InspectorElement
{
    public class InspectorElementPrefabDepend
    {
        /// <summary>
        /// Assets相对路径
        /// </summary>
        public string assetsFilePath;

        /// <summary>
        /// 组
        /// </summary>
        public short groupNumber;
    }


    private bool isGetDependcies = false;


    private InspectorElementPrefabDepend[] mDepends;
    public InspectorElementPrefabDepend[] Depends
    {
        get
        {
            if (isGetDependcies)
                return null;

            if (mDepends == null)
            {
                string[] dependStrings = BuildHelp.GetDependenciesArray(AssetFilePath);
                if (dependStrings == null)
                {
                    isGetDependcies = true;
                    return mDepends;
                }

                mDepends = new InspectorElementPrefabDepend[dependStrings.Length];
                for (int i = 0; i < Depends.Length; i++)
                {
                    mDepends[i] = new InspectorElementPrefabDepend();
                    mDepends[i].assetsFilePath = dependStrings[i];
                }
            }
            return mDepends;
        }
        set { mDepends = value; }

    }


    public void ResetGetDependcies()
    {
        isGetDependcies = false;
    }


    public InspectorElementPrefab(string assetFilePath)
        :base(assetFilePath)
    {
    }

}


//public class InspectorBundleObject : InspectorElement 
//{
//    public class DependsData
//    {
//        /// <summary>
//        /// 打包时是否忽略这个资源
//        /// </summary>
//        public bool isIgnore;

//        /// <summary>
//        /// Assets相对路径
//        /// </summary>
//        public string assetsFilePath;
//    }

//    public string AssetsFilePath;
//    public DependsData[] Depnds;


//    public InspectorBundleObject(string assetFilePath)
//    {
//        AssetName = Path.GetFileNameWithoutExtension(assetFilePath);
//        AssetsFilePath = BundleHelp.FullPath2AssetPath(ref assetFilePath);

//        string[] dependStrings = BuildHelp.GetDependencies(AssetsFilePath, new ExporterPlanBundleObject());
//        Depnds = new DependsData[dependStrings.Length];
//        for (int i = 0; i < Depnds.Length; i++)
//        {
//            Depnds[i] = new DependsData();
//            Depnds[i].assetsFilePath = dependStrings[i];
//        }
//    }


//    //public void Draw()
//    //{
//    //    if (PrefabObject == null)
//    //        PrefabObject = AssetDatabase.LoadAssetAtPath<GameObject>(AssetsFilePath);

//    //    EditorGUILayout.ObjectField("Prefab AssetName", PrefabObject, typeof(GameObject), false);
//    //    EditorGUILayout.BeginScrollView();
//    //}

//}
