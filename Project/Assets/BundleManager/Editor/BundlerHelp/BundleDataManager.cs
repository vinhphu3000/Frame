using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using System.Collections;



[SerializeField]
public class BundleDataManager : Singleton<BundleDataManager>
{
    [SerializeField]
// ReSharper disable once InconsistentNaming
    private List<string> mFilterSuffix;
    [SerializeField]
// ReSharper disable once InconsistentNaming
    private List<string> mFolderList;
    [SerializeField]
// ReSharper disable once InconsistentNaming
    private Dictionary<int, InspectorElement> mResourcElements;
    [SerializeField]
// ReSharper disable once InconsistentNaming
    private List<BuildStrategy> mBuildStrategies;
    [SerializeField]
// ReSharper disable once InconsistentNaming
    private BuildExporterPlan mBuildExporterPlan;


    static readonly string ResourcElementsPath;
    static readonly string FolderListPath;
    static readonly string FilterSuffixPath;
    static readonly string BuildStrategiePath;
    static readonly string BuildExporterPlanPath;

    static BundleDataManager()
    {
        FolderListPath = Application.dataPath + "/../../BuildConfig/FolderList.json";
        FilterSuffixPath = Application.dataPath + "/../../BuildConfig/FilterSuffix.json";
        ResourcElementsPath = Application.dataPath + "/../../BuildConfig/ResourcElements.json";
        BuildStrategiePath = Application.dataPath + "/../../BuildConfig/BuildStrategie.json";
        BuildExporterPlanPath = Application.dataPath + "/../../BuildConfig/BuildExporterPlan.json";
    }


    public List<string> FolderList
    {
        get
        {
            if (mFolderList == null)
            {
                mFolderList = LoadJson<List<string>>(FolderListPath) ?? new List<string>();
            }

            return mFolderList;
        }
    }


    public List<string> FilterSuffix
    {
        get
        {
            if (mFilterSuffix == null)
            {
                mFilterSuffix = LoadJson<List<string>>(FilterSuffixPath) ?? new List<string>();
            }

            return mFilterSuffix;
        }
    } 

    public Dictionary<int, InspectorElement> ResourcElements
    {
        get
        {
            if (mResourcElements == null)
            {
                mResourcElements = LoadJson<Dictionary<int, InspectorElement>>(ResourcElementsPath) ??
                                   new Dictionary<int, InspectorElement>();
            }
            return mResourcElements;
        }
    }


    public List<BuildStrategy> BuildStrategies
    {
        get
        {
            if (mBuildStrategies == null)
            {
                mBuildStrategies = LoadJson<List<BuildStrategy>>(BuildStrategiePath) ??
                                   new List<BuildStrategy>();
            }
            return mBuildStrategies;
        }
    }

    public BuildExporterPlan BuildExporterPlan
    {
        get
        {
            if (mBuildExporterPlan == null)
            {
                mBuildExporterPlan = LoadJson<BuildExporterPlan>(BuildExporterPlanPath) ??
                                   new BuildExporterPlan();
            }
            return mBuildExporterPlan;
        }
    }


    public void SaveBuildExporterPlan()
    {
        WriteJson(BuildExporterPlan, BuildExporterPlanPath);
    }

    public void SaveBuildStrategy()
    {
        WriteJson(BuildStrategies, BuildStrategiePath);
    }


    public void SaveResourcElements()
    {
        WriteJson(ResourcElements, ResourcElementsPath);
    }


    public void SaveFolderList()
    {
        WriteJson(FolderList, FolderListPath);
    }


    public void SaveFilterSuffix()
    {
        WriteJson(FilterSuffix, FilterSuffixPath);
    }


    public void SaveAll()
    {
        SaveResourcElements();
        //SaveFolderList();
        //SaveFilterSuffix();
        //SaveBuildStrategy();
        //SaveBuildExporterPlan();
    }


    public void ClearAll()
    {
        mFilterSuffix = null;
        mFolderList = null;
        mBuildExporterPlan = null;
        mBuildStrategies = null;
        mResourcElements = null;
    }


    public void ReadAgainBuildStrategies()
    {
        mBuildStrategies = null;
    }

    public void ReadAgainBuildExporterPlan()
    {
        mBuildExporterPlan = null;
    }

    public void ReadAgainFolderList()
    {
        mFolderList = null;
    }

    public bool IsFilter(ref string filePath )
    {
        for (int i = 0; i < FilterSuffix.Count; i++)
        {
            if (filePath.EndsWith(mFilterSuffix[i]))
                return true;
        }
        return false;
    }

    public bool OnAddNewElement(ListElement element)
    {
        InspectorElement newElement = InspectorElement.GetInspectorElement(element);

        if (ResourcElements.ContainsKey(element.InstanceID) == false)
        {
            ResourcElements.Add(element.InstanceID, newElement);
            return true;
        }
        else if (ResourcElements[element.InstanceID] != newElement)
        {
            Debug.LogError(string.Format("hash conflict file [{0}] file [{1}] hash [{2}]", ResourcElements[element.InstanceID].AssetFilePath, element.Path, element.InstanceID));
        }

        return false;
    }


    public InspectorElement GetInspectorElement(int instanceID)
    {
        if (ResourcElements.ContainsKey(instanceID))
        {
            return ResourcElements[instanceID];
        }
        return null;
    }

    public InspectorElement CreateInspectorElement(string assetFilePath, bool isFolder)
    {
        InspectorElement element = null;
        if (isFolder)
            element = new InspectorElementFolder(assetFilePath);
        else if (Path.GetExtension(assetFilePath) == ".prefab")
        {
            element = new InspectorElementPrefab(assetFilePath);
        }
        else
        {
            element = new InspectorElement(assetFilePath);
        }

        return element;
    }


    public InspectorElement CreateInspectorElement(ref string assetFilePath, bool isFolder)
    {
        InspectorElement element = null;
        if (isFolder)
            element = new InspectorElementFolder(assetFilePath);
        else if (Path.GetExtension(assetFilePath) == ".prefab")
        {
            element = new InspectorElementPrefab(assetFilePath);
        }
        else
        {
            element = new InspectorElement(assetFilePath);
        }

        return element;
    }

    //public void AddResource(string assetPath, bool IsFolder)
    //{
    //    if (!IsFolder && !assetPath.EndsWith(".prefab"))
    //        return;

    //    //assetPath = BundleHelp.FullPath2AssetPath(ref assetPath);
    //    if (ResourcElements.ContainsKey(assetPath) == false)
    //    {
    //        if (IsFolder)
    //            mResourcElements.Add(assetPath, new InspectorElementFolder(assetPath));
    //        else
    //        {
    //            mResourcElements.Add(assetPath, new InspectorElementPrefab(assetPath));
    //        }
    //    }
    //}

    //public void AddResourceAssetPath(string assetPath, bool isfolder)
    //{
    //    if (mResourcElements.ContainsKey(assetPath) == false)
    //    {
    //        if (isfolder)
    //            mResourcElements.Add(assetPath, new InspectorElementFolder());
    //    }
    //}

    //public void AfreshAllResource(string fullFilePath)
    //{
        //mResourcElements.Clear();
        //string[] allResourceStrings = Directory.GetFileSystemEntries(fullFilePath);
        //string assetFilePath = string.Empty;
        //for (int i = 0; i < allResourceStrings.Length; i++)
        //{
        //    assetFilePath = BundleHelp.FullPath2AssetPath(ref allResourceStrings[i]);
        //    if (mFilterSuffix.Contains(Path.GetExtension(assetFilePath)))
        //    {
        //        if (Directory.Exists(allResourceStrings[i]))
        //        {
        //            mResourcElements.Add(assetFilePath, new InspectorElementFolder(allResourceStrings[i]));
        //        }
        //        else
        //        {
        //            if (assetFilePath.EndsWith(".prefab"))
        //            {
        //                mResourcElements.Add(assetFilePath, new InspectorElementPrefab(allResourceStrings[i]));
        //            }
        //            else
        //            {
        //                mResourcElements.Add(assetFilePath, new InspectorElementPrefab.InspectorElementPrefabDepend(allResourceStrings[i]));
        //            }
        //        }
        //    }
        //}
    //}

    private T LoadJson<T>(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return default (T);

        if (File.Exists(filePath))
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath), new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
        }
        return default(T);
    }



    private void WriteJson(object obj, string filePath)
    {
        if (obj == null || string.IsNullOrEmpty(filePath))
            return;
        string str = Newtonsoft.Json.JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });//EditorJsonUtility.ToJson(obj, true);
        File.WriteAllText(filePath, str);
    }



}
