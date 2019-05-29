using Smod2;
using Smod2.Attributes;
using System.Collections.Generic;
using UnityEngine.Networking;
using MEC;
using System.Linq;

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
		}

		public bool UpToDate { get; private set; } = true;

		public void outdatedmsg()
		{
			this.Error("Your version is out of date, please update the plugin and restart your server when it is convenient for you.");
		}

		IEnumerator<float> UpdateChecker()
		{
			string[] hosts = { "https://storm37k.com/addons/", "http://74.91.115.126/addons/" };
			bool fail = true;
			string errorMSG = "";
			foreach (string host in hosts)
			{
				using (UnityWebRequest webRequest = UnityWebRequest.Get(host + this.Details.name + ".ver"))
				{
					// Request and wait for the desired page.
					yield return Timing.WaitUntilDone(webRequest.SendWebRequest());

					if (webRequest.isNetworkError || webRequest.isHttpError)
					{
						errorMSG = webRequest.error;
					}
					else
					{
						if (webRequest.downloadHandler.text != this.Details.version)
						{
							outdatedmsg();
							UpToDate = false;
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

			try
			{
				string file = System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(Smod2.ConfigManager.Manager.Config.GetConfigPath()), "s37k_g_disableVcheck*", System.IO.SearchOption.TopDirectoryOnly).FirstOrDefault();
				if (file == null)
				{
					Timing.RunCoroutine(UpdateChecker());
				}
				else
				{
					this.Info("Version checker is disabled.");
				}
			}
			catch (System.Exception)
			{
				Timing.RunCoroutine(UpdateChecker());
			}
		}
	}
}
