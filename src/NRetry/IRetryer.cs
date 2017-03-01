using System;

namespace NRetry {
    /// <summary>A self-contained process retryer which has all parameters necessary to retry a target
    /// retryable operation using synchronous or asynchronous methods.</summary>
    public interface IRetryer : IRetryerBase {
        /// <summary>Delegate which will handle the final result of the asynchronous retry process.</summary>
        Action<OperationResultEventArgs> AsyncRetryCallback { set; }

        /// <summary>Attempts the retryable operation to <seealso cref="RetryConfig.MaximumAttempts"/> times,
        /// adding some sleep so that there is a interval of <seealso cref="RetryConfig.RetryInterval"/>
        /// milliseconds between each retry.</summary>
        bool ProcessRetries();

        bool ProcessRetries(out Exception error);
    }

    public interface IRetryer<TReturn> : IRetryerBase {
        Func<TReturn> RetryableOperation { set; }

        Action<OperationResultEventArgs<TReturn>> AsyncRetryCallback { set; }

        TReturn ProcessRetries();

        TReturn ProcessRetries(out Exception error);
    }
}