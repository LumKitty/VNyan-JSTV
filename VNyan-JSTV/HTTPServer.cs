using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace VNyan_JSTV {
    internal class HTTPServer {
        public int _port;
        private HttpListener _listener;
        private string _targetState;

        public HTTPServer(int Port) {
            _port = Port;
            string Binding = "http://localhost:" + _port.ToString() + "/";
            _listener = new HttpListener();
            JSTV.Log("Attempting to bind to URL: " + Binding);
            _listener.Prefixes.Add(Binding);
        }

        public void Start(string TargetState) {
            _targetState = TargetState;
            _listener.Start();
            Receive();
        }

        public void Stop() {
            _listener.Stop();
        }

        private void Receive() {
            _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
        }

        private void ListenerCallback(IAsyncResult result) {
            if (_listener.IsListening) {
                var context = _listener.EndGetContext(result);
                var request = context.Request;
                string TempState = "";
                bool GotAuthCode = false;

                // do something with the request
                JSTV.Log($"Response: {request.Url}");


                string requestString = request.Url.ToString();

                int n = requestString.IndexOf('?') + 1;
                string queryString = requestString[n..];
                JSTV.Log(queryString);
                foreach (string query in queryString.Split('&')) {
                    JSTV.Log("Parsing: " + query);
                    int i = query.IndexOf('=');
                    string value = query[..i];
                    string data = query[(i + 1)..];
                    JSTV.Log("Value: " + value);
                    JSTV.Log("Data : " + data);
                    switch (value) {
                        case "code":
                            JSTV.TempAuthCode = data;
                            GotAuthCode = true;
                            break;
                        case "state":
                            TempState = data;
                            break;
                    }
                }

                var response = context.Response;
                if (GotAuthCode && (TempState == _targetState)) {
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.ContentType = "text/plain";
                    response.OutputStream.Write(Encoding.ASCII.GetBytes("Authorised, you may now close this tab"));
                    response.OutputStream.Close();
                    Stop();
                } else {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.ContentType = "text/plain";
                    string Result = "";
                    if (!GotAuthCode) { Result += "Auth code not received\n"; }
                    if (TempState != _targetState) { Result += "States did not match.\n"; }
                    response.OutputStream.Write(Encoding.ASCII.GetBytes(Result));
                    response.OutputStream.Close();
                    Receive();
                }
            }
        }
    }
}
