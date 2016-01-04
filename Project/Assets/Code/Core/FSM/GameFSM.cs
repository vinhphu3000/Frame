using UnityEngine;
using System.Collections;
using FSM;

/********************************
 * Author   :    Cui'XueLong
 * Date     :    2015年4月3日0:47:11
 * Version  :    V 0.1.0
 * Describe :    游戏状体机
 * ******************************/

public sealed class GameFSM : MonoBehaviour
{


    public static GameFSM Instance { private set; get; }


    public GameState cur_state { get; private set; }
    public GameState pre_state { get; private set; }
    public GameStateType State { get; private set; }

    private GameStartState startState;
    private GlobalState globalState;
    private FighterEnterState fighterEnterState;
    private FighterOverState fighterOverState;
    private FightingState fighting;
    private MainState mainState;
    private LoginState loginState;
    private StartupAuditorState startupAuditorState;

    void Awake()
    {
        Instance = this;

        startState = new GameStartState();
        globalState = new GlobalState();
        mainState = new MainState();
        fighterEnterState = new FighterEnterState();
        fighterOverState = new FighterOverState();
        fighting = new FightingState();
        loginState = new LoginState();
        startupAuditorState = new StartupAuditorState();

        OpenGlobalState();

        State = GameStateType.GameStartState;
        ChangeState(startState);
    }


    void Update()
    {
        if (globalState != null)
            globalState.Update();
    }

    public void LateUpdate()
    {
        if (cur_state != null)
            cur_state.Update();
    }



    public void ChangeState(GameStateType type)
    {
        State = type;
        switch (type)
        {
            case GameStateType.GameStartState: ChangeState(startState);
                break;
            case GameStateType.FighterEnterState:ChangeState(fighterEnterState);
                break;
            case GameStateType.FighterOverState:ChangeState(fighterOverState);
                break;
            case GameStateType.FightingState:ChangeState(fighting);
                break;
            case GameStateType.MainState: ChangeState(mainState);
                break;
            case GameStateType.LoginState: ChangeState(loginState);
                break;
            case GameStateType.StartupAuditorState: ChangeState(startupAuditorState);
                break;
            default:
                throw new System.Exception(string.Format("{0} 状态不能执行!", type.ToString()));
        }
    }


    public void OnDestroy()
    {
        CloseGlobalState();
    }


    private void OpenGlobalState()
    {
        Debug.Log("[GameFSM] => [Open Global State!]");
        globalState.Enter();
    }


    private void CloseGlobalState()
    {
        Debug.Log("[GameFSM] => [Close Global State!]");
        globalState.Leave();
    }


    private void ChangeState( GameState new_state)
    {
        if (cur_state == new_state || new_state == null) return;

        pre_state = new_state;

        if (cur_state != null)
        {
            cur_state.Leave();
            Debug.Log(string.Format("[GameFSM] => [Leave {0} State]", cur_state.ToString()));
        }
        cur_state = new_state;

        cur_state.Enter();
        Debug.Log(string.Format("[GameFSM] => [进入 {0} State]", cur_state.ToString()));
    }



    public void AppStartLoadScene()
    {
        //throw new System.NotImplementedException();
    }




    //public void AppLoadFinish()
    //{
    //    Debug.Log("1111");
    //    cur_state.LoadSceneFinish();
    //}
}
