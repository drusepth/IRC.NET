using System;

namespace DIRC
{
    public class ParseIRC
    {
        public static string GetSpokenLine(string line)
        {
            // :drusepth!drusepth@no-jk1.iiu.5u85no.IP PRIVMSG #test :this is a test

            // Split off first :
            line = line.Substring(1);

            // Strip off everything before next :
            line = line.Substring(line.IndexOf(':') + 1);

            return line;
        }

        public static string GetMessageWindowContext(string line)
        {
            string[] line_parts = line.Split(' ');
            
            // Determine what channel this message is regarding
            switch (line_parts[1])
            {
                // Notices can come from the server or other users
                case "NOTICE":
                    if (line_parts[2] == "Auth")
                    {
                        return line_parts[0]; // Server address
                    } else
                    {
                        return GetUsernameSpeaking(line);
                    }

                // Private messages can come from other users or channels
                case "PRIVMSG":
                    return GetChannel(line);

                // We don't really care about mode changes yet
                case "MODE":
                    return "~background";

                // We also don't really care about people joining/leaving channels yet either
                case "JOIN":
                    return line_parts[2].Split(':')[1];

                // Server-relevant messages
                case "001":
                case "002":
                case "003":
                case "004":
                case "005":
                case "042":
                case "251":
                case "252":
                case "254":
                case "255":
                case "265":
                case "266":
                case "372":
                case "375":
                case "376":
                case "396":
                    return line_parts[0];

                // Channel-relevant messages
                case "332": // TOPIC
                case "333": // TOPIC set-by line
                case "366": // End-of-NAMES message
                    return line_parts[3];
                case "353": // NAMES list
                    return line_parts[4];
                
                // For everything we haven't categorized yet, just chuck it in an unsorted tab for reference
                default:
                    return "~background";
            }
        }

        public static string GetMessageTypeIdentifier(string line)
        {
            return line.Split(' ')[1];
        }

        public static string GetHostSpeaking(string line)
        {
            return line.Split('!')[1].Split(' ')[0];
        }

        public static string GetUsernameSpeaking(string line)
        {
            return line.Split('!')[0].Split(':')[1];
        }

        public static string GetChannel(string line)
        {
            return line.Split(' ')[2];
        }

        public static bool IsMessage(string line)
        {
            string[] splitLine = line.Split(' ');
            return (splitLine.Length > 0 && splitLine[1] == "PRIVMSG");
        }

        public static bool IsPing(string line)
        {
            string[] splitLine = line.Split(' ');
            return (splitLine.Length > 0 && splitLine[0] == "PING");
        }
    }
}
