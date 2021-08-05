using Microsoft.Fhir.Proxy.Channels;
using Microsoft.Fhir.Proxy.Tests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fhir.Proxy.Tests.Proxy
{
    [TestClass]
    public class ChannelTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            ChannelFactory.Clear();
        }

        [TestMethod]
        public void ChannelCollection_Add_Test()
        {
            FakeChannel channel = new();
            ChannelCollection channels = new();
            channels.Add(channel);
            IChannel actual = channels[0];
            Assert.AreEqual(channel.Name, actual.Name, "Name mismatch.");
        }

        [TestMethod]
        public void ChannelCollection_Remove_Test()
        {
            FakeChannel channel = new();
            ChannelCollection channels = new();
            channels.Add(channel);
            IChannel actual = channels[0];
            channels.Remove(actual);
            Assert.IsTrue(channels.Count == 0, "Channel collection should be empty.");
        }

        [TestMethod]
        public void ChannelCollection_RemoveAt_Test()
        {
            FakeChannel channel = new();
            ChannelCollection channels = new();
            channels.Add(channel);
            channels.RemoveAt(0);
            Assert.IsTrue(channels.Count == 0, "Channel collection should be empty.");
        }

        [TestMethod]
        public void ChannelCollection_GetEnumerator_Test()
        {
            FakeChannel channel = new();
            ChannelCollection channels = new();
            channels.Add(channel);

            IEnumerator<IChannel> en = channels.GetEnumerator();
            while (en.MoveNext())
            {
                Assert.AreEqual(channel.Name, en.Current.Name, "Name mismatch.");
            }
        }

        [TestMethod]
        public void ChannelCollection_ToArray_Test()
        {
            FakeChannel channel = new();
            ChannelCollection channels = new();
            channels.Add(channel);

            IChannel[] channelArray = channels.ToArray();
            Assert.IsTrue(channelArray.Length == 1, "Channel collection should be count = 1.");
        }

        [TestMethod]
        public void ChannelCollection_Contains_True_Test()
        {
            FakeChannel channel = new();
            ChannelCollection channels = new();
            channels.Add(channel);
            Assert.IsTrue(channels.Contains(channel), "Channel not found.");
        }

        [TestMethod]
        public void ChannelCollection_Contains_False_Test()
        {
            FakeChannel channel = new();
            ChannelCollection channels = new();
            Assert.IsFalse(channels.Contains(channel), "Channel should not be present.");
        }

        [TestMethod]
        public void ChannelCollection_IndexOf_Test()
        {
            FakeChannel channel = new();
            ChannelCollection channels = new();
            channels.Add(channel);
            Assert.IsTrue(channels.IndexOf(channel) == 0, "Channel index mismatch.");
        }

        [TestMethod]
        public void ChannelCollection_Insert_Test()
        {
            FakeChannel channel = new();
            ChannelCollection channels = new();
            channels.Insert(0, channel);
            Assert.IsTrue(channels.IndexOf(channel) == 0, "Channel index mismatch.");
        }


        [TestMethod]
        public void ChannelCollection_Clear_Test()
        {
            FakeChannel channel = new();
            ChannelCollection channels = new();
            channels.Add(channel);
            Assert.IsTrue(channels.Count == 1, "Channel count should be 1.");
            channels.Clear();
            Assert.IsTrue(channels.Count == 0, "Channel count should be 0.");
        }

        [TestMethod]
        public void ChannelFactory_Register_Test()
        {
            FakeChannel channel = new();
            ChannelFactory.Register(channel.Name, typeof(FakeChannel), null);
            string[] names = ChannelFactory.GetNames();
            Assert.IsTrue(names.Length == 1, "Channel not present.");
            Assert.AreEqual(channel.Name, names[0], "Channel name mismatch.");
        }

        [TestMethod]
        public void ChannelFactory_Register_WithCtorParameters_Test()
        {
            string name = "foo";
            ChannelFactory.Register(name, typeof(FakeChannelWithCtorParam), new object[] { name });
            string[] names = ChannelFactory.GetNames();
            Assert.IsTrue(names.Length == 1, "Channel count invalid.");
            Assert.AreEqual(name, names[0], "Channel name mismatch.");
            IChannel channel = ChannelFactory.Create(name);
            Assert.IsNotNull(channel, "Channel is null.");
        }

        [TestMethod]
        public void ChannelFactory_Clear_Test()
        {
            FakeChannel channel = new();
            ChannelFactory.Register(channel.Name, typeof(FakeChannel), null);
            string[] names = ChannelFactory.GetNames();
            Assert.IsTrue(names.Length == 1, "Channel not present.");
            ChannelFactory.Clear();
            Assert.IsNull(ChannelFactory.GetNames(), "ChannelFactory should be empty.");
        }

        [TestMethod]
        public void ChannelFactory_Create_Test()
        {
            FakeChannel channel = new();
            ChannelFactory.Register(channel.Name, typeof(FakeChannel), null);
            IChannel actualChannel = ChannelFactory.Create(channel.Name);
            Assert.AreEqual(channel.Name, actualChannel.Name, "Channel name mismatch.");
            Assert.IsNotNull(actualChannel, "Channel should not be null.");
        }


        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void ChannelFactory_NameNotFoundException_Test()
        {
            string invalidName = "foo";
            FakeChannel channel = new();
            ChannelFactory.Register(channel.Name, typeof(FakeChannel), null);
            ChannelFactory.Create(invalidName);
        }
    }
}
