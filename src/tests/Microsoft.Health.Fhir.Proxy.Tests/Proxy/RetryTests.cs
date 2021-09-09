using Microsoft.Health.Fhir.Proxy.Clients;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Proxy.Tests.Proxy
{
    [TestClass]
    public class RetryTests
    {
        private int counter;

        [TestMethod]
        public void Retry_Test()
        {
            var backOff = TimeSpan.FromSeconds(1.0);
            int maxAttempts = 4;

            async Task<int> func()
            {
                return await RetryFunc();
            }

            var output = Retry.Execute<int>(func, backOff, maxAttempts);
            int i = output.Result;
            Assert.AreEqual(maxAttempts - 1, i);
        }

        private Task<int> RetryFunc()
        {
            counter++;
            if (counter < 3)
            {
                throw new Exception("Fail < 3.");
            }
            else
            {
                return Task.FromResult(counter);
            }
        }
    }
}
