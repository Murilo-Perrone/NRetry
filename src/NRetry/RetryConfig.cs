namespace NRetry {
    /// <summary>
    /// Configuration of the retrier mechanism.
    /// </summary>
    public struct RetryConfig {
        private bool _unprotectedLog;

        /// <summary>Maximum operation attempts to do before giving up, or zero to retry untill the operation succeeds.</summary>
        public int MaximumAttempts { get; set; }

        /// <summary>Time in milliseconds to wait before doing a retry (after a failed attempt).</summary>
        public int RetryInterval { get; set; }

        /// <summary>Indicates rather if exceptions thrown by recovery operation should be supressed.
        /// In this case, the exception will be logged and the retry process will continue.
        /// Default is false.</summary>
        public bool SupressRecoveryException { get; set; }

        /// <summary>Indicates if an exception thrown by the exception logger delegate should be
        /// catched, or if it will be left uncatched, interrupting the retry cycle. Default is true.</summary>
        public bool SupressLogExceptions {
            get { return !_unprotectedLog; }
            set { _unprotectedLog = !value; }
        }
    }
}
