using System;
using System.ComponentModel;
using System.Threading;

namespace NRetry {
    /// <summary>
    /// Performs configurable retries for any given operation specified that returns TReturn.
    /// </summary>
    public class ProcessRetryer<TReturn> : Retryer<TReturn> {
        public delegate bool FailureDetectorDelegate(TReturn result);

        public ProcessRetryer() {
        
        }
        public ProcessRetryer(IRetryable<TReturn> operation) {
            RetryableOperation = operation.Attempt;
            RecoveryOperation = operation.Recover;
        }

        public FailureDetection FailureDetectionMethod { private get; set; }
        public TReturn FailureReturnValue { private get; set; }
        public FailureDetectorDelegate FailureDetector { private get; set; }

        protected override bool AttemptOnce() {
            _return = default(TReturn);
            _return = RetryableOperation();
            switch (FailureDetectionMethod) {
                case FailureDetection.None:
                    return true;
                case FailureDetection.ByReturnValue:
                    return !Equals(_return, FailureReturnValue);
                case FailureDetection.ByFailureDetector:
                    if (FailureDetector == null) {
                        throw new ArgumentNullException("FailureDetector",
                                                        "Must specify the failure detector delegate when using failure detection method = ByFailureDetector.");
                    }
                    return FailureDetector(_return);
                default:
                    throw new ArgumentOutOfRangeException("FailureDetectionMethod");
            }
        }

        #region Static context support

        public static readonly ProcessRetryer<TReturn> _defaultInstance = new ProcessRetryer<TReturn>();

        public static ProcessRetryer<TReturn> DefaultInstance {
            get { return _defaultInstance; }
        }

        public static TReturn ProcessRetries(Func<TReturn> getter) {
            int tryCount;
            _defaultInstance.TryAll(_defaultInstance.AttemptOnce, out tryCount);
            return _defaultInstance._return;
        }

        public static TReturn ProcessRetries(Func<TReturn> getter, out Exception error) {
            int tryCount;
            _defaultInstance.TryAll(_defaultInstance.AttemptOnce, out tryCount, out error);
            return _defaultInstance._return;
        }

        #endregion
    }
}