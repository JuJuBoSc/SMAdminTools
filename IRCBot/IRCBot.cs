using System;
using System.Threading;
using System.IO;
using System.Reflection;
using SMAdminTools;
using SMAdminTools.Plugins;
using Meebey.SmartIrc4net;

namespace IRCBot
{
	public partial class IRCBot : Plugin
	{

		private const string IRCBotSettingsFile = "IRCBot__Settings.ini";

		private Classes.IRCBotSettings IRCBotSettings = new Classes.IRCBotSettings();
		private IrcClient irc = new IrcClient();
		private bool IsLoaded = false;
		private Thread Thread_IrcListen;
		private SMAdminTools.Classes.ServerManager Server;

		public override string Name {
			get {
				return "IRCBot";
			}
		}

		public override string Author {
			get {
				return "JuJuBoSc";
			}
		}

		public override string Version {
			get {
				return "0.1";
			}
		}

		public override void OnLoad ()
		{

			string currentAssemblyDirectoryName = Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);

			if (File.Exists (currentAssemblyDirectoryName + "/" + IRCBotSettingsFile)) {
				if (!IRCBotSettings.ParseFromIniFile(currentAssemblyDirectoryName + "/" + IRCBotSettingsFile))
				    {
						Console.WriteLine("[IRCBot] Unable to load settings !");
					return;
				}
			} else {
				Console.WriteLine("[IRCBot] " + IRCBotSettingsFile + " not found !");
			}

			irc.Encoding = System.Text.Encoding.UTF8;
			irc.SendDelay = 200;
			irc.ActiveChannelSyncing = true;
			irc.AutoReconnect = true;
			irc.AutoRejoin = true;
			irc.AutoRetry = true;

			irc.OnRawMessage += HandleOnRawMessage;
			irc.OnChannelMessage += HandleOnChannelMessage;

			Console.WriteLine("[IRCBot] Connecting to IRC server ...");

			try {
				irc.Connect (IRCBotSettings.IRC_Server, IRCBotSettings.IRC_Port);
				Console.WriteLine("[IRCBot] Connected to IRC server !");
			} catch {
				Console.WriteLine("[IRCBot] Cannot connect to IRC server !");
				return;
			}


			irc.Login(IRCBotSettings.IRC_Nickname, "SMAdminTools IRC Plugin", 0, "SMAdminTools");
			irc.RfcJoin(IRCBotSettings.IRC_Channel);

			Thread_IrcListen = new Thread(irc.Listen);
			Thread_IrcListen.IsBackground = true;
			Thread_IrcListen.Start();

			IsLoaded = true;

		}

		void HandleOnChannelMessage (object sender, IrcEventArgs e)
		{
			if (e.Data.Channel == IRCBotSettings.IRC_Channel &&
			    IRCBotSettings.ShootMania_IRCChatToGame == 1 &&
			    Server != null) {
				Server.Server.ChatSendServerMessage("$08F$o[IRC]$z <" + e.Data.Nick + "> " + e.Data.Message);
			}
		}

		void HandleOnRawMessage (object sender, IrcEventArgs e)
		{

		}

		public override void OnServerManagerInitialize (SMAdminTools.Classes.ServerManager ServerManager)
		{
			if (IsLoaded) {

				this.Server = ServerManager;
				this.Server.OnConnectionSuccessful += HandleOnConnectionSuccessful;

			}
		}

		void HandleOnPlayerConnect (ShootManiaXMLRPC.Structs.PlayerConnect PC)
		{
			if (IRCBotSettings.ShootMania_SayPlayerConnected == 1) {
				irc.SendMessage(SendType.Message, IRCBotSettings.IRC_Channel, "[ShootMania] Player connected : " + PC.Login);
			}
		}

		void HandleOnPlayerDisconnect (ShootManiaXMLRPC.Structs.PlayerDisconnect PC)
		{
			if (IRCBotSettings.ShootMania_SayPlayerDisconnected == 1) {
				irc.SendMessage(SendType.Message, IRCBotSettings.IRC_Channel, "[ShootMania] Player disconnected : " + PC.Login);
			}
		}

		void HandleOnConnectionSuccessful ()
		{
			this.Server.Server.OnPlayerConnect += HandleOnPlayerConnect;
			this.Server.Server.OnPlayerDisconnect += HandleOnPlayerDisconnect;
			this.Server.Server.OnPlayerChat += HandleOnPlayerChat;
			this.Server.Server.OnModeScriptCallback += HandleOnModeScriptCallback;
			irc.SendMessage(SendType.Message, IRCBotSettings.IRC_Channel, "Connected to ShootMania server !");
		}

		void HandleOnPlayerChat (ShootManiaXMLRPC.Structs.PlayerChat PC)
		{
			if (IRCBotSettings.ShootMania_GameChatToIRC == 1)
			{
				foreach (var player in Server.Server.GetPlayerList(100, 0))
				{
					if (player.Login == PC.Login &&
					    player.TeamId >= 0)
					{
						irc.SendMessage(SendType.Message, IRCBotSettings.IRC_Channel, "[ShootMania] [Chat] <" + SMAdminTools.Functions.Mania.StripNadeoColours(player.Nickname) + "> " + SMAdminTools.Functions.Mania.StripNadeoColours(PC.Text));
					}
				}
			}
		}

		public override void OnConsoleCommand (string Command)
		{
			if (IsLoaded) {

			}
		}

	}
}

