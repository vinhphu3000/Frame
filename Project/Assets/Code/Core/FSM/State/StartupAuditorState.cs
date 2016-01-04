using CS.CSUI;
using Tools;
using UnityEngine;
using System.Collections;


namespace FSM
{

    public class StartupAuditorState : GameState
    {

        private WaitForStartupAuditor startupAuditor;

        public override void Enter()
        {
            startupAuditor = new WaitForStartupAuditor();
            startupAuditor.Push(ResourcesManager.Instance);
            startupAuditor.Push(UIManager.Instance);
            startupAuditor.Push(UIWindowConfig.Instance);
            startupAuditor.Start();
            GameStarter.Instance.AddGlobalEvent(startupAuditor);
        }

        public override void Leave()
        {
            GameStarter.Instance.RemoveGlobalEvent(startupAuditor);
            startupAuditor = null;
        }

        public override void Update()
        {
            if (startupAuditor.IsComplete())
            {
                GameFSM.Instance.ChangeState(GameStateType.LoginState);
            }
        }

    }

}
