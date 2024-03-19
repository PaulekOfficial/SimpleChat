using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleChatServer
{
    public class Client
    {
        public Client(Guid uuid, string username)
        {
            Uuid = uuid;
        }

        public Client(Guid uuid, string username, ServerHandler handler)
        {
            this.Uuid = uuid;
            this.Handler = handler;
            this.Username = username;
        }

        public Guid Uuid { get; set; }

        public ServerHandler Handler { get; set; }

        public string Username { get; set; }
    }
}
