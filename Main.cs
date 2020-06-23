using XLMultiplayerServer;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace XLSMapRotation
{
    public class Main
    {
        private static bool firstRun;
        private static XLMultiplayerServer.Server server;
        private static string currentMap { get; set;}

        [JsonProperty("Interval")]
        private static int INTERVAL { get; set; } = 15;
        public static void Load(XLMultiplayerServer.Server s)
        {
            if (File.Exists(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar.ToString() + "\\Plugins\\Macs.XLSMapRotation\\Config.json"))
            {
                JsonConvert.DeserializeObject<Main>(File.ReadAllText(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar.ToString() + "\\Plugins\\Macs.XLSMapRotation\\Config.json"));
            }
            Main.server = s;
            currentMap = Main.server.currentMapHash;
            ChangeMap();
            ClearMaps();
        }

        private static async void ClearMaps()
        {
            while (true)
            {
                foreach (XLMultiplayerServer.Player player in Main.server.players)
                {
                    if (player != null)
                    {
                        if (player.currentVote != "current") {
                            player.currentVote = currentMap;
                        }
                    }
                }
                await Task.Delay(5000);
            }
        }

        private static async void ChangeMap()
        {
            while (true)
            {
                if (!Main.firstRun)
                {
                    firstRun = true;
                } else
                {
                    Random ran = new Random();
                    string choice = Main.server.mapList.ElementAt(ran.Next(0, Main.server.mapList.Count - 1)).Key;
                    string mapName = Main.server.mapList[choice];
                    byte[] message = server.ProcessMessageCommand("msg:" + 10 + ":" + "b26ade" + " Changing Map To: " + mapName);
                    Main.server.LogMessageCallback("[XLSMapRotation] Changing Map To: " + mapName, ConsoleColor.Green);
                    foreach (XLMultiplayerServer.Player player in Main.server.players)
                    {
                   
                        if (player != null)
                        {
                            player.currentVote = choice;
                            Main.server.fileServer.server.SendMessageToConnection(player.connection, message, Valve.Sockets.SendFlags.Reliable);
                        }
                    }
                    currentMap = choice;
                }     
                await Task.Delay(INTERVAL * 60000);
            }
           
        }
    }
}
