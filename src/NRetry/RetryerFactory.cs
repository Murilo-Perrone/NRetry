using System;

namespace NRetry {
    /// <summary>
    /// Creates a pre-configured CallRetryer, TaskRetryer or ProcessRetryer, depending on your needs.
    /// </summary>
    public class RetryerFactory {
        public RetryConfig Config { get; set; }

        public Action<ExceptionLogArgs> ExceptionLogger { protected get; set; }

        public string ExceptionLoggerParam { protected get; set; }

        public Action RecoveryOperation { protected get; set; }

        private void Configure(IRetryerBase retryer) {
            retryer.Config = Config;
            retryer.ExceptionLogger = ExceptionLogger;
            retryer.ExceptionLoggerParam = ExceptionLoggerParam;
            retryer.RecoveryOperation = RecoveryOperation;
        }

        public ProcessRetryer<T> CreateProcessRetryer<T>(Func<T> operation) {
            return CreateProcessRetryer(operation, null);
        }

        public ProcessRetryer<T> CreateProcessRetryer<T>(Func<T> operation, Action<OperationResultEventArgs<T>> asyncCallback) {
            var retryer = new ProcessRetryer<T>() {
                RetryableOperation = operation,
                AsyncRetryCallback = asyncCallback,
            };
            Configure(retryer);
            return retryer;
        }

        public TaskRetryer CreateTaskRetryer(Func<bool> operation) {
            return CreateTaskRetryer(operation, null);
        }

        public TaskRetryer CreateTaskRetryer(Func<bool> operation, Action<OperationResultEventArgs> asyncCallback) {
            var retryer = new TaskRetryer() {
                RetryableOperation = operation,
                AsyncRetryCallback = asyncCallback,
            };
            Configure(retryer);
            return retryer;
        }

        public CallRetryer CreateCallRetryer(Action operation) {
            return CreateCallRetryer(operation, null);
        }

        public CallRetryer CreateCallRetryer(Action operation, Action<OperationResultEventArgs> asyncCallback) {
            var retryer = new CallRetryer() {
                RetryableOperation = operation,
                AsyncRetryCallback = asyncCallback,
            };
            Configure(retryer);
            return retryer;
        }
    }
}
