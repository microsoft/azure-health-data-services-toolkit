using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Proxy.Json;
using Microsoft.Health.Fhir.Proxy.Json.Transforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Health.Fhir.Proxy.Tests.Proxy
{
    [TestClass]
    public class TransformTests
    {
        [TestMethod]
        public async Task AddTransform_Test()
        {
            string expected = "https://www.microsoft2.com";
            string json = await File.ReadAllTextAsync("../../../Assets/capstmt.json");
            string jpath = "$.contact[0].telecom";
            string appendNode = "{ \"system\": \"url\", \"value\": \"https://www.microsoft2.com\" }";
            AddTransform trans = new() { JsonPath = jpath, AppendNode = appendNode };
            JObject obj = trans.Execute(json);
            JToken jtoken = obj.SelectToken("$.contact[0].telecom");
            JArray jArray = jtoken as JArray;
            JToken outToken = JToken.Parse(jArray.Children<JToken>().ToArray()[1].ToString()).Children().ToArray()[1];
            string actual = outToken.Value<JProperty>().Value.Value<string>();
            Assert.AreEqual(expected, actual, "Mismatch");
        }

        [TestMethod]
        public async Task ReplaceTransform_Test()
        {
            string expected = "https://www.microsoft2.com";
            string json = await File.ReadAllTextAsync("../../../Assets/capstmt.json");
            string jpath = "$.contact[0].telecom";
            string replaceNode = "{ \"system\": \"url\", \"value\": \"https://www.microsoft2.com\" }";
            ReplaceTransform trans = new() { JsonPath = jpath, ReplaceNode = replaceNode };
            JObject obj = trans.Execute(json);
            JToken jtoken = obj.SelectToken("$.contact[0].telecom");
            string actual = jtoken.Children().ToArray()[1].Value<JProperty>().Value.Value<string>();
            Assert.AreEqual(expected, actual, "Mismatch");
        }

        [TestMethod]
        public async Task RemoveTransform_Test()
        {
            string json = await File.ReadAllTextAsync("../../../Assets/capstmt.json");
            string jpath = "$.contact[0].telecom";
            RemoveTransform trans = new() { JsonPath = jpath };
            JObject obj = trans.Execute(json);
            Assert.IsTrue(obj.SelectToken(jpath).IsNullOrEmpty(), "Object should not exist.");
        }

        [TestMethod]
        public async Task Policy_Test()
        {
            string json = await File.ReadAllTextAsync("../../../Assets/capstmt.json");
            string jpath = "$.contact[0].telecom";
            RemoveTransform trans = new() { JsonPath = jpath };
            TransformCollection coll = new();
            coll.Add(trans);
            TransformPolicy policy = new(coll);
            string actualJson = policy.Transform(json);
            JObject obj = JObject.Parse(actualJson);
            Assert.IsTrue(obj.SelectToken(jpath).IsNullOrEmpty(), "Object should not exist.");
        }

        [TestMethod]
        public async Task Policy_MultiTransform_Test()
        {
            string expected = "https://www.microsoft2.com";
            string json = await File.ReadAllTextAsync("../../../Assets/capstmt.json");
            string jpath = "$.contact[0].telecom";
            string replaceNode = "{ \"system\": \"url\", \"value\": \"https://www.microsoft2.com\" }";
            string appendNode = "{\r\n            \"telecom\": [\r\n                {\r\n                    \"system\": \"url\",\r\n                    \"value\": \"https://www.microsoft3.com\"\r\n                }\r\n            ]\r\n        }";
            ReplaceTransform trans1 = new() { JsonPath = jpath, ReplaceNode = replaceNode };

            RemoveTransform trans2 = new() { JsonPath = jpath };

            AddTransform trans3 = new() { JsonPath = "$.contact", AppendNode = appendNode };
            TransformCollection coll = new();
            coll.Add(trans1);
            coll.Add(trans2);
            coll.Add(trans3);
            TransformPolicy policy = new(coll);
            string actualJson = policy.Transform(json);
            JObject obj = JObject.Parse(actualJson);
            JToken jtoken = obj.SelectToken("$.contact[0].telecom");
            string actual = jtoken.Children().ToArray()[1].Value<JProperty>().Value.Value<string>();
            Assert.AreEqual(expected, actual, "Mismatch");
        }

        [TestMethod]
        public void Serialize_SimplePolicy_Test()
        {
            string jpath = "$.contact[0].telecom";
            RemoveTransform trans = new() { JsonPath = jpath };
            TransformCollection transforms = new();
            transforms.Add(trans);
            string policyId = "ABC";
            TransformPolicy policy = new(policyId, transforms);
            string json = JsonConvert.SerializeObject(policy);
            TransformPolicy actual = JsonConvert.DeserializeObject<TransformPolicy>(json);
            Assert.AreEqual(policyId, actual.PolicyId, "Policy ID mismatch.");
            Assert.IsTrue(policy.Transforms.Count == 1, "Tranform count mismatch.");
            Assert.AreEqual(policy.Transforms[0].JsonPath, jpath, "JPath mismatch.");
        }

        [TestMethod]
        public void Serialize_ComplexPolicy_Test()
        {
            string jpath = "$.contact[0].telecom";
            string jpath2 = "$.contact";
            string replaceNode = "{ \"system\": \"url\", \"value\": \"https://www.microsoft2.com\" }";
            string appendNode = "{\r\n            \"telecom\": [\r\n                {\r\n                    \"system\": \"url\",\r\n                    \"value\": \"https://www.microsoft3.com\"\r\n                }\r\n            ]\r\n        }";
            ReplaceTransform replaceTransform = new() { JsonPath = jpath, ReplaceNode = replaceNode };
            RemoveTransform removeTransform = new() { JsonPath = jpath };
            AddTransform addTransform = new() { JsonPath = jpath2, AppendNode = appendNode };
            TransformCollection transforms = new();
            transforms.Add(replaceTransform);
            transforms.Add(removeTransform);
            transforms.Add(addTransform);
            string policyId = "ABC";
            TransformPolicy policy = new(policyId, transforms);
            string json = JsonConvert.SerializeObject(policy);
            TransformPolicy actual = JsonConvert.DeserializeObject<TransformPolicy>(json);
            Assert.AreEqual(policyId, actual.PolicyId, "Policy ID mismatch.");
            Assert.IsTrue(policy.Transforms.Count == 3, "Tranform count mismatch.");
            Assert.AreEqual(policy.Transforms[0].JsonPath, jpath, "JPath mismatch.");
            Assert.AreEqual(((ReplaceTransform)policy.Transforms[0]).ReplaceNode, replaceNode, "Replace node");
            Assert.AreEqual(policy.Transforms[1].JsonPath, jpath, "JPath mismatch.");
            Assert.AreEqual(policy.Transforms[2].JsonPath, jpath2, "JPath mismatch.");
            Assert.AreEqual(((AddTransform)policy.Transforms[2]).AppendNode, appendNode, "Append node");
        }
    }
}
