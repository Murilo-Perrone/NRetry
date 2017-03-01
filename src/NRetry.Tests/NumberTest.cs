using System.Threading;
using NRetry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NRetry.Tests {
    [TestClass()]
    public class NumberTest {
        private int _number;
        private const int Initial = -5;

        /// <summary>Gets or sets the test context which provides
        /// information about and functionality for the current test run.</summary>
        public TestContext TestContext { get; set; }

        [TestInitialize()]
        public void TestInitialize() {
            _number = Initial;
        }

        public int TryGetPositive() {
            return ++_number;
        }

        private ProcessRetryer<int> GetTaskRetryer(int maxAttempts = 0) {
            var retryer = new ProcessRetryer<int> {
                FailureDetectionMethod = FailureDetection.ByFailureDetector,
                FailureDetector = (i) => i < 0,
                RetryableOperation = TryGetPositive,
                ExceptionLogger = (args) => TestContext.WriteLine("Exception message: " + args.Exception.Message),
                Config = new RetryConfig() {
                    MaximumAttempts = maxAttempts, // 0 represents unlimited attempts
                    RetryInterval = 900, // Representing 9 months
                },
            };
            return retryer;
        }

        [TestMethod()]
        public void FailureTest() {
            var retryer = GetTaskRetryer(4);

            int number = retryer.ProcessRetries();
            Assert.AreEqual(Initial + 4, number);
        }

        [TestMethod()]
        public void SuccessTest() {
            var retryer = GetTaskRetryer();

            int number = retryer.ProcessRetries();
            Assert.AreEqual(0, number);
        }
    }
}
