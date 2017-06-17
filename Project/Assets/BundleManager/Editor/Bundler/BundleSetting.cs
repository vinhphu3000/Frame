using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;

#pragma warning disable 0618

public class BundleSetting
{

    /// <summary>
    /// Compress Bundle
    /// </summary>

    public static bool Compress
    {
        get { return BMDataAccessor.BMConfiger.compress; }
        set 
        { 
            if (BMDataAccessor.BMConfiger.compress != value)
            {
                BMDataAccessor.BMConfiger.compress = true;
            }
        }
    }


    /// <summary>
    /// 是否使用5.0版本打包
    /// </summary>

    public static bool IsUseFive
    {
        get
        {
            return BMDataAccessor.BMConfiger.isUse5;
        }
        set
        {
            BMDataAccessor.BMConfiger.isUse5 = value;
        }
    }

    /// <summary>
    /// Build deterministic bundles
    /// </summary>

    public static bool DeterministicBundle
    {
        get { return BMDataAccessor.BMConfiger.deterministicBundle; }
        set
        {
            if (BMDataAccessor.BMConfiger.deterministicBundle != value)
            {
                BMDataAccessor.BMConfiger.deterministicBundle = value;
            }
        }
    }


    /// <summary>
    /// Bunlde Suffix
    /// </summary>

    public static string BundleSuffix
    {
        get { return BMDataAccessor.BMConfiger.bundleSuffix; }
        set
        {
            BMDataAccessor.BMConfiger.bundleSuffix = value;
        }
    }


    /// <summary>
    /// 输出目录
    /// </summary>

    public static string OutputPath
    {
        get { return BMDataAccessor.BMConfiger.GetOutputPath(); }
        set 
        {
            BMDataAccessor.BMConfiger.buildOutputPath = value;
        }
    }


    /// <summary>
    /// 平台输出目录
    /// </summary>

    public static string PlatformOutputPath
    {
        get 
        {
            return Path.Combine(OutputPath, BundleBuildTarget.ToString());
        }
    }


    /// <summary>
    /// 平台输出目录(5.0)
    /// </summary>

    public static string PlatformOutputPathNew
    {
        get
        {
            return Path.Combine(OutputPath, BundleBuildTarget.ToString() + "New");
        }
    }


    /// <summary>
    /// 使用当前目标平台
    /// </summary>

    public static bool UseEditorTarget
    {
        get { return BMDataAccessor.BMConfiger.useEditorTarget; }
        set 
        {
            BMDataAccessor.BMConfiger.useEditorTarget = value;
        }
    }


    /// <summary>
    /// 目标平台
    /// </summary>

    public static BuildPlatform BundleBuildTarget
    {
        get
        {
            return BMDataAccessor.BMConfiger.bundleTarget;
        }
        set
        {
            BMDataAccessor.BMConfiger.bundleTarget = value;
        }
    }


    public static BuildTarget UnityBuildTarget
    {
        get
        {
            if ( UseEditorTarget )
                UnityBuildTarget = EditorUserBuildSettings.activeBuildTarget;


            switch (BundleBuildTarget)
            {
                case BuildPlatform.Standalones:
                    if (Application.platform == RuntimePlatform.OSXEditor)
                        return BuildTarget.StandaloneOSXIntel;
                    else
                        return BuildTarget.StandaloneWindows;
                case BuildPlatform.WebPlayer:
                    return BuildTarget.WebPlayer;
                case BuildPlatform.IOS:
#if UNITY_5
                    return BuildTarget.iOS;
#else
				return BuildTarget.iPhone;
#endif
                case BuildPlatform.Android:
                    return BuildTarget.Android;
                default:
                    Debug.LogError("Internal error. Cannot find BuildTarget for " + BundleBuildTarget);
                    return BuildTarget.StandaloneWindows;
            }
        }

        set 
        {
            switch (value)
            {
                case BuildTarget.StandaloneLinux:
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.StandaloneLinuxUniversal:
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    BundleBuildTarget = BuildPlatform.Standalones;
                    break;
                case BuildTarget.WebPlayer:
                case BuildTarget.WebPlayerStreamed:
                    BundleBuildTarget = BuildPlatform.WebPlayer;
                    break;
#if UNITY_5
                case BuildTarget.iOS:
#else
			case BuildTarget.iPhone:
#endif
                    BundleBuildTarget = BuildPlatform.IOS;
                    break;
                case BuildTarget.Android:
                    BundleBuildTarget = BuildPlatform.Android;
                    break;
                default:
                    Debug.LogError("Internal error. Bundle Manager dosn't support for platform " + value);
                    BundleBuildTarget = BuildPlatform.Standalones;
                    break;
            }
        }
    }


    /// <summary>
    /// Current Build Target
    /// </summary>

    public static BuildAssetBundleOptions CurrentBuildAssetOpt
    {
        get
        {
            return  (BMDataAccessor.BMConfiger.compress ? 0 : BuildAssetBundleOptions.UncompressedAssetBundle) |
                    (BMDataAccessor.BMConfiger.deterministicBundle ? 0 : BuildAssetBundleOptions.DeterministicAssetBundle) |
                    BuildAssetBundleOptions.CollectDependencies;
        }
    }


    public static List<BundleFileData> SelectDefaultPath
    {
        get
        {
            return BMDataAccessor.BMConfiger.selectBuildPath;
        }
    }
}

	
