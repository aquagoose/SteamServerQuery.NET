using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SteamServerQuery
{
    public static class SteamServer
    {
        /// <summary>
        /// Query a steam server with the given IP and port, asynchronously.
        /// </summary>
        /// <param name="ip">The IP of the server.</param>
        /// <param name="port">The port of the server.</param>
        /// <returns>The available info the server returned.</returns>
        public static async Task<ServerInfo> QueryServerAsync(string ip, int port)
        {
            using (UdpClient client = new UdpClient(ip, port))
            {
                // Our request header to send over
                byte[] requestHeader =
                {
                    0xFF, 0xFF, 0xFF, 0xFF, 0x54, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67, 0x69,
                    0x6E, 0x65, 0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00
                };
                
                // Send data and wait for response.
                await client.SendAsync(requestHeader, requestHeader.Length);
                UdpReceiveResult received = await client.ReceiveAsync();
                byte[] receivedData = received.Buffer;

                // Oops! The server returned a challenge number.
                if (receivedData[4] == 0x41)
                {
                    // Create a new request header and append new request header data on the end of it.
                    List<byte> newRequestHeader = new List<byte>(requestHeader);
                    for (int i = 5; i < receivedData.Length; i++)
                        newRequestHeader.Add(receivedData[i]);

                    // Send and await response.
                    await client.SendAsync(newRequestHeader.ToArray(), newRequestHeader.Count);
                    received = await client.ReceiveAsync();
                    receivedData = received.Buffer;
                }

                return new ServerInfo(receivedData);
            }
        }

        public static ServerInfo QueryServer(string ip, int port)
        {
            return QueryServerAsync(ip, port).GetAwaiter().GetResult();
        }
    }
}