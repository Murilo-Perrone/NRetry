using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace NRetry {
    public abstract class Retryer : RetryerBase, IRetryer {
        #region IRetryer Members
        public bool ProcessRetries() {
            return base.ProcessRetries(AttemptOnce);
        }

        public bool ProcessRetries(out Exception error) {
            return base.ProcessRetries(AttemptOnce, out error);
        }

        public Action<OperationResultEventArgs> AsyncRetryCallback { protected get; set; }
        #endregion

        protected override void NotifyAsyncCallback(OperationResultEventArgs args) {
            //bool canceled = !args.Success && args.Error == null;
            AsyncRetryCallback(args);
        }

        #region Asynchronous call wrappers
        public IAsyncResult BeginProcessRetries(AsyncCallback callback, object state) {
            Func<bool> method = ProcessRetries;
            return method.BeginInvoke(callback, state);
        }

        private delegate bool ProcessRetriesDelegate(out Exception error);

        public IAsyncResult BeginProcessRetries(out Exception error, AsyncCallback callback, object state) {
            ProcessRetriesDelegate method = ProcessRetries;
            return method.BeginInvoke(out error, callback, state);
        }

        public bool EndProcessRetries(IAsyncResult result) {
            return EndProcessRetriesStatic(result);
        }

        public bool EndProcessRetries(out Exception error, IAsyncResult result) {
            return EndProcessRetriesStatic(out error, result);
        }

        public static bool EndProcessRetriesStatic(IAsyncResult result) {
            var method = (Func<bool>)((AsyncResult)result).AsyncDelegate;
            return method.EndInvoke(result);
        }

        public static bool EndProcessRetriesStatic(out Exception error, IAsyncResult result) {
            var method = (ProcessRetriesDelegate)((AsyncResult)result).AsyncDelegate;
            return method.EndInvoke(out error, result);
        }
        #endregion
    }

    public abstract class Retryer<TReturn> : RetryerBase, IRetryer<TReturn> {
        [ThreadStatic]
        protected TReturn _return;

        protected Retryer() {
        }

        protected Retryer(IRetryable<TReturn> opeartion) {
            RetryableOperation = opeartion.Attempt;
            RecoveryOperation = opeartion.Recover;
        }

        #region IRetryer<TReturn> Members

        /// <summary>Retryable getter operation</summary>
        public Func<TReturn> RetryableOperation { protected get; set; }

        public TReturn ProcessRetries() {
            bool success = base.ProcessRetries(AttemptOnce);
            return _return;
        }

        public TReturn ProcessRetries(out Exception error) {
            bool success = base.ProcessRetries(AttemptOnce, out error);
            return _return;
        }

        public Action<OperationResultEventArgs<TReturn>> AsyncRetryCallback { protected get; set; }
        #endregion

        protected override bool AttemptOnce() {
            _return = default(TReturn);
            _return = RetryableOperation();
            return true;
        }

        protected override void NotifyAsyncCallback(OperationResultEventArgs args) {
            AsyncRetryCallback(new OperationResultEventArgs<TReturn>(args, _return));
        }

        #region Asynchronous call wrappers
        public IAsyncResult BeginProcessRetries(AsyncCallback callback, object state) {
            Func<TReturn> method = ProcessRetries;
            return method.BeginInvoke(callback, state);
        }

        private delegate TReturn ProcessRetriesDelegate(out Exception error);

        public IAsyncResult BeginProcessRetries(out Exception error, AsyncCallback callback, object state) {
            ProcessRetriesDelegate method = ProcessRetries;
            return method.BeginInvoke(out error, callback, state);
        }

        public TReturn EndProcessRetries(IAsyncResult result) {
            var method = (Func<TReturn>)((AsyncResult)result).AsyncDelegate;
            return method.EndInvoke(result);
        }

        public TReturn EndProcessRetries(out Exception error, IAsyncResult result) {
            var method = (ProcessRetriesDelegate)((AsyncResult)result).AsyncDelegate;
            return method.EndInvoke(out error, result);
        }

        public static TReturn EndProcessRetriesStatic(IAsyncResult result) {
            var method = (Func<TReturn>)((AsyncResult)result).AsyncDelegate;
            return method.EndInvoke(result);
        }

        public static TReturn EndProcessRetriesStatic(out Exception error, IAsyncResult result) {
            var method = (ProcessRetriesDelegate)((AsyncResult)result).AsyncDelegate;
            return method.EndInvoke(out error, result);
        }
        #endregion
    }
}