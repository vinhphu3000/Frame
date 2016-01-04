using UnityEngine;
using System.Collections;


namespace Tools
{
    public class WaitForWww : WaitCoroutine
    {

        public WWW Www { get; private set; }

        public string Url { get; private set; }


        ~WaitForWww()
        {
            Dispose();
            System.GC.SuppressFinalize(this);
        }


        public Coroutine Start( string url )
        {
            Url = url;
            Www = new WWW(Url);
            return base.Start();
        }


        override public bool MoveNext()
        {
			if (Www == null)
				return false;

            if (!string.IsNullOrEmpty (Www.error))
            {
                Debug.Log(string.Format("WWW Error {0} , byt have safe release! ", Www.error));

                Dispose();
                mState = WaitState.Error;
				CallCompleted();

                return false;
            }
            else if (Www.isDone)
            {
                mState = WaitState.Done;
                CallCompleted();
                return false;
            }

            return true;
        }


        override public void Reset()
        {
            base.Reset();
        }


        override public void Dispose()
        {
            if (Www != null)
            {
                Www.Dispose();
                Www = null;
            }

            //Url = string.Empty;
        }
    }


}

