using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyNetworking;
using EasyNetworking.IRC;

namespace IRC_client
{
    public static class Manager
    {
        private static FrameworkIRC irc;
        public static event EventHandler Registered;
        public static event EventHandler<IRC_EventArgs> ChannelJoin;
        public static event EventHandler<Socket_EventArgs> DataReceived;
        public static event EventHandler<IRC_EventArgs> MessageReceived;

        public static bool Connect(string host, int port = 6667)
        {
            try
            {
                irc = new FrameworkIRC(host, port);

                irc.Registered += OnRegistered;
                irc.ChannelJoin += OnChannelJoin;
                irc.DataReceived += OnDataReceived;
                irc.MessageReceived += OnMessageReceived;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string Nick
        {
            get
            {
                return irc.Nick;
            }
        }

        public static void SendMessage(string recpt, string msg)
        {
            if (irc != null)
                irc.SendMessage(recpt, msg);
        }

        public static void SetNick(string nick)
        {
            if (irc != null)
                irc.SetNick(nick);
        }

        private static void OnDataReceived(object sender, Socket_EventArgs e)
        {
            DataReceived?.Invoke(sender, e);
        }

        private static void OnMessageReceived(object sender, IRC_EventArgs e)
        {
            MessageReceived?.Invoke(sender, e);
        }

        private static void OnChannelJoin(object sender, IRC_EventArgs e)
        {
            ChannelJoin?.Invoke(sender, e);
        }

        private static void OnRegistered(object sender, EventArgs e)
        {
            Registered?.Invoke(sender, e);
        }

        public static void JoinChannel(string channel, string password = "")
        {
            if (irc != null)
                irc.JoinChannel(channel, password);
        }

        public static void Disconnect(string reason = "Leaving")
        {
            if (irc != null)
            {
                irc.Close(reason);
                irc = null;
            }
        }

    }
}
