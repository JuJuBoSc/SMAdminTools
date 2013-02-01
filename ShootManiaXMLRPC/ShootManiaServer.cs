using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Text;
using ShootManiaXMLRPC.XmlRpc;

namespace ShootManiaXMLRPC
{
    public class ShootManiaServer
    {

		private object m_lockCallback = new object();
        private delegate void SimpleDelegate();

        public delegate void OnPlayerChatEventHandler(Structs.PlayerChat PC);
        public event OnPlayerChatEventHandler OnPlayerChat;

        public delegate void OnPlayerDisconnectEventHandler(Structs.PlayerDisconnect PC);
        public event OnPlayerDisconnectEventHandler OnPlayerDisconnect;

        public delegate void OnPlayerConnectEventHandler(Structs.PlayerConnect PC);
        public event OnPlayerConnectEventHandler OnPlayerConnect;

        public delegate void OnVoteUpdatedEventHandler(Structs.VoteUpdated VU);
        public event OnVoteUpdatedEventHandler OnVoteUpdated;

        public delegate void ModeScriptCallbackEventHandler(Structs.ModeScriptCallback MSC);
        public event ModeScriptCallbackEventHandler OnModeScriptCallback;

        public XmlRpc.XmlRpcClient Client { get; private set; }

        public ShootManiaServer(String IP, Int32 Port)
        {
            Client = new XmlRpc.XmlRpcClient(IP, Port);
            Client.EventGbxCallback += new XmlRpc.GbxCallbackHandler(Client_EventGbxCallback);
        }

        void Client_EventGbxCallback (object o, XmlRpc.GbxCallbackEventArgs e)
		{

			lock (m_lockCallback) {
				ParseEventGbx(e);
			}

			return;
            SimpleDelegate d = delegate()
            {
                ParseEventGbx(e);
            };

            ThreadStart threadDelegate = new ThreadStart(d);

            new System.Threading.Thread(threadDelegate).Start();

        }

        private void ParseEventGbx(XmlRpc.GbxCallbackEventArgs e)
        {
            if (e.Response.MethodName == "ManiaPlanet.PlayerChat" &&
                e.Response.Params.Count == 4)
            {
                try
                {

                    Structs.PlayerChat PC = new Structs.PlayerChat();
                    PC.PlayerUid = (int)e.Response.Params[0];
                    PC.Login = (string)e.Response.Params[1];
                    PC.Text = (string)e.Response.Params[2];
                    PC.IsRegisteredCmd = (bool)e.Response.Params[3];

                    if (OnPlayerChat != null)
                        OnPlayerChat(PC);

                }
                catch { }
            }
            else if (e.Response.MethodName == "ManiaPlanet.PlayerConnect" &&
                    e.Response.Params.Count == 2)
            {
                try
                {

                    Structs.PlayerConnect PC = new Structs.PlayerConnect();
                    PC.Login = (string)e.Response.Params[0];
                    PC.IsSpectator = (bool)e.Response.Params[1];

                    if (OnPlayerConnect != null)
                        OnPlayerConnect(PC);

                }
                catch { }
            }
            else if (e.Response.MethodName == "ManiaPlanet.PlayerDisconnect" &&
                    e.Response.Params.Count == 1)
            {
                try
                {

                    Structs.PlayerDisconnect PD = new Structs.PlayerDisconnect();
                    PD.Login = (string)e.Response.Params[0];

                    if (OnPlayerDisconnect != null)
                        OnPlayerDisconnect(PD);

                }
                catch { }
            }
            else if (e.Response.MethodName == "ManiaPlanet.VoteUpdated" &&
                    e.Response.Params.Count == 4)
            {
                try
                {

                    Structs.VoteUpdated VU = new Structs.VoteUpdated();
                    VU.StateName = (string)e.Response.Params[0];
                    VU.Login = (string)e.Response.Params[1];
                    VU.CmdName = (string)e.Response.Params[2];
                    VU.CmdParam = (string)e.Response.Params[3];

                    if (OnVoteUpdated != null)
                        OnVoteUpdated(VU);

                }
                catch { }
            }
            else if (e.Response.MethodName == "ManiaPlanet.ModeScriptCallback" &&
                    e.Response.Params.Count == 2)
            {
                try
                {

                    Structs.ModeScriptCallback MSC = new Structs.ModeScriptCallback();
                    MSC.Param1 = (string)e.Response.Params[0];
                    MSC.Param2 = (string)e.Response.Params[1];

                    if (OnModeScriptCallback != null)
                        OnModeScriptCallback(MSC);

                }
                catch { }
            }
            else
            {
                // Unhandled callback
            }
        }

        public int Connect()
        {
            return Client.Connect();
        }

        public void Disconnect()
        {
            try
            {
                Client.Disconnect();
            }
            catch { }
        }

        public bool IsConnected
        {
            get
            {
                return Client.IsConnected;
            }
        }

        public void EnableCallback()
        {
            Client.EnableCallbacks(true);
        }

        public void DisableCallback()
        {
            Client.EnableCallbacks(false);
        }

        public Boolean Authenticate(String Username, String Password)
        {

            GbxCall request = Client.Request("Authenticate", new object[] { Username, Password });

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1)
            {
                return (bool)response.Params[0];
            }
            else
            {
                return false;
            }

        }

        public void ChatSend(String Message)
        {

            GbxCall request = Client.Request("ChatSend", new object[] { Message });

        }

        public void ChatSendServerMessage(String Message)
        {

            GbxCall request = Client.Request("ChatSendServerMessage", new object[] { Message });

        }

        public void SetServerPassword(String Password)
        {

            GbxCall request = Client.Request("SetServerPassword", new object[] { Password });

        }

        public String GetServerPassword()
        {

            GbxCall request = Client.Request("GetServerPassword", new object[] { });

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1)
            {
                return response.Params[0].ToString();
            }
            else
            {
                return String.Empty;
            }

        }

        public void SetServerPasswordForSpectator(String Password)
        {

            GbxCall request = Client.Request("SetServerPasswordForSpectator", new object[] { Password });

        }

        public String GetServerPasswordForSpectator()
        {

            GbxCall request = Client.Request("GetServerPasswordForSpectator", new object[] { });

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1)
            {
                return response.Params[0].ToString();
            }
            else
            {
                return String.Empty;
            }

        }

        public void SetServerComment(String Comment)
        {

            GbxCall request = Client.Request("SetServerComment", new object[] { Comment });

        }

        public String GetServerComment()
        {

            GbxCall request = Client.Request("GetServerComment", new object[] { });

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1)
            {
                return response.Params[0].ToString();
            }
            else
            {
                return String.Empty;
            }

        }

        public void SetServerName(String Comment)
        {

            GbxCall request = Client.Request("SetServerName", new object[] { Comment });

        }

        public void NextMap(Boolean DontClearCupScores)
        {

            GbxCall request = Client.Request("NextMap", new object[] { DontClearCupScores });

        }

        public void RestartMap(Boolean DontClearCupScores)
        {

            GbxCall request = Client.Request("RestartMap", new object[] { DontClearCupScores });

        }

        public String GetServerName()
        {

            GbxCall request = Client.Request("GetServerName", new object[] { });

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1)
            {
                return response.Params[0].ToString();
            }
            else
            {
                return String.Empty;
            }

        }

        public void SetMaxPlayers(Int32 MaxPlayers)
        {

            GbxCall request = Client.Request("SetMaxPlayers", new object[] { MaxPlayers });

        }

        public Structs.MaxPlayers GetMaxPlayers()
        {

            GbxCall request = Client.Request("GetMaxPlayers", new object[] { });

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1 &&
                response.Params[0].GetType() == typeof(Hashtable))
            {
                Hashtable ht = (Hashtable)response.Params[0];
                Structs.MaxPlayers mp = new Structs.MaxPlayers();
                mp.CurrentValue = (int)ht["CurrentValue"];
                mp.NextValue = (int)ht["NextValue"];
                return mp;
            }
            else
            {
                return new Structs.MaxPlayers();
            }

        }

        public void SetMaxSpectators(Int32 MaxSpectators)
        {

            GbxCall request = Client.Request("SetMaxSpectators", new object[] { MaxSpectators });

        }

        public Structs.MaxSpectators GetMaxSpectators()
        {

            GbxCall request = Client.Request("GetMaxSpectators", new object[] { });

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1 &&
                response.Params[0].GetType() == typeof(Hashtable))
            {
                Hashtable ht = (Hashtable)response.Params[0];
                Structs.MaxSpectators ms = new Structs.MaxSpectators();
                ms.CurrentValue = (int)ht["CurrentValue"];
                ms.NextValue = (int)ht["NextValue"];
                return ms;
            }
            else
            {
                return new Structs.MaxSpectators();
            }

        }

        public List<String> GetChatLines()
        {

            List<String> Result = new List<String>();

            GbxCall request = Client.Request("GetChatLines", new object[] { });

            GbxCall response = Client.GetResponse(request.Handle);

            foreach (object o in response.Params)
            {
                if (o.GetType() == typeof(ArrayList))
                {
                    ArrayList Lines = (ArrayList)o;
                    foreach (var line in Lines)
                    {
                        Result.Add(line.ToString());
                    }
                }
            }

            return Result;

        }

        public List<Structs.MapList> GetMapList(int MaxResults, int StartIndex)
        {

            List<Structs.MapList> Result = new List<Structs.MapList>();

            GbxCall request = Client.Request("GetMapList", new object[] { MaxResults, StartIndex });

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1 &&
                response.Params[0].GetType() == typeof(ArrayList))
            {
                foreach (Hashtable ht in (ArrayList)response.Params[0])
                {
                    Structs.MapList ml = new Structs.MapList();
                    ml.Author = (string)ht["Author"];
                    ml.Environnement = (string)ht["Environnement"];
                    ml.FileName = (string)ht["FileName"];
                    ml.MapStyle = (string)ht["MapStyle"];
                    ml.Name = (string)ht["Name"];
                    ml.UId = (string)ht["UId"];
                    ml.GoldTime = (int)ht["GoldTime"];
                    ml.CopperPrice = (int)ht["CopperPrice"];
                    Result.Add(ml);
                }
            }

            return Result;

        }

        public List<Structs.PlayerList> GetPlayerList(int MaxResults, int StartIndex)
        {

            List<Structs.PlayerList> Result = new List<Structs.PlayerList>();

            GbxCall request = Client.Request("GetPlayerList", new object[] { MaxResults, StartIndex });

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1 &&
                response.Params[0].GetType() == typeof(ArrayList))
            {
                foreach (Hashtable ht in (ArrayList)response.Params[0])
                {
                    Structs.PlayerList pl = new Structs.PlayerList();
                    pl.SpectatorStatus = (int)ht["SpectatorStatus"];
                    pl.Flags = (int)ht["Flags"];
                    pl.LadderRanking = (int)ht["LadderRanking"];
                    pl.PlayerId = (int)ht["PlayerId"];
                    pl.TeamId = (int)ht["TeamId"];
                    pl.Login = (string)ht["Login"];
                    pl.Nickname = (string)ht["NickName"];
                    Result.Add(pl);
                }
            }

            return Result;

        }

        public void KickId(Int32 PlayerId, String Message)
        {

            GbxCall request = Client.Request("KickId", new object[] { PlayerId, Message });

        }

        public void BanId(Int32 PlayerId, String Message)
        {

            GbxCall request = Client.Request("BanId", new object[] { PlayerId, Message });

        }

        public void ChooseNextMap(String Filename)
        {

            GbxCall request = Client.Request("ChooseNextMap", new object[] { Filename });

        }

        public void SetApiVersion(String Version)
        {

            GbxCall request = Client.Request("SetApiVersion", new object[] { Version });

        }

        public void UnBan(String ClientName)
        {

            GbxCall request = Client.Request("UnBan", new object[] { ClientName });

        }

        public ShootManiaXMLRPC.Structs.ServerStatus GetStatus()
        {


            GbxCall request = Client.Request("GetStatus", new object[] {  });

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1 &&
                response.Params[0].GetType() == typeof(Hashtable))
            {
                Hashtable ht = (Hashtable)response.Params[0];
                Structs.ServerStatus ss = new Structs.ServerStatus();
                ss.Code = (int)ht["Code"];
                ss.Name = (string)ht["Name"];
                return ss;
            }

            return new Structs.ServerStatus();

        }



        public List<Structs.BanList> GetBanList(int MaxResults, int StartIndex)
        {

            List<Structs.BanList> Result = new List<Structs.BanList>();

            GbxCall request = Client.Request("GetBanList", new object[] { MaxResults, StartIndex });

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1 &&
                response.Params[0].GetType() == typeof(ArrayList))
            {
                foreach (Hashtable ht in (ArrayList)response.Params[0])
                {
                    Structs.BanList bl = new Structs.BanList();
                    bl.Login = (string)ht["Login"];
                    bl.ClientName = (string)ht["ClientName"];
                    bl.IPAddress = (string)ht["IPAddress"];
                    Result.Add(bl);
                }
            }

            return Result;

        }

        public ShootManiaXMLRPC.Structs.ScriptName GetScriptName()
        {


            GbxCall request = Client.Request("GetScriptName", new object[] { });

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1 &&
                response.Params[0].GetType() == typeof(Hashtable))
            {
                Hashtable ht = (Hashtable)response.Params[0];
                Structs.ScriptName sn = new Structs.ScriptName();
                sn.CurrentValue = (string)ht["CurrentValue"];
                sn.NextValue = (string)ht["NextValue"];
                return sn;
            }

            return new Structs.ScriptName();

        }

        public void SetScriptName(String ScriptName)
        {

            GbxCall request = Client.Request("SetScriptName", new object[] { ScriptName });

        }

        public ShootManiaXMLRPC.Structs.CurrentMapInfo GetCurrentMapInfo()
        {


            GbxCall request = Client.Request("GetCurrentMapInfo", new object[] { });

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1 &&
                response.Params[0].GetType() == typeof(Hashtable))
            {
                Hashtable ht = (Hashtable)response.Params[0];
                Structs.CurrentMapInfo cmi = new Structs.CurrentMapInfo();
                cmi.Author = (string)ht["Author"];
                cmi.Environnement = (string)ht["Environnement"];
                cmi.FileName = (string)ht["FileName"];
                cmi.MapStyle = (string)ht["MapStyle"];
                cmi.Name = (string)ht["Name"];
                cmi.UId = (string)ht["UId"];
                cmi.GoldTime = (int)ht["GoldTime"];
                cmi.CopperPrice = (int)ht["CopperPrice"];
                return cmi;
            }

            return new Structs.CurrentMapInfo();

        }

        public void SendNoticeToLogin(String Login, String Text)
        {

            GbxCall request = Client.Request("SendNoticeToLogin", new object[] { Login, Text, "''" });

        }

        public void ChatSendToLogin(String Login, String Text)
        {

            GbxCall request = Client.Request("ChatSendToLogin", new object[] { Login, Text, "''" });

        }

		public Structs.PlayerList GetPlayerListByPlayerLogin(string Login)
		{

			foreach (var player in GetPlayerList(100, 0))
				if (player.Login == Login)
					return player;

			return new ShootManiaXMLRPC.Structs.PlayerList();

		}

    }
}
