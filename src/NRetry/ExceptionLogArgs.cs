using System;

namespace NRetry {
    public class ExceptionLogArgs : EventArgs {
        public ExceptionLogArgs(Exception ex, int tryCount, string parameter) {
            Exception = ex;
            TryCount = tryCount;
            LogParameter = parameter;
        }

        public Exception Exception { get; private set; }
        public int TryCount { get; private set; }
        public string LogParameter { get; private set; }
    }
}