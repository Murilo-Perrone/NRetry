using System;
using System.ComponentModel;

namespace NRetry {
    public class OperationResultEventArgs : AsyncCompletedEventArgs {
        protected readonly int _tryCount;
        protected readonly bool _success;
        private readonly ResultEnum _result;

        public OperationResultEventArgs(ResultEnum result, Exception error, int tryCount)
            : base(error, result == ResultEnum.Canceled, null) {
            _result = result;
            _tryCount = tryCount;
            _success = result == ResultEnum.Success;
        }

        public ResultEnum Result { get { return _result; } }
        public int TryCount { get { return _tryCount; } }
        public bool Success { get { return _success; } }
    }

    public class OperationResultEventArgs<TReturn> : OperationResultEventArgs {
        public OperationResultEventArgs(ResultEnum result, Exception error, int tryCount, TReturn operationReturn)
            : base(result, error, tryCount) {
            Return = operationReturn;
        }

        public OperationResultEventArgs(OperationResultEventArgs args, TReturn result)
            : this(args.Result, args.Error, args.TryCount, result) {
        }

        public TReturn Return { get; private set; }
    }
}