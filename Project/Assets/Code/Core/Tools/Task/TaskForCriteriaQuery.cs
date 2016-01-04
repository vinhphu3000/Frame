using UnityEngine;
using System.Collections;


namespace Tools
{

    public sealed class TaskForCriteriaQuery
    {

        private System.Func<bool> mQueryFunction;


        public TaskForCriteriaQuery(System.Func<bool> func)
        {
            mQueryFunction = func;
        }


        public bool IsQueryConclude()
        {
            if (mQueryFunction != null)
                return mQueryFunction();

            return true;
        }


        public void StopQuery()
        {
            mQueryFunction = null;
        }

    }

}

