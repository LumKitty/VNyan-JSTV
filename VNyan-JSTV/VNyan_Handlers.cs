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
using static VNyan_JSTV.Settings;
using static VNyan_JSTV.Functions;
using WebSocketSharp;
//using static System.Net.WebRequestMethods;

namespace VNyan_JSTV{
    public class VNyan_Handlers : IVNyanPluginManifest, ITriggerHandler, IButtonClickedHandler {
        public string PluginName { get; } = "VNyan-JSTV";
        public string Version { get; } = "0.4-alpha";
        public string Title { get; } = "Joystick.tv integration for VNyan";
        public string Author { get; } = "LumKitty";
        public string Website { get; } = "https://lum.uk/";

        public async void InitializePlugin() {
            try {
                Log("VNyan_JSTV v" + Version + " starting");
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

        public void triggerCalled(string name, int num1, int num2, int num3, string text1, string text2, string text3) {
            if (name.Length > 10) {
                name = name.ToLower();
                if (name.Substring(0, 10) == "_lum_jstv_") {
                    Log("Received: " + name);
                    switch (name.ToLower().Substring(9)) {
                        case "_sendchat":
                            JSTV.SendChatMessage(text1);
                            break;
                        case "_sendwhisper":
                            JSTV.SendWhisper(text1, text2);
                            break;
                        case "_connect":
                            JSTV.ConnectJSTV();
                            break;
                        case "_disconnect":
                            JSTV.DisconnectJSTV();
                            break;
                    }
                }
            }
        }
    }
}
