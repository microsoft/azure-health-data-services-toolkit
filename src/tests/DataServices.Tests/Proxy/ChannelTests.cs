using System.Collections.Generic;
using System.Linq;
using DataServices.Channels;
using DataServices.Tests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataServices.Tests.Proxy
{
    [TestClass]
    public class ChannelTests
    {

        [TestMethod]
        public void InputChannelCollection_Add_Test()
        {
            FakeChannel channel = new();
            InputChannelCollection channels = new();
            channel.OnError += (a, args) =>
            {
                Assert.Fail("Should not trigger error event.");
            };
            channels.Add(channel);
            IChannel actual = channels[0];
            Assert.AreEqual(channel.Name, actual.Name, "Name mismatch.");
        }

        [TestMethod]
        public void InputChannelCollection_Remove_Test()
        {
            FakeChannel channel = new();
            channel.OnError += (a, args) =>
            {
                Assert.Fail("should not error");
            };

            InputChannelCollection channels = new();
            channels.Add(channel);
            IChannel actual = channels[0];
            channels.Remove(actual);
            Assert.IsTrue(channels.Count == 0, "Channel collection should be empty.");
        }

        [TestMethod]
        public void InputChannelCollection_RemoveAt_Test()
        {
            FakeChannel channel = new();
            InputChannelCollection channels = new();
            channels.Add(channel);
            channels.RemoveAt(0);
            Assert.IsTrue(channels.Count == 0, "Channel collection should be empty.");
        }

        [TestMethod]
        public void InputChannelCollection_GetEnumerator_Test()
        {
            FakeChannel channel = new();
            InputChannelCollection channels = new();
            channels.Add(channel);

            IEnumerator<IChannel> en = channels.GetEnumerator();
            while (en.MoveNext())
            {
                Assert.AreEqual(channel.Name, en.Current.Name, "Name mismatch.");
            }
        }

        [TestMethod]
        public void InputChannelCollection_ToArray_Test()
        {
            FakeChannel channel = new();
            InputChannelCollection channels = new();
            channels.Add(channel);

            IChannel[] channelArray = channels.ToArray();
            Assert.IsTrue(channelArray.Length == 1, "Channel collection should be count = 1.");
        }

        [TestMethod]
        public void InputChannelCollection_Contains_True_Test()
        {
            FakeChannel channel = new();
            InputChannelCollection channels = new();
            channels.Add(channel);
            Assert.IsTrue(channels.Contains(channel), "Channel not found.");
        }

        [TestMethod]
        public void InputChannelCollection_Contains_False_Test()
        {
            FakeChannel channel = new();
            InputChannelCollection channels = new();
            Assert.IsFalse(channels.Contains(channel), "Channel should not be present.");
        }

        [TestMethod]
        public void InputChannelCollection_IndexOf_Test()
        {
            FakeChannel channel = new();
            InputChannelCollection channels = new();
            channels.Add(channel);
            Assert.IsTrue(channels.IndexOf(channel) == 0, "Channel index mismatch.");
        }

        [TestMethod]
        public void InputChannelCollection_Insert_Test()
        {
            FakeChannel channel = new();
            InputChannelCollection channels = new();
            channels.Insert(0, channel);
            Assert.IsTrue(channels.IndexOf(channel) == 0, "Channel index mismatch.");
        }


        [TestMethod]
        public void InputChannelCollection_Clear_Test()
        {
            FakeChannel channel = new();
            InputChannelCollection channels = new();
            channels.Add(channel);
            Assert.IsTrue(channels.Count == 1, "Channel count should be 1.");
            channels.Clear();
            Assert.IsTrue(channels.Count == 0, "Channel count should be 0.");
        }


        #region output channel
        [TestMethod]
        public void OutputChannelCollection_Add_Test()
        {
            FakeChannel channel = new();
            OutputChannelCollection channels = new();
            channel.OnError += (a, args) =>
            {
                Assert.Fail("Should not trigger error event.");
            };
            channels.Add(channel);
            IChannel actual = channels[0];
            Assert.AreEqual(channel.Name, actual.Name, "Name mismatch.");
        }

        [TestMethod]
        public void OutputChannelCollection_Remove_Test()
        {
            FakeChannel channel = new();
            channel.OnError += (a, args) =>
            {
                Assert.Fail("should not error");
            };

            OutputChannelCollection channels = new();
            channels.Add(channel);
            IChannel actual = channels[0];
            channels.Remove(actual);
            Assert.IsTrue(channels.Count == 0, "Channel collection should be empty.");
        }

        [TestMethod]
        public void OutputChannelCollection_RemoveAt_Test()
        {
            FakeChannel channel = new();
            OutputChannelCollection channels = new();
            channels.Add(channel);
            channels.RemoveAt(0);
            Assert.IsTrue(channels.Count == 0, "Channel collection should be empty.");
        }

        [TestMethod]
        public void OutputChannelCollection_GetEnumerator_Test()
        {
            FakeChannel channel = new();
            OutputChannelCollection channels = new();
            channels.Add(channel);

            IEnumerator<IChannel> en = channels.GetEnumerator();
            while (en.MoveNext())
            {
                Assert.AreEqual(channel.Name, en.Current.Name, "Name mismatch.");
            }
        }

        [TestMethod]
        public void OutputChannelCollection_ToArray_Test()
        {
            FakeChannel channel = new();
            OutputChannelCollection channels = new();
            channels.Add(channel);

            IChannel[] channelArray = channels.ToArray();
            Assert.IsTrue(channelArray.Length == 1, "Channel collection should be count = 1.");
        }

        [TestMethod]
        public void OutputChannelCollection_Contains_True_Test()
        {
            FakeChannel channel = new();
            OutputChannelCollection channels = new();
            channels.Add(channel);
            Assert.IsTrue(channels.Contains(channel), "Channel not found.");
        }

        [TestMethod]
        public void OutputChannelCollection_Contains_False_Test()
        {
            FakeChannel channel = new();
            OutputChannelCollection channels = new();
            Assert.IsFalse(channels.Contains(channel), "Channel should not be present.");
        }

        [TestMethod]
        public void OutputChannelCollection_IndexOf_Test()
        {
            FakeChannel channel = new();
            OutputChannelCollection channels = new();
            channels.Add(channel);
            Assert.IsTrue(channels.IndexOf(channel) == 0, "Channel index mismatch.");
        }

        [TestMethod]
        public void OutputChannelCollection_Insert_Test()
        {
            FakeChannel channel = new();
            OutputChannelCollection channels = new();
            channels.Insert(0, channel);
            Assert.IsTrue(channels.IndexOf(channel) == 0, "Channel index mismatch.");
        }


        [TestMethod]
        public void OutputChannelCollection_Clear_Test()
        {
            FakeChannel channel = new();
            OutputChannelCollection channels = new();
            channels.Add(channel);
            Assert.IsTrue(channels.Count == 1, "Channel count should be 1.");
            channels.Clear();
            Assert.IsTrue(channels.Count == 0, "Channel count should be 0.");
        }

        #endregion

    }
}
