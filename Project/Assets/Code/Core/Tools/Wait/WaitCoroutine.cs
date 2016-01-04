using UnityEngine;
using System.Collections;


namespace Tools
{

    public enum WaitState
    {
        Run,
        Done,
        Error,
    }


    public class WaitCoroutine : IEnumerator, System.IDisposable
    {


        public System.Action<WaitCoroutine> OnCompleted;

        public WaitState mState { get; protected set; }



        protected Coroutine Start()
        {
            mState = WaitState.Run;
            return WaitHelp.Instance.StartCoroutine(this);
        }


        public object Current
        {
            get { return mState; }
        }

        virtual public bool MoveNext()
        {
            return true;
        }

        virtual public void Reset()
        {
            OnCompleted = null;
        }

        virtual public void Dispose()
        {
        }

        protected void CallCompleted()
        {
            if (OnCompleted != null)
                OnCompleted(this);
        }
    }
}