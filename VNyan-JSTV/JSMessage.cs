using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp;

namespace VNyan_JSTV {
    internal class JSMessage {
        internal static async void MessageReceived(object sender, MessageEventArgs args) {
            VNyan_JSTV.Log(args.Data);
            JObject Results = JObject.Parse(args.Data);
            if (Results.ContainsKey("identifier")) {
                if (Results.ContainsKey("message")) {
                    VNyan_JSTV.Log("Message subkey:" + Results["message"].ToString());
                    JObject Message = (JObject)Results["message"];
                    if (Message.ContainsKey("event")) {
                        switch (Message["event"].ToString()) {
                            case "ChatMessage":
                                if (Message["botCommand"].ToString() != "") {
                                    int arg = 0;
                                    int.TryParse(Message["botCommandArg"].ToString(), out arg);
                                    string Cmd = Message["botCommand"].ToString();
                                    VNyan_JSTV.CallVNyan("_jscmd_" + Cmd, arg, 0, 0, Message["author"]["username"].ToString(), Message["botCommandArg"].ToString(), Cmd);
                                } else {
                                    VNyan_JSTV.CallVNyan("_lum_jstv_chat", 0, 0, 0, Message["author"]["username"].ToString(), Message["text"].ToString(), "");
                                }
                                break;
                        }
                    }
                }
            }
        }
    }
}
