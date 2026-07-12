using Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VNyanInterface;
//using static System.Net.WebRequestMethods;

namespace VNyan_JSTV{
    public class VNyan_JSTV : IVNyanPluginManifest, ITriggerHandler, IButtonClickedHandler {
        public string PluginName { get; } = "VNyan-JSTV";
        public string Version { get; } = "0.3.2-alpha";
        public string Title { get; } = "Joystick.tv integration for VNyan";
        public string Author { get; } = "LumKitty";
        public string Website { get; } = "https://lum.uk/";

        private static string SettingsFile = "";
        //const string PwMask = "*************";
        
        // User Settings
        
        private static bool ConnectOnStartup = true;
        internal static List<string> FilterEvents = new List<string>();
        internal static bool LogSpam = false;
        internal static bool TriggerOnAllChat = false;

        // State

        internal static void Log(string message) {
            UnityEngine.Debug.Log("[JSTV] " + message.Replace(JSTV.ClientSecret, "**CLIENTSECRET**").Replace(JSTV.EncodedAuth, "**BASE64AUTH**")
                .Replace(JSTV.UserAccessToken, "**ACCESSTOKEN**").Replace(JSTV.UserRefreshToken, "**REFRESHTOKEN**"));
        }
        private static void ErrorHandler(Exception e) {
            Log("ERROR: " + e.ToString());
        }

        public void triggerCalled(string name, int num1, int num2, int num3, string text1, string text2, string text3) {
            JSTrigger.TriggerCalled(ref name, ref num1, ref num2, ref num3, ref text1, ref text2, ref text3);
        }

        internal static void SaveSettings() {
            JObject Config = new JObject(
                new JProperty("Port", JSTV.Port),
                new JProperty("ApplicationID", JSTV.ApplicationID),
                new JProperty("ClientID", JSTV.ClientID),
                new JProperty("ClientSecret", JSTV.ClientSecret),
                new JProperty("RefreshToken", JSTV.UserRefreshToken),
                new JProperty("UserName", JSTV.UserName),
                new JProperty("ChannelID", JSTV.ChannelID),
                new JProperty("ConnectOnStarup", ConnectOnStartup),
                new JProperty("FilterEvents", JArray.FromObject(FilterEvents)),
                new JProperty("LogSpam", LogSpam),
                new JProperty("TriggerOnAllChat", TriggerOnAllChat)
            );
            File.WriteAllText(SettingsFile, Config.ToString());
        }

        private static bool LoadSettings() {
            if (File.Exists(SettingsFile)) {
                Log("Reading Settings File: " + SettingsFile);
                JObject Config = JObject.Parse(File.ReadAllText(SettingsFile));

                if (Config.ContainsKey("ApplicationID")) { JSTV.ApplicationID    = Config["ApplicationID"].ToString(); }
                if (Config.ContainsKey("ClientID"))      { JSTV.ClientID         = Config["ClientID"].ToString(); }
                if (Config.ContainsKey("ClientSecret"))  { JSTV.ClientSecret     = Config["ClientSecret"].ToString(); }
                if (Config.ContainsKey("Port"))          { JSTV.Port             = int.Parse(Config["Port"].ToString()); }
                if (Config.ContainsKey("RefreshToken"))  { JSTV.UserRefreshToken = Config["RefreshToken"].ToString(); }
                if (Config.ContainsKey("UserName"))      { JSTV.UserName         = Config["UserName"].ToString(); }
                if (Config.ContainsKey("ChannelID"))     { JSTV.ChannelID        = Config["ChannelID"].ToString(); }
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

                Log("Port: " + JSTV.Port.ToString());
                Log("UserName: " + JSTV.UserName);
                Log("ChannelID: " + JSTV.ChannelID);
                Log("ConnectOnStartup: " + ConnectOnStartup.ToString());
                Log("FilterEvents: " + string.Join(",", FilterEvents));
                Log("LogSpam: "+LogSpam.ToString());
                Log("TriggerOnAllChat: " + TriggerOnAllChat.ToString());

                if (JSTV.ClientSecret == "null-clientsecret" || JSTV.ApplicationID == "" || JSTV.ClientID == "") {
                    return false;
                } else {
                    return true;
                }
            } else {
                SaveSettings();
                return false;
            }
        }

        public async void InitializePlugin() {
            try {
                Log("VNyan_JSTV v" + Version + " starting");
                SettingsFile = Path.Combine(VNyanInterface.VNyanInterface.VNyanSettings.getProfilePath(), "JSTV.json");
                VNyanInterface.VNyanInterface.VNyanTrigger.registerTriggerListener(this);
                VNyanInterface.VNyanInterface.VNyanUI.registerPluginButton("Joystick.tv connection toggle", this);

                if (LoadSettings()) {


                    if (ConnectOnStartup) { JSTV.ConnectJSTV(); }
                } else {
                    Log("Please visit https://joystick.tv/applications create a new bot and then fill in ApplicationID, ClientID and ClientSecret in: " + SettingsFile);
                }
            } catch (Exception ex) {
                Log("ERROR: "+ex.ToString());
            }
        }

        public void pluginButtonClicked() {
            if (JSTV.ConnectionWanted) {
                JSTV.DisconnectJSTV();
            } else {
                JSTV.ConnectJSTV();
            }
        }

        internal static void CallVNyan(string TriggerName, int int1, int int2, int int3, string text1, string text2, string text3) {
            Log("Sending VNyan trigger: " + TriggerName);
            Log("Int1 : " + int1.ToString() + " | Int2 : " + int2.ToString() + " | Int3 : " + int3.ToString());
            if (text1 != "") { Log("Text1: " + text1); }
            if (text2 != "") { Log("Text2: " + text2); }
            if (text3 != "") { Log("Text3: " + text3); }
            VNyanInterface.VNyanInterface.VNyanTrigger.callTrigger(TriggerName, int1, int2, int3, text1, text2, text3);
        }

        

        
        /*
        public static void OAuthBullshit() {
            int Timeout = 60 * 1000;
            int PollFrequency = 100;
            int MaxPolls = Timeout / PollFrequency;
            Log("ID: " + ClientID + ", Pass: " + ClientSecret);

            var requestProcessor = new RequestProcessor();
            var httpServer = new HttpServer("127.0.0.1", 6969, requestProcessor);
            string[] args = new string[0];

            string url = "https://api.joystick.tv/api/oauth/authorize?response_type=code&client_id=" + ClientID + "&scope=bot&state=" + State;

            Log("Starting webserver");
            httpServer.StartAsync(args);

            Log("Launching browser");
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });

            Log("Waiting for OAuth response");
            int NumPolls = 0;
            while ((TempState == "") && (TempAuthCode == "") && (NumPolls < MaxPolls)) {
                System.Threading.Thread.Sleep(PollFrequency);
                NumPolls++;
            }

            if (NumPolls == MaxPolls) {
                Log("Timed out waiting for OAuth");
                UserConnected = false;
                return;
            }

            Log("Response received");
            //Log("Auth code: " + TempAuthCode);
            Log("State    : " + TempState);

            Log("Stopping HTTP server");
            httpServer.Stop();
            Log("Thread terminating");
        }
        */

        


    }
}
