using System.Threading;
using NRetry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NRetry.Tests {
    /// <summary>
    /// This is a test class for CallRetryerTest and is intended
    /// to contain all CallRetryerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CallRetryerTest {
        private int _tries = 0;
        private const int TriesToFail = 10;

        [TestInitialize()]
        public void TestInitialize() {
            _tries = 0;
        }

        public void Try() {
            if (++_tries <= TriesToFail)
                throw new OperationCanceledException("Failing...");
        }

        private static RetryConfig GetConfigForFailure() {
            return new RetryConfig() {
                MaximumAttempts = TriesToFail,
                RetryInterval = 500,
            };
        }

        private static RetryConfig GetConfigForSuccess() {
            return new RetryConfig() {
                MaximumAttempts = 0,
                RetryInterval = 500,
            };
        }

        private CallRetryer GetRetryer(bool staticInstance, bool succeed) {
            var config = succeed ? GetConfigForSuccess() : GetConfigForFailure();
           
            if (staticInstance) {
                var instance = CallRetryer.DefaultInstance;
                instance.Config = config;
                instance.RetryableOperation = Try;
                return instance;
            }

            return new CallRetryer {
                Config = config,
                RetryableOperation = Try,
            };
        }

        #region Static Synch Tests
        public void TestStatic(bool synchronous, bool succeed, bool supressException) {
            var instance = GetRetryer(true, succeed);

            if (supressException) {
                // Trying with exception suppression
                Exception error;
                if (synchronous) {
                    int actual = CallRetryer.ProcessRetries(Try, out error);
                    Assert.AreEqual(_tries, actual);
                }
                else {
                    IAsyncResult result = CallRetryer.BeginProcessRetriesStatic(out error, null, null);
                    Thread.Sleep(1000);
                    bool actual = CallRetryer.EndProcessRetriesStatic(out error, result);
                    Assert.AreEqual(succeed, actual);
                }
                if (succeed) {
                    Assert.AreEqual(TriesToFail + 1, _tries);
                    Assert.IsNull(error);
                }
                else {
                    Assert.AreEqual(TriesToFail, _tries);
                    Assert.IsInstanceOfType(error, typeof(OperationCanceledException));
                }
            }
            else {
                // Trying without exception suppression
                if (synchronous) {
                    int actual = CallRetryer.ProcessRetries(Try);
                    if (succeed)
                        Assert.AreEqual(_tries, actual);
                }
                else {
                    IAsyncResult result = CallRetryer.BeginProcessRetriesStatic(null, null);
                    Thread.Sleep(1000);
                    bool actual = CallRetryer.EndProcessRetriesStatic(result);
                    if (succeed)
                        Assert.AreEqual(true, actual);
                }
                if (succeed)
                    Assert.AreEqual(TriesToFail + 1, _tries);
            }
        }

        [TestMethod()]
        public void SuccessSyncStaticTest1() {
            TestStatic(true, true, false);
        }

        [TestMethod()]
        public void SuccessSyncStaticTest2() {
            TestStatic(true, true, true);
        }

        [TestMethod()]
        public void FailureSyncStaticTest1() {
            TestStatic(true, false, true);
        }

        [TestMethod()]
        [ExpectedException(typeof(OperationCanceledException))]
        public void FailureSyncStaticTest2() {
            TestStatic(true, false, false);
        }

        [TestMethod()]
        public void SuccessAsyncStaticTest1() {
            TestStatic(false, true, false);
        }

        [TestMethod()]
        public void SuccessAsyncStaticTest2() {
            TestStatic(false, true, true);
        }

        [TestMethod()]
        public void FailureAsyncStaticTest1() {
            TestStatic(false, false, true);
        }

        [TestMethod()]
        [ExpectedException(typeof(OperationCanceledException))]
        public void FailureAsyncStaticTest2() {
            TestStatic(false, false, false);
        }
        #endregion

        #region Non-static Tests
        public void Test(bool synchronous, bool succeed, bool supressException) {
            var instance = GetRetryer(false, succeed);

            if (supressException) {
                // Trying with exception suppression
                Exception error;
                bool actual;

                if (synchronous)
                    actual = instance.ProcessRetries(out error);
                else {
                    IAsyncResult result = instance.BeginProcessRetries(out error, null, null);
                    Thread.Sleep(1000);
                    actual = instance.EndProcessRetries(out error, result);
                }

                Assert.AreEqual(succeed, actual);
                if (succeed) {
                    Assert.AreEqual(TriesToFail + 1, _tries);
                    Assert.IsNull(error);
                }
                else {
                    Assert.AreEqual(TriesToFail, _tries);
                    Assert.IsInstanceOfType(error, typeof(OperationCanceledException));
                }
            }
            else {
                // Trying without exception suppression
                bool actual;
                if (synchronous)
                    actual = instance.ProcessRetries();
                else {
                    IAsyncResult result = instance.BeginProcessRetries(null, null);
                    Thread.Sleep(1000);
                    actual = instance.EndProcessRetries(result);
                }
                if (succeed) {
                    Assert.AreEqual(true, actual);
                    Assert.AreEqual(TriesToFail + 1, _tries);
                }
            }
        }

        [TestMethod()]
        public void SucccessSynchTest1() {
            Test(true, true, true);
        }

        [TestMethod()]
        public void SucccessSynchTest2() {
            Test(true, true, false);
        }

        [TestMethod()]
        public void FailureSynchTest1() {
            Test(true, false, true);
        }

        [TestMethod()]
        [ExpectedException(typeof(OperationCanceledException))]
        public void FailureSynchTest2() {
            Test(true, false, false);
        }

        [TestMethod()]
        public void SucccessAsynchTest1() {
            Test(false, true, true);
        }

        [TestMethod()]
        public void SucccessAsynchTest2() {
            Test(false, true, false);
        }

        [TestMethod()]
        public void FailureAsynchTest1() {
            Test(false, false, true);
        }

        [TestMethod()]
        [ExpectedException(typeof(OperationCanceledException))]
        public void FailureAsynchTest2() {
            Test(false, false, false);
        }
        #endregion

    }
}
