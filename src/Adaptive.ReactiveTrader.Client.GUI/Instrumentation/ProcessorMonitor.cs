using System;
using System.Diagnostics;
using Adaptive.ReactiveTrader.Client.Domain.Instrumentation;

namespace Adaptive.ReactiveTrader.Client.Instrumentation
{
    public class ProcessorMonitor : IProcessorMonitor
    {
        private readonly Process _currentProcess;
        private TimeSpan _lastProcessTime;

        public ProcessorMonitor()
        {
            _currentProcess = Process.GetCurrentProcess();
            _lastProcessTime = _currentProcess.UserProcessorTime;
        }

        public TimeSpan CalculateProcessingAndReset()
        {
            var currentProcessTime = _currentProcess.UserProcessorTime;
            var result = currentProcessTime.Subtract(_lastProcessTime);
            _lastProcessTime = currentProcessTime;

            return result;
        }

        public bool IsAvailable { get { return true; } }
    }
}