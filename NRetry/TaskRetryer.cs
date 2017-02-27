using System;

namespace NRetry {
    public class TaskRetryer : Retryer {
        public Func<bool> RetryableOperation;

        public TaskRetryer() {
        }

        public TaskRetryer(IRetryable<bool> operation) {
            RetryableOperation = operation.Attempt;
            RecoveryOperation = operation.Recover;
        }

        protected override bool AttemptOnce() {
            return RetryableOperation();
        }

        /// <summary>Retries the process up to <seealso cref="RetryerBase{TResult}.MaximumAttempts"/> by calling the
        /// <see cref="attempter"/> and the <see cref="recoverer"/> each times the attempt fails.
        /// Before each retry, it will wait for <seealso cref="RetryerBase{TResult}.RetryInterval"/> milliseconds.</summary>
        /// <remarks>Throws ArgumentNullException if no attempt method is defined.
        /// Ignores any previously stored Attempt and Recovery delegates.</remarks>
        /// <returns>True if any of the attempts succeeds.</returns>
        public bool Process(Func<bool> attempter, Action recoverer) {
            if (attempter == null)
                throw new ArgumentNullException("attempter");

            RetryableOperation = attempter;
            RecoveryOperation = recoverer;
            return ProcessRetries();
        }

        #region Static context support

        public static readonly TaskRetryer _defaultInstance = new TaskRetryer();

        public static TaskRetryer DefaultInstance {
            get { return _defaultInstance; }
        }

        public static bool ProcessRetries(Func<bool> attempter, Action recoverer) {
            return DefaultInstance.Process(attempter, recoverer);
        }

        public static bool ProcessRetries(IRetryable<bool> operation) {
            return DefaultInstance.Process(operation.Attempt, operation.Recover);
        }

        #endregion
    }
}