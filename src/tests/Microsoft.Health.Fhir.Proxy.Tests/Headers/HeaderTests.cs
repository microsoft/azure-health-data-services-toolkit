using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Proxy.Clients;
using Microsoft.Health.Fhir.Proxy.Clients.Headers;
using Microsoft.Health.Fhir.Proxy.Tests.Assets;
using Microsoft.Health.Fhir.Proxy.Tests.Assets.SimpleFilterServiceAsset;
using Microsoft.Health.Fhir.Proxy.Tests.Assets.SimpleWebServiceAsset;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Proxy.Tests.Headers
{
    [TestClass]
    public class HeaderTests
    {

        #region Custom Headers

        [TestMethod]
        public void HttpCustomHeaderCollection_Add_Test()
        {
            NameValuePair nvp1 = new NameValuePair("name1", "value1");
            NameValuePair nvp2 = new NameValuePair("name2", "value2");
            NameValuePair nvp3 = new NameValuePair("name2", "value2");
            INameValuePair[] items = new INameValuePair[] { nvp1, nvp2 };

            HttpCustomHeaderCollection headers = new(items);
            Assert.AreEqual(items.Length, headers.Count, "Invald number of items.");
            headers.Add(nvp3);
            Assert.AreEqual(items.Length + 1, headers.Count, "Invalid number of items.");
        }

        [TestMethod]
        public void HttpCustomHeaderCollection_Remove_Test()
        {
            NameValuePair nvp1 = new NameValuePair("name1", "value1");
            NameValuePair nvp2 = new NameValuePair("name2", "value2");
            INameValuePair[] items = new INameValuePair[] { nvp1, nvp2 };

            HttpCustomHeaderCollection headers = new(items);
            Assert.AreEqual(items.Length, headers.Count, "Invald number of items.");
            headers.Remove(nvp1);
            Assert.AreEqual(items.Length - 1, headers.Count, "Invalid number of items.");
        }

        [TestMethod]
        public void HttpCustomHeaderCollection_RemoveAt_Test()
        {
            NameValuePair nvp1 = new NameValuePair("name1", "value1");
            NameValuePair nvp2 = new NameValuePair("name2", "value2");
            NameValuePair nvp3 = new NameValuePair("name2", "value2");
            INameValuePair[] items = new INameValuePair[] { nvp1, nvp2, nvp3 };

            HttpCustomHeaderCollection headers = new(items);
            Assert.AreEqual(items.Length, headers.Count, "Invald number of items.");
            headers.RemoveAt(1);
            Assert.AreEqual(headers[0].Name, nvp1.Name, "Mismatched item.");
            Assert.AreEqual(headers[1].Name, nvp3.Name, "Mismatched item.");
        }

        [TestMethod]
        public void HttpCustomHeaderCollection_GetEnumerator_Test()
        {
            string[] names = new string[] { "name1", "name2" };
            NameValuePair nvp1 = new NameValuePair(names[0], "value1");
            NameValuePair nvp2 = new NameValuePair(names[1], "value2");
            INameValuePair[] items = new INameValuePair[] { nvp1, nvp2 };
            HttpCustomHeaderCollection headers = new(items);

            Assert.AreEqual(names.Length, headers.Count, "Item count invalid.");
            IEnumerator<INameValuePair> en = headers.GetEnumerator();
            int i = 0;
            while (en.MoveNext())
            {
                Assert.AreEqual(names[i], en.Current.Name, "Name mismatch.");
                i++;
            }
        }

        [TestMethod]
        public void HttpCustomHeaderCollection_ToArray_Test()
        {
            string[] names = new string[] { "name1", "name2" };
            NameValuePair nvp1 = new NameValuePair(names[0], "value1");
            NameValuePair nvp2 = new NameValuePair(names[1], "value2");
            INameValuePair[] items = new INameValuePair[] { nvp1, nvp2 };

            HttpCustomHeaderCollection headers = new(items);
            INameValuePair[] actual = new INameValuePair[2];
            headers.CopyTo(actual, 0);
            Assert.AreEqual(actual[0].Name, names[0], "Name mismatch.");
            Assert.AreEqual(actual[1].Name, names[1], "Name mismatch.");
        }

        [TestMethod]
        public void HttpCustomHeaderCollection_Contains_True_Test()
        {
            string[] names = new string[] { "name1", "name2" };
            NameValuePair nvp1 = new NameValuePair(names[0], "value1");
            NameValuePair nvp2 = new NameValuePair(names[1], "value2");
            INameValuePair[] items = new INameValuePair[] { nvp1, nvp2 };

            HttpCustomHeaderCollection headers = new(items);
            Assert.IsTrue(headers.Contains(nvp2), "Item not found.");
        }

        [TestMethod]
        public void HttpCustomHeaderCollection_Contains_False_Test()
        {
            string[] names = new string[] { "name1", "name2" };
            NameValuePair nvp1 = new NameValuePair(names[0], "value1");
            NameValuePair nvp2 = new NameValuePair(names[1], "value2");
            NameValuePair fake = new NameValuePair("boom", "bang");
            INameValuePair[] items = new INameValuePair[] { nvp1, nvp2 };

            HttpCustomHeaderCollection headers = new(items);
            Assert.IsFalse(headers.Contains(fake), "Item should not be present.");
        }

        [TestMethod]
        public void HttpCustomHeaderCollection_IndexOf_Test()
        {
            string[] names = new string[] { "name1", "name2" };
            NameValuePair nvp1 = new NameValuePair(names[0], "value1");
            NameValuePair nvp2 = new NameValuePair(names[1], "value2");
            INameValuePair[] items = new INameValuePair[] { nvp1, nvp2 };

            HttpCustomHeaderCollection headers = new(items);
            Assert.IsTrue(headers.IndexOf(nvp1) == 0, "Item index mismatch.");
        }

        [TestMethod]
        public void HttpCustomHeaderCollection_Insert_Test()
        {
            string[] names = new string[] { "name1", "name2", "name3" };
            NameValuePair nvp1 = new NameValuePair(names[0], "value1");
            NameValuePair nvp2 = new NameValuePair(names[1], "value2");
            NameValuePair nvp3 = new NameValuePair(names[2], "bang");
            INameValuePair[] items = new INameValuePair[] { nvp1, nvp2 };

            HttpCustomHeaderCollection headers = new(items);

            headers.Insert(0, nvp3);
            Assert.AreEqual(headers[0].Name, names[2], "Item order mismatch.");
            Assert.AreEqual(headers[1].Name, names[0], "Item order mismatch.");
            Assert.AreEqual(headers[2].Name, names[1], "Item order mismatch.");
        }


        [TestMethod]
        public void HttpCustomHeaderCollection_Clear_Test()
        {
            string[] names = new string[] { "name1", "name2" };
            NameValuePair nvp1 = new NameValuePair(names[0], "value1");
            NameValuePair nvp2 = new NameValuePair(names[1], "value2");
            INameValuePair[] items = new INameValuePair[] { nvp1, nvp2 };

            HttpCustomHeaderCollection headers = new(items);

            Assert.AreEqual(headers.Count, names.Length, "Item count.");
            headers.Clear();
            Assert.AreEqual(headers.Count, 0, "Item count.");
        }

        [TestMethod]
        public void HttpCustomHeaderCollection_GetHeaders_Test()
        {
            string[] names = new string[] { "name1", "name2" };
            NameValuePair nvp1 = new NameValuePair(names[0], "value1");
            NameValuePair nvp2 = new NameValuePair(names[1], "value2");
            INameValuePair[] items = new INameValuePair[] { nvp1, nvp2 };

            HttpCustomHeaderCollection headers = new(items);

            var nvc = headers.GetHeaders();

            Assert.AreEqual(names[0], nvc.GetKey(0), "Not name.");
            Assert.AreEqual(names[1], nvc.GetKey(1), "Not name.");
        }

        [TestMethod]
        public void HttpCustomHeaderCollection_AppendHeaders_Test()
        {
            string[] names = new string[] { "name1", "name2" };
            NameValuePair nvp1 = new NameValuePair(names[0], "value1");
            NameValuePair nvp2 = new NameValuePair(names[1], "value2");
            INameValuePair[] items = new INameValuePair[] { nvp1, nvp2 };

            HttpCustomHeaderCollection headers = new(items);

            NameValueCollection nvc = new NameValueCollection
            {
                { "header1", "v1" },
                { "header2", "v2" }
            };

            var actual = headers.AppendHeaders(nvc);

            Assert.AreEqual(nvc.GetKey(0), actual.GetKey(0), "Not name.");
            Assert.AreEqual(nvc.GetValues(0)[0], actual.GetValues(0)[0], "Not value.");
            Assert.AreEqual(nvc.GetKey(1), actual.GetKey(1), "Not name.");
            Assert.AreEqual(nvc.GetValues(1)[0], actual.GetValues(1)[0], "Not value.");
            Assert.AreEqual(nvp1.Name, actual.GetKey(2), "Not name.");
            Assert.AreEqual(nvp1.Value, actual.GetValues(2)[0], "Not value.");
            Assert.AreEqual(nvp2.Name, actual.GetKey(3), "Not name.");
            Assert.AreEqual(nvp2.Value, actual.GetValues(3)[0], "Not value.");
        }

        #endregion

        #region Custom Identity Headers

        [TestMethod]
        public void HttpCustomIdentityHeaderCollection_Add_Test()
        {
            ClaimValuePair nvp1 = new ClaimValuePair("name1", "value1");
            ClaimValuePair nvp2 = new ClaimValuePair("name2", "value2");
            ClaimValuePair nvp3 = new ClaimValuePair("name3", "value3");
            IClaimValuePair[] items = new IClaimValuePair[] { nvp1, nvp2 };

            HttpCustomIdentityHeaderCollection headers = new(items);
            Assert.AreEqual(items.Length, headers.Count, "Invald number of items.");
            headers.Add(nvp3);
            Assert.AreEqual(items.Length + 1, headers.Count, "Invalid number of items.");
        }

        [TestMethod]
        public void HttpCustomIdentityHeaderCollection_Remove_Test()
        {
            ClaimValuePair nvp1 = new ClaimValuePair("name1", "value1");
            ClaimValuePair nvp2 = new ClaimValuePair("name2", "value2");
            IClaimValuePair[] items = new IClaimValuePair[] { nvp1, nvp2 };

            HttpCustomIdentityHeaderCollection headers = new(items);
            Assert.AreEqual(items.Length, headers.Count, "Invald number of items.");
            headers.Remove(nvp1);
            Assert.AreEqual(items.Length - 1, headers.Count, "Invalid number of items.");
        }

        [TestMethod]
        public void HttpCustomIdentityHeaderCollection_RemoveAt_Test()
        {
            ClaimValuePair nvp1 = new ClaimValuePair("name1", "value1");
            ClaimValuePair nvp2 = new ClaimValuePair("name2", "value2");
            ClaimValuePair nvp3 = new ClaimValuePair("name2", "value2");
            IClaimValuePair[] items = new IClaimValuePair[] { nvp1, nvp2, nvp3 };

            HttpCustomIdentityHeaderCollection headers = new(items);
            Assert.AreEqual(items.Length, headers.Count, "Invald number of items.");
            headers.RemoveAt(1);
            Assert.AreEqual(headers[0].HeaderName, nvp1.HeaderName, "Mismatched item.");
            Assert.AreEqual(headers[1].HeaderName, nvp3.HeaderName, "Mismatched item.");
        }

        [TestMethod]
        public void HttpCustomIdentityHeaderCollection_GetEnumerator_Test()
        {
            string[] names = new string[] { "name1", "name2" };
            ClaimValuePair nvp1 = new ClaimValuePair(names[0], "value1");
            ClaimValuePair nvp2 = new ClaimValuePair(names[1], "value2");
            IClaimValuePair[] items = new IClaimValuePair[] { nvp1, nvp2 };
            HttpCustomIdentityHeaderCollection headers = new(items);

            Assert.AreEqual(names.Length, headers.Count, "Item count invalid.");
            IEnumerator<IClaimValuePair> en = headers.GetEnumerator();
            int i = 0;
            while (en.MoveNext())
            {
                Assert.AreEqual(names[i], en.Current.HeaderName, "Name mismatch.");
                i++;
            }
        }

        [TestMethod]
        public void HttpCustomIdentityHeaderCollection_ToArray_Test()
        {
            string[] names = new string[] { "name1", "name2" };
            ClaimValuePair nvp1 = new ClaimValuePair(names[0], "value1");
            ClaimValuePair nvp2 = new ClaimValuePair(names[1], "value2");
            IClaimValuePair[] items = new IClaimValuePair[] { nvp1, nvp2 };

            HttpCustomIdentityHeaderCollection headers = new(items);
            IClaimValuePair[] actual = new IClaimValuePair[2];
            headers.CopyTo(actual, 0);
            Assert.AreEqual(actual[0].HeaderName, names[0], "Name mismatch.");
            Assert.AreEqual(actual[1].HeaderName, names[1], "Name mismatch.");
        }

        [TestMethod]
        public void HttpCustomIdentityHeaderCollection_Contains_True_Test()
        {
            string[] names = new string[] { "name1", "name2" };
            ClaimValuePair nvp1 = new ClaimValuePair(names[0], "value1");
            ClaimValuePair nvp2 = new ClaimValuePair(names[1], "value2");
            IClaimValuePair[] items = new IClaimValuePair[] { nvp1, nvp2 };

            HttpCustomIdentityHeaderCollection headers = new(items);
            Assert.IsTrue(headers.Contains(nvp2), "Item not found.");
        }

        [TestMethod]
        public void HttpCustomIdentityHeaderCollection_Contains_False_Test()
        {
            string[] names = new string[] { "name1", "name2" };
            ClaimValuePair nvp1 = new ClaimValuePair(names[0], "value1");
            ClaimValuePair nvp2 = new ClaimValuePair(names[1], "value2");
            ClaimValuePair fake = new ClaimValuePair("boom", "bang");
            IClaimValuePair[] items = new IClaimValuePair[] { nvp1, nvp2 };

            HttpCustomIdentityHeaderCollection headers = new(items);
            Assert.IsFalse(headers.Contains(fake), "Item should not be present.");
        }

        [TestMethod]
        public void HttpCustomIdentityHeaderCollection_IndexOf_Test()
        {
            string[] names = new string[] { "name1", "name2" };
            ClaimValuePair nvp1 = new ClaimValuePair(names[0], "value1");
            ClaimValuePair nvp2 = new ClaimValuePair(names[1], "value2");
            IClaimValuePair[] items = new IClaimValuePair[] { nvp1, nvp2 };

            HttpCustomIdentityHeaderCollection headers = new(items);
            Assert.IsTrue(headers.IndexOf(nvp1) == 0, "Item index mismatch.");
        }

        [TestMethod]
        public void HttpCustomIdentityHeaderCollection_Insert_Test()
        {
            string[] names = new string[] { "name1", "name2", "name3" };
            ClaimValuePair nvp1 = new ClaimValuePair(names[0], "value1");
            ClaimValuePair nvp2 = new ClaimValuePair(names[1], "value2");
            ClaimValuePair nvp3 = new ClaimValuePair(names[2], "bang");
            IClaimValuePair[] items = new IClaimValuePair[] { nvp1, nvp2 };

            HttpCustomIdentityHeaderCollection headers = new(items);

            headers.Insert(0, nvp3);
            Assert.AreEqual(headers[0].HeaderName, names[2], "Item order mismatch.");
            Assert.AreEqual(headers[1].HeaderName, names[0], "Item order mismatch.");
            Assert.AreEqual(headers[2].HeaderName, names[1], "Item order mismatch.");
        }


        [TestMethod]
        public void HttpCustomIdentityHeaderCollection_Clear_Test()
        {
            string[] names = new string[] { "name1", "name2" };
            ClaimValuePair nvp1 = new ClaimValuePair(names[0], "value1");
            ClaimValuePair nvp2 = new ClaimValuePair(names[1], "value2");
            IClaimValuePair[] items = new IClaimValuePair[] { nvp1, nvp2 };

            HttpCustomIdentityHeaderCollection headers = new(items);

            Assert.AreEqual(headers.Count, names.Length, "Item count.");
            headers.Clear();
            Assert.AreEqual(headers.Count, 0, "Item count.");
        }

        [TestMethod]
        public void HttpCustomIdentityHeaderCollection_Clear_Test1()
        {
            string jwtString = File.ReadAllText("../../../Assets/jwttest.txt");
            string headerName = "Location";
            string headerValue = "Basement";
            string name = "William Zhang (microsoft.com)";
            string customHeaderName = "X-MS-HEADER";
            ClaimValuePair nvp1 = new ClaimValuePair(customHeaderName, "name");
            IClaimValuePair[] items = new IClaimValuePair[] { nvp1 };

            HttpCustomIdentityHeaderCollection headers = new(items);

            HttpRequestMessage request = new();
            request.Headers.Authorization = new("http", $"Bearer {jwtString}");
            request.Headers.Add(headerName, headerValue);
            var nvc = request.GetHeaders();
            var actual = headers.AppendCustomHeaders(request, nvc);
            Assert.AreEqual(actual.Count, 3, "Header count");
            actual.Remove("Authorization");

            Assert.AreEqual(actual.Keys[0], headerName, "Name");
            Assert.AreEqual(actual.GetValues(0)[0], headerValue, "Value");
            Assert.AreEqual(actual.Keys[1], customHeaderName, "Identity claim type.");
            Assert.AreEqual(actual.GetValues(1)[0], name, "Identity claim value");
        }

        #endregion


        #region Web Tests

        [TestMethod]
        public async Task ConfigurationWeb_Test()
        {
            string expectedValue = "filter;WebApi;customvalue;William Zhang (microsoft.com)";
            int webServicePort = 1212;
            int filterServicePort = 1211;
            SimpleWebService webhost = new(webServicePort);
            webhost.Start();

            SimpleService simple = new(1211);
            simple.Start();

            await Task.Delay(2000);

            string baseUrl = $"http://localhost:{filterServicePort}";
            string path = "simple";
            string method = "Post";
            TestMessage msg = new TestMessage() { Value = "test" };
            string payload = JsonConvert.SerializeObject(msg);
            byte[] content = Encoding.UTF8.GetBytes(payload);
            string jwtString = File.ReadAllText("../../../Assets/jwttest.txt");
            RestRequestBuilder builder = new(method, baseUrl, jwtString, path, null, null, content, "application/json");
            RestRequest request = new(builder);
            HttpResponseMessage response = await request.SendAsync();
            string msgJson = await response.Content.ReadAsStringAsync();
            TestMessage actual = JsonConvert.DeserializeObject<TestMessage>(msgJson);

            Assert.AreEqual(actual.Value, expectedValue, "not expected value");





            simple.Stop();
            webhost.Stop();
        }


        #endregion
    }
}
