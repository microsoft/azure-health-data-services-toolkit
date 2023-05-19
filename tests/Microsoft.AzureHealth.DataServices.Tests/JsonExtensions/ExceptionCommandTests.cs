using System.IO;
using Microsoft.AzureHealth.DataServices.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.AzureHealth.DataServices.Tests.JsonExtensions
{
    [TestClass]
    public class ExceptionCommandTests
    {
        private static string json;

        [ClassInitialize]
        public static void JTokenInit(TestContext context)
        {
            context.WriteLine("JTokenExtensions");
            json = File.ReadAllTextAsync("../../../Assets/BundleRequest.json").GetAwaiter().GetResult();
        }

        [TestMethod]
        public void TokenExistsCommand_Test()
        {
            JToken token = JToken.Parse(json);
            string jpath = "$.resourceType";
            TokenExistsCommand cmd = new(token, jpath);
            cmd.Execute();
        }

        [TestMethod]
        [ExpectedException(typeof(CommandException))]
        public void TokenExistsCommand_Exception_Test()
        {
            JToken token = JToken.Parse(json);
            string jpath = "$.resourceTypeX";
            TokenExistsCommand cmd = new(token, jpath);
            cmd.Execute();
        }

        [TestMethod]
        public void IsArrayCommand_Test()
        {
            JToken token = JToken.Parse(json);
            string jpath = "$.entry";
            IsArrayCommand cmd = new(token, jpath);
            cmd.Execute();
        }

        [TestMethod]
        [ExpectedException(typeof(CommandException))]
        public void IsArrayCommand_Exception_Test()
        {
            JToken token = JToken.Parse(json);
            string jpath = "$.entryX";
            IsArrayCommand cmd = new(token, jpath);
            cmd.Execute();
        }

        [TestMethod]
        public void IsNullOrEmptyCommand_Test()
        {
            JToken token = JToken.Parse(json);
            string jpath = "$.foo";
            IsNullOrEmptyCommand cmd = new(token, jpath);
            cmd.Execute();
        }

        [TestMethod]
        [ExpectedException(typeof(CommandException))]
        public void IsNullOrEmptyCommand_Exception_Test()
        {
            JToken token = JToken.Parse(json);
            string jpath = "$.foo";
            IsNotNullOrEmptyCommand cmd = new(token, jpath);
            cmd.Execute();
        }

        [TestMethod]
        public void IsMatchCommand_Test()
        {
            JToken token = JToken.Parse(json);
            string jpath = "$.resourceType";
            string value = "Bundle";
            var cmd = new IsMatchCommand<string>(token, jpath, value);
            cmd.Execute();
        }

        [TestMethod]
        [ExpectedException(typeof(CommandException))]
        public void IsMatchCommand_Exception_Test()
        {
            JToken token = JToken.Parse(json);
            string jpath = "$.resourceType";
            string value = "PatientX";
            var cmd = new IsMatchCommand<string>(token, jpath, value);
            cmd.Execute();
        }
    }
}
