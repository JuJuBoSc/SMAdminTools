using System;
using Meebey.SmartIrc4net;

namespace IRCBot
{
	partial class IRCBot
	{

		void HandleOnModeScriptCallback (ShootManiaXMLRPC.Structs.ModeScriptCallback MSC)
		{

			if (MSC.Param1 == "startRound" &&
				IRCBotSettings.ShootMania_SayBeginRound == 1) {

				irc.SendMessage(SendType.Message, IRCBotSettings.IRC_Channel, "[ShootMania] Round " + MSC.Param2 + " started !");

			}
			else if (MSC.Param1 == "endRound" &&
				IRCBotSettings.ShootMania_SayEndRound == 1) {

				string[] Params = MSC.Param2.Split(';');

				if (Params.Length == 3)
				{
					
					string Unk1 = Params[0];
					string Winner = Params[1];
					string Unk2 = Params[2];

					irc.SendMessage(SendType.Message, IRCBotSettings.IRC_Channel, "[ShootMania] " + Winner + " win the round !");

				}

			}
			else if (MSC.Param1 == "hit" &&
				IRCBotSettings.ShootMania_SayHit == 1) {

				string[] Params = MSC.Param2.Split(';');

				if (Params.Length == 3)
				{

					string Shooter = Params[0];
					string Unk1 = Params[1];
					string Victim = Params[2];
					
					var playerShooter = Server.Server.GetPlayerListByPlayerLogin(Shooter);
					var playerVictim = Server.Server.GetPlayerListByPlayerLogin(Victim);

					if (playerShooter.Nickname.Length > 0 &&
					    playerVictim.Nickname.Length > 0)
					{
						irc.SendMessage(SendType.Message, IRCBotSettings.IRC_Channel, "[ShootMania] " + SMAdminTools.Functions.Mania.StripNadeoColours(playerShooter.Nickname) + " hit " + SMAdminTools.Functions.Mania.StripNadeoColours(playerVictim.Nickname) + " !");
					}

				}

			}
			else if (MSC.Param1 == "frag" &&
				IRCBotSettings.ShootMania_SayFrag == 1) {

				string[] Params = MSC.Param2.Split(';');

				if (Params.Length == 3)
				{

					string Shooter = Params[0];
					string Unk1 = Params[1];
					string Victim = Params[2];
					
					var playerShooter = Server.Server.GetPlayerListByPlayerLogin(Shooter);
					var playerVictim = Server.Server.GetPlayerListByPlayerLogin(Victim);

					if (playerShooter.Nickname.Length > 0 &&
					    playerVictim.Nickname.Length > 0)
					{
						irc.SendMessage(SendType.Message, IRCBotSettings.IRC_Channel, "[ShootMania] " + SMAdminTools.Functions.Mania.StripNadeoColours(playerShooter.Nickname) + " killed " + SMAdminTools.Functions.Mania.StripNadeoColours(playerVictim.Nickname) + " !");
					}

				}

			}
			else if (MSC.Param1 == "endMap" &&
				IRCBotSettings.ShootMania_SayEndMap == 1) {

				irc.SendMessage(SendType.Message, IRCBotSettings.IRC_Channel, "[ShootMania] Map ended !");

			}
			else if (MSC.Param1 == "beginMap" &&
				IRCBotSettings.ShootMania_SayBeginMap == 1) {

				irc.SendMessage(SendType.Message, IRCBotSettings.IRC_Channel, "[ShootMania] Map started : " + SMAdminTools.Functions.Mania.StripNadeoColours(MSC.Param2) + " !");

			}

		}

	}
}

