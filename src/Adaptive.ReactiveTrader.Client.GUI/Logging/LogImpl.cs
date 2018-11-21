using System;
using System.Diagnostics;
using Adaptive.ReactiveTrader.Shared.Logging;

namespace Adaptive.ReactiveTrader.Client.Logging
{
    internal class LogImpl : ILog
    {
        public void Info(string msg, Exception ex = null)
        {
            Debug.WriteLine(msg + " " + ex);
        }

        public void InfoFormat(string msg, params object[] parameters)
        {
            Debug.WriteLine(msg, parameters);
        }

        public void Warn(string msg, Exception ex = null)
        {
            Debug.WriteLine(msg + " " + ex);
        }

        public void WarnFormat(string msg, params object[] parameters)
        {
            Debug.WriteLine(msg, parameters);
        }

        public void Error(string msg, Exception ex = null)
        {
            Debug.WriteLine(msg + " " + ex);
        }

        public void ErrorFormat(string msg, params object[] parameters)
        {
            Debug.WriteLine(msg, parameters);
        }
    }
}