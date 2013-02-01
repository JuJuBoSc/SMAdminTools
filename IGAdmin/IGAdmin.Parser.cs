using System;

namespace IGAdmin
{
	partial class IGAdmin
	{

		private void ParseChatCommand (ShootManiaXMLRPC.Structs.PlayerChat PC)
		{
			if (PC.Text == "!admins") {
				foreach (string admin in Admins) {
					ChatSendServerMessage ("Admin : " + admin);
				}
			} else if (PC.Text == "!players" &&
				Admins.Contains (PC.Login)) {
				foreach (var player in ServerManager.Server.GetPlayerList(100, 0)) {
					if (player.PlayerId > 0) {
						ChatSendServerMessage ("[" + player.PlayerId + "] " + player.Nickname);
					}
				}
			} else if (PC.Text.StartsWith ("!addadmin") &&
				Admins.Contains (PC.Login)) {

				string admin = PC.Text.Replace ("!addadmin ", string.Empty);

				if (!Admins.Contains (admin)) {
					Admins.Add (admin);
					SaveAdmins ();
					ChatSendServerMessage ("Admin added : " + admin);
				} else {
					ChatSendServerMessage ("Admin already exist : " + admin);
				}

			} else if (PC.Text.StartsWith ("!deladmin") &&
				Admins.Contains (PC.Login)) {

				string admin = PC.Text.Replace ("!deladmin ", string.Empty);

				if (admin != PC.Login) {

					if (Admins.Contains (admin)) {
						Admins.Remove (admin);
						SaveAdmins ();
						ChatSendServerMessage ("Admin removed : " + admin);
					} else {
						ChatSendServerMessage ("Admin not found : " + admin);
					}

				} else {
					ChatSendServerMessage ("You can't remove yourself !");
				}

			} else if (PC.Text == "!restartmap" &&
				Admins.Contains (PC.Login)) {
				ChatSendServerMessage ("Restart map ...");
				ServerManager.Server.RestartMap (false);
			} else if (PC.Text == "!nextmap" &&
				Admins.Contains (PC.Login)) {
				ChatSendServerMessage ("Next map ...");
				ServerManager.Server.NextMap (false);
			} else if (PC.Text.StartsWith ("!map ") &&
				Admins.Contains (PC.Login)) {

				string newMap = PC.Text.Replace ("!map ", string.Empty);

				foreach (var map in ServerManager.Server.GetMapList(1000, 0)) {

					if (map.FileName.ToLower ().Contains (newMap.ToLower ())) {
						ChatSendServerMessage ("Map found : " + map.Name);
						ServerManager.Server.ChooseNextMap (map.FileName);
						ServerManager.Server.NextMap (false);
						return;
					}

				}

				ChatSendServerMessage ("No map found with the pattern : " + newMap);

			} else if (PC.Text.StartsWith ("!kick ") &&
				Admins.Contains (PC.Login)) {

				try {

					int PlayerId = Convert.ToInt32 (PC.Text.Replace ("!kick ", string.Empty));

					ServerManager.Server.KickId (PlayerId, "Kicked by admin");
					ChatSendServerMessage ("Played kicked");

				} catch {
					ChatSendServerMessage ("Invalid player id !");
				}

			} else if (PC.Text.StartsWith ("!ban ") &&
				Admins.Contains (PC.Login)) {

				try {

					int PlayerId = Convert.ToInt32 (PC.Text.Replace ("!ban ", string.Empty));

					ServerManager.Server.BanId (PlayerId, "Banned by admin");
					ChatSendServerMessage ("Played banned");

				} catch {
					ChatSendServerMessage ("Invalid player id !");
				}

			} else if (PC.Text.StartsWith ("!password ") &&
				Admins.Contains (PC.Login)) {

				string newPassword = PC.Text.Replace ("!password ", string.Empty);

				ServerManager.Server.SetServerPassword (newPassword);
				ChatSendServerMessage ("Password set to : " + newPassword);

			}

		}
	}
}


