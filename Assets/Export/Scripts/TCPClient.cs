using Newtonsoft.Json.Linq;
using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

public static class TCPClient
{
    private static TcpClient sock;
    private static string msgs = "";

    public static void Start()
    {
        sock = new TcpClient("192.168.190.100", 1882);
        Thread thread = new Thread(new ThreadStart(Listen))
        {
            IsBackground = true
        };
        thread.Start();
    }

    public static void Update()
    {
        string[] lines = msgs.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string l in lines)
        {
            App.Log(l);
        }
        msgs = "";
    }

    private static void Listen()
    {
        byte[] lenbuf;
        byte[] buf;
        while (true)
        {
            NetworkStream stream = sock.GetStream();
            lenbuf = ReadBytes(stream, 4);
            Array.Reverse(lenbuf);
            int size = BitConverter.ToInt32(lenbuf, 0);
            buf = ReadBytes(stream, size);
            string msg_str = Encoding.UTF8.GetString(buf);
            msg_str = Regex.Replace(msg_str, @"\s+", "");
            msgs += "> " + DateTime.UtcNow + " : " + msg_str + "\n";

            JObject obj = JObject.Parse(msg_str);
            /**
             * Process message
             */
            if (obj["msgtype"].Value<int>() == 4)
            {
                JObject msg = obj["msg"].ToObject<JObject>();
                string table = msg["table"].ToString();
                string data = msg["data"].ToString();
                App.UpdateTable(table, data);
            }

        }

        static byte[] ReadBytes(NetworkStream stream, int length)
        {
            int read = 0, offset = 0;
            byte[] buf = new byte[length];

            while (length > offset + 1)
            {
                read = stream.Read(buf, offset, length - offset);
                offset += read;
            }

            return buf;
        }
    }

    public static void SendMessage(object msg)
    {
        Thread thread = new Thread(new ThreadStart(() => Send(msg)))
        {
            IsBackground = true
        };
        thread.Start();
    }

    private static void Send(object msg)
    {
        if (sock == null)
        {
            return;
        }

        NetworkStream stream = sock.GetStream();
        if (!stream.CanWrite)
        {
            return;
        }

        string msg_str = JObject.FromObject(msg).ToString();
        msg_str = Regex.Replace(msg_str, @"\s+", "");
        byte[] bytes = Encoding.UTF8.GetBytes(msg_str);
        byte[] len = BitConverter.GetBytes(bytes.Length);
        msgs += "< " + DateTime.UtcNow + " : " + msg_str + "\n";
        stream.Write(len, 0, len.Length);
        stream.Write(bytes, 0, bytes.Length);
    }
}
