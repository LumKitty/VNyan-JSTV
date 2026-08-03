using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using static VNyan_JSTV.Settings;

namespace VNyan_JSTV {
    internal static class Functions {
        internal static void Log(string message) {
            if (!ClientSecret.IsNullOrEmpty())     { message = message.Replace(ClientSecret, "**CLIENTSECRET**");     }
            if (!EncodedAuth.IsNullOrEmpty())      { message = message.Replace(EncodedAuth, "**BASE64AUTH**");        }
            if (!UserAccessToken.IsNullOrEmpty())  { message = message.Replace(UserAccessToken, "**ACCESSTOKEN**");   }
            if (!UserRefreshToken.IsNullOrEmpty()) { message = message.Replace(UserRefreshToken, "**REFRESHTOKEN**"); }
            UnityEngine.Debug.Log("[JSTV] " + message);
        }
        internal static void ErrorHandler(Exception e) {
            Log("ERROR: " + e.ToString());
        }
        internal static void CallVNyan(string TriggerName, int int1, int int2, int int3, string text1, string text2, string text3) {
            Log("Sending VNyan trigger: " + TriggerName);
            Log("Int1 : " + int1.ToString() + " | Int2 : " + int2.ToString() + " | Int3 : " + int3.ToString());
            if (text1 != "") { Log("Text1: " + text1); }
            if (text2 != "") { Log("Text2: " + text2); }
            if (text3 != "") { Log("Text3: " + text3); }
            VNyanInterface.VNyanInterface.VNyanTrigger.callTrigger(TriggerName, int1, int2, int3, text1, text2, text3);
        }
        internal static string GenerateRandomState() {
            //TODO: Actually make random
            return "piss";
        }
        internal static HttpResponseMessage? MakeHttpRequest(HttpRequestMessage requestMessage) {
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
        internal static string? HttpResponseToContent(HttpResponseMessage response) {
            Task<String> authCodeReader = response.Content.ReadAsStringAsync();
            if (authCodeReader.Wait(5000)) {
                return authCodeReader.Result;
            } else {
                Log("Failed to read server in time");
                return null;
            }
        }

        internal static string? MakeHttpRequestString(HttpRequestMessage requestMessage) {
            return HttpResponseToContent(MakeHttpRequest(requestMessage));
        }

    }
}
