using System.Threading;
using NRetry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NRetry.Tests {
    /// <summary>
    ///Tests a fictitious method with random evolutional behavior, using TaskRetryer
    ///</summary>
    [TestClass()]
    public class KidsCountTest {
        private int _kidsCount = 0; // Number of kids I have
        private const int DesiredKids = 3; // Number of kids I wish to

        /// <summary>Gets or sets the test context which provides
        /// information about and functionality for the current test run.</summary>
        public TestContext TestContext { get; set; }

        [TestInitialize()]
        public void TestInitialize() {
            _kidsCount = 0;
        }

        public bool CheckKidsCount() {
            if (_kidsCount < DesiredKids) {
                // Trying to make kids with my wife
                int newKid = (new Random().Next() % 2); // 0 to 1 kids being made
                _kidsCount += newKid;

                if (newKid == 0)
                    throw new OperationCanceledException("Need to make more kids !\n");

                if (_kidsCount < DesiredKids) {
                    TestContext.WriteLine("Not enough kids yet, wait for next opportunity.\n");
                    return false;
                }
            }
            if (_kidsCount == DesiredKids) {
                TestContext.WriteLine("Success !\n");
                return true;
            }
            TestContext.WriteLine("Too many kids, give up !\n");
            return false;
        }

        private TaskRetryer GetTaskRetryer(int maxAttempts = 0) {
            var retryer = new TaskRetryer {
                RetryableOperation = CheckKidsCount,
                ExceptionLogger = (args) => TestContext.WriteLine("Exception message: " + args.Exception.Message),
                Config = new RetryConfig() {
                    MaximumAttempts = maxAttempts, // 0 represents unlimited attempts
                    RetryInterval = 900, // Representing 9 months
                },
            };
            return retryer;
        }

        [TestMethod()]
        public void KidsProductionTest() {
            var retryer = GetTaskRetryer();

            bool success = retryer.ProcessRetries();
            Assert.AreEqual(true, success);
            Assert.AreEqual(DesiredKids, _kidsCount);
        }

        [TestMethod()]
        public void KidsProductionTest2() {
            var retryer = GetTaskRetryer(5);

            bool success = false;
            try {
                success = retryer.ProcessRetries();
            }
            catch (Exception ex) {
                TestContext.WriteLine("Final failure: " + ex.Message);
            }
            Assert.AreEqual(success, _kidsCount == DesiredKids);
        }
    }
}
