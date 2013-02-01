using System;

namespace SMAdminTools.Classes
{
	public class Config
	{

		public string ShootMania__IP = "";
		public int ShootMania__XML_RPC_Port = 0;
		public int ShootMania__ReconnectTimeout = 0;
		public string ShootMania__SuperAdmin_Login = string.Empty;
		public string ShootMania__SuperAdmin_Password = string.Empty;
		public bool SMAdminTools__Debug = false;

		public Boolean ParseFromIniFile (string fileName)
		{

			IniFile ini = new IniFile (fileName);

			this.ShootMania__IP = ini.GetValue ("ShootMania", "IP", string.Empty);

			if (this.ShootMania__IP == string.Empty) {
				Console.WriteLine ("Invalid ShootMania IP !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("ShootMania", "XML_RPC_Port", string.Empty), out this.ShootMania__XML_RPC_Port)) {
				Console.WriteLine ("Invalid ShootMania XML-RPC Port !");
				return false;
			}

			if (this.ShootMania__XML_RPC_Port == 0) {
				Console.WriteLine ("Invalid ShootMania XML-RPC Port !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("ShootMania", "ReconnectTimeout", string.Empty), out this.ShootMania__ReconnectTimeout)) {
				Console.WriteLine ("Invalid ShootMania Reconnect Timeout !");
				return false;
			}

			if (this.ShootMania__ReconnectTimeout == 0) {
				Console.WriteLine ("Invalid ShootMania Reconnect Timeout !");
				return false;
			}

			this.ShootMania__SuperAdmin_Login = ini.GetValue ("ShootMania", "SuperAdmin_Login", string.Empty);

			if (this.ShootMania__SuperAdmin_Login == string.Empty) {
				Console.WriteLine ("Invalid ShootMania SuperAdmin login !");
				return false;
			}

			this.ShootMania__SuperAdmin_Password = ini.GetValue ("ShootMania", "SuperAdmin_Password", string.Empty);

			if (this.ShootMania__SuperAdmin_Password == string.Empty) {
				Console.WriteLine ("Invalid ShootMania SuperAdmin password !");
				return false;
			}

			int m_SMAdminTools__Debug = ini.GetValue("SMAdminTools", "Debug", 0);

			if (m_SMAdminTools__Debug == 1)
				this.SMAdminTools__Debug = true;
		
			return true;

		}

	}
}

