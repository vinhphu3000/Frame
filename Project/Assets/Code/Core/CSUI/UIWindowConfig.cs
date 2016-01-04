using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CS.CSUI;


public class UIWindowConfig : Singleton<UIWindowConfig>, ISingleManager
{


    public readonly static Dictionary<UIType, UIWindowInfo> WindowConfig = new Dictionary<UIType, UIWindowInfo>();

    public IEnumerator Init()
    {
        WindowConfig.Add(UIType.BackpackWnd, new UIWindowInfo(UIType.BackpackWnd, typeof(BackpackWnd)));
        WindowConfig.Add(UIType.RankWnd, new UIWindowInfo(UIType.RankWnd, typeof(ShopWnd), wEffect: UIWindowEffect.CommonBack | UIWindowEffect.ExclusiveView));
        WindowConfig.Add(UIType.RecuitWnd, new UIWindowInfo(UIType.RecuitWnd, typeof(ShopWnd), wEffect: UIWindowEffect.CommonBack | UIWindowEffect.ExclusiveView));
        WindowConfig.Add(UIType.RuleWnd, new UIWindowInfo(UIType.RuleWnd, typeof(ShopWnd), wEffect: UIWindowEffect.CommonBack | UIWindowEffect.ExclusiveView));
        WindowConfig.Add(UIType.RootGobackWnd, new UIWindowInfo(UIType.RootGobackWnd, typeof(ShopWnd)));

        isInit = true;
        yield break;
    }



    public bool IsInitComplete()
    {
        return isInit;
    }
}
