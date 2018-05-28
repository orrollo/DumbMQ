using System;
using System.Collections.Generic;
using System.Security.Policy;

namespace Proto01
{
    public class RouterLinkEnd
    {
        public RouterLinkEnd(RouterLink link, string id, string remoteId)
        {
            Link = link;
            RemoteId = remoteId;
        }

        public string RemoteId { get; set; }
        public RouterLink Link { get; set; }
    }

    public class RouterLink
    {
        internal RouterLinkEnd left;
        internal RouterLinkEnd right;

        public RouterLink(Router router1, Router router2)
        {
            Router1 = router1;
            Router2 = router2;
            //

            //
            Router1.AddLink(Router2.Id, this);
            Router2.AddLink(Router1.Id, this);
            //
            SyncRoutes();
        }

        public void SyncRoutes()
        {
            Router1.UpdateRoutes(Router2.Id, Router2.GetAllRoutes(), true);
            Router2.UpdateRoutes(Router1.Id, Router1.GetAllRoutes(), true);
            // propagate to all sides
            Router1.PropagateRoutes("");
            Router2.PropagateRoutes("");
        }

        public Router Router2 { get; set; }
        public Router Router1 { get; set; }

        public void PropagateRoutes(string fromId)
        {
            if (Router1.Id == fromId) Router2.UpdateRoutes(Router1.Id, Router1.GetAllRoutes());
            if (Router2.Id == fromId) Router1.UpdateRoutes(Router2.Id, Router2.GetAllRoutes());
        }

        Dictionary<string,Action<Message>> messageProcessors = new Dictionary<string, Action<Message>>();

        public void Forward(Message message, string nextId)
        {
            if (messageProcessors.ContainsKey(nextId)) messageProcessors[nextId](message);
        }

        public void RegisterProcessMessage(string id, Action<Message> processMessage)
        {
            messageProcessors[id] = processMessage;
        }
    }
}