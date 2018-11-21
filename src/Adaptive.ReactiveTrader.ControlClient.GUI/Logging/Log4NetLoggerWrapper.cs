using System;
using Adaptive.ReactiveTrader.Shared.Logging;

namespace Adaptive.ReactiveTrader.ControlClient.GUI.Logging
{
    internal class Log4NetLoggerWrapper : ILog
    {
        private readonly log4net.ILog _logger;

        public Log4NetLoggerWrapper(log4net.ILog logger)
        {
            _logger = logger;
        }

        public void Info(string msg, Exception ex = null)
        {
            _logger.Info(msg, ex);
        }

        public void InfoFormat(string msg, params object[] parameters)
        {
            _logger.InfoFormat(msg, parameters);
        }

        public void Warn(string msg, Exception ex = null)
        {
            _logger.Warn(msg, ex);
        }

        public void WarnFormat(string msg, params object[] parameters)
        {
            _logger.WarnFormat(msg, parameters);
        }

        public void Error(string msg, Exception ex = null)
        {
            _logger.Error(msg, ex);
        }

        public void ErrorFormat(string msg, params object[] parameters)
        {
            _logger.ErrorFormat(msg, parameters);
        }
    }
}