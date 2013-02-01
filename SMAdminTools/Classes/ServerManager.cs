using System;
using System.Threading;
using ShootManiaXMLRPC;

namespace SMAdminTools.Classes
{
	public class ServerManager
	{
		
		public Config Config { get; private set; }
		public ShootManiaServer Server { get; private set; }
		private Thread Thread_HandleConnection;

        public delegate void OnConnectionSuccessfulEventHandler();
        public event OnConnectionSuccessfulEventHandler OnConnectionSuccessful;

		public ServerManager (Config Config)
		{
			this.Config = Config;
			this.Server = new ShootManiaServer(this.Config.ShootMania__IP, this.Config.ShootMania__XML_RPC_Port);
		}

		private void HandleConnection()
		{
			while (true)
			{

				if (!Server.IsConnected)
				{

					this.Server = new ShootManiaServer(this.Config.ShootMania__IP, this.Config.ShootMania__XML_RPC_Port);

					if (Server.Connect() == 0)
					{

						Console.WriteLine("Connected to server !");
						Console.WriteLine("Authentication ...");

						if (Server.Authenticate(Config.ShootMania__SuperAdmin_Login, Config.ShootMania__SuperAdmin_Password))
						{

							Console.WriteLine("Authentication success !");

							Console.WriteLine("Set API version : " + Settings.ShootManiaApiVersion + " ...");
							Server.SetApiVersion(Settings.ShootManiaApiVersion);
							Console.WriteLine("Ok ...");

							Console.WriteLine("Enable callbacks ...");
							Server.EnableCallback();
							Console.WriteLine("Ok ...");
							
							Console.WriteLine("Register events ...");
							Server.Client.EventGbxCallback += HandleEventGbxCallback;
							Server.OnPlayerConnect += HandleOnPlayerConnect;
							Server.OnPlayerDisconnect += HandleOnPlayerDisconnect;
							Console.WriteLine("Ok ...");

							Console.WriteLine("Calling OnConnectionSuccessful ...");

							if (OnConnectionSuccessful != null)
								OnConnectionSuccessful();

							Console.WriteLine("Ok ...");

							Server.ChatSendServerMessage("$08F$o[SMAdminTools]$z Connected !");

							Console.WriteLine("Everythings is up and running !");

						}
						else
						{
							Console.WriteLine("Authentication failed ...");
							Server.Disconnect();
						}

					}
				    else
				    {
						Console.WriteLine("Unable to connect to server " + Config.ShootMania__IP + ":" + Config.ShootMania__XML_RPC_Port + " ...");
					}

				}

				if (!Server.IsConnected)
					Console.WriteLine("Retry in " + Config.ShootMania__ReconnectTimeout + "ms ...");

				Thread.Sleep(Config.ShootMania__ReconnectTimeout);
			}
		}

		void HandleEventGbxCallback (object o, ShootManiaXMLRPC.XmlRpc.GbxCallbackEventArgs e)
		{
			if (Config.SMAdminTools__Debug) {
				Console.WriteLine ("[DEBUG] Callback received : " + e.Response.MethodName + " (Params : " + e.Response.Params.Count + ")");
			}

			if (e.Response.MethodName == "ManiaPlanet.ModeScriptCallback") {
				//Console.WriteLine(e.Response.Params[0].ToString() + " - " + e.Response.Params[1].ToString());
			}

		}

		void HandleOnPlayerDisconnect (ShootManiaXMLRPC.Structs.PlayerDisconnect PC)
		{
			Console.WriteLine("Player [" + PC.Login + "] disconnected");
		}

		void HandleOnPlayerConnect (ShootManiaXMLRPC.Structs.PlayerConnect PC)
		{

			string PlayerConnectText = "Player [" + PC.Login + "] connected";

			if (PC.IsSpectator) {
				PlayerConnectText += " (Spectator)";
			}

			Console.WriteLine(PlayerConnectText);

			Server.SendNoticeToLogin(PC.Login, "$08FThis server is running SMAdminTools " + Settings.Version);

		}

		public void Initialize ()
		{

			foreach (Plugins.Plugin plugin in Plugins.Manager.LoadedPlugins) {
				plugin.OnServerManagerInitialize(this);
			}

			Thread_HandleConnection = new Thread(HandleConnection);
			Thread_HandleConnection.IsBackground = true;
			Thread_HandleConnection.Start();

		}

	}
}

