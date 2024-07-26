using System;
using UnityEngine;

namespace PLATEAU.Util
{
    /// <summary>
    /// 条件を満たしたときだけログを出すクラスです。
    /// </summary>
    public class ConditionalLogger
    {
        private Func<bool> condition;

        public ConditionalLogger(Func<bool> condition)
        {
            this.condition = condition;
        }

        public void Log(string logText)
        {
            if(this.condition()) Debug.Log(logText);
        }
    }
}