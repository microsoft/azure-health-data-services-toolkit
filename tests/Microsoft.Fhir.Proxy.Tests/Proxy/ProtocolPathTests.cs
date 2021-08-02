using Microsoft.Fhir.Proxy.Protocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Microsoft.Fhir.Proxy.Tests.Proxy
{
    [TestClass]
    public class ProtocolPathTests
    {
        [TestMethod]
        public void FhirPath_Default_Complete_Uri_Test()
        {
            string method = "GET";
            string resource = "Resource";
            string id = "Id";
            string operation = "Operation";
            string version = "Version";
            string routePrefix = "fhir";
            string requestUriString = $"https://example.org/{routePrefix}/{resource}/{id}/{operation}/{version}";
            Uri uri = new(requestUriString);
            string normalizedPath = uri.LocalPath.Replace(routePrefix, "");

            FhirPath fhirPath = new(method, requestUriString, routePrefix);
            Assert.AreEqual(resource, fhirPath.Resource, "Resource mismatch.");
            Assert.AreEqual(id, fhirPath.Id, "ID mismatch.");
            Assert.AreEqual(operation, fhirPath.Operation, "Operation mismatch.");
            Assert.AreEqual(version, fhirPath.Version, "Version mismatch.");
            Assert.AreEqual(method, fhirPath.Method, "Method mismatch.");
            Assert.AreEqual(routePrefix, fhirPath.RoutePrefix, "Route prefix mismatch.");
            Assert.AreEqual(uri.LocalPath.TrimStart('/'), fhirPath.Path, "Path mismatch.");
            Assert.AreEqual(normalizedPath.TrimStart('/'), fhirPath.NormalizedPath, "Normalized path mismatch.");
        }

        [TestMethod]
        public void FhirPath_Default_Complete_Uri_LongReoutePrefix_Test()
        {
            string method = "GET";
            string resource = "Resource";
            string id = "Id";
            string operation = "Operation";
            string version = "Version";
            string routePrefix = "fhir/long";
            string requestUriString = $"https://example.org/{routePrefix}/{resource}/{id}/{operation}/{version}";
            Uri uri = new(requestUriString);
            string normalizedPath = uri.LocalPath.Replace(routePrefix, "");

            FhirPath fhirPath = new(method, requestUriString, routePrefix);
            Assert.AreEqual(resource, fhirPath.Resource, "Resource mismatch.");
            Assert.AreEqual(id, fhirPath.Id, "ID mismatch.");
            Assert.AreEqual(operation, fhirPath.Operation, "Operation mismatch.");
            Assert.AreEqual(version, fhirPath.Version, "Version mismatch.");
            Assert.AreEqual(method, fhirPath.Method, "Method mismatch.");
            Assert.AreEqual(routePrefix, fhirPath.RoutePrefix, "Route prefix mismatch.");
            Assert.AreEqual(uri.LocalPath.TrimStart('/'), fhirPath.Path, "Path mismatch.");
            Assert.AreEqual(normalizedPath.TrimStart('/'), fhirPath.NormalizedPath, "Normalized path mismatch.");
        }

        [TestMethod]
        public void FhirPath_NoRoutePrefix_Complete_Uri_Test()
        {
            string method = "GET";
            string resource = "Resource";
            string id = "Id";
            string operation = "Operation";
            string version = "Version";
            string requestUriString = $"https://example.org/{resource}/{id}/{operation}/{version}";
            Uri uri = new(requestUriString);

            FhirPath fhirPath = new(method, requestUriString, null);
            Assert.AreEqual(resource, fhirPath.Resource, "Resource mismatch.");
            Assert.AreEqual(id, fhirPath.Id, "ID mismatch.");
            Assert.AreEqual(operation, fhirPath.Operation, "Operation mismatch.");
            Assert.AreEqual(version, fhirPath.Version, "Version mismatch.");
            Assert.AreEqual(method, fhirPath.Method, "Method mismatch.");
            Assert.IsNull(fhirPath.RoutePrefix, "Route prefix not null.");
            Assert.AreEqual(uri.LocalPath.TrimStart('/'), fhirPath.Path, "Path mismatch.");
            Assert.AreEqual(uri.LocalPath.TrimStart('/'), fhirPath.NormalizedPath, "Normalized path mismatch.");
        }


        [TestMethod]
        public void FhirPath_Default_Fragment_Uri_Test()
        {
            string method = "GET";
            string resource = "Resource";
            string id = "Id";
            string routePrefix = "fhir";
            string requestUriString = $"https://example.org/{routePrefix}/{resource}/{id}";

            FhirPath fhirPath = new(method, requestUriString, routePrefix);
            Assert.AreEqual(resource, fhirPath.Resource, "Resource mismatch.");
            Assert.AreEqual(id, fhirPath.Id, "ID mismatch.");
            Assert.IsNull(fhirPath.Operation, "Operation not null.");
            Assert.IsNull(fhirPath.Version, "Version not null.");
            Assert.AreEqual(method, fhirPath.Method, "Method mismatch.");
            Assert.AreEqual(routePrefix, fhirPath.RoutePrefix, "Route prefix mismatch.");
        }


        [TestMethod]
        public void FhirPath_HasQueryKey_True_Test()
        {
            string method = "GET";
            string resource = "Resource";
            string id = "Id";
            string routePrefix = "fhir";
            string key = "key";
            string requestUriString = $"https://example.org/{routePrefix}/{resource}/{id}?{key}=value";

            FhirPath fhirPath = new(method, requestUriString, routePrefix);
            Assert.IsTrue(fhirPath.HasQueryParameter(key), "Key not be found.");
        }

        [TestMethod]
        public void FhirPath_HasQueryKey_False_Test()
        {
            string method = "GET";
            string resource = "Resource";
            string id = "Id";
            string routePrefix = "fhir";
            string key = "key";
            string requestUriString = $"https://example.org/{routePrefix}/{resource}/{id}?{key}=value";

            string badKey = "badkey";
            FhirPath fhirPath = new(method, requestUriString, routePrefix);
            Assert.IsFalse(fhirPath.HasQueryParameter(badKey), "Key should not be found.");
        }
    }
}
