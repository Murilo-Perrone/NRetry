using System;
using System.ComponentModel;
using System.Threading;

namespace NRetry {
    public interface IRetryerBase {
        RetryConfig Config { get; set; }

        /// <summary>Optional method which will be called before each retry and have access to the
        /// captured Exception from the call which failed.</summary>
        Action<ExceptionLogArgs> ExceptionLogger { set; }

        /// <summary>Optional string which should be provided to the exception logger (for delegate reusage).</summary>
        string ExceptionLoggerParam { set; }

        /// <summary>Method which must be callled after each failure to do roll-back operation before another
        /// attempt can be done. It should not be responsable for give-up operations as more attempts might
        /// still have to be done.</summary>
        Action RecoveryOperation { set; }

        /// <summary>Creates a BackgroundWorker all configured to run the retryable task in background,
        /// allowing the caller to asynchronously cancel it. The results will be provided to the specified
        /// callback delegate</summary>
        BackgroundWorker CreateBackgroundRetryWorker();

        /// <summary>Starts the operation retry process asynchronously using a Timer to schedule each retry.
        /// The Timer used is provided so that it can be stoped and then disposed or started again. The 
        /// callback delegate will be provided with the retry process result, but stopping and disposing the
        /// timer will prevent the callback from being called.</summary>
        /// <returns>Activated timer, which can be used to cancel or
        /// temporarilly suspend the retry process through Timer.Start() and Timer.Stop() methods.</returns>
        Timer ProcessRetriesUsingTimer();
    }
}