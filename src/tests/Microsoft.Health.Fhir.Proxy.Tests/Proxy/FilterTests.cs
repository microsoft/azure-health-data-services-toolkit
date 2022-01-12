using Microsoft.Health.Fhir.Proxy.Filters;
using Microsoft.Health.Fhir.Proxy.Tests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Microsoft.Health.Fhir.Proxy.Tests.Proxy
{
    [TestClass]
    public class FilterTests
    {

        #region input filter collection

        [TestMethod]
        public void InputFilterCollection_Add_Test()
        {
            FakeFilter filter = new();
            filter.OnFilterError += (a, args) =>
            {
                Assert.Fail("Should not trigger on error event.");
            };
            IInputFilterCollection filters = new InputFilterCollection
            {
                filter
            };
            IFilter actual = filters[0];
            Assert.AreEqual(filter.Name, actual.Name, "Name mismatch.");
        }

        [TestMethod]
        public void InputFilterCollection_Remove_Test()
        {
            FakeFilter filter = new();
            IInputFilterCollection filters = new InputFilterCollection
            {
                filter
            };
            IFilter actual = filters[0];
            filters.Remove(actual);
            Assert.IsTrue(filters.Count == 0, "Filter collection should be empty.");
        }

        [TestMethod]
        public void InputFilterCollection_RemoveAt_Test()
        {
            FakeFilter filter = new();
            IInputFilterCollection filters = new InputFilterCollection
            {
                filter
            };
            filters.RemoveAt(0);
            Assert.IsTrue(filters.Count == 0, "Filter collection should be empty.");
        }

        [TestMethod]
        public void InputFilterCollection_GetEnumerator_Test()
        {
            FakeFilter filter = new();
            IInputFilterCollection filters = new InputFilterCollection
            {
                filter
            };

            IEnumerator<IFilter> en = filters.GetEnumerator();
            while (en.MoveNext())
            {
                Assert.AreEqual(filter.Name, en.Current.Name, "Name mismatch.");
            }
        }

        [TestMethod]
        public void InputFilterCollection_ToArray_Test()
        {
            FakeFilter filter = new();
            IInputFilterCollection filters = new InputFilterCollection
            {
                filter
            };
            IFilter[] filterArray = filters.ToArray();
            Assert.IsTrue(filterArray.Length == 1, "Filter collection should be count = 1.");
        }

        [TestMethod]
        public void InputFilterCollection_Contains_True_Test()
        {
            FakeFilter filter = new();
            IInputFilterCollection filters = new InputFilterCollection
            {
                filter
            };
            Assert.IsTrue(filters.Contains(filter), "Filter not found.");
        }

        [TestMethod]
        public void InputFilterCollection_Contains_False_Test()
        {
            FakeFilter filter = new();
            IInputFilterCollection filters = new InputFilterCollection();
            Assert.IsFalse(filters.Contains(filter), "Filter should not be present.");
        }

        [TestMethod]
        public void InputFilterCollection_IndexOf_Test()
        {
            FakeFilter filter = new();
            IInputFilterCollection filters = new InputFilterCollection
            {
                filter
            };
            Assert.IsTrue(filters.IndexOf(filter) == 0, "Filter index mismatch.");
        }

        [TestMethod]
        public void InputFilterCollection_Insert_Test()
        {
            FakeFilter filter = new();
            IInputFilterCollection filters = new InputFilterCollection();
            filters.Insert(0, filter);
            Assert.IsTrue(filters.IndexOf(filter) == 0, "Filter index mismatch.");
        }


        [TestMethod]
        public void InputFilterCollection_Clear_Test()
        {
            FakeFilter filter = new();
            IInputFilterCollection filters = new InputFilterCollection
            {
                filter
            };
            Assert.IsTrue(filters.Count == 1, "Filter count should be 1.");
            filters.Clear();
            Assert.IsTrue(filters.Count == 0, "Filter count should be 0.");
        }

        #endregion

        #region output filter collection

        [TestMethod]
        public void OutputFilterCollection_Add_Test()
        {
            FakeFilter filter = new();
            filter.OnFilterError += (a, args) =>
            {
                Assert.Fail("Should not trigger on error event.");
            };
            IOutputFilterCollection filters = new OutputFilterCollection
            {
                filter
            };
            IFilter actual = filters[0];
            Assert.AreEqual(filter.Name, actual.Name, "Name mismatch.");
        }

        [TestMethod]
        public void OutputFilterCollection_Remove_Test()
        {
            FakeFilter filter = new();
            IOutputFilterCollection filters = new OutputFilterCollection
            {
                filter
            };
            IFilter actual = filters[0];
            filters.Remove(actual);
            Assert.IsTrue(filters.Count == 0, "Filter collection should be empty.");
        }

        [TestMethod]
        public void OutputFilterCollection_RemoveAt_Test()
        {
            FakeFilter filter = new();
            IOutputFilterCollection filters = new OutputFilterCollection
            {
                filter
            };
            filters.RemoveAt(0);
            Assert.IsTrue(filters.Count == 0, "Filter collection should be empty.");
        }

        [TestMethod]
        public void OutputFilterCollection_GetEnumerator_Test()
        {
            FakeFilter filter = new();
            IOutputFilterCollection filters = new OutputFilterCollection
            {
                filter
            };

            IEnumerator<IFilter> en = filters.GetEnumerator();
            while (en.MoveNext())
            {
                Assert.AreEqual(filter.Name, en.Current.Name, "Name mismatch.");
            }
        }

        [TestMethod]
        public void OutputFilterCollection_ToArray_Test()
        {
            FakeFilter filter = new();
            IOutputFilterCollection filters = new OutputFilterCollection
            {
                filter
            };
            IFilter[] filterArray = filters.ToArray();
            Assert.IsTrue(filterArray.Length == 1, "Filter collection should be count = 1.");
        }

        [TestMethod]
        public void OutputFilterCollection_Contains_True_Test()
        {
            FakeFilter filter = new();
            IOutputFilterCollection filters = new OutputFilterCollection
            {
                filter
            };
            Assert.IsTrue(filters.Contains(filter), "Filter not found.");
        }

        [TestMethod]
        public void OutputFilterCollection_Contains_False_Test()
        {
            FakeFilter filter = new();
            IOutputFilterCollection filters = new OutputFilterCollection();
            Assert.IsFalse(filters.Contains(filter), "Filter should not be present.");
        }

        [TestMethod]
        public void OutputFilterCollection_IndexOf_Test()
        {
            FakeFilter filter = new();
            IOutputFilterCollection filters = new OutputFilterCollection
            {
                filter
            };
            Assert.IsTrue(filters.IndexOf(filter) == 0, "Filter index mismatch.");
        }

        [TestMethod]
        public void OutputFilterCollection_Insert_Test()
        {
            FakeFilter filter = new();
            IOutputFilterCollection filters = new OutputFilterCollection();
            filters.Insert(0, filter);
            Assert.IsTrue(filters.IndexOf(filter) == 0, "Filter index mismatch.");
        }


        [TestMethod]
        public void OutputFilterCollection_Clear_Test()
        {
            FakeFilter filter = new();
            IOutputFilterCollection filters = new OutputFilterCollection
            {
                filter
            };
            Assert.IsTrue(filters.Count == 1, "Filter count should be 1.");
            filters.Clear();
            Assert.IsTrue(filters.Count == 0, "Filter count should be 0.");
        }

        #endregion


        [TestMethod]
        public void Filter_SignalErrorEvent_Test()
        {
            string name = "ErrorFilter";
            bool fatal = true;
            string errorMessage = "Boom!";
            Exception error = new(errorMessage);
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            string body = "stuff";
            FakeFilterWithError filter = new(name, fatal, error, code, body);
            
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
