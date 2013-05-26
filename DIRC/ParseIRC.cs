using System;

namespace DIRC
{
    public class ParseIRC
    {
        public static string GetSpokenLine(string line)
        {
            if (line.Split(':').Length >= 2)
                return line.Split(':')[2];

            return "";
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
    }
}
