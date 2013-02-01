using System;

namespace SMAdminTools.Plugins
{
    public abstract class Plugin
    {

        public abstract String Name { get; }

        public abstract String Author { get; }

        public abstract String Version { get; }

        public abstract void OnLoad();

        public abstract void OnServerManagerInitialize(Classes.ServerManager ServerManager);

		public abstract void OnConsoleCommand(string Command);

    }
}

