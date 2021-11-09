using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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
        /// <param name="timeout">The number of milliseconds the request will time out if no data is received.</param>
        /// <returns>The available info the server returned.</returns>
        public static async Task<ServerInfo> QueryServerAsync(string ip, int port, int timeout = 5000)
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
                byte[] receivedData = await SendReceiveData(requestHeader, client, timeout);

                // Oops! The server returned a challenge number.
                if (receivedData[4] == 0x41)
                {
                    // Create a new request header and append new request header data on the end of it.
                    List<byte> newRequestHeader = new List<byte>(requestHeader);
                    for (int i = 5; i < receivedData.Length; i++)
                        newRequestHeader.Add(receivedData[i]);

                    // Send and await response.
                    receivedData = await SendReceiveData(newRequestHeader.ToArray(), client, timeout);
                }

                return new ServerInfo(receivedData);
            }
        }

        /// <summary>
        /// Get an array of players & their info on the given server, asynchronously.
        /// </summary>
        /// <param name="ip">The IP of the server.</param>
        /// <param name="port">The port of the server.</param>
        /// <param name="timeout">The number of milliseconds the request will time out if no data is received.</param>
        /// <returns>An array of <see cref="PlayerInfo"/>.</returns>
        public static async Task<PlayerInfo[]> QueryPlayersAsync(string ip, int port, int timeout = 5000)
        {
            using (UdpClient client = new UdpClient(ip, port))
            {
                byte[] requestHeader =
                {
                    0xFF, 0xFF, 0xFF, 0xFF, 0x55, 0xFF, 0xFF, 0xFF, 0xFF
                };

                byte[] recievedData = await SendReceiveData(requestHeader, client, timeout);

                if (recievedData[4] == 0x41)
                {
                    recievedData[4] = 0x55;

                    recievedData = await SendReceiveData(recievedData, client, timeout);
                }

                using (MemoryStream memoryStream = new MemoryStream(recievedData))
                {
                    using (BinaryReader reader = new BinaryReader(memoryStream))
                    {
                        // Useless data (all F's)
                        reader.ReadBytes(4);
                        if (reader.ReadByte() != 0x44)
                            throw new SteamException(
                                "The data received from the server is not valid - header data must equal 0x44");
                        int players = reader.ReadByte();
                        List<PlayerInfo> infos = new List<PlayerInfo>();
                        for (int i = 0; i < players; i++)
                        {
                            // Index - we don't need this value
                            reader.ReadByte();
                            string name = reader.ReadNullTerminatedString();
                            int score = reader.ReadInt32();
                            float duration = reader.ReadSingle();
                            infos.Add(new PlayerInfo(name, score, duration));
                        }

                        return infos.ToArray();
                    }
                }
            }
        }

        private static async Task<byte[]> SendReceiveData(byte[] data, UdpClient client, int timeout)
        {
            IAsyncResult awaitResult = client.BeginSend(data, data.Length, null, null);
            if (awaitResult == null)
                throw new SteamException(
                    "An error occurred when sending the header data - The await result was null.");
            awaitResult.AsyncWaitHandle.WaitOne(timeout);
            if (!awaitResult.IsCompleted)
                throw new SteamException("Request to server timed out when trying to send header data.");
            client.EndSend(awaitResult);
            awaitResult = null;
                
            awaitResult = client.BeginReceive(null, null);
            if (awaitResult == null)
                throw new SteamException("An error occurred when receiving data - The await result was null.");
            awaitResult.AsyncWaitHandle.WaitOne(timeout);
            if (!awaitResult.IsCompleted)
                throw new SteamException("A connection was made, however a request to server timed out when trying to receive data.");

            IPEndPoint endPoint = null;
            byte[] receivedData = client.EndReceive(awaitResult, ref endPoint);
            return receivedData;
        }


        /// <summary>
        /// Query a steam server with the given IP and port, synchronously.
        /// </summary>
        /// <param name="ip">The IP of the server.</param>
        /// <param name="port">The port of the server.</param>
        /// <param name="timeout">The number of milliseconds the request will time out if no data is received.</param>
        /// <returns>The available info the server returned.</returns>
        public static ServerInfo QueryServer(string ip, int port, int timeout = 5000)
        {
            return QueryServerAsync(ip, port, timeout).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get an array of players & their info on the given server, synchronously.
        /// </summary>
        /// <param name="ip">The IP of the server.</param>
        /// <param name="port">The port of the server.</param>
        /// <param name="timeout">The number of milliseconds the request will time out if no data is received.</param>
        /// <returns>An array of <see cref="PlayerInfo"/>.</returns>
        public static PlayerInfo[] QueryPlayers(string ip, int port, int timeout = 5000)
        {
            return QueryPlayersAsync(ip, port, timeout).GetAwaiter().GetResult();
        }
    }
}