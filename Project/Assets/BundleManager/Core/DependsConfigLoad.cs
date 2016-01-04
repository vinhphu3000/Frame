using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Tools;

public class DependsConfigLoad  
{

    WaitForWww mWww;

    public Coroutine Start(string url)
    {
        mWww = new WaitForWww();
        return mWww.Start(url);
    }


#if USE_5_BUNDLE
    public AssetBundleManifest GetConfig()
#else
    public List<BundleDependsData> GetConfig()
#endif
    {
#if USE_5_BUNDLE

        if (mWww == null)
        {
            Debug.LogError("加载AssetBundleManifest失败!");
            return new AssetBundleManifest();
        }

        var depends = mWww.Www.assetBundle;
        AssetBundleManifest resultManifest = (AssetBundleManifest) depends.LoadAsset("AssetBundleManifest");
        return resultManifest;

#else
        if (mWww == null)
            return new List<BundleDependsData>();

        List<BundleDependsData> result = new List<BundleDependsData>();
        if (mWww.mState == WaitState.Done)
        {
            string textData = mWww.Www.text;
            var lines = textData.Split(System.Environment.NewLine.ToCharArray());

            for (int i = 0; i < lines.Length; i++)
            {
                BundleDependsData data = BundleDependsData.Deserialize(lines[i]);
                if (data != null)
                    result.Add(data);
            }
        }

        return result;
#endif
    }


}
