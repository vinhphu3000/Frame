using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class BundleManager
{

    private Dictionary<string, BundleData> mBundleData;
    private Dictionary<string, BundleBuildState> mBuildStates;

    static private BundleManager instance;
    static public BundleManager Instance
    {
        get 
        {
            if (instance != null)
                return instance;

            instance = new BundleManager();

            instance.Init();

            return instance;
        }
    }


    private void Init()
    {
        mBuildStates = new Dictionary<string, BundleBuildState>();
        var buildList = BMDataAccessor.BuildStates;
        for (int i = 0; i < buildList.Count; i++)
        {
            mBuildStates.Add(buildList[i].bundlePath, buildList[i]);
        }


        mBundleData = new Dictionary<string, BundleData>();
    }


    static public BundleData[] GetAllBundleData()
    {
        return Instance.mBundleData.Values.ToArray();
    }


    static public BundleData GetBundleData( string bundlePath)
    {
        if (Instance.mBundleData.ContainsKey(bundlePath))
            return Instance.mBundleData[bundlePath];
        return null;
    }

    static public bool IsHaveBundleData(string bundlePath)
    {
        return Instance.mBundleData.ContainsKey(bundlePath);
    }


    static public void AddNewBundleData( BundleData data )
    {
        Instance.mBundleData.Add(data.assetPath, data);
    }


    static public void ClearBundleData()
    {
        Instance.mBundleData.Clear();
    }

    static public BundleBuildState GetBuildState( string bundlePath )
    {
        BundleBuildState result = null;
        Instance.mBuildStates.TryGetValue(bundlePath, out result);
        return result;
    }


    static public BundleBuildState GetBuildStateNoNull(string bundlePath)
    {
        BundleBuildState bundleState = null;
        Instance.mBuildStates.TryGetValue(bundlePath, out bundleState);

        if (bundleState == null)
        {
            bundleState = new BundleBuildState();
            bundleState.bundlePath = bundlePath;
            Instance.mBuildStates.Add(bundlePath, bundleState);
            BMDataAccessor.BuildStates.Add(bundleState);
        }

        return bundleState;
    }


    static public void UpdateBundleChangeTime( string bundlePath )
    {
        Instance.mBuildStates[bundlePath].changeTime = DateTime.Now.ToBinary();
    }

}
