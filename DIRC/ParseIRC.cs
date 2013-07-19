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
