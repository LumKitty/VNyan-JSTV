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
                    JSTV.Log("Received: " + name);
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
