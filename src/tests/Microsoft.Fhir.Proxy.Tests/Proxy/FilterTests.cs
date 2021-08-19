using Microsoft.Fhir.Proxy.Filters;
using Microsoft.Fhir.Proxy.Tests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Microsoft.Fhir.Proxy.Tests.Proxy
{
    [TestClass]
    public class FilterTests
    {

        [TestInitialize]
        public void TestInitialize()
        {
            FilterFactory.Clear();
        }


        [TestMethod]
        public void FilterCollection_Add_Test()
        {
            FakeFilter filter = new();
            filter.OnFilterError += (a, args) =>
            {
                Assert.Fail("Should not trigger on error event.");
            };
            FilterCollection filters = new();
            filters.Add(filter);
            IFilter actual = filters[0];
            Assert.AreEqual(filter.Name, actual.Name, "Name mismatch.");
        }

        [TestMethod]
        public void FilterCollection_Remove_Test()
        {
            FakeFilter filter = new();
            FilterCollection filters = new();
            filters.Add(filter);
            IFilter actual = filters[0];
            filters.Remove(actual);
            Assert.IsTrue(filters.Count == 0, "Filter collection should be empty.");
        }

        [TestMethod]
        public void FilterCollection_RemoveAt_Test()
        {
            FakeFilter filter = new();
            FilterCollection filters = new();
            filters.Add(filter);
            filters.RemoveAt(0);
            Assert.IsTrue(filters.Count == 0, "Filter collection should be empty.");
        }

        [TestMethod]
        public void FilterCollection_GetEnumerator_Test()
        {
            FakeFilter filter = new();
            FilterCollection filters = new();
            filters.Add(filter);

            IEnumerator<IFilter> en = filters.GetEnumerator();
            while (en.MoveNext())
            {
                Assert.AreEqual(filter.Name, en.Current.Name, "Name mismatch.");
            }
        }

        [TestMethod]
        public void FilterCollection_ToArray_Test()
        {
            FakeFilter filter = new();
            FilterCollection filters = new();
            filters.Add(filter);
            IFilter[] filterArray = filters.ToArray();
            Assert.IsTrue(filterArray.Length == 1, "Filter collection should be count = 1.");
        }

        [TestMethod]
        public void FilterCollection_Contains_True_Test()
        {
            FakeFilter filter = new();
            FilterCollection filters = new();
            filters.Add(filter);
            Assert.IsTrue(filters.Contains(filter), "Filter not found.");
        }

        [TestMethod]
        public void FilterCollection_Contains_False_Test()
        {
            FakeFilter filter = new();
            FilterCollection filters = new();
            Assert.IsFalse(filters.Contains(filter), "Filter should not be present.");
        }

        [TestMethod]
        public void FilterCollection_IndexOf_Test()
        {
            FakeFilter filter = new();
            FilterCollection filters = new();
            filters.Add(filter);
            Assert.IsTrue(filters.IndexOf(filter) == 0, "Filter index mismatch.");
        }

        [TestMethod]
        public void FilterCollection_Insert_Test()
        {
            FakeFilter filter = new();
            FilterCollection filters = new();
            filters.Insert(0, filter);
            Assert.IsTrue(filters.IndexOf(filter) == 0, "Filter index mismatch.");
        }


        [TestMethod]
        public void FilterCollection_Clear_Test()
        {
            FakeFilter filter = new();
            FilterCollection filters = new();
            filters.Add(filter);
            Assert.IsTrue(filters.Count == 1, "Filter count should be 1.");
            filters.Clear();
            Assert.IsTrue(filters.Count == 0, "Filter count should be 0.");
        }

        [TestMethod]
        public void FilterFactory_Register_Test()
        {
            FakeFilter filter = new();
            FilterFactory.Register(filter.Name, typeof(FakeFilter), null);
            string[] names = FilterFactory.GetNames();
            Assert.IsTrue(names.Length == 1, "Filter not present.");
            Assert.AreEqual(filter.Name, names[0], "Filter name mismatch.");
        }

        [TestMethod]
        public void FilterFactory_Register_WithCtorParameters_Test()
        {
            string name = "foo";
            FilterFactory.Register(name, typeof(FakeFilterWithCtorParam), new object[] { name });
            string[] names = FilterFactory.GetNames();
            Assert.IsTrue(names.Length == 1, $"Filter count invalid.");
            Assert.AreEqual(name, names[0], "Filter name mismatch.");
            IFilter filter = FilterFactory.Create(name);
            filter.OnFilterError += (object sender, FilterErrorEventArgs args) =>
             {
                 Assert.IsFalse(args.IsFatal, "Should not be a fatal error.");
             };

            Assert.IsNotNull(filter, "Filter is null.");
        }

        [TestMethod]
        public void FilterFactory_Clear_Test()
        {
            FakeFilter filter = new();
            FilterFactory.Register(filter.Name, typeof(FakeFilter), null);
            string[] names = FilterFactory.GetNames();
            Assert.IsTrue(names.Length == 1, "Filter not present.");
            FilterFactory.Clear();
            Assert.IsNull(FilterFactory.GetNames(), "FilterFactory should be empty.");
        }

        [TestMethod]
        public void FilterFactory_Create_Test()
        {
            FakeFilter filter = new();
            FilterFactory.Register(filter.Name, typeof(FakeFilter), null);
            IFilter actualFilter = FilterFactory.Create(filter.Name);
            Assert.AreEqual(filter.Name, actualFilter.Name, "Filter name mismatch.");
            Assert.IsNotNull(actualFilter, "Filter should not be null.");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void FilterFactory_NameNotFoundException_Test()
        {
            string invalidName = "foo";
            FakeFilter filter = new();
            filter.OnFilterError += (object sender, FilterErrorEventArgs args) =>
            {
                Assert.IsFalse(args.IsFatal, "Should not be a fatal error.");
            };

            FilterFactory.Register(filter.Name, typeof(FakeFilter), null);
            FilterFactory.Create(invalidName);
        }

        [TestMethod]
        public void Filter_SignalErrorEvent_Test()
        {
            string name = "ErrorFilter";
            bool fatal = true;
            string errorMessage = "Boom!";
            Exception error = new(errorMessage);
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string body = "stuff";
            FilterFactory.Register(name, typeof(FakeFilterWithError), new object[] { name, fatal, error, code, body });
            IFilter filter = FilterFactory.Create(name);
            bool trigger = false;
            filter.OnFilterError += (object sender, FilterErrorEventArgs args) =>
            {
                trigger = true;
                Assert.AreEqual(name, args.Name, "Filter name mismatch.");
                Assert.IsNotNull(args.Id, "Filter ID cannot be null.");
                Assert.AreEqual(fatal, args.IsFatal, "Fatal mismatch.");
                Assert.IsNotNull(args.Error, "Missing error.");
                Assert.AreEqual(errorMessage, args.Error.Message, "Error message mismatch.");
                Assert.AreEqual(code, args.Code, "Http status code mismatch.");
                Assert.AreEqual(body, args.ResponseBody, "Response body mismatch.");
            };

            _ = filter.ExecuteAsync(null);
            Assert.IsTrue(trigger, "Error event not triggered.");
        }


    }
}
