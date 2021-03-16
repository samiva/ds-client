using System;
using IniParser;
using IniParser.Model;

namespace BombPeli
{
    static class Config
    {
        private static IniData data;
        public static int GetPort()
        {
            var port = int.Parse(data["client"]["localport"]);
            return port;
        }

        public static void OpenIni(string filename)
        {
            var parser = new FileIniDataParser();
            data = parser.ReadFile(filename);
        }

        public static Role MyRole{get; set;}
    }
}