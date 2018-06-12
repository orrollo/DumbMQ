using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Proto02
{
    class TransportLayer
    {
        private static TransportLayer _transport;
        private const int defaultPort = 9090;

        protected TransportLayer() : this(defaultPort)
        {
            
        }

        public static TransportLayer GetTransport()
        {
            return _transport ?? (_transport = new TransportLayer());
        }

        Dictionary<string,Node> nodes = new Dictionary<string, Node>();

        private Thread thread;

        private TransportLayer(int port)
        {
            thread = new Thread(PacketsProc);
            thread.IsBackground = true;
            thread.Start();
        }

        private void PacketsProc()
        {
            var stop = false;
            while (!stop)
            {
                try
                {
                    DataPacket pck = null;
                    lock (packets)
                    {
                        if (packets.Count > 0) pck = packets.Dequeue();
                    }
                    if (pck == null)
                    {
                        Thread.Sleep(50);
                        continue;
                    }
                    // отработка сообщений
                    var toId = pck.ToId;
                    lock (nodes)
                    {
                        if (nodes.ContainsKey(toId))
                        {
                            (new Thread(() =>
                            {
                                nodes[toId].ProcessIncoming(pck);
                            })).Start();
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    stop = true;
                }
                catch (Exception)
                {
                    
                }
            }
        }

        public void RegisterNode(Node node)
        {
            lock (nodes)
            {
                var id = node.Id;
                if (!nodes.ContainsKey(id))
                {
                    nodes[id] = node;
                    SendToAllHello(id);
                }
            }
        }

        protected Queue<DataPacket> packets = new Queue<DataPacket>();

        private void SendToAllHello(string nodeId)
        {
            string[] nodeIds;
            lock (nodes)
            {
                nodeIds = nodes.Keys.ToArray();
            }
            lock (packets)
            {
                foreach (var id in nodeIds)
                {
                    if (id == nodeId) continue;
                    packets.Enqueue(new DataPacket(nodeId, id, id, "hello"));
                }
            }
        }

        public void UnregisterNode(Node node)
        {
            lock (nodes)
            {
                var id = node.Id;
                if (nodes.ContainsKey(id))
                {
                    SendToAllBye(id);
                    nodes.Remove(id);
                }
            }
        }

        private void SendToAllBye(string nodeId)
        {
            string[] nodeIds;
            lock (nodes)
            {
                nodeIds = nodes.Keys.ToArray();
            }
            lock (packets)
            {
                foreach (var id in nodeIds)
                {
                    if (id == nodeId) continue;
                    packets.Enqueue(new DataPacket(nodeId, id, id, "bye"));
                }
            }
        }

        public void SendDataPacket(DataPacket pck)
        {
            throw new NotImplementedException();
        }
    }

    internal class DataPacket
    {
        public string FromId {get; set; }
        public string ToId { get; set; }
        public string TargetId { get; set; }
        public string Subj { get; set; }

        public DataPacket(string fromId, string toId, string targetId, string subj)
        {
            FromId = fromId;
            ToId = toId;
            TargetId = targetId;
            Subj = subj;
        }
    }

    class Node : IDisposable
    {
        private TransportLayer transport;

        public string Id { get; protected set; }

        public Node(string id, TransportLayer transportLayer = null)
        {
            Id = id.Trim().ToUpper();

            thread = new Thread(PacketProcessor);
            thread.IsBackground = true;
            thread.Start();

            transport = transportLayer ?? TransportLayer.GetTransport();
            transport.RegisterNode(this);
        }

        private void PacketProcessor()
        {
            var stop = false;
            while (!stop)
            {
                try
                {
                    DataPacket pck = null;
                    lock (packets)
                    {
                        if (packets.Count > 0) pck = packets.Dequeue();
                    }
                    if (pck == null)
                        Thread.Sleep(50);
                    else if (pck.TargetId == Id)
                        ProcessDataPacket(pck);
                    else
                    {
                        pck.ToId = GetNextId(pck.TargetId) ?? GetDefaultNextId();
                        transport.SendDataPacket(pck);
                    }
                }
                catch (ThreadAbortException)
                {
                    stop = true;
                }
                catch (Exception)
                {
                    
                }
            }
        }

        private string GetDefaultNextId()
        {
            throw new NotImplementedException();
        }

        private string GetNextId(string targetId)
        {
            throw new NotImplementedException();
        }

        private void ProcessDataPacket(DataPacket pck)
        {
            throw new NotImplementedException();
        }

        protected Queue<DataPacket> packets = new Queue<DataPacket>();

        private bool disposed = false;

        private Thread thread;

        public void Dispose()
        {
            if (disposed) return;
            lock (this)
            {
                if (disposed) return;

                transport.UnregisterNode(this);
                transport = null;

                thread.Abort();
                thread.Join(1000);

                disposed = true;
            }
        }

        public void ProcessIncoming(DataPacket pck)
        {
            lock (packets)
            {
                packets.Enqueue(pck);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var transportLayer = TransportLayer.GetTransport();
            var node1 = new Node("N1", transportLayer);
            var node2 = new Node("N2", transportLayer);
        }
    }
}
