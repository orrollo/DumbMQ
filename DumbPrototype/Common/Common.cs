using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;

namespace Common
{
    [TestFixture]
    public class InProcessTests
    {
        [Test]
        public void ServerCreationTest()
        {
            var serverInterface = TestsHelper.GetInProcessServerInterface();
            Assert.IsNotNull(serverInterface);
        }

        [Test]
        public void RegisterEndpointTest1()
        {
            var serverInterface = TestsHelper.GetInProcessServerInterface();
            Assert.IsNotNull(serverInterface);

            var endPoint = serverInterface.RegisterEndpoint("test1");
            Assert.IsNotNull(endPoint);
        }

        [Test]
        public void RegisterEndpointTest2()
        {
            var serverInterface = TestsHelper.GetInProcessServerInterface();
            Assert.IsNotNull(serverInterface);

            var endPoint1 = serverInterface.RegisterEndpoint("test1");
            Assert.IsNotNull(endPoint1);

            var endPoint2 = serverInterface.RegisterEndpoint("test2");
            Assert.IsNotNull(endPoint2);
        }

        [Test]
        public void FindEndpointTest1()
        {
            var serverInterface = TestsHelper.GetInProcessServerInterface();
            Assert.IsNotNull(serverInterface);

            var endPoint = serverInterface.RegisterEndpoint("test1");
            Assert.IsNotNull(endPoint);

            var foundEndPoint = serverInterface.FindEndpoint("test1");
            Assert.IsNotNull(foundEndPoint);
            Assert.AreEqual(endPoint, foundEndPoint);
        }

        [Test]
        public void FindEndpointTest2()
        {
            var serverInterface = TestsHelper.GetInProcessServerInterface();
            Assert.IsNotNull(serverInterface);

            var endPoint1 = serverInterface.RegisterEndpoint("test1");
            Assert.IsNotNull(endPoint1);

            var endPoint2 = serverInterface.RegisterEndpoint("test1");
            Assert.IsNotNull(endPoint2);

            var foundEndPoint = serverInterface.FindEndpoint("test1");
            Assert.IsNotNull(foundEndPoint);
            Assert.IsTrue(endPoint1 == foundEndPoint || endPoint2 == foundEndPoint);
        }

        [Test]
        public void FindEndpointTest3()
        {
            var serverInterface = TestsHelper.GetInProcessServerInterface();
            Assert.IsNotNull(serverInterface);

            var endPoint1 = serverInterface.RegisterEndpoint("test1");
            Assert.IsNotNull(endPoint1);

            var endPoint2 = serverInterface.RegisterEndpoint("test2");
            Assert.IsNotNull(endPoint2);

            var foundEndPoint1 = serverInterface.FindEndpoint("test1");
            Assert.IsNotNull(foundEndPoint1);
            Assert.AreEqual(endPoint1, foundEndPoint1);

            var foundEndPoint2 = serverInterface.FindEndpoint("test2");
            Assert.IsNotNull(foundEndPoint2);
            Assert.AreEqual(endPoint2, foundEndPoint2);
        }

        [Test]
        public void CreateMessageTest1()
        {
            var serverInterface = TestsHelper.GetInProcessServerInterface();
            Assert.IsNotNull(serverInterface);

            var endPoint1 = serverInterface.RegisterEndpoint("test1");
            Assert.IsNotNull(endPoint1);

            var message = endPoint1.CreateMessage("test2");
            Assert.IsNotNull(message);
        }

        [Test]
        public void CreateMessageTest2()
        {
            var serverInterface = TestsHelper.GetInProcessServerInterface();
            Assert.IsNotNull(serverInterface);

            var endPoint1 = serverInterface.RegisterEndpoint("test1");
            Assert.IsNotNull(endPoint1);

            var message = endPoint1.CreateMessage("test2");
            Assert.IsNotNull(message);

            var bytes = Encoding.UTF8.GetBytes("Hello, world!");
            message.Body = bytes;
            Assert.AreEqual(bytes, message.Body);
        }

        [Test]
        public void CreateMessageTest3()
        {
            var serverInterface = TestsHelper.GetInProcessServerInterface();
            Assert.IsNotNull(serverInterface);

            var endPoint1 = serverInterface.RegisterEndpoint("test1");
            Assert.IsNotNull(endPoint1);

            var message = endPoint1.CreateMessage("test2");
            Assert.IsNotNull(message);

            message.Body = Encoding.UTF8.GetBytes("Hello, world!");

            Assert.Catch<ArgumentException>(() => message.CreateReply());
        }
    }

    public static class TestsHelper
    {
        public static IDmqMessageBox GetInProcessServerInterface()
        {
            return new DmqInProcessServer();
        }
    }

    public class DmqInProcessServer : IDmqMessageBox
    {
        readonly Dictionary<string,List<IDmqEndpoint>> _endPoints = new Dictionary<string, List<IDmqEndpoint>>();

        public IDmqEndpoint RegisterEndpoint(string serviceName)
        {
            serviceName = serviceName.NormalizeServiceName();
            var ep = new DmqInProcessEndpoint(serviceName);
            lock (_endPoints)
            {
                if (!_endPoints.ContainsKey(serviceName)) _endPoints.Add(serviceName, new List<IDmqEndpoint>());
                var uniqId = ((IDmqEndpoint) ep).GetUniqueId();
                if (_endPoints[serviceName].Any(x => x.GetUniqueId() == uniqId)) throw new ArgumentException("already registered");
                _endPoints[serviceName].Add(ep);
            }
            return ep;
        }

        public IDmqEndpoint FindEndpoint(string serviceName)
        {
            serviceName = serviceName.NormalizeServiceName();

            lock (_endPoints)
            {
                if (!_endPoints.ContainsKey(serviceName)) return null;
                var list = _endPoints[serviceName];
                var count = list.Count;
                if (count == 0) return null;
                if (count == 1) return list[0];
                var index = (new Random()).Next(count);
                return list[index];
            }
        }
    }

    public static class DmqHelper
    {
        public static string NormalizeServiceName(this string serviceName)
        {
            if (String.IsNullOrWhiteSpace(serviceName)) throw new ArgumentException("targetServiceName is empty");
            return serviceName.Trim().ToUpper();
        }
    }

    public interface IUniqueInstance
    {
        Guid GetUniqueId();
    }

    public abstract class UniqueInstance : IUniqueInstance
    {
        protected Guid UniqueId = Guid.NewGuid();

        public Guid GetUniqueId()
        {
            return UniqueId;
        }
    }

    public class DmqInProcessEndpoint : UniqueInstance, IDmqEndpoint
    {
        public DmqInProcessEndpoint(string serviceName)
        {
            this.ServiceName = serviceName;
        }

        public string ServiceName { get; protected set; }

        public IDmqMessage CreateMessage(string targetServiceName)
        {
            return new DmqMessage(this, targetServiceName);
        }

        public string GetServiceName()
        {
            return ServiceName;
        }
    }

    public class DmqMessage : UniqueInstance, IDmqMessage
    {
        protected IDmqEndpoint EndPoint;
        private bool _isReply = false;

        public string TargetServiceName { get; protected set; }
        public string SourceServiceName { get { return EndPoint.GetServiceName(); } }

        public Guid? ParentId { get; protected set; }

        public bool IsIncoming { get; protected set; }

        public bool IsReply
        {
            get { return _isReply; }
        }

        private byte[] _body = null;

        public byte[] Body
        {
            get { return _body; }
            set { SetBody(value); }
        }

        private void SetBody(byte[] value)
        {
            if (IsIncoming) throw new ArgumentException("changes are not allowed");
            if (value == null) throw new ArgumentException("null is not allowed");
            _body = value;
        }

        public DmqMessage(IDmqEndpoint endPoint, string targetServiceName)
        {
            this.TargetServiceName = targetServiceName;
            this.EndPoint = endPoint;
            this._isReply = false;
            this.IsIncoming = false;
        }

        public IDmqMessage CreateReply()
        {
            if (!this.IsIncoming) throw new ArgumentException("unable to write response to outgoing message");
            var msg = new DmqMessage(EndPoint, SourceServiceName);
            msg.ParentId = this.GetUniqueId();
            msg._isReply = true;
            msg.IsIncoming = false;
            return msg;
        }
    }

    public interface IDmqMessageBox
    {
        IDmqEndpoint RegisterEndpoint(string serviceName);
        IDmqEndpoint FindEndpoint(string serviceName);
    }

    public interface IDmqEndpoint : IUniqueInstance
    {
        IDmqMessage CreateMessage(string targetServiceName);
        string GetServiceName();
    }

    public interface IDmqMessage : IUniqueInstance
    {
        string TargetServiceName { get; }
        string SourceServiceName { get; }
        bool IsReply { get; }
        Guid? ParentId { get; }
        byte[] Body { get; set; }
        bool IsIncoming { get; }
        IDmqMessage CreateReply();
    }
}
