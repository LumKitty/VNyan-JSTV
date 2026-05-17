using Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VNyanInterface;
using WebSocketSharp;
//using static System.Net.WebRequestMethods;

namespace VNyan_JSTV{
    public class VNyan_JSTV : IVNyanPluginManifest, ITriggerHandler {
        public string PluginName { get; } = "VNyan-JSTV";
        public string Version { get; } = "0.1-alpha";
        public string Title { get; } = "Joystick.tv integration for VNyan";
        public string Author { get; } = "LumKitty";
        public string Website { get; } = "https://lum.uk/";

        private static string SettingsFile = "";
        //const string PwMask = "*************";
        private static string UserName = "";
        internal static string ChannelID = "";
        private static string ApplicationID = "";
        private static string ClientID = "";
        private static string ClientSecret = "null-clientsecret";
        private static int Port = 6969;
        private static string RedirURL = "";
        private static string EncodedAuth = "null-encodedauth";
        private static string UserAccessToken = "null-useraccesstoken";
        private static string UserRefreshToken = "null-refreshtoken";
        internal static string TempAuthCode = "";
        internal static string TempState = "";
        internal static bool UserConnected = false;
        internal static bool BotConnected = false;
        internal static string State = GenerateRandomState();
        private static WebSocketSharp.WebSocket wsClient;
        private static System.Threading.CancellationToken CT = new System.Threading.CancellationToken();


        internal static void Log(string message) {
            UnityEngine.Debug.Log("[JSTV] " + message.Replace(ClientSecret, "**CLIENTSECRET**").Replace(EncodedAuth, "**BASE64AUTH**")
                .Replace(UserAccessToken, "**ACCESSTOKEN**").Replace(UserRefreshToken, "**REFRESHTOKEN**"));
        }
        private static void ErrorHandler(Exception e) {
            Log("ERROR: " + e.ToString());
        }

        public void triggerCalled(string name, int num1, int num2, int num3, string text1, string text2, string text3) {
            JSTrigger.TriggerCalled(ref name, ref num1, ref num2, ref num3, ref text1, ref text2, ref text3);
        }

        private static void SaveSettings() {
            JObject Config = new JObject(
                new JProperty("Port", Port),
                new JProperty("ApplicationID", ApplicationID),
                new JProperty("ClientID", ClientID),
                new JProperty("ClientSecret", ClientSecret),
                new JProperty("RefreshToken", UserRefreshToken),
                new JProperty("UserName", UserName),
                new JProperty("ChannelID", ChannelID)
            );
            File.WriteAllText(SettingsFile, Config.ToString());
        }

        private static bool LoadSettings() {
            if (File.Exists(SettingsFile)) {
                dynamic Config = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(SettingsFile));
                if (Config.ApplicationID != null) { ApplicationID = Config.ApplicationID; }
                if (Config.ClientID      != null) { ClientID = Config.ClientID; }
                if (Config.ClientSecret  != null) { ClientSecret = Config.ClientSecret; }
                if (Config.Port          != null) { Port = Config.Port; }
                if (Config.RefreshToken  != null) { UserRefreshToken = Config.RefreshToken; }
                if (Config.UserName      != null) { UserName = Config.UserName; }
                if (Config.ChannelID     != null) { ChannelID = Config.ChannelID; }

                if (ClientSecret == "null-clientsecret" || ApplicationID == "" || ClientID == "") {
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

                if (LoadSettings()) {
                    VNyanInterface.VNyanInterface.VNyanTrigger.registerTriggerListener(this);

                    AuthoriseUser();
                    while (!UserConnected) { System.Threading.Thread.Sleep(100); }

                    Log("Authorised user. Connecting bot");

                    wsClient = new WebSocketSharp.WebSocket("wss://api.joystick.tv/cable?token=" + EncodedAuth, "actioncable-v1-json");
                    wsClient.OnOpen += ServerConnected;
                    wsClient.OnClose += ServerDisconnected;
                    wsClient.OnError += ServerDisconnected;
                    wsClient.OnMessage += JSMessage.MessageReceived;
                    wsClient.Connect();


                    while (!BotConnected) {
                        //Console.Write(".");
                        System.Threading.Thread.Sleep(100);
                    }

                    Log("Bot connected. Sending subscribe message");

                    JObject Message = new JObject(
                        new JProperty("command", "subscribe"),
                        new JProperty("identifier", "{\"channel\":\"GatewayChannel\"}")
                    );
                    WSSend(ref Message);
                } else {
                    Log("Please visit https://joystick.tv/applications create a new bot and then fill in ApplicationID, ClientID and ClientSecret in: " + SettingsFile);
                }
            } catch (Exception ex) {
                Log("ERROR: "+ex.ToString());
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

        

        static async void ServerConnected(object sender, EventArgs args) {
            Log("Server connected");
            BotConnected = true;
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("_lum_jstv_connected", 1);
            CallVNyan("_lum_jstv_connected", 0, 0, 0, "", "", "");
        }

        static async void ServerDisconnected(object sender, EventArgs args) {
            Log("Server disconnected");
            Log(args.ToString());
            BotConnected = false;
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("_lum_jstv_connected", 0);
            CallVNyan("_lum_jstv_disconnected", 0, 0, 0, "", "", "");
        }

        internal static void WSSend(ref JObject json) {
            string data = JsonConvert.SerializeObject(json);
            Log("WS Sending: " + data);
            wsClient.Send(data);
        }

        private static string GenerateRandomState() {
            //TODO: Actually make random
            return "piss";
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

        private static HttpResponseMessage? MakeHttpRequest(HttpRequestMessage requestMessage) {
            try {
                System.Net.Http.HttpClient Http = new System.Net.Http.HttpClient();
                Log("Sending");
                Task<HttpResponseMessage> authCodeTask = Http.SendAsync(requestMessage);
                Log("Waiting");
                if (authCodeTask.Wait(5000)) {
                    Log("Got Result");
                    return authCodeTask.Result;
                } else {
                    Log("Failed to get server response code in time");
                    return null;
                }
            } catch (Exception e) {
                Log(e.ToString());
                return null;
            }
        }

        private static string? HttpResponseToContent(HttpResponseMessage response) {
            Task<String> authCodeReader = response.Content.ReadAsStringAsync();
            if (authCodeReader.Wait(5000)) {
                return authCodeReader.Result;
            } else {
                Log("Failed to read server in time");
                return null;
            }
        }
        
        private static string? MakeHttpRequestString(HttpRequestMessage requestMessage) {
            return HttpResponseToContent(MakeHttpRequest(requestMessage));
        }

        public static void AuthoriseUser() {
            HttpRequestMessage requestMessage;
            System.Net.Http.HttpClient Http = new System.Net.Http.HttpClient();
            HttpResponseMessage response;

            string content;
            int Timeout = 60 * 1000;
            int PollFrequency = 100;
            int MaxPolls = Timeout / PollFrequency;


            if (UserRefreshToken == null) { UserRefreshToken = "null-refreshtoken"; }
            EncodedAuth = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(ClientID + ":" + ClientSecret));

            if (UserRefreshToken != "null-refreshtoken") {
                Log("Logging in with refresh code");
                requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.joystick.tv/api/oauth/token?refresh_token=" + UserRefreshToken + "&grant_type=refresh_token");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", EncodedAuth);
                requestMessage.Headers.Add("X-JOYSTICK-STATE", State);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                requestMessage.Content = new StringContent("", Encoding.ASCII, "application/x-www-form-urlencoded");

                //make the request
                response = MakeHttpRequest(requestMessage);
                content = HttpResponseToContent(response);

                Log("Headers: " + response.Headers.ToString());
                if (!response.IsSuccessStatusCode) {
                    Log("Failed with statuscode: " + response.StatusCode.ToString());
                    UserConnected = false;
                } else {
                    dynamic JsonContent = JsonConvert.DeserializeObject<dynamic>(content);

                    UserAccessToken = JsonContent.access_token;
                    UserRefreshToken = JsonContent.refresh_token;
                    Log(JsonContent.ToString());
                    UserConnected = true;
                    Log("Content: " + content);
                    SaveSettings();
                }
            }
            if (!UserConnected) {
                string[] args = new string[0];

                string url = "https://api.joystick.tv/api/oauth/authorize?response_type=code&client_id=" + ClientID + "&scope=bot&state=" + State;

                Log("Starting webserver");
                HTTPServer Server = new HTTPServer(6969);
                Server.Start(State);

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

                var Data = new StringContent("", Encoding.ASCII);
                Data.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

                
                requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.joystick.tv/api/oauth/token?redirect_uri=.&code=" + TempAuthCode + "&grant_type=authorization_code");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", EncodedAuth);
                requestMessage.Headers.Add("X-JOYSTICK-STATE", State);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                requestMessage.Content = new StringContent("", Encoding.ASCII, "application/x-www-form-urlencoded");

                //make the request
                Log("Logging in with auth code: "+requestMessage.RequestUri);

                Log("Sending message");
                response = MakeHttpRequest(requestMessage);
                Log("Receiving message");
                content = HttpResponseToContent(response);

                Log("Headers: " + response.Headers.ToString());
                if (!response.IsSuccessStatusCode) {
                    Log("Failed with statuscode: " + response.StatusCode.ToString());
                    UserConnected = false;
                } else {
                    dynamic JsonContent = JsonConvert.DeserializeObject<dynamic>(content);
                    UserAccessToken = JsonContent.access_token;
                    UserRefreshToken = JsonContent.refresh_token;
                    Log("Content: " + content);
                    UserConnected = true;
                }
            }
            Log("Requesting Streamer Settings");
            requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.joystick.tv/api/users/stream-settings");
            requestMessage.Headers.Add("Authorization", "Bearer " + UserAccessToken);
            requestMessage.Headers.Add("X-JOYSTICK-STATE", State);
            //requestMessage.Content = new StringContent("", Encoding.ASCII, "application/json");
            Log("Sending message");
            response = MakeHttpRequest(requestMessage);
            Log("Receiving message");
            content = HttpResponseToContent(response);
            Log("Received");
            if (!response.IsSuccessStatusCode) {
                Log("Failed with statuscode: " + response.StatusCode.ToString());
                Log(content);
                UserConnected = false;
                return;
            } else {
                Log(content);
                dynamic JSON = JsonConvert.DeserializeObject<dynamic>(content);
                UserName = JSON.username;
                ChannelID = JSON.channel_id;
                Log("Detected username: " + UserName);
                Log("Detected channel ID:" + ChannelID);
                SaveSettings();
                UserConnected = true;
            }
        }
    
    }
}
