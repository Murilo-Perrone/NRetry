using System.Threading;
using NRetry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NRetry.Tests {
    /// <summary>
    ///This is a test class for TaskRetryerTest and is intended
    ///to contain all TaskRetryerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TaskRetryerTest {
        private int _tries = 0;
        private const int TriesToFail = 10;

        [TestInitialize()]
        public void TestInitialize() {
            _tries = 0;
        }

        public bool Try() {
            if (++_tries <= TriesToFail)
                throw new OperationCanceledException("Failing...");
            return true;
        }

        private static RetryConfig GetConfigForFailure() {
            return new RetryConfig() {
                MaximumAttempts = TriesToFail,
                RetryInterval = 500,
            };
        }

        private TaskRetryer GetRetryer() {
            return new TaskRetryer {
                Config = GetConfigForFailure(),
                RetryableOperation = Try
            };
        }

        [TestMethod()]
        [ExpectedException(typeof(OperationCanceledException))]
        public void FailureSyncStaticTest() {
            Exception error = null;

            var instance = TaskRetryer.DefaultInstance;
            instance.Config = GetConfigForFailure();
            instance.RetryableOperation = Try;
            instance.ExceptionLogger = (ex) => error = ex.Exception;

            // Trying with exception suppression
            bool actual = TaskRetryer.ProcessRetries(Try, null); // out error
            Assert.AreEqual(_tries, TriesToFail);
            Assert.AreEqual(_tries, actual);
            Assert.IsInstanceOfType(error, typeof(OperationCanceledException));

            // Trying without exception suppression
            TestInitialize();
            TaskRetryer.ProcessRetries(Try, null);
        }

        [TestMethod()]
        public void FailureSynchTest1() {
            var instance = GetRetryer();

            // Trying with exception suppression
            Exception error;
            IAsyncResult result = instance.BeginProcessRetries(out error, null, null);
            Thread.Sleep(1000);
            bool actual = instance.EndProcessRetries(out error, result);
            Assert.AreEqual(_tries, TriesToFail);
            Assert.AreEqual(false, actual);
            Assert.IsInstanceOfType(error, typeof(OperationCanceledException));
        }

        [TestMethod()]
        [ExpectedException(typeof(OperationCanceledException))]
        public void FailureSynchTest2() {
            var instance = GetRetryer();

            // Trying without exception suppression
            IAsyncResult result = instance.BeginProcessRetries(null, null);
            bool actual = instance.EndProcessRetries(result);
        }

        [TestMethod()]
        public void FailureAsyncTest1() {
            var instance = GetRetryer();

            // Trying with exception suppression
            Exception error;
            IAsyncResult result = instance.BeginProcessRetries(out error, null, null);
            Thread.Sleep(1000);
            bool actual = instance.EndProcessRetries(out error, result);
            Assert.AreEqual(_tries, TriesToFail);
            Assert.AreEqual(false, actual);
            Assert.IsInstanceOfType(error, typeof(OperationCanceledException));
        }

        [TestMethod()]
        [ExpectedException(typeof(OperationCanceledException))]
        public void FailureAsyncTest2() {
            var instance = GetRetryer();

            // Trying without exception suppression
            IAsyncResult result = instance.BeginProcessRetries(null, null);
            bool actual = instance.EndProcessRetries(result);
        }
    }
}
