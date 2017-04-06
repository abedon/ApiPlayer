using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace AutoTest.Common
{
    public static class Utils
    {
        public static string GetFormattedXml(string strXml)
        {
            var sbXml = new StringBuilder();
            var strWriter = new StringWriter(sbXml);
            var xmlWriter = new XmlTextWriter(strWriter);
            xmlWriter.Formatting = Formatting.Indented;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(strXml.Replace("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">", ""));
                xmlDoc.WriteTo(xmlWriter);
            }
            catch (Exception)
            {
                return strXml;
            }

            return sbXml.ToString();
        }

        public static string GetStringFromFile(string addr, string tmServer = null, NetworkCredential credential = null)
        {
            if (addr.ToLower().StartsWith("http"))
            {
                var addrUri = new Uri(addr);
                if (!string.IsNullOrWhiteSpace(tmServer))
                    addr = addr.Replace(addrUri.Host, tmServer);
                var webClient = new WebClient();
                if (credential != null)
                    webClient.Credentials = credential;
                webClient.Headers.Set("Pragma", "no-cache");
                webClient.Headers.Set("Host", addrUri.Host);
                return webClient.DownloadString(addr);
            }
            else
                return File.ReadAllText(addr);
        }
        
        // time is defined in milliseconds
        public static long GetTime()
        {
            return DateTime.Now.Ticks / (TimeSpan.TicksPerMillisecond);
        }

        public static int[] StringArrayToIntegerArray(string[] _stringArray)
        {
            List<int> tempInts = new List<int>();

            try
            {
                foreach (string parameter in _stringArray)
                {
                    tempInts.Add(int.Parse(parameter));
                }
            }
            catch (FormatException)
            {
                throw new System.FormatException("  Input string contains non-numberic test ID.  Please use integers only.");
            }

            return tempInts.ToArray();
        }

        public static string GetLocalIP()
        {
            IPAddress[] ipAddresses = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (IPAddress ipAddress in ipAddresses)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork && !ipAddress.IsIPv6LinkLocal)
                {
                    return ipAddress.ToString();
                }
            }

            return "";
        }

        #region Communication Infrastructure
        public static string GetBaseResponse(Uri uri, RequestType method, WebHeaderCollection headers, string strRequestXml, bool getNtlmMsg = false, Socket s = null)
        {
            try
            {
                if (s == null)
                    s = GetTcpConnection(uri.Host, uri.Port);

                try
                {
                    if (s.Connected)
                    {
                        int bytesRecv = 0;
                        string output = "";
                        byte[] bytes;

                        string Command = method.ToString().ToUpper() + " " + uri.PathAndQuery + " HTTP/1.1\r\n";
                        Command += "Pragma: no-cache\r\n";
                        Command += "Host: " + uri.Host + "\r\n";

                        if (headers != null)
                        {
                            foreach (var headerKey in headers.AllKeys)
                            {
                                Command += headerKey + ": " + headers[headerKey] + "\r\n";
                            }
                        }

                        if (!string.IsNullOrEmpty(strRequestXml))
                        {
                            Command += "Content-Type: " + (strRequestXml.TrimStart().StartsWith("<") ? "application/xml" : "application/json") + "\r\n";
                            Command += "Content-Length: " + strRequestXml.Length + "\r\n";
                        }

                        Command += "\r\n";
                        Command += strRequestXml;

                        byte[] msg = Encoding.Default.GetBytes(Command);

                        //"Sent " + intSend.ToString() + " bytes to server"
                        int intSend = s.Send(msg, msg.Length, 0);

                        do
                        {
                            bytes = new byte[4096];
                            bytesRecv = s.Receive(bytes, bytes.Length, SocketFlags.None);
                            output += Encoding.Default.GetString(bytes, 0, bytesRecv);
                            Thread.Sleep(100);
                        } while (s.Connected && s.Available > 0);

                        if (getNtlmMsg)
                        {
                            int iStartPos = output.IndexOf("WWW-Authenticate:");
                            int iEndPos = output.IndexOf("\r\n", iStartPos);
                            output = output.Substring(iStartPos, iEndPos - iStartPos).Replace("WWW-Authenticate: NTLM", "").Trim();
                        }
                        else
                        {
                            while (output.Contains("HTTP/1.1 207 Multi Status") && (output.IndexOf("\r\n\r\n") + 4) < output.Length)
                            {
                                output = output.Substring(output.IndexOf("\r\n\r\n") + 4);
                            }

                            if (output.Trim().Length == 0)
                                throw new ApplicationException("No response received for [" + uri.PathAndQuery + "].");
                        }

                        return output;
                    }
                    else
                        throw new ApplicationException("Can't create IPEndPoint from " + uri.Host);
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    if (!getNtlmMsg)
                        CloseTcpConnection(s);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Socket GetTcpConnection(string host, int port)
        {
            IPAddress[] IPs = Dns.GetHostAddresses(host);
            IPAddress ip = null;
            IPEndPoint rip = null;
            Socket s = null;

            if (IPs != null)
            {
                if (host.ToLower() == "localhost")
                    ip = IPs[1];
                else
                    ip = IPs[0];
            }

            if (ip != null)
                rip = new IPEndPoint(ip, port);

            try
            {
                if (rip != null)
                {
                    s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    s.Connect(rip);
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return s;
        }

        public static void CloseTcpConnection(Socket s)
        {
            if (s != null && s.Connected)
            {
                s.Shutdown(SocketShutdown.Both);
                s.Close();
            }
        }
        #endregion
    }
}
