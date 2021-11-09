using System;
using System.Collections.Generic;
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
                
                Console.WriteLine(receivedData[4] == 0x41);

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

        public static ServerInfo QueryServer(string ip, int port)
        {
            return QueryServerAsync(ip, port).GetAwaiter().GetResult();
        }
    }
}