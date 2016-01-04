using System;
using System.Linq;
using System.Text;
using CSTools;
using JetBrains.Annotations;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Object = UnityEngine.Object;


/// <summary>
/// 5.0之前版本打包
/// </summary>

public class BuildManager
{
    public static bool IsIgnoreState;
    public static GameObject ShaderList;
    static public List<BundleDependsData> DependsData;


    [MenuItem("Resource Tools/Obsolesce Builder All")]
    public static void BuildSelectObject()
    {
        IsIgnoreState = false;
        BuildAll();
    }

    [MenuItem("Resource Tools/Obsolesce Restart Builder All")]
    public static void RestartBuild()
    {
        IsIgnoreState = true;
        BuildAll();
    }


    [MenuItem("Resource Tools/Obsolesce Builder Alone Select")]
    public static void BuildAloneSelectObject()
    {

        Object[] objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
        if (objs.Length == 0)
            return;

        InitState();

        List<BundleData> bundles = Objects2Bundles(objs);
        for (int i = 0; i < bundles.Count; i++)
        {
            if (BuildHelp.BuildRootBundle(bundles[i]))
            {
                DependsData.Add(CreateDependsData(bundles[i]));
            }
        }

        string dependsConfigPath = BuildHelp.GenerateOutputDependsPath();
        if (File.Exists(dependsConfigPath))
        {
            List<BundleDependsData> diskConfigData = BMDataAccessor.LoadObjectFromJsonFile<List<BundleDependsData>>(dependsConfigPath);
            for (int i = 0; i < DependsData.Count; i++)
            {
                InsertBundleData(DependsData[i], ref diskConfigData);
            }
            BMDataAccessor.SaveObjectToJsonFile<List<BundleDependsData>>(diskConfigData, dependsConfigPath);
        }
        else
        {
            BMDataAccessor.SaveObjectToJsonFile<List<BundleDependsData>>(DependsData, dependsConfigPath);
        }

    }

    private static void BuildAll()
    {
        InitState();

        LoadShaderList();

        List<string> buildList = GetObjectPathByConfig();

        List<BundleData> bundles = new List<BundleData>();
        for (int i = 0; i < buildList.Count; i++)
        {
            bundles.Add(AddRootBundleData(buildList[i]));
        }


        for (int i = 0; i < bundles.Count; i++)
        {
            if (BuildHelp.BuildRootBundle(bundles[i]))
            {
                DependsData.Add(CreateDependsData(bundles[i]));
            }
        }
        //BuildPipeline.BuildAssetBundles()
        //  
        if (DependsData.Count > 0)
        {
            string dependsConfigPath = BuildHelp.GenerateOutputDependsPath();
            BMDataAccessor.SaveBundleBuildeStates();
            BMDataAccessor.SaveObjectToJsonFile<List<BundleDependsData>>(DependsData, dependsConfigPath);
            BMDataAccessor.SaveReleaseDependsData(DependsData, BundleSetting.PlatformOutputPath + "/DependsData.release");
            BMDataAccessor.ClearBundleBuildStates(DependsData);
            DependsData.Clear();
        }
        else
        {
            Debug.Log("There is no written DependsData.txt!");
        }
    }






    [MenuItem("Resource Tools/Log Depends Info &i")]
    public static void LogDependsInfo()
    {
        StringBuilder sb = new StringBuilder(1024);
        Object[] selectObjects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
        foreach (var iter in selectObjects)
        {
            sb.Length = 0;
            var ownerObj = AssetDatabase.GetAssetPath(iter);
            var depends = AssetDatabase.GetDependencies(new[] { ownerObj });
            sb.AppendLine(ownerObj);
            sb.AppendLine("[");
            for (int i = 0; i < depends.Length; i++)
            {
                sb.AppendFormat("\t{0}\n", depends[i]);
            }
            sb.AppendLine("]");
            Debug.Log(sb.ToString());
        }
    }


    [MenuItem("Resource Tools/Log Filter Depends Info #&i")]
    public static void LogFilterDependsInfo()
    {
        StringBuilder sb = new StringBuilder(1024);
        Object[] selectObjects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
        foreach (var iter in selectObjects)
        {
            sb.Length = 0;
            var ownerObj = AssetDatabase.GetAssetPath(iter);
            var depends = BuildHelp.GetDependencies(ownerObj);
            sb.AppendLine(ownerObj);
            sb.AppendLine("[");
            for (int i = 0; i < depends.Count; i++)
            {
                sb.AppendFormat("\t{0}\n", depends[i]);
            }
            sb.AppendLine("]");
            Debug.Log(sb.ToString());
        }
    }

    #region New Bundle Builder

    [MenuItem("Resource Tools/New Builder All")]
    private static void NewBuildAssetBundle()
    {
        List<BundleData> bundleDatas = GetAllSelectBundleDatas();


        //  all shader
        
        var shaderList = BuildHelp.GetDependencies("Assets/LocalResources/Shader.prefab", s => s.EndsWith(".shader"));
        AssetBundleBuild buildShader = new AssetBundleBuild();
        buildShader.assetBundleName = "shader";
        buildShader.assetNames = new string[shaderList.Count];
        int index = 0;
        foreach (string shaderPath in shaderList)
        {
            buildShader.assetNames[index] = shaderPath;
            SetBundleName(shaderPath, buildShader.assetNames[index]);
            ++index;
        }

        HashSet<string> filterStr = new HashSet<string>();
        List<AssetBundleBuild> allListBuild = new List<AssetBundleBuild>();
        allListBuild.Clear();
        allListBuild.Add(buildShader);

        for (int i = 0; i < bundleDatas.Count; i++)
        {
            if (!filterStr.Contains(bundleDatas[i].name))
            {
                allListBuild.Add(CreateBundleBuild(bundleDatas[i].assetPath, bundleDatas[i].name));
                filterStr.Add(bundleDatas[i].name);
            }
            foreach (var dependPath in bundleDatas[i].depends)
            {
                string name = Path.GetFileNameWithoutExtension(dependPath);
                if (!filterStr.Contains(name))
                {
                    allListBuild.Add(CreateBundleBuild(dependPath, name));
                    filterStr.Add(name);
                }
            }
        }

        if (Directory.Exists(BundleSetting.PlatformOutputPathNew) == false)
            Directory.CreateDirectory(BundleSetting.PlatformOutputPathNew);

        BuildPipeline.BuildAssetBundles(BundleSetting.PlatformOutputPathNew, allListBuild.ToArray(), BuildAssetBundleOptions.None, BundleSetting.UnityBuildTarget);
        Debug.Log(BundleSetting.PlatformOutputPathNew);
    }


    [MenuItem("Resource Tools/Clear All Bundle AssetName")]
    private static void ClearBundleName()
    {
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        for (int i = 0; i < allAssetPaths.Length; i++)
        {
            if (!allAssetPaths[i].EndsWith(".cs"))
                SetBundleName(allAssetPaths[i], null);
        }
    }

    [MenuItem("Resource Tools/Build Test Model")]
    private static void BuildTestModel()
    {
        //BuildExporterPlan.ExporterPlanData data = new BuildExporterPlan.ExporterPlanData();
        //data.FileMatching = "Assets/_Art/Atlas";
        //data.Name = "Atlas";
        //data.FileType = new[] { ".prefab", ".ttf" };
        //BundleDataManager.Instance.BuildExporterPlan.mExporterPlanDatas.Add(data);

        //BundleDataManager.Instance.SaveBuildExporterPlan();

        //BuildStrategy strategy = new BuildStrategy();
        //strategy.Name = "Atlas";
        //strategy.GroupData = new BuildStrategy.StrategyGroup[2];

        //strategy.GroupData[0] = new BuildStrategy.StrategyGroup();
        //strategy.GroupData[0].GroupNumber = 1;
        //strategy.GroupData[0].Type = BuildResourceType.Texture;

        //strategy.GroupData[1] = new BuildStrategy.StrategyGroup();
        //strategy.GroupData[1].GroupNumber = 1;
        //strategy.GroupData[1].Type = BuildResourceType.Material;

        ////  folder contain "Windows"
        //strategy.Matching = new string[] { "/Windows" };
        //BundleDataManager.Instance.BuildStrategies.Add(strategy);
        //BundleDataManager.Instance.SaveBuildStrategy();

        //ClearBundleName();

        //var guildCommon = GetCommonBuilds();
        //List<AssetBundleBuild> allListBuild = new List<AssetBundleBuild>(guildCommon);
        //string filePath = "Assets/LocalResources/Model/Actor_meiying.prefab";
        //allListBuild.Add(CreateBundleBuild(filePath, "Actor_meiying_Test"));

        //string outputFile = Application.dataPath + "/../../Resources/TestBuild";
        //if (Directory.Exists(outputFile) == false)
        //    Directory.CreateDirectory(outputFile);

        //BuildPipeline.BuildAssetBundles(outputFile, allListBuild.ToArray(), BuildAssetBundleOptions.None, BundleSetting.UnityBuildTarget);
        //Debug.Log(outputFile);
    }


    public static AssetBundleManifest TryLoadBeforBundleManifest(string outputFile)
    {
        int index = outputFile.LastIndexOf("/", StringComparison.Ordinal);
        string beforBundleManifest = outputFile.Substring(index, outputFile.Length - index);
        string beforBundleManifestPath = outputFile + "/" + beforBundleManifest;
        if (File.Exists(beforBundleManifestPath))
        {
            AssetBundle ab = AssetBundle.CreateFromMemoryImmediate(File.ReadAllBytes(beforBundleManifestPath));
            return ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
        
        return null;
    }

    private static Dictionary<short, List<InspectorElementPrefab.InspectorElementPrefabDepend>> tempDictionary = new Dictionary<short, List<InspectorElementPrefab.InspectorElementPrefabDepend>>();
    public static void BuildByElementPrefab(InspectorElementPrefab elementPrefab)
    {
        //ClearBundleName();

        var guildCommon = GetCommonBuilds();
        List<AssetBundleBuild> allListBuild = new List<AssetBundleBuild>(guildCommon);
        //allListBuild.Add(CreateBundleBuild(elementPrefab.AssetFilePath, elementPrefab.AssetName));
        allListBuild.AddRange(GetBundleBuildByElementPrefab(elementPrefab));

        BuildAssetBundles(Application.dataPath + "/../../Resources/TestBuildNew", ref allListBuild);
        //string outputFile = Application.dataPath + "/../../Resources/TestBuildNew";
        //if (Directory.Exists(outputFile) == false)
        //    Directory.CreateDirectory(outputFile);

        //var beforManifest = TryLoadBeforBundleManifest(outputFile);
        //var nowManigest = BuildPipeline.BuildAssetBundles(outputFile, allListBuild.ToArray(), BuildAssetBundleOptions.None, BundleSetting.UnityBuildTarget);
        //if (nowManigest != null && beforManifest != null)
        //{
        //    var finalManifest = BundleManifest.CombineBundleManifest(outputFile,beforManifest, nowManigest);
        //    if (finalManifest != null)
        //    {
        //        finalManifest.SaveToFile(outputFile + "/ReleaseManifest");
        //    }
        //    else
        //    {
        //        Logged.LogColor("ff0000", string.Format("BundleManifest.CombineBundleManifest result is null!"));
        //    }
        //}
        //else
        //{
        //    var finalManifest = new BundleManifest(nowManigest);
        //    finalManifest.SaveToFile(outputFile + "/ReleaseManifest");
        //}
        //Debug.Log(outputFile);
    }


    public static void BuildSingleByElementFolder(InspectorElementFolder folder, ListElement noder, bool immediatelyExecute = false)
    {
        ClearBundleName();

        if (!immediatelyExecute && folder.IsIgnoreAll)
        {
            Debug.Log(string.Format("忽略文件 [{0}]!", folder.AssetFilePath));
            return;
        }


        //  common
        List<AssetBundleBuild> allBundle = new List<AssetBundleBuild>();
        allBundle.AddRange(GetCommonBuilds());

        //  all file
        List<ListElement> allElements = new List<ListElement>();

        if (folder.isIgnore)
            GetSubFolderChildElements(noder, ref allElements);
        else
        {
            GetAllChildElements(noder, ref allElements);
        }


        if (allElements.Count == 0)
        {
            Debug.Log(string.Format("当前文件夹中 [{0}] 不包含任何文件!", folder.AssetFilePath));
            return;
        }

        HashSet<string> filterStr = new HashSet<string>();

        for (int i = 0; i < allElements.Count; i++)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            var elementData = allElements[i].ElementData as HierarchyListWindow.HierarchyElementData;
            if (elementData != null && !elementData.IsFolder)
            {
                InspectorElement elementPrefab = null;
                if (BundleDataManager.Instance.ResourcElements.TryGetValue(elementData.AssetFilePath, out elementPrefab))
                {
                    if (elementPrefab.isIgnore == false)
                    {
                        var eledata = GetBundleBuildByElementPrefab((InspectorElementPrefab) elementPrefab);
                        for (int j = 0; j < eledata.Count; j++)
                        {
                            if (filterStr.Contains(eledata[j].assetBundleName) == false)
                            {
                                filterStr.Add(eledata[j].assetBundleName);
                                allBundle.Add(eledata[j]);
                            }
                        }
                    }
                    else
                    {
                        Debug.Log(string.Format("Ignore Asset File [{0}]!", elementPrefab.AssetFilePath));
                    }
                }
            }
        }


        BuildAssetBundles(Application.dataPath + "/../../Resources/TestBuildNew", ref allBundle);

        //string outputFile = Application.dataPath + "/../../Resources/TestBuildNew";
        //if (Directory.Exists(outputFile) == false)
        //    Directory.CreateDirectory(outputFile);

        //BuildPipeline.BuildAssetBundles(outputFile, allBundle.ToArray(), BuildAssetBundleOptions.None, BundleSetting.UnityBuildTarget);
        //Debug.Log(outputFile);
    }


    public static void BuildAssetBundles(string outputFile, ref List<AssetBundleBuild> allBundle, BuildAssetBundleOptions options = BuildAssetBundleOptions.None)
    {
        if (Directory.Exists(outputFile) == false)
            Directory.CreateDirectory(outputFile);

        var beforManifest = TryLoadBeforBundleManifest(outputFile);
        var nowManigest = BuildPipeline.BuildAssetBundles(outputFile, allBundle.ToArray(), BuildAssetBundleOptions.None, BundleSetting.UnityBuildTarget);
        if (nowManigest != null && beforManifest != null)
        {
            var finalManifest = BundleManifest.CombineBundleManifest(outputFile, beforManifest, nowManigest);
            if (finalManifest != null)
            {
                finalManifest.SaveToFile(outputFile + "/ReleaseManifest");
            }
            else
            {
                Logged.LogColor("ff0000", string.Format("BundleManifest.CombineBundleManifest result is null!"));
            }
        }
        else
        {
            var finalManifest = new BundleManifest(nowManigest);
            finalManifest.SaveToFile(outputFile + "/ReleaseManifest");
        }
        Debug.Log(outputFile);
    }


    public static void GetAllChildElements(ListElement parentElement, ref List<ListElement> result)
    {
        if (parentElement.ChildElements.Count == 0)
        {
            result.Add(parentElement);
            return;
        }

        for (int i = 0; i < parentElement.ChildElements.Count; i++)
        {
            GetAllChildElements(parentElement.ChildElements[i], ref result);
        }
    }


    public static void GetCurrentChildElements(ListElement parentElement, ref List<ListElement> result)
    {
        //  子文件没有子节点
        if (parentElement.ChildElements.Count == 0)
        {
            result.Add(parentElement);
            return;
        }

        for (int i = 0; i < parentElement.ChildElements.Count; i++)
        {
            if (parentElement.ChildElements[i].ChildElements.Count == 0)
                result.Add(parentElement.ChildElements[i]);
        }
    }


    public static void GetSubFolderChildElements(ListElement parentElement, ref List<ListElement> result)
    {
        //  子文件没有子节点
        if (parentElement.ChildElements.Count == 0)
        {
            result.Add(parentElement);
            return;
        }

        for (int i = 0; i < parentElement.ChildElements.Count; i++)
        {
            if (parentElement.ChildElements[i].ChildElements.Count > 0)
                GetAllChildElements(parentElement, ref result);
            else
            {
                InspectorElement element = InspectorElement.GetInspectorElement(parentElement.ChildElements[i]);
                Debug.Log(string.Format("忽略文件 [{0}]!", element.AssetFilePath));
            }
        }

    }


    public static void ApplyStrategyGroup(ListElement folder, int strategyIndex)
    {
        List<ListElement> child = new List<ListElement>();
        GetCurrentChildElements(folder, ref child);

        for (int i = 0; i < child.Count; i++)
        {
            InspectorElement element = InspectorElement.GetInspectorElement(child[i]);
            if (element != null)
            {
                ApplyStrategyGroup((InspectorElementPrefab) element, strategyIndex);
            }
        }
    }


    public static void ApplyStrategyGroup(InspectorElementPrefab prefab, int strategyIndex)
    {
        if (prefab == null || strategyIndex >= BundleDataManager.Instance.BuildStrategies.Count)
            return;

        if (prefab.Depends == null)
            return;

        BuildStrategy strategy = BundleDataManager.Instance.BuildStrategies[strategyIndex];
        if (strategy.Name == "None" )
        {
            prefab.StrategieIndex = strategyIndex;
            for (int i = 0; i < prefab.Depends.Length; i++)
            {
                prefab.Depends[i].groupNumber = 0;
            }
        }
        else
        {

            if (strategy.IsMatching(ref prefab.AssetFilePath) )
            {
                prefab.StrategieIndex = strategyIndex;
                for (int i = 0; i < prefab.Depends.Length; i++)
                {
                    prefab.Depends[i].groupNumber = strategy.GroupNumber(ref prefab.Depends[i].assetsFilePath);
                }
            }
        }
    }

    //public static void FilterChildElements(ref List<ListElement> ownerElements, InspectorElementFolder folder)
    //{
    //    int state = folder.isIgnore ? 0 : 1;
    //    state = folder.IsIgnoreAll ? 3 : state;
    //    if (state == 0)
    //        return;

        

    //    for (int i = 0; i < ownerElements.Count; i++)
    //    {
            
    //    }
    //}



    static List<AssetBundleBuild> GetBundleBuildByElementPrefab(InspectorElementPrefab elementPrefab)
    {
        List<AssetBundleBuild> result = new List<AssetBundleBuild>();

        if (elementPrefab.Depends == null)
            return result;

        //  main
        tempDictionary.Clear();

        InspectorElementPrefab.InspectorElementPrefabDepend depend;

        AssetBundleBuild mainBuild = new AssetBundleBuild();
        mainBuild.assetBundleName = elementPrefab.AssetName;
        mainBuild.assetNames = new string[elementPrefab.Depends.Length];


        for (int i = 0; i < elementPrefab.Depends.Length; i++)
        {
            depend = elementPrefab.Depends[i];
            mainBuild.assetNames[i] = depend.assetsFilePath;
            if (depend.groupNumber == -1)
            {
                tempDictionary.Add((short)(1000 + i),new List<InspectorElementPrefab.InspectorElementPrefabDepend>() {depend});
            }
            else
            {
                if (tempDictionary.ContainsKey(depend.groupNumber))
                {
                    tempDictionary[depend.groupNumber].Add(depend);
                }
                else
                {
                    tempDictionary.Add(depend.groupNumber, new List<InspectorElementPrefab.InspectorElementPrefabDepend>() { depend });
                }
            }
            
        }

        result.Add(CreateBundleBuild(elementPrefab.AssetFilePath, elementPrefab.AssetName));

        //  grounp
        foreach (var iter in tempDictionary)
        {
            if (iter.Key > 0)
            {
                AssetBundleBuild subBuild = new AssetBundleBuild();
                if (iter.Key >= 1000)
                {
                    //  single
                    subBuild.assetBundleName = Path.GetFileNameWithoutExtension(iter.Value[0].assetsFilePath);
                }
                else
                {
                    subBuild.assetBundleName = elementPrefab.AssetName + iter.Key.ToString();
                }
                subBuild.assetNames = new string[iter.Value.Count];
                for (int i = 0; i < iter.Value.Count; i++)
                {
                    subBuild.assetNames[i] = iter.Value[i].assetsFilePath;
                    SetBundleName(subBuild.assetNames[i], Path.GetFileNameWithoutExtension(subBuild.assetNames[i]));
                }
                result.Add(subBuild);
            }
        }


        //for (int i = 0; i < UPPER; i++)
        //{
            
        //}

        

        return result;
    }



    private static AssetBundleBuild[] GetCommonBuilds()
    {
        AssetBundleBuild[] result = new AssetBundleBuild[1];

        //  common shader 
        var shaderList = BuildHelp.GetDependencies("Assets/LocalResources/Shader.prefab", s => s.EndsWith(".shader"));
        AssetBundleBuild buildShader = new AssetBundleBuild();
        buildShader.assetBundleName = "shader";
        buildShader.assetNames = new string[shaderList.Count];
        int index = 0;
        foreach (string shaderPath in shaderList)
        {
            buildShader.assetNames[index] = shaderPath;
            SetBundleName(shaderPath, buildShader.assetNames[index]);
            ++index;
        }
        result[0] = buildShader;


        return result;
    }


    private static void SetBundleName(string path, string name)
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath(path);
        if (assetImporter != null)
        {
            assetImporter.assetBundleName = name;
        }
    }

    private static AssetBundleBuild CreateBundleBuild(string path, string name)
    {
        SetBundleName(path, name);
        AssetBundleBuild bundleBuild = new AssetBundleBuild();
        bundleBuild.assetBundleName = name;
        bundleBuild.assetNames = new[] { path };
        return bundleBuild;
    }

    private static List<BundleData> GetAllSelectBundleDatas()
    {
        LoadShaderList();

        InitState();

        List<string> buildList = GetObjectPathByConfig();

        List<BundleData> bundles = new List<BundleData>();
        for (int i = 0; i < buildList.Count; i++)
        {
            bundles.Add(AddRootBundleData(buildList[i]));
        }

        return bundles;
    }

    #endregion




    private static void LoadShaderList()
    {
        ShaderList = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LocalResources/Shader.prefab");
        if (ShaderList == null)
        {
            Debug.LogError("Shader List Can't Load!");
        }
    }


    private static void AddCommonDepends(BundleData bundleData)
    {
        bundleData.depends.Add("Assets/LocalResources/Shader.prefab");
    }

    private static void InitState()
    {
        if (DependsData == null)
            DependsData = new List<BundleDependsData>();
        else
            DependsData.Clear();

        BundleManager.ClearBundleData();
        Debug.ClearDeveloperConsole();
        BuildHelp.CreateOutputPath();
    }

    private static BundleData AddRootBundleData(string rootPath)
    {
        BundleData root = BundleManager.GetBundleData(rootPath);
        if (root == null)
        {
            root = CreateBundleData(rootPath);
            BundleManager.AddNewBundleData(root);
        }

        for (int i = 0; i < root.depends.Count; i++)
        {
            if (!BundleManager.IsHaveBundleData(root.depends[i]))
                BundleManager.AddNewBundleData(CreateBundleData(root.depends[i]));
        }

        return root;
    }

    private static BundleData CreateBundleData(string path)
    {
        BundleData data = new BundleData();
        data.assetPath = path;
        data.name = Path.GetFileNameWithoutExtension(path);
        data.guid = AssetDatabase.AssetPathToGUID(path);
        data.depends = BuildHelp.GetDependencies(path);
        return data;
    }


    private static BundleDependsData CreateDependsData(BundleData data)
    {
        BundleDependsData dependsData = new BundleDependsData();
        dependsData.depends = new string[data.depends.Count];
        for (int i = 0; i < data.depends.Count; i++)
        {
            dependsData.depends[i] = Path.GetFileNameWithoutExtension(data.depends[i]);
        }
        dependsData.name = data.name;
        return dependsData;
    }


    private static List<string> GetObjectPathByConfig()
    {
        List<string> result = new List<string>();

        var defaultPath = BundleSetting.SelectDefaultPath;
        //  do somethings
        for (int i = 0; i < defaultPath.Count; i++)
        {
            result.AddRange(BuildHelp.FindAllSuffixAssetToFile(defaultPath[i].path, defaultPath[i].suffix));
        }

        return result;
    }


    private static List<BundleData> Objects2Bundles(Object[] objects)
    {
        List<BundleData> result = new List<BundleData>();
        for (int i = 0; i < objects.Length; i++)
        {
            result.Add(AddRootBundleData(AssetDatabase.GetAssetPath(objects[i].GetInstanceID())));
        }
        return result;
    }



    /// <summary>
    /// 插入一个bundle数据到lists
    /// </summary>
    /// <param name="data"></param>
    /// <param name="lists"></param>
    /// <returns>0 错误参数 1 新添加 2 覆盖</returns>
    private static int InsertBundleData(BundleDependsData data, ref List<BundleDependsData> lists)
    {
        if (data == null || lists == null)
            return 0;

        int index = lists.FindIndex((BundleDependsData value) => { return data.name == value.name; });
        if (index == -1)
        {
            lists.Add(data);
            return 1;
        }
        else
        {
            lists[index] = data;
            return 2;
        }
    }

}
