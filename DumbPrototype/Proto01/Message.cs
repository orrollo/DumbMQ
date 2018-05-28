using System;

namespace Proto01
{
    public class Message
    {
        public string Id { get; set; }
        public string CorellationId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Label { get; set; }
        public byte[] Body { get; set; }
        public int TTL { get; set; }

        public Router LinkedRouter { get; set; }

        public Message()
        {
            Id = Guid.NewGuid().ToString("N");
            CorellationId = string.Empty;
        }

        public void Send()
        {
            if (LinkedRouter == null) throw new ArgumentException();
            LinkedRouter.ProcessMessage(this);
        }

        public bool StepToDie()
        {
            if (TTL <= 0) return false;
            TTL--;
            return TTL == 0;
        }

        public Message Response()
        {
            return new Message()
            {
                To = this.From,
                From = this.To,
                LinkedRouter = this.LinkedRouter
            };
        }
    }
}