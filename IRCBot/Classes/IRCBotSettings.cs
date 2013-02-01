using System;

namespace IRCBot.Classes
{
	public class IRCBotSettings
	{

		public string IRC_Server = "irc.quakenet.org";
		public int IRC_Port = 6667;
		public string IRC_Channel = "#SMAdminTools";
		public string IRC_Nickname = "SM|IRCBot";
		public int ShootMania_IRCChatToGame = 1;
		public int ShootMania_GameChatToIRC = 1;
		public int ShootMania_SayPlayerConnected = 1;
		public int ShootMania_SayPlayerDisconnected = 1;
		public int ShootMania_SayHit = 1;
		public int ShootMania_SayFrag = 1;
		public int ShootMania_SayBeginMap = 1;
		public int ShootMania_SayEndMap = 1;
		public int ShootMania_SayBeginRound = 1;
		public int ShootMania_SayEndRound = 1;

				public Boolean ParseFromIniFile (string fileName)
		{

			SMAdminTools.Classes.IniFile ini = new SMAdminTools.Classes.IniFile (fileName);

			this.IRC_Server = ini.GetValue ("IRC", "Server", string.Empty);

			if (this.IRC_Server == string.Empty) {
				Console.WriteLine ("[IRCBot] Invalid IRC Server !");
				return false;
			}

			this.IRC_Channel = ini.GetValue ("IRC", "Channel", string.Empty);

			if (this.IRC_Channel == string.Empty) {
				Console.WriteLine ("[IRCBot] Invalid IRC Channel !");
				return false;
			}

			this.IRC_Nickname = ini.GetValue ("IRC", "Nickname", string.Empty);

			if (this.IRC_Nickname == string.Empty) {
				Console.WriteLine ("[IRCBot] Invalid IRC Nickname !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("IRC", "Port", string.Empty), out this.IRC_Port)) {
				Console.WriteLine ("[IRCBot] Invalid IRC Port !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("ShootMania", "GameChatToIRC", "1"), out this.ShootMania_GameChatToIRC)) {
				Console.WriteLine ("[IRCBot] Invalid ShootMania GameChatToIRC !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("ShootMania", "IRCChatToGame", "1"), out this.ShootMania_IRCChatToGame)) {
				Console.WriteLine ("[IRCBot] Invalid ShootMania IRCChatToGame !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("ShootMania", "SayPlayerConnected", "1"), out this.ShootMania_SayPlayerConnected)) {
				Console.WriteLine ("[IRCBot] Invalid ShootMania SayPlayerConnected !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("ShootMania", "SayPlayerDisconnected", "1"), out this.ShootMania_SayPlayerDisconnected)) {
				Console.WriteLine ("[IRCBot] Invalid ShootMania SayPlayerDisconnected !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("ShootMania", "SayHit", "1"), out this.ShootMania_SayHit)) {
				Console.WriteLine ("[IRCBot] Invalid ShootMania SayHit !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("ShootMania", "SayFrag", "1"), out this.ShootMania_SayFrag)) {
				Console.WriteLine ("[IRCBot] Invalid ShootMania SayFrag !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("ShootMania", "SayBeginMap", "1"), out this.ShootMania_SayBeginMap)) {
				Console.WriteLine ("[IRCBot] Invalid ShootMania SayBeginMap !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("ShootMania", "SayEndMap", "1"), out this.ShootMania_SayEndMap)) {
				Console.WriteLine ("[IRCBot] Invalid ShootMania SayEndMap !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("ShootMania", "SayBeginRound", "1"), out this.ShootMania_SayBeginRound)) {
				Console.WriteLine ("[IRCBot] Invalid ShootMania SayBeginRound !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("ShootMania", "SayEndRound", "1"), out this.ShootMania_SayEndRound)) {
				Console.WriteLine ("[IRCBot] Invalid ShootMania SayEndRound !");
				return false;
			}
		
			return true;

		}

	}
}

