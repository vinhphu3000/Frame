using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor;
using System.Text;
using System;

#pragma warning disable 0618

public static class BuildHelp  
{



    //  返回文件夹(包含子文件夹)中的所有符合后缀的文件路径(相对于Assets)

    static public List<string> FindAllSuffixAssetToFile( string path, string suffix)
    {
        List<string> result = new List<string>();

        if ( Directory.Exists(path) == false)
        {
            Debug.LogError(string.Format("Cannot find path " + path));
            return result;
        }

        string[] findDatas = Directory.GetFiles(path, suffix, SearchOption.AllDirectories);
        string assetPath = string.Empty;

        for (int i = 0; i < findDatas.Length; i++)
        {
            int index = findDatas[i].IndexOf("Assets", System.StringComparison.Ordinal);
            assetPath = findDatas[i].Substring(index, findDatas[i].Length - index);
            assetPath = assetPath.Replace(@"\", @"/");
            result.Add(assetPath);
        }

        return result;
    }


    static public string[] GetFilterCsDepends(string assetPath)
    {
        var assetDepend = AssetDatabase.GetDependencies(new string[] { assetPath });
        var resultDepend = (from objPath in assetDepend
                            where objPath != assetPath && !objPath.EndsWith(".cs")
                            select objPath).ToArray();

        return resultDepend;
    }


    static public List<string> GetDependencies( string assetPath)
    {
        var assetDepend = AssetDatabase.GetDependencies(new string[] { assetPath });
        var resultDepend = ( from objPath in assetDepend
                             where objPath != assetPath && BMDataAccessor.ExporterPlan.IsExportFileType(objPath)
                             select objPath).ToList();

        return resultDepend;
    }


    static public string[] GetDependenciesArray(string assetPath)
    {
        var assetDepend = AssetDatabase.GetDependencies(new string[] { assetPath });

        string[] resultList = default (string[]);

        for (int i = 0; i < assetDepend.Length; i++)
        {
            if (assetDepend[i] != assetPath && BundleDataManager.Instance.BuildExporterPlan.IsExportFileType(assetDepend[i]))
            {
                Array.Resize(ref resultList, resultList == null ? 1 : resultList.Length + 1);
                resultList[resultList.Length - 1] = assetDepend[i];
            }
        }
        return resultList;
    }


    static public List<string> GetDependencies(string assetPath, System.Func<string, bool> filterAction)
    {
        var assetDepend = AssetDatabase.GetDependencies(new string[] { assetPath });
        var resultDepend = (from objPath in assetDepend
                            where objPath != assetPath && filterAction(objPath)
                            select objPath).ToList();

        return resultDepend;
    }


    public static string GetSuffix (string filePath)
    {
        return filePath.Replace(Path.GetExtension(filePath), "");
    }


    public static string RemoveSuffix ( string path)
    {
        return path.Remove(0, path.IndexOf('.'));
    }


    public static string GetMD5HashFromFile(string path)
    {
        try
        {
            FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();

            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            int length = retVal.Length;
            for (int i = 0; i < length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }

            return sb.ToString();
        }
        catch (System.Exception ex)
        {
            throw new System.Exception(" GetMD5HashFromFile fail! error. " + ex.Message);
        }
    }


    public static bool BuildRootBundle( BundleData bundle)
    {
        if (!IsNeedBuild(bundle))
        {
            Debug.Log(string.Format("Bundle {0} Skiped!", bundle.name));
            return true;
        }

        PushCommontDepends();

        BuildPipeline.PushAssetDependencies();
        for (int i = 0; i < bundle.depends.Count; i++)
        {
            BundleData dependBundle = BundleManager.GetBundleData(bundle.depends[i]);
            if (!BuildSingleBundle(dependBundle))
            {
                Debug.Log(string.Format("Build {0} Fail!", dependBundle.name));
                goto ONE_POP;
            }
        }

        BuildPipeline.PushAssetDependencies();

        BundleData mainBundle = BundleManager.GetBundleData(bundle.assetPath);
        if ( !BuildSingleBundle(mainBundle))
        {
            goto TWO_POP;
        }

        goto SUCCEED;


    ONE_POP:
        Debug.Log( string.Format("{0} Depends Build Error! ", bundle.name));
        BuildPipeline.PopAssetDependencies();
        PopCommontDepends();
        return false;
    TWO_POP:
        Debug.Log(string.Format("{0} Self Build Error! ", bundle.name));
        BuildPipeline.PopAssetDependencies();
        BuildPipeline.PopAssetDependencies();
        PopCommontDepends();
        return false;
    SUCCEED:
        Debug.Log(string.Format("Build {0} Succeed!", bundle.name));
        BuildPipeline.PopAssetDependencies();
        BuildPipeline.PopAssetDependencies();
        PopCommontDepends();
        return true;
    }


    public static bool BuildSingleBundle( BundleData bundle)
    {
        

        UnityEngine.Object assetObj = null;
        string outputPath = GenerateOutputBundlePath(bundle.name);

        if (!LoadAssetObjectByAssetPath( bundle.assetPath, out assetObj))
        {
            return false;
        }

        uint crc = 0;
        long changTime = DateTime.Now.ToBinary();

        bool succeed = BuildAssetBundle(assetObj, outputPath, out crc);

        BundleBuildState bundleState = BundleManager.GetBuildStateNoNull(bundle.assetPath);
        if (succeed)
        {
            bundleState.crc = crc;
            bundleState.lastDependencies = bundle.depends.ToArray();
            FileInfo info = new FileInfo(outputPath);
            bundleState.size = info.Length;
            bundleState.changeTime = changTime;
        }
        else
        {
            bundleState.lastDependencies = null;
        }

        //  每次写入,文件过多会有性能问题
        //BMDataAccessor.SaveBundleBuildeStates();
        return succeed;
    }


    public static bool BuildAssetBundle( UnityEngine.Object assetObj, string outputPath, out uint crc)
    {
        crc = 0;

        #if UNITY_4_2 || UNITY_4_1 || UNITY_4_0
		return BuildPipeline.BuildAssetBundle(	assetObj, 
		                                        null, 
											    outputPath, 
		                                        CurrentBuildAssetOpts,
												BuildConfiger.UnityBuildTarget);
#else
        return BuildPipeline.BuildAssetBundle(  assetObj,
                                                null,
                                                outputPath,
                                                out crc,
                                                BundleSetting.CurrentBuildAssetOpt,
                                                BundleSetting.UnityBuildTarget);
#endif
    }


    public static void PushCommontDepends()
    {
        BuildPipeline.PushAssetDependencies();
        string outputPath = GenerateOutputBundlePath("Shader");
        uint crc;
        BuildAssetBundle(BuildManager.ShaderList, outputPath, out crc);
    }


    public static void PopCommontDepends()
    {
        BuildPipeline.PopAssetDependencies();
    }


    public static bool IsNeedBuild( BundleData bundle )
    {

        if (BuildManager.IsIgnoreState)
            return true;

        string outputPath = GenerateOutputBundlePath(bundle.name);
        if (!File.Exists(outputPath))
            return true;

        BundleBuildState bundleBuildState = BundleManager.GetBuildState(bundle.assetPath);

        if (bundleBuildState == null)
            return true;

        //  输出文件是否被修改
        DateTime lastBuildTime = File.GetLastWriteTime(outputPath);
        DateTime bundleChangeTime = bundleBuildState.changeTime == -1 ? DateTime.MaxValue : DateTime.FromBinary(bundleBuildState.changeTime);
        if ( System.DateTime.Compare(lastBuildTime, bundleChangeTime) < 0)
        {
            return true;
        }

        if (DateTime.Compare(lastBuildTime, File.GetLastWriteTime(bundle.assetPath)) < 0)
        {
            return true;
        }

        //  依赖是否改变
        if(!EqualStrArray(bundle.depends, bundleBuildState.lastDependencies))
        {
            return true;
        }


        string[] allResDepends = GetFilterCsDepends(bundle.assetPath);
        for (int i = 0; i < allResDepends.Length; i++)
        {
            var fullPaht = BundleHelp.AssetPath2FullPath(ref allResDepends[i]);

            DateTime test = File.GetLastWriteTime(fullPaht);

            if (DateTime.Compare(lastBuildTime, File.GetLastWriteTime(fullPaht)) < 0)
            {
                return true;
            }

            if (DateTime.Compare(lastBuildTime, File.GetLastWriteTime(fullPaht + ".meta")) < 0)
            {
                return true;
            }
        }

        //  依赖文件是否被改变
        //for (int i = 0; i < bundle.depends.Count; i++)
        //{

        //    //  TODO:没有检测纹理之内的资源是否变化

        //    if (!File.Exists(GenerateOutputBundlePath(Path.GetFileNameWithoutExtension(bundle.depends[i]))))
        //        return true;

        //    if (DateTime.Compare(lastBuildTime, File.GetLastWriteTime(bundle.depends[i])) < 0)
        //    {
        //        return true;
        //    }

        //    //  meta change 
        //    //  Texture Format Change
        //    if (DateTime.Compare(lastBuildTime, File.GetLastWriteTime(bundle.depends[i] + ".meta")) < 0)
        //    {
        //        return true;
        //    }
        //}

        return false;
    }

    
    public static bool LoadAssetObjectByGUID( string guid , out UnityEngine.Object assetObj)
    {
        string filePath = AssetDatabase.GUIDToAssetPath(guid);
        assetObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath);

        if (assetObj == null)
        {
            Debug.LogError(string.Format("Can't Load Asset Path {0} GUID {1}!", filePath, guid));
        }

        return assetObj != null;
    }


    private static bool LoadAssetObjectByAssetPath(string assetPath, out UnityEngine.Object assetObj)
    {
        assetObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);

        if (assetObj == null)
        {
            Debug.LogError(string.Format("Can't Load Asset Path {0}!", assetPath));
        }
        return assetObj != null;
    }


    private static bool EqualStrArray(string[] strList1, string[] strList2)
    {
        if (strList1 == null || strList2 == null)
            return false;

        if (strList1.Length != strList2.Length)
            return false;

        for (int i = 0; i < strList1.Length; ++i)
        {
            if (strList1[i] != strList2[i])
                return false;
        }

        return true;
    }


    private static bool EqualStrArray(List<string> strList1, List<string> strList2)
    {
        if (strList1 == null || strList2 == null)
            return false;

        if (strList1.Count != strList2.Count)
            return false;

        for (int i = 0; i < strList1.Count; ++i)
        {
            if (strList1[i] != strList2[i])
                return false;
        }

        return true;
    }


    private static bool EqualStrArray(List<string> strList1, string[] strList2)
    {
        if (strList1 == null || strList2 == null)
            return false;

        if (strList1.Count != strList2.Length)
            return false;

        for (int i = 0; i < strList1.Count; ++i)
        {
            if (strList1[i] != strList2[i])
                return false;
        }

        return true;
    }

    //public static bool BuildSingleBundle( BundleData bundle)
    //{
    //    string outputPath = GenerateOutputBundlePath(bundle.name);
    //    string bundleStoreDir = Path.GetDirectoryName(outputPath);
    //    if (!Directory.Exists(bundleStoreDir))
    //        Directory.CreateDirectory(bundleStoreDir);


    //}
    

    public static string GenerateOutputBundlePath( string bundleName )
    {
        return Path.Combine(BundleSetting.PlatformOutputPath, bundleName + "." + BundleSetting.BundleSuffix);
    }


    public static string GenerateOutputDependsPath()
    {
        return Path.Combine(BundleSetting.PlatformOutputPath, "DependsData.txt");
    }


    public static void CreateOutputPath()
    {
        if (Directory.Exists(BundleSetting.PlatformOutputPath) == false)
            Directory.CreateDirectory(BundleSetting.PlatformOutputPath);
    }
  
}
