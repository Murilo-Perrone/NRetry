using System;
using System.ComponentModel;
using System.Threading;

namespace NRetry {
    /// <summary>
    /// Base class with all the logic for synch or asynch operation retries.
    /// </summary>
    /// <remarks>
    /// The retryable operation may fail by signalizing failure or fails by throwing an exception.
    /// 
    /// The recovery operation is always called whenever the retryable operation fails.
    /// 
    /// The exception logger is used when an exception is catched from the
    /// retryable operation or the recovery operation. If the SupressLogExceptions config option is
    /// used, then exceptions thrown by the exception logger will interrupt the retry cycle
    /// and will reach the retry caller (as thrown or as a parameter, depending on the retry strattegy).
    /// 
    /// The asynchronous retry through background worker works the same way as the synchronous operation,
    /// but in a background thread and the retry caller is allowed to cancel it asynchronously in bettween
    /// each retryable operation attempt.
    /// 
    /// The asynchronous retry through timer creates in threads in just-in-time when they are needed
    /// to run a retryable operation attempt. It can also be cancelled by disposing the timer.
    /// 
    /// The retry caller will always have access to the last exception thrown by the retryable operation
    /// (thrown in the last operation attempt). In asynchronous retries, it will be provided back through
    /// the callback delegate. In synchronous retries, the caller can have it in two different wais,
    /// depending on the synchronous retry method chosen. Options:
    /// 1 - have the last exception catched and provided through an output parameter (of the type Exception)
    /// 2 - have the last exception left uncatched. The last try attempt will not be wrapped with a
    /// try-catch, hence the exception logger and recovery operation delegates will be skipped in this case
    /// (only in the the last try).
    /// </remarks>
    public abstract class RetryerBase : IRetryerBase {
        protected RetryerBase() {
            Config = new RetryConfig {
                //MaximumAttempts = 3,
                //RetryInterval = 60 * 1000, // 60 seconds
            };
        }

        public RetryConfig Config { get; set; }

        public Action<ExceptionLogArgs> ExceptionLogger { protected get; set; }

        public string ExceptionLoggerParam { protected get; set; }

        public Action RecoveryOperation { protected get; set; }

        /// <summary>Does one attempt and indicates if it succeeded, as in
        /// the template method design pattern.</summary>
        protected abstract bool AttemptOnce();

        protected abstract void NotifyAsyncCallback(OperationResultEventArgs args);

        public BackgroundWorker CreateBackgroundRetryWorker() {
            return CreateBackgroundRetryWorker(AttemptOnce, NotifyAsyncCallback);
        }

        public Timer ProcessRetriesUsingTimer() {
            return ProcessRetriesUsingTimer(AttemptOnce, NotifyAsyncCallback);
        }

        /// <summary>Tries all attempts, with try-catch blocks in all attempts.</summary>
        protected bool TryAll(Func<bool> processor, out int tryCount, out Exception error) {
            error = null;

            int tries = Config.MaximumAttempts;
            for (tryCount = 1; tries == 0 || tryCount <= tries; tryCount++) {
                if (TryOnce(tryCount, processor, out error))
                    return true;
            }

            tryCount--;
            return false;
        }

        /// <summary>Tries all attempts, without the use of a try-catch block on the last attempt.</summary>
        protected bool TryAll(Func<bool> processor, out int tryCount) {
            int tries = Config.MaximumAttempts;
            for (tryCount = 1; tries == 0 || tryCount < tries; tryCount++) {
                Exception error;
                if (TryOnce(tryCount, processor, out error))
                    return true;
            }

            // Trying for the last time, now without error catching
            return processor();
        }

        private void Log(Exception ex, int tryCount) {
            if (ExceptionLogger == null)
                return;
            var args = new ExceptionLogArgs(ex, tryCount, ExceptionLoggerParam);
            if (Config.SupressLogExceptions) {
                try {
                    ExceptionLogger(args);
                }
                catch { }
            }
            else {
                ExceptionLogger(args);
            }
        }

        private bool TryOnce(int tryCount, Func<bool> processor, out Exception lastError) {
            lastError = null;

            try {
                // Attempt
                if (processor())
                    return true; // Ending gracefully (no exceptions)
            }
            catch (Exception ex) {
                if (Config.MaximumAttempts > 0 && tryCount == Config.MaximumAttempts)
                    lastError = ex;

                Log(ex, tryCount);
            }
            CallRecoveryOperation(tryCount);
            Thread.Sleep(Config.RetryInterval);
            return false;
        }

        private void CallRecoveryOperation(int tryCount) {
            if (RecoveryOperation != null) {
                if (Config.SupressRecoveryException) {
                    try {
                        RecoveryOperation();
                    }
                    catch (Exception ex) {
                        Log(ex, tryCount);
                    }
                }
                else RecoveryOperation();
            }
        }

        protected virtual bool ProcessRetries(Func<bool> attempter) {
            int tryCount;
            return TryAll(attempter, out tryCount);
        }

        protected virtual bool ProcessRetries(Func<bool> attempter, out Exception error) {
            int tryCount;
            return TryAll(attempter, out tryCount, out error);
        }

        protected BackgroundWorker CreateBackgroundRetryWorker(Func<bool> attempter, Action<OperationResultEventArgs> callback) {
            var worker = new BackgroundWorker();
            bool success = false;
            int tryCount = 0;
            Exception error = null;

            // Creating the handler delegate, which creates another delegate itself
            worker.DoWork +=
                delegate(object sender, DoWorkEventArgs e) {
                    Func<bool> callWrapper =
                        delegate {
                            if (worker.CancellationPending) {
                                e.Cancel = true; // Indicate that cancellation occured (no exception)
                                return true; // Returning without an exception will end the retry cycle
                            }
                            return attempter();
                        };

                    success = TryAll(callWrapper, out tryCount, out error);
                };

            if (callback != null) {
                worker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs args) {
                    var result =
                        args.Error != null ? ResultEnum.Failure :
                            args.Cancelled ? ResultEnum.Canceled :
                                             ResultEnum.Success;
                    var resultArgs = new OperationResultEventArgs(result, args.Error ?? error, tryCount);
                    callback(resultArgs);
                };
            }

            return worker;
        }

        private OperationResultEventArgs TryOnce(Func<bool> attempter, int tryCount, Timer timer) {
            bool success = false;
            Exception error = null;
            try {
                success = attempter();
            }
            catch (Exception ex) {
                error = ex;
            }

            if (success)
                // Ending gracefully
                return new OperationResultEventArgs(ResultEnum.Success, null, tryCount);

            CallRecoveryOperation(tryCount);

            if (Config.MaximumAttempts > 0 && tryCount == Config.MaximumAttempts)
                // Giving up
                return new OperationResultEventArgs(ResultEnum.Failure, error, tryCount);

            if (error != null)
                Log(error, tryCount);

            // Scheduling next retry
            try {
                timer.Change(Config.RetryInterval, 0);
            }
            catch (ObjectDisposedException) {
                // Retry process has been aborted by disposing the timer.
                // Last exception now turns out to be the final result.
                return new OperationResultEventArgs(ResultEnum.Canceled, null, tryCount);
            }
            return null;
        }

        protected Timer ProcessRetriesUsingTimer(Func<bool> attempter, Action<OperationResultEventArgs> callback) {
            int tryCount = 0;
            Timer timer = null;

            TimerCallback timerCallback = (state) => {
                try {
                    OperationResultEventArgs resultArgs = null;
                    try {
                        resultArgs = TryOnce(attempter, ++tryCount, timer);
                    }
                    catch (Exception ex) {
                        resultArgs = new OperationResultEventArgs(ResultEnum.Failure, ex, tryCount);
                    }
                    if (resultArgs != null && callback != null)
                        callback(resultArgs);
                }
                // Supressing all exceptions to avoid an unhandled exception from killing the entire windows process
                catch { }
            };

            // Creating a single-time and immediate timer for the first attempt
            timer = new Timer(timerCallback, null, 0, 0);
            return timer;
        }
    }
}
