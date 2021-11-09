using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SteamServerQuery;

namespace SteamServerQuery.Tests
{
    public class ServerTests
    {
        public static async Task Main()
        {
            Dictionary<string, int> servers = new Dictionary<string, int>()
            {
                // Space engineers server
                { "131.153.23.218", 10816 },
                { "88.87.69.183", 27016 },
                { "93.186.198.191", 27016 },

                // Rust server
                { "147.135.104.162", 28015 },
                { "104.238.229.65", 28015 },

                // CSgo server
                { "145.239.5.44", 27000 }, // FIXME: CSgo server fails - "Output char buffer is too small to contain decoded characters, encoding utf8 fallback" Line 15 extensions.cs
                { "54.37.245.51", 25135 },
                { "185.114.224.63", 27169 },
                
                // Random servers/invalid
                { "0.0.0.0", 0 },
                { "192.168.0.1", 0 }
            };

            StringBuilder sb = new StringBuilder();

            int server = 0;
            foreach ((string ip, int port) in servers)
            {
                ConsoleColor color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Testing server {server}.");
                Console.ForegroundColor = color;
                try
                {
                    ServerInfo info = await SteamServer.QueryServerAsync(ip, port);

                    foreach (PropertyInfo property in typeof(ServerInfo).GetProperties())
                        sb.AppendLine($"{property.Name}: {property.GetValue(info)}");

                    Console.WriteLine(sb.ToString());
                    
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Test pass");
                    Console.ForegroundColor = color;
                    Console.WriteLine("\n");
                    sb.Clear();
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Test FAIL - {e}");
                    Console.ForegroundColor = color;
                }

                server++;
            }
        }
    }
}