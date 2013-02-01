using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace SMAdminTools.Plugins
{
	public class Manager
	{

		internal static List<Plugin> LoadedPlugins = new List<Plugin>();

		internal static void LoadPlugins ()
		{

			if (!Directory.Exists ("Plugins")) {
				Console.WriteLine ("Warning : Plugins folder not found !");
				return;
			}

			List<FileInfo> Dlls = new List<FileInfo> ();

			foreach (FileInfo file in new DirectoryInfo("Plugins").GetFiles()) {

				if (file.Extension.ToLower () == ".dll") {
					Dlls.Add (file);
				}

			}

			foreach (FileInfo dll in Dlls) {

				try
				{

					Assembly assembly = Assembly.LoadFile(dll.FullName);

					foreach (Type type in assembly.GetTypes())
					{

						if (type.BaseType == typeof(Plugin))
						{

							Plugin plugin = (Plugin)Activator.CreateInstance(type);

							LoadedPlugins.Add(plugin);

							Console.WriteLine("Plugin found : " + plugin.Name + " (Author : " + plugin.Author + ")" + " (Version : " + plugin.Version + ")");

							plugin.OnLoad();

						}

					}

				}
				catch { }
			}

		}

	}
}

