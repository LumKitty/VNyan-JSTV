using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using static VNyan_JSTV.Settings;
using static VNyan_JSTV.Functions;

namespace VNyan_JSTV {
    internal class JSTV {
        internal static bool ConnectionWanted = true;

        private static WebSocketSharp.WebSocket wsClient;
        //private static System.Threading.CancellationToken CT = new System.Threading.CancellationToken();

        internal static async void ConnectJSTV() {
            JSTV_Auth.AuthoriseUser();
            while (!UserConnected) { System.Threading.Thread.Sleep(100); }

            Log("Authorised user. Connecting bot");
            ConnectionWanted = true;
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
        }

        internal static async void DisconnectJSTV() {
            ConnectionWanted = false;
            wsClient.Close();
        }

        static async void ServerConnected(object sender, EventArgs args) {
            Log("Server connected");
            BotConnected = true;
            JSMessage.ServerConnected();
        }

        static async void ServerDisconnected(object sender, EventArgs args) {
            Log("Server disconnected");
            if (ConnectionWanted) { ConnectJSTV(); }
            Log(args.ToString());
            BotConnected = false;
            JSMessage.ServerDisconnected();
        }

        internal static void WSSend(ref JObject json) {
            string data = JsonConvert.SerializeObject(json);
            Log("WS Sending: " + data);
            wsClient.Send(data);
        }

        internal static void SendChatMessage(string Message) {
            JObject MessageJSON = new JObject(
                new JProperty("command", "message"),
                new JProperty("identifier", "{\"channel\":\"GatewayChannel\"}"),
                new JProperty("data", new JObject(
                    new JProperty("action", "send_message"),
                    new JProperty("text", Message),
                    new JProperty("channelId", ChannelID)
                ).ToString())
            );
            Log(JsonConvert.SerializeObject(MessageJSON));
            WSSend(ref MessageJSON);
        }

        internal static void SendWhisper(string Message, string UserName) {
            JObject MessageJSON = new JObject(
                new JProperty("command", "message"),
                new JProperty("identifier", "{\"channel\":\"GatewayChannel\"}"),
                new JProperty("data", new JObject(
                    new JProperty("action", "send_message"),
                    new JProperty("username", UserName),
                    new JProperty("text", Message),
                    new JProperty("channelId", ChannelID)
                ).ToString())
            );
            Log(JsonConvert.SerializeObject(MessageJSON));
            WSSend(ref MessageJSON);
        }

    }
}
