using UnityEngine;
using System.Collections;


namespace Tools
{

    public class WaitForFunction : WaitCoroutine
    {
        readonly System.Func<bool> mRepetitionCall;

        public WaitForFunction(System.Func<bool> call)
        {
            mRepetitionCall = call;
        }


        public new Coroutine Start()
        {
            return base.Start();
        }


        public override bool MoveNext()
        {
            try
            {

                if (mRepetitionCall())
                {
                    mState = WaitState.Done;
                    CallCompleted();

                    //  immediately clear ref
                    OnCompleted = null;

                    return false;
                }

                return true;
            }
            catch (System.Exception e)
            {
                Debug.Log(string.Format("Error {0}", e.ToString()));
                return false;
            }
        }


        public bool IsDone
        {
            get { return mState == WaitState.Done; }
        }

        //public override void Reset()
        //{
        //    base.Reset();
        //}

        //public override void Dispose()
        //{
        //    base.Dispose();
        //}
    }

}

