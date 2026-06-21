using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace VNyan_JSTV {
    internal static class JSTrigger {
        internal static void TriggerCalled(ref string name, ref int num1, ref int num2, ref int num3, ref string text1, ref string text2, ref string text3) {
            if (name.Length > 10) {
                name = name.ToLower();
                if (name.Substring(0, 10) == "_lum_jstv_") {
                    VNyan_JSTV.Log("Received: " + name);
                    switch (name.ToLower().Substring(9)) {
                        case "_sendchat":
                            SendChatMessage(text1);
                            break;
                        case "_sendwhisper":
                            SendWhisper(text1, text2);
                            break;
                        case "_connect":
                            VNyan_JSTV.ConnectJSTV();
                            break;
                        case "_disconnect":
                            VNyan_JSTV.DisconnectJSTV();
                            break;
                    }
                }
            }
        }

        private static void SendChatMessage(string Message) {
            JObject MessageJSON = new JObject(
                new JProperty("command", "message"),
                new JProperty("identifier", "{\"channel\":\"GatewayChannel\"}"),
                new JProperty("data", new JObject(
                    new JProperty("action", "send_message"),
                    new JProperty("text", Message),
                    new JProperty("channelId", VNyan_JSTV.ChannelID)
                ).ToString())
            );
            VNyan_JSTV.Log(JsonConvert.SerializeObject(MessageJSON));
            VNyan_JSTV.WSSend(ref MessageJSON);
        }

        private static void SendWhisper(string Message, string UserName) {
            JObject MessageJSON = new JObject(
                new JProperty("command", "message"),
                new JProperty("identifier", "{\"channel\":\"GatewayChannel\"}"),
                new JProperty("data", new JObject(
                    new JProperty("action", "send_message"),
                    new JProperty("username", UserName),
                    new JProperty("text", Message),
                    new JProperty("channelId", VNyan_JSTV.ChannelID)
                ).ToString())
            );
            VNyan_JSTV.Log(JsonConvert.SerializeObject(MessageJSON));
            VNyan_JSTV.WSSend(ref MessageJSON);
        }
    }
}
