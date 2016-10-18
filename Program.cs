using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LPRWFF
{
    class Program
    {

        public static string LoadUrl(string url)
        {
            HttpWebRequest test = (HttpWebRequest)HttpWebRequest.Create(url);
            test.Timeout = 30 * 1000;
            test.AllowAutoRedirect = true;
            WebResponse res = test.GetResponse();
            StreamReader reader = null;
            string data = null;
            try
            {
                var cs = ((HttpWebResponse)res).CharacterSet;
                reader = new StreamReader(res.GetResponseStream(), string.IsNullOrEmpty(cs) ? System.Text.Encoding.Default : System.Text.Encoding.GetEncoding(cs));
                data = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to load the URL: " + ex.Message);
            }
            finally
            {
                if (reader != null) reader.Close();
                if (res != null) res.Close();
            }
            return data;
        }


        static void Main(string[] args)
        {
            string User = "admin";
            string Password = "SMRT@admin";
            string Server = "http://smrt.workflowfirst.net/vms2";
            int Port = 8002;
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "lprwff.cfg")))
            {
                // if there is an email server, then use that to retrieve the username etc.
                foreach (string line in File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "emailserver.cfg")))
                {
                    if (line.Contains("="))
                    {
                        string[] items = line.Split('=');
                        if (items[0] == "User") User = items[1];
                        if (items[0] == "Password") Password = items[1];
                        if (items[0] == "Server") Server = items[1];
                        if (items[0] == "LPRPort") Port = int.Parse(items[1]);
                    }
                }
            }


            IPEndPoint ServerEndPoint = new IPEndPoint(IPAddress.Any, Port);
            Socket WinSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            WinSocket.Bind(ServerEndPoint);

            Console.Write("Waiting for sender\r\n");
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);
            byte[] data = new byte[64000];
            while (true)
            {
                try
                {
                    int recv = WinSocket.ReceiveFrom(data, ref Remote);
                    Console.WriteLine("Message received from {0}:", Remote.ToString());
                    string str = Encoding.ASCII.GetString(data, 0, recv);
                    string license = str.Split(' ')[0];
                    Console.WriteLine(license);
                    // send it to WFF...
                    string json = "{ \"LP\": \"" + license + "\", \"Source\":\"LPR\" }";
                    json = System.Web.HttpUtility.UrlEncode(json);
                    LoadUrl(Server.Trim('/') + "/runfunction.aspx?USERNAME=" + User + "&PASSWORD=" + Password + "&id=Functions:_SeenLP&_format=json&json=" + json + "_=" + Guid.NewGuid().ToString().Replace("-", ""));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: " + ex.Message);
                    System.Threading.Thread.Sleep(5000);

                }
            }
        }
    }
}
