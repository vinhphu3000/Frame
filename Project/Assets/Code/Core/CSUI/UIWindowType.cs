using UnityEngine;
using System.Collections;


namespace CS.CSUI
{


    public enum UIType
    {
        BackpackWnd,
        RankWnd,
        RecuitWnd,
        RuleWnd,
        RootGobackWnd,
    }



    public enum UIWindowType : int
    {
        Background = 0,
        ModelType = 1000,
        Dialog = 2000,
        TopLevel = 3000,
    }




    [System.Flags]
    public enum UIWindowEffect 
    {
        Default,

        /// <summary>
        /// 根节点
        /// </summary>
        Root,

        /// <summary>
        /// 显示时独占窗口视图
        /// </summary>
        ExclusiveView,

        /// <summary>
        /// 通用返回
        /// </summary>
        CommonBack,
        
    }

    public class UIWindowInfo
    {

        public System.Type Owner;

        public UIType Key { get; private set; }

        public UIWindowType WinType { get; private set; }

        public UIWindowEffect WinEffect { get; private set; }

        private string mResourceName;

        /// <summary>
        /// 窗口信息
        /// </summary>
        /// <param name="key">标识</param>
        /// <param name="t">宿主脚本</param>
        /// <param name="resName">资源名称,如果resName为null取key.tostring()</param>
        /// <param name="wType">窗口类型</param>
        /// <param name="wEffect">窗口效果</param>
        public UIWindowInfo(UIType key, System.Type t, string resName = "", UIWindowType wType = UIWindowType.ModelType, UIWindowEffect wEffect = UIWindowEffect.Default)
        {
            Key = key;
            Owner = t;
            WinType = wType;
            WinEffect = wEffect;
            mResourceName = resName;
        }

        public string GetResourcePath()
        {
            return string.IsNullOrEmpty(mResourceName) ? Key.ToString() : mResourceName;
        }


    }

}

