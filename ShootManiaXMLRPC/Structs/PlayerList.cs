using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShootManiaXMLRPC.Structs
{
    public struct PlayerList
    {
        public string Login;
        public string Nickname;
        public int PlayerId;
        public int TeamId;
        public int SpectatorStatus;
        public int LadderRanking;
        public int Flags;
    }
}
