using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SteamServerQuery.Tests
{
    [TestClass]
    public class SteamServerTests
    {
        [TestMethod]
        public async Task TestServerQuery()
        {
            (string, int, bool)[] servers = new (string, int, bool)[]
            {
                // Space engineers server
                ( "131.153.23.218", 10816, false ),
                ( "88.87.69.183", 27016, false ),
                ( "93.186.198.191", 27016, false ),

                // Rust server
                ( "147.135.104.162", 28015, false ),
                ( "104.238.229.65", 28015, false ),

                // CSgo server
                ( "145.239.5.44", 27000, false ), // FIXME: CSgo server fails - "Output char buffer is too small to contain decoded characters, encoding utf8 fallback" Line 15 extensions.cs
                ( "54.37.245.51", 25135, false ),
                ( "185.114.224.63", 27169, false ),
                
                // Random servers/invalid
                ( "0.0.0.0", 0, true ),
                ( "108.61.156.4", 443, true )
            };

            StringBuilder sb = new StringBuilder();

            int server = 0;
            foreach ((string ip, int port, bool shouldFail) in servers)
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
                    if (shouldFail)
                    {
                        Console.WriteLine(e);
                        Console.WriteLine("\n");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Test pass");
                        Console.ForegroundColor = color;
                        Console.WriteLine("\n");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Test FAIL - {e}");
                        Console.ForegroundColor = color;
                    }
                }

                server++;
            }
        }
    }
}