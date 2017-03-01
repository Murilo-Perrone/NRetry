using System.Threading;
using NRetry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NRetry.Tests {
    [TestClass()]
    public class MultiServerTest {
        private const string WorkingServer = "MailServer3";
        private static readonly string[] Servers =
            new []{"MailServer1", "MailServer2", WorkingServer};

        /// <summary>Gets or sets the test context which provides
        /// information about and functionality for the current test run.</summary>
        public TestContext TestContext { get; set; }

        [TestMethod()]
        public void Test() {
            bool success = SendEmailUsingMultipleHosts(Servers.AsEnumerable(), "Email body");
            Assert.AreEqual(true, success);
        }

        public bool SendEmailUsingMultipleHosts(IEnumerable<string> hostsList, string email) {

            var retryer = new TaskRetryer() {
                // Optional configuration attempts
                Config = new RetryConfig {
                    RetryInterval = 0, // Time in milliseconds to wait (default = 0)
                    MaximumAttempts = 10, // Default = 0 indicating no limit of attempts
                },
            };

            var mailer = new FakeMailer();
            var enumerator = hostsList.GetEnumerator();
            retryer.RetryableOperation = delegate {
                enumerator.MoveNext(); // No need to check since it will only be called servers.Count times
                string host = enumerator.Current;
                mailer.SmtpHost = host.Trim();
                var output = mailer.SendEmail(email);

                if (output == false)
                    TestContext.WriteLine("Failure when sending email. Chilkat error: {0}", mailer.LastErrorText);

                return output;
            };
            retryer.ExceptionLogger =
               args => TestContext.WriteLine("Email send failure, try #{0}: {1}", args.TryCount, args.Exception.Message);

            return retryer.ProcessRetries();
        }

        public class FakeMailer {
            public string SmtpHost { get; set; }
            public string LastErrorText { get; set; }

            public bool SendEmail(string email) {
                LastErrorText = string.Empty;
                if (SmtpHost == WorkingServer) return true;
                LastErrorText = "Unable to access " + SmtpHost;
                return false;
            }
        }
    }
}
