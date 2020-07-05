using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using static DIRC.Connection;

namespace DIRC.Models
{
    public class Network
    {
        public string Server { get; set; }
        public int Port { get; set; } = 6667;
        public List<Channel> ActiveChannels { get; set; } = new List<Channel>();
        public List<Channel> AutojoinChannels { get; set; } = new List<Channel>();

        public List<IRCLineHandler> LineHandlers { get; set; } = new List<IRCLineHandler>();

        // Socket stuff
        private StreamWriter swrite;
        private StreamReader sread;
        private NetworkStream sstream;
        private TcpClient irc;

        public bool Enabled { get; set; } = false;

        public Network(string server, int port, string[] autojoin_channel_names)
        {
            Server = server;
            Port = port;

            foreach (string channel_name in autojoin_channel_names)
            {
                AutojoinChannels.Add(new Channel(channel_name));
            }
        }

        public Network(string server, int port)
        {
            Server = server;
            Port = port;
        }

        public Network(string server)
        {
            Server = server;
        }

        public List<Channel> AddAutojoinChannel(string channel_name)
        {
            AddAutojoinChannel(new Channel(channel_name));
            return AutojoinChannels;
        }

        public List<Channel> AddAutojoinChannel(Channel channel)
        {
            AutojoinChannels.Add(channel);
            return AutojoinChannels;
        }
        
        public bool ConnectAs(User user)
        {
            irc = new TcpClient(Server, Port);
            sstream = irc.GetStream();
            sread = new StreamReader(sstream);
            swrite = new StreamWriter(sstream);

            IdentifyAs(user);
            
            Thread tIRC = new Thread(IrcThread);
            tIRC.Start();

            Enabled = true;
            return Enabled;
        }

        private bool IdentifyAs(User user)
        {
            swrite.WriteLine("USER {0} {0} {0} :{1}", user.Nickname, user.Nickname);
            swrite.Flush();

            swrite.WriteLine("NICK {0}", user.Nickname);
            swrite.Flush();

            // TODO: negotiate a new nickname if this one is taken

            return true;
        }

        // Handle the IRC connection
        private void IrcThread()
        {
            string line;        // incoming line
            string[] splitLine; // array of line, expoded by \s

            while (Enabled)
            {
                if ((line = sread.ReadLine()) != null)
                {
                    // Call interface handlers first
                    for (int i = 0; i < LineHandlers.Count; i++)
                    {
                        LineHandlers[i](line);
                    }

                    splitLine = line.Split(' ');
                    if (splitLine.Length > 0)
                    {
                        switch (splitLine[1])
                        {
                            case "366":
                                break;

                            case "376":
                            case "422":
                                for (int i = 0; i < AutojoinChannels.Count; i++)
                                {
                                    swrite.WriteLine("JOIN {0}", AutojoinChannels[i].Name);
                                    swrite.Flush();

                                    // TODO: It might be better to wait for the "you joined channel X" message to add to active channels
                                    ActiveChannels.Add(AutojoinChannels[i]);
                                }
                                break;
                        }

                        if (splitLine[0] == "PING")
                        {
                            swrite.WriteLine("PONG {0}", splitLine[1]);
                            swrite.Flush();
                        }
                    }
                }
            }

            Disconnect();
        }

        // Send a message to all joined channels
        public void SendGlobalMessage(string msg)
        {
            for (int i = 0; i < ActiveChannels.Count; i++)
            {
                swrite.WriteLine("PRIVMSG {0} :{1}", ActiveChannels[i], msg);
                swrite.Flush();
            }
        }

        // Send a private message to a specific user
        public void SendUserMessage(string user, string message)
        {
            swrite.WriteLine("NOTICE {0} :{1}", user, message);
            swrite.Flush();
        }

        // Send a public message to a channel
        public void SendChannelMessage(string channel, string message)
        {
            swrite.WriteLine("PRIVMSG {0} :{1}", channel, message);
            swrite.Flush();
        }

        public void Disconnect()
        {
            Enabled = false;

            swrite.Close();
            sread.Close();
            irc.Close();
        }
    }
}
