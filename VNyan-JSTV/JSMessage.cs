using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling.Memory.Experimental;
using WebSocketSharp;

namespace VNyan_JSTV {
    internal class JSMessage {
        internal static async void MessageReceived(object sender, MessageEventArgs args) {
            try {
                if (VNyan_JSTV.LogSpam) { VNyan_JSTV.Log(args.Data); }
                JObject Results = JObject.Parse(args.Data);
                if (Results.ContainsKey("identifier")) {
                    if (Results.ContainsKey("message")) {
                        //VNyan_JSTV.Log("Message subkey:" + Results["message"].ToString());
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
                                        if (VNyan_JSTV.TriggerOnAllChat) {
                                            VNyan_JSTV.CallVNyan("_jschat", 0, 0, 0, Message["author"]["username"].ToString(), Message["text"].ToString(), "");
                                        }
                                    }
                                    break;
                                case "streamevent":
                                    string UserName = "";
                                    string Item = "";
                                    int Value1 = 0;
                                    int Value2 = 0;
                                    int Value3 = 0;
                                    string TempTime = "";
                                    string EventType = Message["type"].ToString();
                                    if (VNyan_JSTV.FilterEvents.Contains(EventType)) {
                                        if (VNyan_JSTV.LogSpam) { VNyan_JSTV.Log("Filtered event of type: " + EventType); }
                                    } else {
                                        VNyan_JSTV.Log("Received stream event of type: " + EventType);
                                        if (Message.ContainsKey("metadata")) {
                                            //VNyan_JSTV.Log("Metadata found. Parsing");
                                            //VNyan_JSTV.Log(Message["metadata"].GetType().ToString());
                                            //;VNyan_JSTV.Log(Message["metadata"].ToString());
                                            JObject Metadata = JObject.Parse(Message["metadata"].ToString()); // I hate this
                                                                                                              //VNyan_JSTV.Log(Metadata.ToString());
                                            switch (EventType) {
                                                case "FollowerCountUpdated":
                                                    ProcessJObject(Metadata, "number_of_followers", ref Value1);
                                                    break;
                                                case "SubscriberCountUpdated":
                                                    ProcessJObject(Metadata, "number_of_subscribers", ref Value1);
                                                    break;
                                                case "ViewerCountUpdated":
                                                    ProcessJObject(Metadata, "number_of_viewers", ref Value1);
                                                    break;
                                                case "DropinStream":  // raid out
                                                    ProcessJObject(Metadata, "destination", ref UserName);
                                                    ProcessJObject(Metadata, "number_of_viewers", ref Value1);
                                                    break;
                                                case "TipGoalDeleted":
                                                case "TipGoalUpdated":
                                                case "TipMenuItemUnlocked":
                                                case "TipMenuItemLocked":
                                                case "TipGoalCreated":
                                                    ProcessJObject(Metadata, "title", ref Item);
                                                    ProcessJObject(Metadata, "amount", ref Value1);
                                                    break;
                                                case "TipGoalIncreased":
                                                    ProcessJObject(Metadata, "title", ref Item);
                                                    ProcessJObject(Metadata, "amount", ref Value1);
                                                    ProcessJObject(Metadata, "current", ref Value2);
                                                    ProcessJObject(Metadata, "previous", ref Value3);
                                                    break;
                                                case "Followed":
                                                case "UserMuted":
                                                case "UserUnmuted":
                                                case "ChatTimersCleared":
                                                case "Ended":
                                                case "StreamEnding":
                                                case "StreamModeUpdated":
                                                case "StreamResuming":
                                                case "Started":
                                                    ProcessJObject(Metadata, "who", ref UserName);
                                                    break;
                                                case "StreamDroppedIn":  // raid in
                                                    ProcessJObject(Metadata, "who", ref UserName);
                                                    ProcessJObject(Metadata, "number_of_viewers", ref Value1);
                                                    break;
                                                case "GiftedSubscriptions":
                                                    ProcessJObject(Metadata, "who", ref UserName);
                                                    ProcessJObject(Metadata, "how_much", ref Value1);
                                                    break;
                                                case "MilestoneCompleted":
                                                    ProcessJObject(Metadata, "who", ref UserName);
                                                    ProcessJObject(Metadata, "title", ref Item);
                                                    ProcessJObject(Metadata, "amount", ref Value1);
                                                    break;
                                                case "Resubscribed":
                                                    ProcessJObject(Metadata, "who", ref UserName);
                                                    ProcessJObject(Metadata, "how_much", ref Value1);
                                                    ProcessJObject(Metadata, "how_long", ref Value2);
                                                    break;
                                                case "Subscribed":
                                                    ProcessJObject(Metadata, "who", ref UserName);
                                                    ProcessJObject(Metadata, "how_much", ref Value1);
                                                    break;
                                                case "TipGoalMet":
                                                    ProcessJObject(Metadata, "who", ref UserName);
                                                    ProcessJObject(Metadata, "title", ref Item);
                                                    ProcessJObject(Metadata, "amount", ref Value1);
                                                    break;
                                                case "Tipped":
                                                    ProcessJObject(Metadata, "who", ref UserName);
                                                    ProcessJObject(Metadata, "tip_menu_item", ref Item);
                                                    ProcessJObject(Metadata, "how_much", ref Value1);
                                                    break;
                                                case "WheelSpinClaimed":
                                                    ProcessJObject(Metadata, "who", ref UserName);
                                                    ProcessJObject(Metadata, "prize", ref Item);
                                                    ProcessJObject(Metadata, "how_much", ref Value1);
                                                    break;
                                                case "ChatTimerStarted":
                                                    ProcessJObject(Metadata, "name", ref Item);
                                                    ProcessJObject(Metadata, "endsAt", ref TempTime);
                                                    Value1 = ISO8601toMilisecondTimespan(TempTime);
                                                    break;
                                                case "PvpSessionRequested":
                                                case "PvpSessionReady":
                                                case "PvpSessionEnded":
                                                case "PvpSessionEnding":
                                                case "PvpSessionStarted":
                                                    ProcessJObject(Metadata, "where", ref UserName);
                                                    ProcessJObject(Metadata, "when", ref TempTime);
                                                    Value1 = ISO8601toMilisecondTimespan(TempTime);
                                                    break;
                                                    //These events exist, but have no data so are handled automatically
                                                    //DeviceConnected
                                                    //DeviceDisconnected
                                                    //DeviceSettingsUpdated
                                                    //SettingsUpdated
                                            }
                                        }
                                        VNyan_JSTV.CallVNyan("_jsevent_" + EventType, Value1, Value2, Value3, UserName, Item, "");
                                    }
                                    break;
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                VNyan_JSTV.Log("ERROR: " + ex.Message);
            }
        }

        private static int ISO8601toMilisecondTimespan(string TimeStamp) {
            DateTime EndTime = DateTime.Parse(TimeStamp, null, DateTimeStyles.RoundtripKind);
            TimeSpan TimerDuration = EndTime - DateTime.UtcNow;
            return (int)TimerDuration.TotalMilliseconds;
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

        internal static void ServerConnected() {
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("_lum_jstv_connected", 1);
            VNyan_JSTV.CallVNyan("_lum_jstv_connected", 0, 0, 0, "", "", "");
        }

        internal static void ServerDisconnected() {
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat("_lum_jstv_connected", 0);
            VNyan_JSTV.CallVNyan("_lum_jstv_disconnected", 0, 0, 0, "", "", "");
        }

        internal static void SaveSettings() {
            VNyan_JSTV.SaveSettings();
        }
    }
}
