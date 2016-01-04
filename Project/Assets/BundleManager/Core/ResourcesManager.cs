using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Tools;
using System.IO;

using ResourceDepends = BundleDependsData;

public class ResourcesData
{
    public ResourcesData(string name)
    {
        mName = name;
        IsLoad = false;
        mReferenceCount = 0;
    }


    /// <summary>
    /// 
    /// ReferenceCount == 0
    /// 
    /// 1.	load 	"email" 	A  	0
    /// 2.	load 	"bag" 		A	1 (没有加载完)
    /// 3.	加载完成 			A	2
    /// 4.	unload 	"email"		A	1 
    /// 5.	unload	"bag"		A	0 

    /// 1.	load 	"email" 	A  	0
    /// 2.	load 	"bag" 		A	1 (没有加载完)
    /// 3.	unload 	"email"		A	0 (没有加载完)
    /// 4.	unload	"bag"		A	-1
    /// 3.	加载完成 			A	0

    /// 1.	load 	"email" 	A  	0
    /// 2.	加载完成 			A	1
    /// 3.	load 	"bag" 		A	2 (没有加载完)
    /// 4.	unload 	"email"		A	1 
    /// 5.	unload	"bag"		A	0
    /// </summary>
    /// <returns></returns>

    public bool IsCanCollection()
    {
        return mReferenceCount == 0 && IsLoad;
    }

    public void AddReference()
    {
        mReferenceCount += 1;
    }

    public void LessenReference()
    {
        mReferenceCount -= 1;

        if (IsCanCollection())
        {
            ResourcesManager.Instance.OnResNotReference(this);
        }
    }

    public UnityEngine.AssetBundle Bundle
    {
        get { return mBundle; }
        set { mBundle = value; }
    }

    public bool IsLoad { get; set; }

    public int ReferenceCount { get { return mReferenceCount; } }

    public string Name { get { return mName; } }

    public void Dispose()
    {
        if (mBundle != null)
            mBundle.Unload(true);
    }


    private string mName;
    private int mReferenceCount;
    private AssetBundle mBundle;
}


public class ResourcesChunkData
{
    public string chunkName;
    public HashSet<string> resHash;
    //public List<MultiForWww> multiTask;

    public ResourcesChunkData(string chunk)
    {
        chunkName = chunk;
        resHash = new HashSet<string>();
        //multiTask = new List<MultiForWww>();
    }
}



/// <summary>
/// Resources manager.
/// </summary>

public sealed class ResourcesManager : Singleton<ResourcesManager>, ISingleManager, IGlobalEvent
{

#if USE_5_BUNDLE
    //AssetBundleManifest mDependsConfig;
    BundleManifest mDependsConfig;
#else
    List<ResourceDepends> mDependsConfig;
#endif

    List<MultiForWww> mLoadTask;
    Dictionary<string, ResourcesData> mCacheData;
    Dictionary<string, ResourcesChunkData> mChunkData;

    int DeleteCount = 5;
    float DeleteTime;
    float DeleteRangeTime = 3f;
    List<ResourcesData> mDelete;


    public ResourcesManager()
    {
        mCacheData = new Dictionary<string, ResourcesData>();
        mChunkData = new Dictionary<string, ResourcesChunkData>();
        mLoadTask = new List<MultiForWww>();
        mDelete = new List<ResourcesData>();

        //  TODO: 
        System.GC.Collect();
        Resources.UnloadUnusedAssets();

        GameStarter.Instance.AddGlobalEvent(this);
    }


    public bool IsInitComplete()
    {
        return isInit;
    }

     
    public IEnumerator Init()
    {
#if USE_5_BUNDLE
        //DependsConfigLoad load = new DependsConfigLoad();
        //yield return load.Start(Path.Combine(AppSetting.ResourceUrl, "AndroidNew"));
        //mDependsConfig = load.GetConfig();

        mDependsConfig = new BundleManifest();
        mDependsConfig.LoadFile(Path.Combine(AppSetting.ResourcePath, "ReleaseManifest"));
        isInit = true;
        yield break;

        

#else
        DependsConfigLoad load = new DependsConfigLoad();
        yield return load.Start(Path.Combine(AppSetting.ResourceUrl, "DependsData.release"));
        mDependsConfig = load.GetConfig();

        if (mDependsConfig == null)
        {
            Debug.Log("Depends Config is Empty!");
            mDependsConfig = new List<ResourceDepends>();
        }
#endif



        Load(new string[] { "Shader" }, "Root", LoadRootResourcesComplete);
    }

    private void LoadRootResourcesComplete()
    {
        isInit = true;
    }


    public T GetResources<T>(string resName) where T : UnityEngine.Object
    {
#if USE_5_BUNDLE
        resName = resName.ToLower();
#endif
        ResourcesData data;
        if (mCacheData.TryGetValue(resName, out data))
        {
            if (data.IsLoad)
            {
                return (T)data.Bundle.LoadAsset(resName);
            }

            Debug.Log(string.Format("[{0}] is no load !", resName));
        }

        return null;
    }


    public void Load(string res, string chunk, System.Action Call)
    {
        Load(new string[] { res }, chunk, Call);
    }


    public void Load(string[] resArray, string chunk, System.Action call)
    {

#if USE_5_BUNDLE
        for (int i = 0; i < resArray.Length; i++)
        {
            resArray[i] = resArray[i].ToLower();
        }
#endif

        //	1. 是否有相同的块
        //	2. 资源是否存在缓存中
        //	3. 资源是否正在加载中(存在于块中)

        List<string> loadArray = null;
        ResourcesChunkData chunkData = null;
        if (!ExistInChunk(chunk))
        {
            chunkData = new ResourcesChunkData(chunk);
            mChunkData.Add(chunk, chunkData);
        }
        else
            chunkData = mChunkData[chunk];

        //  TODO: 这里可以先过滤一遍chunk
        //  FilterChunk(,)

        //  获取资源列表
        loadArray = GetFullLoadList(resArray);

        //  加入资源列表.
        //  1. 如果是新资源,在加载前自动加入缓存,ref = 0.
        //  2. 当加载成功ref自增加一
        //  3. 引用为0时并且IsLoad = true,表示这个资源没有被任何块引用.可以卸载.
        PushResources(chunkData, ref loadArray);

        CreateLoadTask(chunkData, loadArray, call);
    }


    public void Unload(string chunkName)
    {
        ResourcesChunkData chunkData = null;
        if (mChunkData.TryGetValue(chunkName, out chunkData))
        {
            foreach (string iter in chunkData.resHash)
            {
                ResourcesData resData;
                if (mCacheData.TryGetValue(iter, out resData))
                {
                    mCacheData[iter].LessenReference();
                }
            }

            mChunkData.Remove(chunkName);
        }
    }

#if UNITY_EDITOR

    public void Editor_UnloadRes(string resName)
    {
        ResourcesData data;
        if (mCacheData.TryGetValue(resName, out data))
        {
            while (data.ReferenceCount > 0)
            {
                data.LessenReference();
            }
        }
    }

#endif


    private bool ExistInChunk(string chunk)
    {
        return mChunkData.ContainsKey(chunk);
    }


    public bool ExistInCache(string res)
    {
        ResourcesData resData;
        if (mCacheData.TryGetValue(res, out resData))
        {
            return resData.IsLoad;
        }
        return false;
    }


    private bool FindInDelete(string resName, out int index)
    {
        index = -1;
        for (int i = 0; i < mDelete.Count; i++)
        {
            if (mDelete[i].Name == resName)
            {
                index = i;
                return true;
            }
        }

        return false;
    }

    private void CreateLoadTask(ResourcesChunkData chunkData, List<string> resList, System.Action call)
    {
        MultiForWww multi = new MultiForWww
        {
            Data = call,
            OnAllCompleted = OnMultiCompleted,
            OnSubCompleted = OnLoadCompleted
        };
        multi.Start(resList);
        mLoadTask.Add(multi);
    }


    /// <summary>
    /// 多个资源加载完成
    /// </summary>
    /// <param name="multi"></param>

    private void OnMultiCompleted(MultiForWww multi)
    {
        System.Action call = (System.Action)multi.Data;
        if (call != null)
        {
            call();
        }

        Debug.Log("Call Multi Finish !");
    }


    /// <summary>
    /// 一个原子资源加载完成
    /// </summary>
    /// <param name="www"></param>

    private void OnLoadCompleted(WaitForWww www)
    {
        string name = Path.GetFileNameWithoutExtension(www.Url);
        ResourcesData resData = null;

        if (mCacheData.TryGetValue(name, out resData))
        {
            resData.IsLoad = true;

            if (www.mState == WaitState.Done)
            {
                resData.AddReference();
                resData.Bundle = www.Www.assetBundle;
            }
            else
            {
                Debug.LogError(string.Format("Load Fail : {0} [{1}]", name, www.Url));
            }
        }
        else
            Debug.LogError(string.Format("Can't find name : {0} [{1}]", name, www.Url));
    }


    /// <summary>
    /// 资源引用为零触发
    /// </summary>
    /// <param name="data"></param>

    public void OnResNotReference(ResourcesData data)
    {
        mCacheData.Remove(data.Name);
        mDelete.Add(data);
    }


    /// <summary>
    /// 获取全部加载列表
    /// </summary>
    /// <returns>The full load list.</returns>
    /// <param name="resList">Res list.</param>

    public List<string> GetFullLoadList(string res)
    {
        List<string> result = new List<string>();
        string[] depends = GetDepends(res);
        if (depends != null)
            result.AddRange(depends);

        result.Add(res);
        return result;
    }


    /// <summary>
    /// 获取全部加载列表
    /// </summary>
    /// <returns>The full load list.</returns>
    /// <param name="resList">Res list.</param>

    public List<string> GetFullLoadList(string[] resList)
    {
        List<string> result = new List<string>();
        for (int i = 0; i < resList.Length; i++)
        {

            string[] depends = GetDepends(resList[i]);
            if (depends != null)
            {
                result.AddRange(depends);
            }
            result.Add(resList[i]);
        }

        return result;
    }


    /// <summary>
    /// 获取一个资源依赖列表
    /// </summary>
    /// <returns>The depends.</returns>
    /// <param name="resName">Res name.</param>

    public string[] GetDepends(string resName)
    {
        if (mDependsConfig == null)
            return default(string[]);

#if USE_5_BUNDLE
        return mDependsConfig.GetAllDependencies(resName);
#else
        for (int i = 0; i < mDependsConfig.Count; i++)
        {
            if (mDependsConfig[i].name == resName)
                return mDependsConfig[i].depends;
        }
#endif

        return null;
    }

    /// <summary>
    /// 这个资源是否已经被加载或者正在加载中
    /// </summary>
    /// <returns><c>true</c> if this instance is underway the specified res resPath; otherwise, <c>false</c>.</returns>
    /// <param name="res">Res.</param>
    /// <param name="resPath">Res path.</param>

    private bool IsLoadOrLoading(string res)
    {
        if (mCacheData.ContainsKey(res))
        {
            return true;
        }

        foreach (var iter in mChunkData.Values)
        {
            if (iter.resHash.Contains(res))
                return true;
        }

        return false;
    }


    private void FilterChunk(ResourcesChunkData chunkData, ref List<string> resArray)
    {
        for (int i = resArray.Count - 1; i >= 0; i--)
        {
            if (chunkData.resHash.Contains(resArray[i]))
                resArray.RemoveAt(i);
        }
    }

    /// <summary>
    /// 把资源列表加入指定块中,并且返回需下载的资源列表.
    /// 保证如果存在块中,这个资源必然已经加载或者正在加载中.
    /// </summary>
    /// <param name="chunk">Chunk.</param>
    /// <param name="resArray">过滤后需要下载的列表</param>

    private void PushResources(ResourcesChunkData chunkData, ref List<string> resArray)
    {
        for (int i = resArray.Count - 1; i >= 0; i--)
        {
            if (!chunkData.resHash.Contains(resArray[i]))
            {
                int deleteIndex;

                //  如果有缓存不用加入
                if (mCacheData.ContainsKey(resArray[i]))
                {
                    mCacheData[resArray[i]].AddReference();
                    chunkData.resHash.Add(resArray[i]);
                    resArray.RemoveAt(i);
                }
                else if (FindInDelete(resArray[i], out deleteIndex))
                {
                    ResourcesData data = ActivateDeleteCache(deleteIndex);
                    data.AddReference();
                    mCacheData.Add(data.Name, data);
                    chunkData.resHash.Add(resArray[i]);
                    resArray.RemoveAt(i);
                }
                else
                {
                    mCacheData.Add(resArray[i], new ResourcesData(resArray[i]));
                    chunkData.resHash.Add(resArray[i]);
                }
            }
            else
            {
                resArray.RemoveAt(i);
            }
        }
    }


    private ResourcesData ActivateDeleteCache(int index)
    {
        ResourcesData result = mDelete[index];
        mDelete.RemoveAt(index);
        return result;
    }


    public void DoUpdate()
    {
        for (int i = mLoadTask.Count - 1; i >= 0; i--)
        {
            mLoadTask[i].DoUpdate();
            if (mLoadTask[i].State == MultiState.Done)
            {
                mLoadTask.RemoveAt(i);
            }
        }

        if (Time.realtimeSinceStartup - DeleteTime >= DeleteRangeTime)
        {
            int count = Mathf.Min(mDelete.Count, DeleteCount);

            for (int i = count - 1; i >= 0; i--)
            {
                mDelete[i].Dispose();
                mDelete.RemoveAt(i);
            }

            if (count > 0)
            {
                System.GC.Collect();
                Resources.UnloadUnusedAssets();
            }

            DeleteTime = Time.realtimeSinceStartup;
        }
    }





    public void OnAppExit()
    {
        //throw new System.NotImplementedException();
    }
}