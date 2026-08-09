using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using WebSocketSharp;
using static VNyan_JSTV.Settings;
using static VNyan_JSTV.Functions;

namespace VNyan_JSTV {
    internal static class JSTV_Auth {
        internal static string TempAuthCode = "";
        internal static string TempState = "";
        public static void AuthoriseUser() {
            HttpRequestMessage requestMessage;
            System.Net.Http.HttpClient Http = new System.Net.Http.HttpClient();
            HttpResponseMessage response;

            string content;
            int Timeout = 60 * 1000;
            int PollFrequency = 100;
            int MaxPolls = Timeout / PollFrequency;

            EncodedAuth = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(ClientID + ":" + ClientSecret));

            if (!UserRefreshToken.IsNullOrEmpty()) {
                Log("Logging in with refresh code");
                requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://joystick.tv/api/oauth/token?refresh_token=" + UserRefreshToken + "&grant_type=refresh_token");
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

                string url = "https://joystick.tv/api/oauth/authorize?response_type=code&client_id=" + ClientID + "&scope=bot&state=" + State;

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
                Log("Logging in with auth code: " + requestMessage.RequestUri);

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
