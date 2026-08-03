using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using WebSocketSharp;
using static VNyan_JSTV.Functions;

namespace VNyan_JSTV {
    internal static class Settings {
        internal static string UserName = "";
        internal static string ChannelID = "";
        internal static string ApplicationID = "";
        internal static string ClientID = "";
        internal static string ClientSecret = "";
        internal static int Port = 6969;

        internal static string RedirURL = "";
        internal static string EncodedAuth = "";
        internal static string UserAccessToken = "";
        internal static string UserRefreshToken = "";

        internal static bool ConnectOnStartup = true;
        internal static List<string> FilterEvents = new List<string>();
        internal static bool LogSpam = false;
        internal static bool TriggerOnAllChat = false;

        internal static readonly string SettingsFile = Path.Combine(VNyanInterface.VNyanInterface.VNyanSettings.getProfilePath(), "JSTV.json");

        internal static bool UserConnected = false;
        internal static bool BotConnected = false;
        internal static string State = GenerateRandomState();

        internal static void SaveSettings() {
            JObject Config = new JObject(
                new JProperty("Port", Port),
                new JProperty("ApplicationID", ApplicationID),
                new JProperty("ClientID", ClientID),
                new JProperty("ClientSecret", ClientSecret),
                new JProperty("RefreshToken", UserRefreshToken),
                new JProperty("UserName", UserName),
                new JProperty("ChannelID", ChannelID),
                new JProperty("ConnectOnStarup", ConnectOnStartup),
                new JProperty("FilterEvents", JArray.FromObject(FilterEvents)),
                new JProperty("LogSpam", LogSpam),
                new JProperty("TriggerOnAllChat", TriggerOnAllChat)
            );
            File.WriteAllText(SettingsFile, Config.ToString());
        }

        internal static bool LoadSettings() {
            if (File.Exists(SettingsFile)) {
                Log("Reading Settings File: " + SettingsFile);
                JObject Config = JObject.Parse(File.ReadAllText(SettingsFile));

                if (Config.ContainsKey("ApplicationID")) { ApplicationID = Config["ApplicationID"].ToString(); }
                if (Config.ContainsKey("ClientID")) { ClientID = Config["ClientID"].ToString(); }
                if (Config.ContainsKey("ClientSecret")) { ClientSecret = Config["ClientSecret"].ToString(); }
                if (Config.ContainsKey("Port")) { Port = int.Parse(Config["Port"].ToString()); }
                if (Config.ContainsKey("RefreshToken")) { UserRefreshToken = Config["RefreshToken"].ToString(); }
                if (Config.ContainsKey("UserName")) { UserName = Config["UserName"].ToString(); }
                if (Config.ContainsKey("ChannelID")) { ChannelID = Config["ChannelID"].ToString(); }
                if (Config.ContainsKey("ConnectOnStartup")) {
                    bool.TryParse(Config["ConnectOnStartup"].ToString(), out ConnectOnStartup);
                }
                if (Config.ContainsKey("FilterEvents")) {
                    Log("Reading filter events: " + Config["FilterEvents"].ToString());
                    FilterEvents = ((JArray)Config["FilterEvents"]).ToObject<List<String>>();
                } else {
                    Log("Using default filter events");
                    FilterEvents.Add("FollowerCountUpdated");
                    FilterEvents.Add("SubscriberCountUpdated");
                    FilterEvents.Add("ViewerCountUpdated");
                }
                if (Config.ContainsKey("LogSpam")) { LogSpam = bool.Parse(Config["LogSpam"].ToString()); }
                if (Config.ContainsKey("TriggerOnAllChat")) { TriggerOnAllChat = bool.Parse(Config["TriggerOnAllChat"].ToString()); }

                Log("Port: " + Port.ToString());
                Log("UserName: " + UserName);
                Log("ChannelID: " + ChannelID);
                Log("ConnectOnStartup: " + ConnectOnStartup.ToString());
                Log("FilterEvents: " + string.Join(",", FilterEvents));
                Log("LogSpam: " + LogSpam.ToString());
                Log("TriggerOnAllChat: " + TriggerOnAllChat.ToString());

                if (ClientSecret.IsNullOrEmpty() || ApplicationID.IsNullOrEmpty() || ClientID.IsNullOrEmpty()) {
                    return false;
                } else {
                    return true;
                }
            } else {
                SaveSettings();
                return false;
            }
        }

    }
}
