using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShootManiaXMLRPC.Structs
{
    public struct PlayerChat
    {
        public int PlayerUid;
        public string Login;
        public string Text;
        public bool IsRegisteredCmd;
    }
}
