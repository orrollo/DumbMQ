using System;
using System.Collections.Generic;
using System.Security.Policy;

namespace Proto01
{
    public class RouterLinkEnd
    {
        public RouterLinkEnd(RouterLink link, Router router, string remoteId)
        {
            Link = link;
            Id = router.Id;
            LinkedRouter = router;
            RemoteId = remoteId;
            router.AddLinkEnd(this);
        }

        public Router LinkedRouter { get; set; }
        public string Id { get; set; }
        public string RemoteId { get; set; }
        public RouterLink Link { get; set; }

        public void PropagateRoutes(Dictionary<string, int> allRoutes)
        {
            if (allRoutes == null) allRoutes = LinkedRouter.GetAllRoutes();
            Link.PropagateRoutesExt(this, allRoutes);
        }

        public void Forward(Message message)
        {
            Link.Forward(message, RemoteId);
        }

        public void RegisterProcessMessage(Action<Message> processMessage)
        {
            Link.RegisterProcessMessage(Id, processMessage);
        }

        public void UpdateRoutes(Dictionary<string, int> allRoutes, bool skipPropagate = false)
        {
            LinkedRouter.UpdateRoutes(RemoteId, allRoutes, skipPropagate);
        }
    }

    public class RouterLink
    {
        internal RouterLinkEnd left;
        internal RouterLinkEnd right;

        public RouterLink(Router router1, Router router2)
        {
            //Router1 = router1;
            //Router2 = router2;
            ////
            left = new RouterLinkEnd(this, router1, router2.Id);
            right = new RouterLinkEnd(this, router2, router1.Id);
            //Router1.AddLinkEnd(left);
            //Router2.AddLinkEnd(right);
            ////
            //Router1.AddLink(Router2.Id, this);
            //Router2.AddLink(Router1.Id, this);
            //
            left.UpdateRoutes(router2.GetAllRoutes(), true);
            right.UpdateRoutes(router1.GetAllRoutes(), true);

            //Router1.UpdateRoutes(Router2.Id, Router2.GetAllRoutes(), true);
            //Router2.UpdateRoutes(Router1.Id, Router1.GetAllRoutes(), true);
            //// propagate to all sides
            //Router1.PropagateRoutes("");
            //Router2.PropagateRoutes("");

            left.PropagateRoutes(null);
            right.PropagateRoutes(null);
        }

        //public Router Router2 { get; set; }
        //public Router Router1 { get; set; }

        //public void PropagateRoutes(string fromId)
        //{
        //    if (Router1.Id == fromId) Router2.UpdateRoutes(Router1.Id, Router1.GetAllRoutes());
        //    if (Router2.Id == fromId) Router1.UpdateRoutes(Router2.Id, Router2.GetAllRoutes());
        //}

        Dictionary<string,Action<Message>> messageProcessors = new Dictionary<string, Action<Message>>();

        internal void Forward(Message message, string nextId)
        {
            if (messageProcessors.ContainsKey(nextId)) messageProcessors[nextId](message);
        }

        public void RegisterProcessMessage(string id, Action<Message> processMessage)
        {
            messageProcessors[id] = processMessage;
        }

        public void PropagateRoutesExt(RouterLinkEnd linkEnd, Dictionary<string, int> allRoutes)
        {
            if (linkEnd == left)
            {
                right.UpdateRoutes(allRoutes);
            }
            else if (linkEnd == right)
            {
                left.UpdateRoutes(allRoutes);
            }
        }
    }
}