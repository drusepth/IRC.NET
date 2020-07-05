/*
 * DIRC, a .NET DLL for integrating IRC with applications
 * Latest release as of: 07/04/2020
 * 
 * by Andrew Brown
 * http://www.drusepth.net/
 * 
 * Description:
 * 
 * Provides an easy-to-use interface for applications to open IRC connections
 * and manage a standard IRC presence.
 * 
 * Usage:
 * var bot = new DIRC.Connection(nickname, server, port);
 * bot.AddAutojoinChannel(default_channel);
 * bot.Connect();
 * bot.SendGlobalMessage("Hello world!");
 * 
 */

using System;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using DIRC.Models;

namespace DIRC
{
    public class Connection
    {
        public readonly User user;
        public readonly Network network;

        // Interfaces
        public delegate void IRCLineHandler(string line);

        public Connection(User userIn, Network networkIn)
        {
            user = userIn;
            network = networkIn;
        }

        public Connection(string nickIn, Network networkIn)
        {
            user = new User(nickIn);
            network = networkIn;
        }

        // Prepare a new connection to IRC
        public Connection(string nickIn, string serverIn, int portIn)
        {
            user = new User(nickIn);
            network = new Network(serverIn, portIn);
        }

        // Add a channel to the list of channels to join when we connect
        public void AddAutojoinChannel(string channel)
        {
            network.AddAutojoinChannel(new Channel(channel));
        }

        public void JoinChannel(string channel)
        {
            network.JoinChannel(new Channel(channel));
        }

        // Add a function to be called on every IRC line
        public void AddLineHandler(IRCLineHandler function)
        {
            network.LineHandlers.Add(function);
        }

        // Connect to the specified server and identify
        public bool Connect()
        {
            network.ConnectAs(user);
            return network.Enabled;
        }

        public bool Disconnect()
        {
            network.Disconnect();
            return network.Enabled;
        }

        // Send a message to all joined channels
        public void SendGlobalMessage(string msg)
        {
            network.SendGlobalMessage(msg);
        }

        // Send a private message to a specific user
        public void SendUserMessage(string user, string message)
        {
            network.SendUserMessage(user, message);
        }

        // Send a public message to a channel
        public void SendChannelMessage(string channel, string message)
        {
            network.SendChannelMessage(channel, message);
        }

        /*
         * 
         * TODO move these into ParseIRC if they aren't already there
         * 
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

        // Get all channels a user is in
        public List<string> UserChannels(string name)
        {
            // :i247.at.tddirc.net 307 testt clueless :is a registered nick
            // :i247.at.tddirc.net 319 testt clueless :#thunked
            // :i247.at.tddirc.net 312 testt clueless ddg.us.tddirc.net :TDDIRC US (DDG)
            // :i247.at.tddirc.net 671 testt clueless :is using a Secure Connection
            // :i247.at.tddirc.net 318 testt CLUELESS :End of /WHOIS list.
            swrite.WriteLine("WHOIS {0}", name);
            swrite.Flush();

            List<string> channels = new List<string>();

            string line;
            while ((line = sread.ReadLine()) != null)
            {
                string[] splitLine = line.Split(' ');

                if (splitLine[1] == "318")
                {
                    break;
                }

                if (splitLine[1] == "319")
                {
                    for (int i = 4; i < splitLine.Length; i++) {

                        string sanitized_channel = splitLine[i]
                            .Replace(':', ' ')
                            .Replace('+', ' ')
                            .Replace('%', ' ')
                            .Replace('@', ' ')
                            .Replace('&', ' ')
                            .Replace('~', ' ')
                            .Replace('#', ' ')
                            .Trim();

                        if (sanitized_channel.Length > 0 && !channels.Contains(sanitized_channel))
                            channels.Add(sanitized_channel);
                    }
                }
            }

            return channels;
        }

        // Get all users in a channel
        public List<string> UsersInChannel(string channel)
        {
            // :hub.irc.amazdong.com 353 boros = #dongs :boros @dru
            // :hub.irc.amazdong.com 366 boros #dongs :End of /NAMES list.
            swrite.WriteLine("NAMES {0}", channel);
            swrite.Flush();

            List<string> names = new List<string>();

            string line;
            while ((line = sread.ReadLine()) != null)
            {
                string[] splitLine = line.Split(' ');

                if (splitLine[1] == "366")
                    break;

                if (splitLine[1] == "353")
                {
                    for (int i = 5; i < splitLine.Length; i++)
                    {
                        string sanitized_name = splitLine[i]
                            .Replace(':', ' ')
                            .Replace('+', ' ')
                            .Replace('%', ' ')
                            .Replace('@', ' ')
                            .Replace('&', ' ')
                            .Replace('~', ' ')
                            .Trim();

                        if (sanitized_name.Length > 0 && !names.Contains(sanitized_name))
                        {
                            names.Add(sanitized_name);
                        }
                    }
                }
            }

            return names;
        }
        */
    }
}
