using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling.Memory.Experimental;
using WebSocketSharp;

namespace VNyan_JSTV {
    internal class JSMessage {
        internal static async void MessageReceived(object sender, MessageEventArgs args) {
            try {
                VNyan_JSTV.Log(args.Data);
                JObject Results = JObject.Parse(args.Data);
                if (Results.ContainsKey("identifier")) {
                    if (Results.ContainsKey("message")) {
                        VNyan_JSTV.Log("Message subkey:" + Results["message"].ToString());
                        JObject Message = (JObject)Results["message"];
                        if (Message.ContainsKey("event")) {
                            string EventClass = Message["event"].ToString().ToLower();
                            switch (EventClass) {
                                case "chatmessage":
                                    if (Message["botCommand"].ToString() != "") {
                                        int arg = 0;
                                        int.TryParse(Message["botCommandArg"].ToString(), out arg);
                                        string Cmd = Message["botCommand"].ToString();
                                        VNyan_JSTV.CallVNyan("_jscmd_" + Cmd, arg, 0, 0, Message["author"]["username"].ToString(), Message["botCommandArg"].ToString(), Cmd);
                                    } else {
                                        VNyan_JSTV.CallVNyan("_lum_jstv_chat", 0, 0, 0, Message["author"]["username"].ToString(), Message["text"].ToString(), "");
                                    }
                                    break;
                                case "streamevent":
                                    string UserName = "";
                                    string Item = "";
                                    int Value = 0;
                                    string EventType = Message["type"].ToString();
                                    VNyan_JSTV.Log("Received stream event of type: " + EventType);
                                    if (Message.ContainsKey("metadata")) {
                                        //VNyan_JSTV.Log("Metadata found. Parsing");
                                        //VNyan_JSTV.Log(Message["metadata"].GetType().ToString());
                                        //;VNyan_JSTV.Log(Message["metadata"].ToString());
                                        JObject Metadata = JObject.Parse(Message["metadata"].ToString()); // I hate this
                                        //VNyan_JSTV.Log(Metadata.ToString());
                                        ProcessJObject(Metadata, "who", ref UserName);
                                        ProcessJObject(Metadata, "title", ref Item);
                                        ProcessJObject(Metadata, "tip_menu_item", ref Item);
                                        ProcessJObject(Metadata, "prize", ref Item);
                                        ProcessJObject(Metadata, "amount", ref Value);
                                        ProcessJObject(Metadata, "how_much", ref Value);
                                        ProcessJObject(Metadata, "number_of_viewers", ref Value);
                                        ProcessJObject(Metadata, "number_of_followers", ref Value);
                                    }
                                    VNyan_JSTV.CallVNyan("_jsevent_" + EventType, Value, 0, 0, UserName, Item, "");
                                    break;
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                VNyan_JSTV.Log("ERROR: " + ex.Message);
            }
        }

        private static void ProcessJObject(JObject jObject, string KeyName, ref int Result) {
            if (jObject.ContainsKey(KeyName)) {
                //VNyan_JSTV.Log("Parsing int: " + KeyName);
                int.TryParse(jObject[KeyName].ToString(), out Result);
            }
        }
        private static void ProcessJObject(JObject jObject, string KeyName, ref string Result) {
            if (jObject.ContainsKey(KeyName)) {
                //VNyan_JSTV.Log("Parsing string: " + KeyName);
                Result = jObject[KeyName].ToString();
            }
        }
    }
}
