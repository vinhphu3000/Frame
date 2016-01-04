using UnityEngine;
using System.Collections;

/********************************
 * Author   :    Cui'XueLong
 * Date     :    2015年4月3日0:11:09
 * Version  :    V 0.1.0
 * Describe :    游戏状态机
 * ******************************/

namespace FSM
{

    public enum GameStateType
    {
        GlobalState,
        GameStartState,
        MainState,
        MainCityEnterState,
        LoginState,
        FighterEnterState,
        FighterOverState,
        FightingState,
        StartupAuditorState,
    }


    public class LoadSceneInfo
    {
        public string LoadSceneName { get; set; }

        public System.Action OnSceneLoadOver { get; private set; }

        public void AddEventListen(System.Action call)
        { 
            OnSceneLoadOver += call; 
        }

        virtual public void Call()
        {
            if (OnSceneLoadOver != null)
                OnSceneLoadOver();
        }

        virtual public void Reset()
        {
            OnSceneLoadOver = null;
            LoadSceneName = string.Empty; 
        }
    }

    public abstract class GameState
    {

        public bool IsInit { get; protected set; }

        /// <summary>
        /// 状态进入  
        /// 优先级 最高
        /// </summary>

        public abstract void Enter();


        /// <summary>
        /// 场景加载完成
        /// 触发来自有场景加载
        /// </summary>

        public virtual void LoadSceneFinish() {  }


        /// <summary>
        /// 状态离开
        /// </summary>

        public abstract void Leave();


        /// <summary>
        /// 帧更新   
        /// 优先级 状态进入第二帧会执行 LateUpdate 
        /// </summary>

        public abstract void Update();

    }

}

