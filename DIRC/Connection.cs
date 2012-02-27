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

        // Prepare a new connection to IRC
        public Connection(string nickIn, string serverIn, int portIn)
        {
            nickname = nickIn;
            server = serverIn;
            port = portIn;
        }

        // Add a channel to the list of channels to join
        public void AddChannel(string channel)
        {
            channels.Add(channel);
        }

        // Connect to the specified server and identify
        public bool Connect()
        {
            irc = new TcpClient(server, port);
            sstream = irc.GetStream();
            sread = new StreamReader(sstream);
            swrite = new StreamWriter(sstream);

            Identify(nickname);

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
            return line.Split(' ')[2];
        }

        // Get who said the last message
        public string GetUsernameSpeaking()
        {
            return line.Split('!')[0].Split(':')[1];
        }

        // Get the host of who said the last message
        public string GetHostSpeaking()
        {
            return line.Split('!')[1].Split(' ')[0];
        }

        // Get the message that was said last
        public string GetSpokenLine()
        {
            if (line.Split(':').Length >= 2)
                return line.Split(':')[2];

            return "";
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
    }
}
