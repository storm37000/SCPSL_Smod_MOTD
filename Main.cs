using Smod2;
using Smod2.Attributes;
using System.Collections.Generic;

namespace MOTD
{
	[PluginDetails(
		author = "storm37000",
		name = "MOTD",
		description = "Displays a custom message to the player whenever a player joins.",
		id = "s37k.motd",
		version = "1.0.1",
		SmodMajor = 3,
		SmodMinor = 2,
		SmodRevision = 0
		)]
	class Main : Plugin
	{
		public override void OnDisable()
		{
			this.Info(this.Details.name + " has been disabled.");
		}
		public override void OnEnable()
		{
			this.Info(this.Details.name + " has been enabled.");
			string[] hosts = { "https://storm37k.com/addons/", "http://74.91.115.126/addons/" };
			ushort version = ushort.Parse(this.Details.version.Replace(".", string.Empty));
			bool fail = true;
			string errorMSG = "";
			foreach (string host in hosts)
			{
				using (UnityEngine.WWW req = new UnityEngine.WWW(host + this.Details.name + ".ver"))
				{
					while (!req.isDone) { }
					errorMSG = req.error;
					if (string.IsNullOrEmpty(req.error))
					{
						ushort fileContentV = 0;
						if (!ushort.TryParse(req.text, out fileContentV))
						{
							errorMSG = "Parse Failure";
							continue;
						}
						if (fileContentV > version)
						{
							this.Error("Your version is out of date, please visit the Smod discord and download the newest version");
						}
						fail = false;
						break;
					}
				}
			}
			if (fail)
			{
				this.Error("Could not fetch latest version txt: " + errorMSG);
			}
		}
		
		public override void Register()
		{
			// Register Events
			this.AddEventHandlers(new EventHandler(this));

			this.AddConfig(new Smod2.Config.ConfigSetting("motd_messages", new Dictionary<string, string>() { { "Welcome to $PlayerListTitle !","5" } }, Smod2.Config.SettingType.DICTIONARY, true, "List of messages one line at a time."));
			this.AddConfig(new Smod2.Config.ConfigSetting("motd_printplugins", true , Smod2.Config.SettingType.BOOL, true, "Print to players the list of plugins including basic info."));
			this.AddConfig(new Smod2.Config.ConfigSetting("info_ignoredplugin", new string[]{}, Smod2.Config.SettingType.LIST, true, "Plugins to hide from being displayed.  Backwards compat with PluginInfo."));
			this.AddConfig(new Smod2.Config.ConfigSetting("motd_ignoredplugins", new string[]{}, Smod2.Config.SettingType.LIST, true, "Plugins to hide from being displayed"));
			this.AddConfig(new Smod2.Config.ConfigSetting("motd_printpluginsformat", "<size=25%><color='yellow'>$name</color> $version by $author - '$description'</size>", Smod2.Config.SettingType.STRING, true, "Format for plugin printouts."));
		}
	}
}
