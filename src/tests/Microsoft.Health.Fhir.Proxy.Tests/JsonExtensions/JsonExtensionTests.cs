using Microsoft.Health.Fhir.Proxy.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Microsoft.Health.Fhir.Proxy.Tests.JsonExtensions
{
    [TestClass]
    public class JTokenExtensionTests
    {
        [ClassInitialize()]
        public static void JTokenInit(TestContext context)
        {
            context.WriteLine("JTokenExtensions");
            json = File.ReadAllTextAsync("../../../Assets/BundleRequest.json").GetAwaiter().GetResult();
        }

        private static string json;

        [TestMethod]
        public void JToken_IsMatch_True_Test()
        {
            JToken token = JToken.Parse(json);
            string jpath = "$.resourceType";
            string value = "Bundle";
            Assert.IsTrue(token.IsMatch(jpath, value), "value mismatch.");
        }

        [TestMethod]
        public void JToken_IsMatch2_True_Test()
        {
            JToken token = JToken.Parse(json);
            string jpath = "$.resourceType";
            string value = "Bundle";
            Assert.IsTrue(token.IsMatch<string>(jpath, value), "value mismatch.");
        }

        [TestMethod]
        public void JToken_IsMatch_False_Test()
        {
            JToken token = JToken.Parse(json);
            string jpath = "$.resourceType";
            string value = "Patient";
            Assert.IsFalse(token.IsMatch(jpath, value), "value should not match.");
        }

        [TestMethod]
        public void JToken_IsMatch2_False_Test()
        {
            JToken token = JToken.Parse(json);
            string jpath = "$.resourceType";
            string value = "Patient";
            Assert.IsFalse(token.IsMatch<string>(jpath, value), "value should not match.");
        }

        [TestMethod]
        public void JToken_GetValue_Test()
        {
            JToken token = JToken.Parse(json);
            string jpath = "$.resourceType";
            string expectedValue = "Bundle";
            string value = token.GetValue<string>(jpath);
            Assert.AreEqual(expectedValue, value, "value mismatch.");
        }

        [TestMethod]
        public void JToken_Exists_True_Test()
        {
            JToken token = JToken.Parse(json);
            string jpath = "$.resourceType";
            Assert.IsTrue(token.Exists(jpath), "token should exist.");
        }

        [TestMethod]
        public void JToken_Exists_False_Test()
        {
            JToken token = JToken.Parse(json);
            string jpath = "$.foo";
            Assert.IsFalse(token.Exists(jpath), "token should not exist.");
        }

        [TestMethod]
        public void JToken_GetArray_Test()
        {
            JToken token = JToken.Parse(json);
            string jpath = "$.entry";
            JArray array = token.GetArray(jpath);
            Assert.IsNotNull(array, "array must not be null.");
            Assert.IsTrue(array.Count > 0, "array must have members.");
        }


        [TestMethod]
        public void JToken_IsArray_True_Test()
        {
            JToken token = JToken.Parse(json);
            string jpath = "$.entry";
            Assert.IsTrue(token.IsArray(jpath), "must be array");
        }

        [TestMethod]
        public void JToken_IsArray_False_Test()
        {
            JToken token = JToken.Parse(json);
            string jpath = "$.resourceType";
            Assert.IsFalse(token.IsArray(jpath), "must not be array");
        }

        [TestMethod]
        public void JToken_IsNullOrEmpty_False_Test()
        {
            JToken token = JToken.Parse(json);
            string jpath = "$.entry";
            Assert.IsFalse(token.IsNullOrEmpty(jpath), "must be not null.");
        }

        [TestMethod]
        public void JToken_IsNullOrEmpty_True_Test()
        {
            JToken token = JToken.Parse(json);
            string jpath = "$.foo";
            Assert.IsTrue(token.IsNullOrEmpty(jpath), "Must be null or empty.");
        }
    }
}
