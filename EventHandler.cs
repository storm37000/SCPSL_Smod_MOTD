using Smod2;
using Smod2.Events;
using Smod2.EventHandlers;
using System.Collections.Generic;

namespace MOTD
{
	class EventHandler : IEventHandlerPlayerJoin, IEventHandlerWaitingForPlayers
	{
		private Main plugin;
		private Dictionary<string,uint>  msglist = new Dictionary<string, uint>();

		public EventHandler(Main plugin)
		{
			this.plugin = plugin;
		}

		private IEnumerable<string> EnclosedStrings(string s,string begin,string end)
		{
			int beginPos = s.IndexOf(begin, 0);
			while (beginPos >= 0)
			{
				int start = beginPos + begin.Length;
				int stop = s.IndexOf(end, start);
				if (stop < 0)
					yield break;
				yield return s.Substring(start, stop - start);
				beginPos = s.IndexOf(begin, stop + end.Length);
			}
		}

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			msglist.Clear();
			Dictionary<string, string> tempdict = plugin.GetConfigDict("motd_messages");
			foreach (KeyValuePair<string, string> entry in tempdict)
			{
				uint time = 5;
				if (uint.TryParse(entry.Value, out time) == false)
				{
					plugin.Error(entry.Value + " is not a valid integer time for message '" + entry.Key + "'!");
					continue;
				}
				string tempmsg = entry.Key;
				foreach (string config in EnclosedStrings(entry.Key,"$S[","]"))//strings
				{
					tempmsg = tempmsg.Replace("$S[" + config + "]", ConfigManager.Manager.Config.GetStringValue(config,"-"));
				}
				foreach (string config in EnclosedStrings(entry.Key, "$I[", "]"))//integers
				{
					tempmsg = tempmsg.Replace("$I[" + config + "]", "" + ConfigManager.Manager.Config.GetIntValue(config,0));
				}
				foreach (string config in EnclosedStrings(entry.Key, "$F[", "]"))//floats
				{
					tempmsg = tempmsg.Replace("$F[" + config + "]", "" + ConfigManager.Manager.Config.GetFloatValue(config,0f));
				}
				foreach (string config in EnclosedStrings(entry.Key, "$B[", "]"))//bools
				{
					tempmsg = tempmsg.Replace("$B[" + config + "]", "" + ConfigManager.Manager.Config.GetBoolValue(config,false));
				}
//				foreach (string config in EnclosedStrings(entry.Key, "$SL[", "]"))//string lists
//				{
//					tempmsg = tempmsg.Replace("$SL[" + config + "]", "");
//				}
//				foreach (string config in EnclosedStrings(entry.Key, "$IL[", "]"))//integer lists
//				{
//					tempmsg = tempmsg.Replace("$IL[" + config + "]", "");
//				}
				msglist.Add(tempmsg, time);
			}
			if (plugin.GetConfigBool("motd_printplugins"))
			{
				msglist.Add("This Server is Running Smod " + PluginManager.SMOD_MAJOR + "." + PluginManager.SMOD_MINOR + "." + PluginManager.SMOD_REVISION + "-" + PluginManager.SMOD_BUILD + " with plugins: ", 5);
				string fmttmp = plugin.GetConfigString("motd_printpluginsformat");
				foreach (Plugin plugin in PluginManager.Manager.EnabledPlugins)
				{
					string fmt = fmttmp;
					fmt = fmt.Replace("$name", plugin.Details.name);
					fmt = fmt.Replace("$version", plugin.Details.version);
					fmt = fmt.Replace("$author", plugin.Details.author);
					fmt = fmt.Replace("$description", plugin.Details.description);
					msglist.Add(fmt, 5);
				}
			}
		}

		public void OnPlayerJoin(PlayerJoinEvent ev)
		{
			foreach (KeyValuePair<string, uint> entry in msglist)
			{
				string tempmsg = entry.Key;
				tempmsg = tempmsg.Replace("$ServerName", PluginManager.Manager.Server.Name);
				tempmsg = tempmsg.Replace("$PlayerListTitle", PluginManager.Manager.Server.PlayerListTitle);
				tempmsg = tempmsg.Replace("$curPlayerCount", "" + PluginManager.Manager.Server.NumPlayers);
				tempmsg = tempmsg.Replace("$roundDurSec", "" + PluginManager.Manager.Server.Round.Duration);
				tempmsg = tempmsg.Replace("$roundDurMin", "" + PluginManager.Manager.Server.Round.Duration/60);

				ev.Player.PersonalBroadcast(entry.Value, tempmsg, false);
			}

			//PluginManager.Manager.EnabledPlugins[1].Details;
		}
	}
}