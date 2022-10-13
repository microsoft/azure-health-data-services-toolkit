using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AzureHealth.DataServices.Clients;
using Microsoft.AzureHealth.DataServices.Clients.Headers;
using Microsoft.AzureHealth.DataServices.Tests.Assets;
using Microsoft.AzureHealth.DataServices.Tests.Assets.SimpleFilterServiceAsset;
using Microsoft.AzureHealth.DataServices.Tests.Assets.SimpleWebServiceAsset;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.AzureHealth.DataServices.Tests.Headers
{
    [TestClass]
    public class HeaderTests
    {

        #region Custom Headers

        [TestMethod]
        public void HttpCustomHeaderCollection_Add_Test()
        {
            HeaderNameValuePair nvp1 = new HeaderNameValuePair("name1", "value1", CustomHeaderType.RequestStatic);
            HeaderNameValuePair nvp2 = new HeaderNameValuePair("name2", "value2", CustomHeaderType.RequestStatic);
            HeaderNameValuePair nvp3 = new HeaderNameValuePair("name2", "value2", CustomHeaderType.RequestStatic);
            IHeaderNameValuePair[] items = new IHeaderNameValuePair[] { nvp1, nvp2 };

            HttpCustomHeaderCollection headers = new(items);
            Assert.AreEqual(items.Length, headers.Count, "Invald number of items.");
            headers.Add(nvp3);
            Assert.AreEqual(items.Length + 1, headers.Count, "Invalid number of items.");
        }

        [TestMethod]
        public void HttpCustomHeaderCollection_Remove_Test()
        {
            HeaderNameValuePair nvp1 = new HeaderNameValuePair("name1", "value1", CustomHeaderType.RequestStatic);
            HeaderNameValuePair nvp2 = new HeaderNameValuePair("name2", "value2", CustomHeaderType.RequestStatic);
            IHeaderNameValuePair[] items = new IHeaderNameValuePair[] { nvp1, nvp2 };

            HttpCustomHeaderCollection headers = new(items);
            Assert.AreEqual(items.Length, headers.Count, "Invald number of items.");
            headers.Remove(nvp1);
            Assert.AreEqual(items.Length - 1, headers.Count, "Invalid number of items.");
        }

        [TestMethod]
        public void HttpCustomHeaderCollection_RemoveAt_Test()
        {
            HeaderNameValuePair nvp1 = new HeaderNameValuePair("name1", "value1", CustomHeaderType.RequestStatic);
            HeaderNameValuePair nvp2 = new HeaderNameValuePair("name2", "value2", CustomHeaderType.RequestStatic);
            HeaderNameValuePair nvp3 = new HeaderNameValuePair("name2", "value2", CustomHeaderType.RequestStatic);
            IHeaderNameValuePair[] items = new IHeaderNameValuePair[] { nvp1, nvp2, nvp3 };

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
            HeaderNameValuePair nvp1 = new HeaderNameValuePair(names[0], "value1", CustomHeaderType.RequestStatic);
            HeaderNameValuePair nvp2 = new HeaderNameValuePair(names[1], "value2", CustomHeaderType.RequestStatic);
            IHeaderNameValuePair[] items = new IHeaderNameValuePair[] { nvp1, nvp2 };
            HttpCustomHeaderCollection headers = new(items);

            Assert.AreEqual(names.Length, headers.Count, "Item count invalid.");
            IEnumerator<IHeaderNameValuePair> en = headers.GetEnumerator();
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
            HeaderNameValuePair nvp1 = new HeaderNameValuePair(names[0], "value1", CustomHeaderType.RequestStatic);
            HeaderNameValuePair nvp2 = new HeaderNameValuePair(names[1], "value2", CustomHeaderType.RequestStatic);
            IHeaderNameValuePair[] items = new IHeaderNameValuePair[] { nvp1, nvp2 };

            HttpCustomHeaderCollection headers = new(items);
            IHeaderNameValuePair[] actual = new IHeaderNameValuePair[2];
            headers.CopyTo(actual, 0);
            Assert.AreEqual(actual[0].Name, names[0], "Name mismatch.");
            Assert.AreEqual(actual[1].Name, names[1], "Name mismatch.");
        }

        [TestMethod]
        public void HttpCustomHeaderCollection_Contains_True_Test()
        {
            string[] names = new string[] { "name1", "name2" };
            HeaderNameValuePair nvp1 = new HeaderNameValuePair(names[0], "value1", CustomHeaderType.RequestStatic);
            HeaderNameValuePair nvp2 = new HeaderNameValuePair(names[1], "value2", CustomHeaderType.RequestStatic);
            IHeaderNameValuePair[] items = new IHeaderNameValuePair[] { nvp1, nvp2 };

            HttpCustomHeaderCollection headers = new(items);
            Assert.IsTrue(headers.Contains(nvp2), "Item not found.");
        }

        [TestMethod]
        public void HttpCustomHeaderCollection_Contains_False_Test()
        {
            string[] names = new string[] { "name1", "name2" };
            HeaderNameValuePair nvp1 = new HeaderNameValuePair(names[0], "value1", CustomHeaderType.RequestStatic);
            HeaderNameValuePair nvp2 = new HeaderNameValuePair(names[1], "value2", CustomHeaderType.RequestStatic);
            HeaderNameValuePair fake = new HeaderNameValuePair("boom", "bang", CustomHeaderType.RequestStatic);
            IHeaderNameValuePair[] items = new IHeaderNameValuePair[] { nvp1, nvp2 };

            HttpCustomHeaderCollection headers = new(items);
            Assert.IsFalse(headers.Contains(fake), "Item should not be present.");
        }

        [TestMethod]
        public void HttpCustomHeaderCollection_IndexOf_Test()
        {
            string[] names = new string[] { "name1", "name2" };
            HeaderNameValuePair nvp1 = new HeaderNameValuePair(names[0], "value1", CustomHeaderType.RequestStatic);
            HeaderNameValuePair nvp2 = new HeaderNameValuePair(names[1], "value2", CustomHeaderType.RequestStatic);
            IHeaderNameValuePair[] items = new IHeaderNameValuePair[] { nvp1, nvp2 };

            HttpCustomHeaderCollection headers = new(items);
            Assert.IsTrue(headers.IndexOf(nvp1) == 0, "Item index mismatch.");
        }

        [TestMethod]
        public void HttpCustomHeaderCollection_Insert_Test()
        {
            string[] names = new string[] { "name1", "name2", "name3" };
            HeaderNameValuePair nvp1 = new HeaderNameValuePair(names[0], "value1", CustomHeaderType.RequestStatic);
            HeaderNameValuePair nvp2 = new HeaderNameValuePair(names[1], "value2", CustomHeaderType.RequestStatic);
            HeaderNameValuePair nvp3 = new HeaderNameValuePair(names[2], "bang", CustomHeaderType.RequestStatic);
            IHeaderNameValuePair[] items = new IHeaderNameValuePair[] { nvp1, nvp2 };

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
            HeaderNameValuePair nvp1 = new HeaderNameValuePair(names[0], "value1", CustomHeaderType.RequestStatic);
            HeaderNameValuePair nvp2 = new HeaderNameValuePair(names[1], "value2", CustomHeaderType.RequestStatic);
            IHeaderNameValuePair[] items = new IHeaderNameValuePair[] { nvp1, nvp2 };

            HttpCustomHeaderCollection headers = new(items);

            Assert.AreEqual(headers.Count, names.Length, "Item count.");
            headers.Clear();
            Assert.AreEqual(headers.Count, 0, "Item count.");
        }

        [TestMethod]
        public void HttpCustomHeaderCollection_GetStaticHeaders_Test()
        {
            string[] names = new string[] { "header1", "header2", "name1", "name2" };
            string[] values = new string[] { "v1", "v2", "value1", "value2" };
            HeaderNameValuePair nvp1 = new HeaderNameValuePair(names[2], values[2], CustomHeaderType.RequestStatic);
            HeaderNameValuePair nvp2 = new HeaderNameValuePair(names[3], values[3], CustomHeaderType.RequestStatic);
            IHeaderNameValuePair[] items = new IHeaderNameValuePair[] { nvp1, nvp2 };

            HttpCustomHeaderCollection headers = new(items);
            HttpRequestMessage httpRequestMessage = new();
            httpRequestMessage.Headers.Add(names[0], values[0]);
            httpRequestMessage.Headers.Add(names[1], values[1]);
            var nvc = headers.RequestAppendAndReplace(httpRequestMessage);

            Assert.IsTrue(nvc.Count == headers.Count + 2, "Count headers mismatch.");
            Assert.AreEqual(names[0], nvc.GetKey(0), "Not name.");
            Assert.AreEqual(values[0], nvc.GetValues(0)[0], "Not value.");
            Assert.AreEqual(names[1], nvc.GetKey(1), "Not name.");
            Assert.AreEqual(values[1], nvc.GetValues(1)[0], "Not value.");
            Assert.AreEqual(names[2], nvc.GetKey(2), "Not name.");
            Assert.AreEqual(values[2], nvc.GetValues(2)[0], "Not value.");
            Assert.AreEqual(names[3], nvc.GetKey(3), "Not name.");
            Assert.AreEqual(values[3], nvc.GetValues(3)[0], "Not value.");
        }

        [TestMethod]
        public void HttpCustomHeaderCollection_GetStaticHeadersWithReplace_Test()
        {
            string[] names = new string[] { "name1", "name2", "name1", "name2" };
            string[] values = new string[] { "v1", "v2", "value1", "value2" };
            HeaderNameValuePair nvp1 = new HeaderNameValuePair(names[2], values[2], CustomHeaderType.RequestStatic);
            HeaderNameValuePair nvp2 = new HeaderNameValuePair(names[3], values[3], CustomHeaderType.RequestStatic);
            IHeaderNameValuePair[] items = new IHeaderNameValuePair[] { nvp1, nvp2 };

            HttpCustomHeaderCollection headers = new(items);
            HttpRequestMessage httpRequestMessage = new();
            httpRequestMessage.Headers.Add(names[0], values[0]);
            httpRequestMessage.Headers.Add(names[1], values[1]);
            var nvc = headers.RequestAppendAndReplace(httpRequestMessage);

            Assert.IsTrue(nvc.Count == headers.Count, "Count headers mismatch.");
            Assert.AreEqual(names[0], nvc.GetKey(0), "Not name.");
            Assert.AreEqual(values[2], nvc.GetValues(0)[0], "Not value.");
            Assert.AreEqual(names[1], nvc.GetKey(1), "Not name.");
            Assert.AreEqual(values[3], nvc.GetValues(1)[0], "Not value.");
        }

        [TestMethod]
        public void HttpCustomHeaderCollection_GetRequestHeaders_Test()
        {
            string[] names = new string[] { "name1", "name2", "xname1", "xname2" };
            string[] values = new string[] { "v1", "v2" };
            HeaderNameValuePair nvp1 = new HeaderNameValuePair(names[0], names[2], CustomHeaderType.RequestMatch);
            HeaderNameValuePair nvp2 = new HeaderNameValuePair(names[1], names[3], CustomHeaderType.RequestMatch);
            IHeaderNameValuePair[] items = new IHeaderNameValuePair[] { nvp1, nvp2 };

            HttpCustomHeaderCollection headers = new(items);
            HttpRequestMessage httpRequestMessage = new();
            httpRequestMessage.Headers.Add(names[0], values[0]);
            httpRequestMessage.Headers.Add(names[1], values[1]);
            var nvc = headers.RequestAppendAndReplace(httpRequestMessage);

            Assert.IsTrue(nvc.Count == headers.Count, "Count headers mismatch.");
            Assert.AreEqual(names[2], nvc.GetKey(0), "Not name.");
            Assert.AreEqual(values[0], nvc.GetValues(0)[0], "Not value.");
            Assert.AreEqual(names[3], nvc.GetKey(1), "Not name.");
            Assert.AreEqual(values[1], nvc.GetValues(1)[0], "Not value.");
        }

        [TestMethod]
        public void HttpCustomHeaderCollection_GetIdentityHeaders_Test()
        {
            string jwtString = File.ReadAllText("../../../Assets/jwttest.txt");
            string val1 = "William Zhang (microsoft.com)";
            string val2 = "willzhan@microsoft.com";
            string[] names = new string[] { "Location", "X-MS-IDENTITY", "X-MS-EMAIL" };
            string[] values = new string[] { "Basement", "name", "email" };

            HeaderNameValuePair nvp1 = new(names[1], values[1], CustomHeaderType.RequestIdentity);
            HeaderNameValuePair nvp2 = new(names[2], values[2], CustomHeaderType.RequestIdentity);
            IHeaderNameValuePair[] items = new HeaderNameValuePair[] { nvp1, nvp2 };
            HttpCustomHeaderCollection headers = new(items);
            HttpRequestMessage request = new();
            request.Headers.Authorization = new("http", $"Bearer {jwtString}");
            request.Headers.Add(names[0], values[0]);

            var nvc = headers.RequestAppendAndReplace(request);
            nvc.Remove("Authorization");

            Assert.IsTrue(nvc.Count == headers.Count + 1, "Count headers mismatch.");
            Assert.AreEqual(names[0], nvc.GetKey(0), "Not name.");
            Assert.AreEqual(values[0], nvc.GetValues(0)[0], "Not value.");
            Assert.AreEqual(names[1], nvc.GetKey(1), "Not name.");
            Assert.AreEqual(val1, nvc.GetValues(1)[0], "Not value.");
            Assert.AreEqual(names[2], nvc.GetKey(2), "Not name.");
            Assert.AreEqual(val2, nvc.GetValues(2)[0], "Not value.");
        }

        [TestMethod]
        public void HttpCustomHeaderCollection_UpdateFromResponse_Test()
        {
            IHeaderNameValuePair[] items = new IHeaderNameValuePair[] {
                new HeaderNameValuePair("RequestName1", "RequestValue1", CustomHeaderType.RequestStatic),
                new HeaderNameValuePair("ResponseName1", "ResponseValue1", CustomHeaderType.ResponseStatic),
                new HeaderNameValuePair("ResponseName2", "ResponseValue2", CustomHeaderType.ResponseStatic),
            };

            HttpCustomHeaderCollection headers = new(items);
            HttpResponseMessage response = new();
            response.Headers.Add("ResponseName1", "fail");
            response.Headers.Add("ResponseName3", "ResponseValue3");
            headers.UpdateFromResponse(response);

            var responseHeaders = headers.Where(x => x.HeaderType == CustomHeaderType.ResponseStatic).ToList();
            Assert.IsTrue(responseHeaders.Count == 3, "Count headers mismatch.");
            Assert.IsTrue(responseHeaders[0].Name == "ResponseName1" && responseHeaders[0].Value == "ResponseValue1");
            Assert.IsTrue(responseHeaders[1].Name == "ResponseName2" && responseHeaders[1].Value == "ResponseValue2");
            Assert.IsTrue(responseHeaders[2].Name == "ResponseName3" && responseHeaders[2].Value == "ResponseValue3");
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
