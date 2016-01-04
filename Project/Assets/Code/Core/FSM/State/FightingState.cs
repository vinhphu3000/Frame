using UnityEngine;
using System.Collections;

/********************************
 * Author   :    Cui'XueLong
 * Date     :    2015年4月3日0:19:38
 * Version  :    V 0.1.0
 * Describe :    战斗中
 * ******************************/

namespace FSM
{

    public class FightingState : GameState
    {
        public override void Enter()
        {
            //BattleMsg.Instance.CG_FIGHT_INIT(SceneID.fightScenePID, 1, 0, 0);  
        }

        public override void Leave()
        {
            //SceneManagerBase.Clear();
            //Joystick.JoystickList.Clear();
            //BattlerDataManager.Instance.Clean();
        }

        public override void Update()
        {
            //InputManager.Instance.DoUpdate();
            //FightSceneManager.Current.DoUpdate();
        }
    }

}

