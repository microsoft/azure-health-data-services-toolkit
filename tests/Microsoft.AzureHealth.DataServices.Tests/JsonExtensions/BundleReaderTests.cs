using System.Collections.Generic;
using System.IO;
using Microsoft.AzureHealth.DataServices.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.AzureHealth.DataServices.Tests.JsonExtensions
{
    [TestClass]
    public class BundleReaderTests
    {
        private static string json;

        [ClassInitialize]
        public static void JTokenInit(TestContext context)
        {
            context.WriteLine("JTokenExtensions");
            json = File.ReadAllTextAsync("../../../Assets/BundleRequest.json").GetAwaiter().GetResult();
        }

        [TestMethod]
        public void BundleReader_Read_IfNoneExist_True_Test()
        {
            JObject jobject = JObject.Parse(json);
            BundleReader reader = new(jobject, true);
            int count = 0;
            IEnumerator<JToken> en = reader.GetEnumerator();
            while (en.MoveNext())
            {
                count++;
            }

            Assert.IsTrue(count == 1);
        }

        [TestMethod]
        public void BundleReader_Read_IfNoneExist_False_Test()
        {
            JObject jobject = JObject.Parse(json);
            BundleReader reader = new(jobject, false);
            int count = 0;
            IEnumerator<JToken> en = reader.GetEnumerator();
            while (en.MoveNext())
            {
                count++;
            }

            Assert.IsTrue(count == 10);
        }
    }
}
