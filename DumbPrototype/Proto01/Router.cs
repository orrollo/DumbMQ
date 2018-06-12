using System;
using System.Collections.Generic;
using System.Linq;

namespace Proto01
{
    public class Router
    {
        protected object locker = new object();

        public Router(string id)
        {
            Id = id;
        }

        public Router() : this(Guid.NewGuid().ToString("N"))
        {
            
        }

        public string Id { get; set; }

        //internal Dictionary<string,RouterLink> links = new Dictionary<string, RouterLink>();

        internal Dictionary<string, RouterLinkEnd> linkEnds = new Dictionary<string, RouterLinkEnd>();
        internal Dictionary<string, RouteInfo> routes = new Dictionary<string, RouteInfo>();

        //public void AddLink(string targetId, RouterLink routerLink)
        //{
        //    lock (locker)
        //    {
        //        links[targetId] = routerLink;
        //        routerLink.RegisterProcessMessage(Id, ProcessMessage);
        //        UpdateRoute(targetId, targetId, 1);
        //    }
        //}

        public bool UpdateRoute(string targetId, string nextId, int length)
        {
            if (targetId == Id) return false;
            lock (locker)
            {
                var doChange = !routes.ContainsKey(targetId) || (routes[targetId].Length > length);
                if (doChange) routes[targetId] = new RouteInfo(targetId, nextId, length);
                return doChange;
            }
        }

        public bool IsRoutesChanged { get; set; }

        public void UpdateRoutes(string nextId, Dictionary<string,int> routeTable, bool skipPropagate = false)
        {
            lock (locker)
            {
                var isChanged = false;
                foreach (var route in routeTable) isChanged |= UpdateRoute(route.Key, nextId, route.Value + 1);
                if (isChanged && (!skipPropagate)) PropagateRoutes(nextId);
            }
        }

        public void PropagateRoutes(string skipId)
        {
            lock (locker)
            {
                var allRoutes = GetAllRoutes();
                foreach (var link in linkEnds)
                {
                    if (link.Key == skipId) continue;
                    link.Value.PropagateRoutes(allRoutes);
                }
            }
        }

        public void ProcessMessage(Message message)
        {
            lock (locker)
            {
                message.LinkedRouter = this;
                var to = message.To;
                if (to == Id)
                {
                    AddToInbox(message);
                }
                else if (!routes.ContainsKey(message.To))
                {
                    if (!message.StepToDie()) AddToDelayed(message);
                }
                else if (!message.StepToDie())
                {
                    var nextId = routes[message.To].NextId;
                    linkEnds[nextId].Forward(message);
                }
            }
        }

        public Message BuildMessage(string to)
        {
            return new Message()
            {
                From = this.Id,
                To = to,
                LinkedRouter = this
            };
        }

        internal Queue<Message> Inbox = new Queue<Message>();
        internal Queue<Message> Delayed = new Queue<Message>();

        private void AddToDelayed(Message message)
        {
            lock (locker)
            {
                Delayed.Enqueue(message);
            }
        }

        public void TryProcessDelayed()
        {
            lock (locker)
            {
                var list = Delayed.ToList();
                Delayed.Clear();
                foreach (var message in list) ProcessMessage(message);
            }
        }

        private void AddToInbox(Message message)
        {
            lock (locker)
            {
                Inbox.Enqueue(message);
            }
        }

        public Dictionary<string, int> GetAllRoutes()
        {
            lock (locker)
            {
                return routes.ToDictionary(info => info.Key, info => info.Value.Length);
            }
        }

        public void AddLinkEnd(RouterLinkEnd linkEnd)
        {
            lock (locker)
            {
                linkEnds[linkEnd.RemoteId] = linkEnd;
                linkEnd.RegisterProcessMessage(ProcessMessage);
                UpdateRoute(linkEnd.RemoteId, linkEnd.RemoteId, 1);
            }
        }
    }
}