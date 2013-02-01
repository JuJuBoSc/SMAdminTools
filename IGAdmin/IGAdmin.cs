using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using SMAdminTools;
using SMAdminTools.Plugins;

namespace IGAdmin
{
	public partial class IGAdmin : Plugin
	{

		private SMAdminTools.Classes.ServerManager ServerManager { get; set; }
		private const string AdminsCfgFile = "IGAdmin__Admins.cfg";
		private List<string> Admins = new List<string>();

		public override string Name {
			get {
				return "IGAdmin";
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
			RefreshAdmins();
		}

		public override void OnServerManagerInitialize (SMAdminTools.Classes.ServerManager ServerManager)
		{

			this.ServerManager = ServerManager;
			this.ServerManager.OnConnectionSuccessful += HandleOnConnectionSuccessful;

		}

		public override void OnConsoleCommand (string Command)
		{

			if (Command == "igadmin reload") {
				RefreshAdmins();
			}

		}

		void HandleOnConnectionSuccessful ()
		{
			this.ServerManager.Server.OnPlayerChat += HandleOnPlayerChat;
			this.ServerManager.Server.OnPlayerConnect += HandleOnPlayerConnect;;
		}

		void HandleOnPlayerConnect (ShootManiaXMLRPC.Structs.PlayerConnect PC)
		{
			if (Admins.Contains (PC.Login)) {
				ChatSendServerMessage("Admin connected : " + PC.Login);
			}
		}

		void HandleOnPlayerChat (ShootManiaXMLRPC.Structs.PlayerChat PC)
		{

			ParseChatCommand(PC);

		}

		private void RefreshAdmins ()
		{

			Console.WriteLine("[IGAdmin] Load admins ...");

			Admins.Clear();

			string currentAssemblyDirectoryName = Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);

			if (File.Exists (currentAssemblyDirectoryName + "/" + AdminsCfgFile)) {

				StreamReader sr = new StreamReader(currentAssemblyDirectoryName + "/" + AdminsCfgFile);

				string line = sr.ReadLine();

				while (line != null)
				{

					line = line.Trim();

					if (line != string.Empty &&
					    !line.StartsWith("#") &&
					    !Admins.Contains(line))
					{
						Console.WriteLine("[IGAdmin] Admin found : " + line);
						Admins.Add(line);
					}

					line = sr.ReadLine();
				}

				sr.Close();

			} else {
				Console.WriteLine("[IGAdmin] Unable to find : " + AdminsCfgFile);
			}

		}

		private void SaveAdmins ()
		{

			string currentAssemblyDirectoryName = Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);

			if (File.Exists (currentAssemblyDirectoryName + "/" + AdminsCfgFile)) {
				try
				{
					File.Delete(currentAssemblyDirectoryName + "/" + AdminsCfgFile);
				}
				catch
				{
					Console.WriteLine("Error on deleting " + AdminsCfgFile + " !");
				}
			}

			StreamWriter sw = new StreamWriter(currentAssemblyDirectoryName + "/" + AdminsCfgFile);

			foreach (string admin in Admins)
				sw.WriteLine(admin);

			sw.Close();

		}

		public void ChatSendServerMessage(String Message)
		{
			ServerManager.Server.ChatSendServerMessage("$08F$o[IGAdmin]$z " + Message);
		}

	}
}

