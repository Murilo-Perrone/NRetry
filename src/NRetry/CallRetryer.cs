using System;

namespace NRetry {
    /// <summary>
    /// Performs configurable retries for any given operation specified through a
    /// parameter-less and void-return delegate.
    /// </summary>
    public class CallRetryer : Retryer {
        public Action RetryableOperation { private get; set; }

        protected override bool AttemptOnce() {
            if (RetryableOperation == null)
                throw new ArgumentNullException("RetryableOperation");
            RetryableOperation();
            return true;
        }

        #region Static context support

        private static readonly CallRetryer _defaultInstance = new CallRetryer();

        public static CallRetryer DefaultInstance {
            get { return _defaultInstance; }
        }

        public static int ProcessRetries(Action call) {
            int tryCount;
            _defaultInstance.TryAll(_defaultInstance.AttemptOnce, out tryCount);
            return tryCount;
        }

        public static int ProcessRetries(Action call, out Exception error) {
            int tryCount;
            _defaultInstance.TryAll(_defaultInstance.AttemptOnce, out tryCount, out error);
            return tryCount;
        }

        public static IAsyncResult BeginProcessRetriesStatic(AsyncCallback callback, object state) {
            return _defaultInstance.BeginProcessRetries(callback, state);
        }

        public static IAsyncResult BeginProcessRetriesStatic(out Exception error, AsyncCallback callback, object state) {
            return _defaultInstance.BeginProcessRetries(out error, callback, state);
        }

        #endregion
    }
}