/*
 * DIRC, a .NET DLL for integrating IRC with applications
 * Latest release as of: 02/26/11
 * 
 * by Andrew Brown
 * http://www.drusepth.net/
 * 
 * Description:
 * 
 * Provides a 
 * 
 * Usage:
 * var bot = new DIRC.Connection(nickname, server, port);
 * bot.AddChannel(default_channel);
 * bot.Connect();
 * bot.SendGlobalMessage("Hello, world!");
 * 
 */

using System;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

namespace DIRC
{
    public class Connection
    {
        // Bot Information
        private static string nickname;
        private static string server;
        private static int port;
        private static List<string> channels = new List<string>();

        // Socket Stuff
        private StreamWriter swrite;
        private StreamReader sread;
        private NetworkStream sstream;
        private TcpClient irc;
        private bool enabled;

        // Other information
        private string line; // incoming line
        private string[] splitLine; // array of line, expoded by \s

        // Interfaces
        public delegate void IRCLineHandler(string line);
        List<IRCLineHandler> handlers;

        // Prepare a new connection to IRC
        public Connection(string nickIn, string serverIn, int portIn)
        {
            nickname = nickIn;
            server = serverIn;
            port = portIn;

            handlers = new List<IRCLineHandler>();
        }

        // Add a channel to the list of channels to join
        public void AddChannel(string channel)
        {
            channels.Add(channel);
        }

        // Add a function to be called on every IRC line
        public void AddLineHandler(IRCLineHandler function)
        {
            handlers.Add(function);
        }

        // Connect to the specified server and identify
        public bool Connect()
        {
            irc = new TcpClient(server, port);
            sstream = irc.GetStream();
            sread = new StreamReader(sstream);
            swrite = new StreamWriter(sstream);

            Identify(nickname);

            enabled = true;
            Thread tIRC = new Thread(ircThread);
            tIRC.Start();

            return true;
        }

        // Set user/nick on IRC
        private bool Identify(string nick)
        {
            swrite.WriteLine("USER {0} {0} {0} :{1}", nick, nick);
            swrite.Flush();

            swrite.WriteLine("NICK {0}", nick);
            swrite.Flush();

            return true;
        }

        // Handle the IRC connection
        private void ircThread()
        {
            while (enabled)
            {
                if ((line = sread.ReadLine()) != null)
                {
                    // Call interface handlers first
                    for (int i = 0; i < handlers.Count; i++)
                    {
                        handlers[i](line);
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
                                for (int i = 0; i < channels.Count; i++)
                                {
                                    swrite.WriteLine("JOIN {0}", channels[i]);
                                    swrite.Flush();
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

            // Clean up
            swrite.Close();
            sread.Close();
            irc.Close();

        }

        // Get the channel the last message came from
        public string GetChannel()
        {
            return ParseIRC.GetChannel(line);
        }

        // Get who said the last message
        public string GetUsernameSpeaking()
        {
            return ParseIRC.GetUsernameSpeaking(line);
        }

        // Get the host of who said the last message
        public string GetHostSpeaking()
        {
            return ParseIRC.GetHostSpeaking(line);
        }

        // Get the message that was said last
        public string GetSpokenLine()
        {
            return ParseIRC.GetSpokenLine(line);
        }

        // Send a message to all joined channels
        public void SendGlobalMessage(string msg)
        {
            for (int i = 0; i < channels.Count; i++)
            {
                swrite.WriteLine("PRIVMSG {0} :{1}", channels[i], msg);
                swrite.Flush();
            }
        }

        // Send a private message to a specific user
        public void MessageUser(string user, string message)
        {
            swrite.WriteLine("NOTICE {0} :{1}", user, message);
            swrite.Flush();
        }

        // Send a public message to a channel
        public void MessageChannel(string channel, string message)
        {
            swrite.WriteLine("PRIVMSG {0} :{1}", channel, message);
            swrite.Flush();
        }
    }
}
