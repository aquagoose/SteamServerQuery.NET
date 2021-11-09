using System;
using System.IO;

#nullable enable
namespace SteamServerQuery
{
    public struct ServerInfo
    {
        /// <summary>
        /// The protocol version the server uses.
        /// </summary>
        public int Protocol { get; }
        
        /// <summary>
        /// The name of the server.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// The map the server currently has loaded.
        /// </summary>
        public string Map { get; }
        
        /// <summary>
        /// The name of the folder containing the game files.
        /// </summary>
        public string Folder { get; }
        
        /// <summary>
        /// The full name of the game.
        /// </summary>
        public string Game { get; }
        
        /// <summary>
        /// The Steam application ID of the game.
        /// </summary>
        public int Id { get; }
        
        /// <summary>
        /// The current number of players currently on the server.
        /// </summary>
        public int Players { get; }
        
        /// <summary>
        /// The maximum number of players allowed on the server.
        /// </summary>
        public int MaxPlayers { get; }
        
        /// <summary>
        /// The number of bots currently on the server.
        /// </summary>
        public int Bots { get; }
        
        /// <summary>
        /// The type of server.
        /// </summary>
        public ServerType ServerType { get; }
        
        /// <summary>
        /// The operating system of the server.
        /// </summary>
        public ServerEnvironment Environment { get; }
        
        /// <summary>
        /// Whether the server requires a password.
        /// </summary>
        public Visibility Visibility { get; }
        
        /// <summary>
        /// Whether the server uses VAC.
        /// </summary>
        public VacStatus Vac { get; }
        
        /// <summary>
        /// The version of the game installed on the server.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// The port number of the server. This may be 0.
        /// </summary>
        public int? Port { get; }
        
        /// <summary>
        /// The server's steam ID. This may be 0.
        /// </summary>
        public long? SteamId { get; }
        
        /// <summary>
        /// The SourceTV spectator port number. Only valid if the server uses SourceTV.
        /// </summary>
        public int? SourceTvPort { get; }
        
        /// <summary>
        /// The name of the server for SourceTV. Only valid if the server uses SourceTV.
        /// </summary>
        public string? SourceTvName { get; }
        
        /// <summary>
        /// Keywords that describe the server. This may be blank.
        /// </summary>
        public string? Keywords { get; }
        
        /// <summary>
        /// The 64-bit ID of the server. This may be 0.
        /// </summary>
        public long? GameId { get; }

        internal ServerInfo(byte[] buffer)
        {
            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {
                    // These 4 bytes are useless - all F's
                    reader.ReadBytes(4);

                    if (reader.ReadByte() != 0x49)
                        throw new SteamException(
                            "The data received from the server is not valid - header data must equal 0x49");

                    Protocol = reader.ReadByte();
                    Name = reader.ReadNullTerminatedString();
                    Map = reader.ReadNullTerminatedString();
                    Folder = reader.ReadNullTerminatedString();
                    Game = reader.ReadNullTerminatedString();
                    Id = reader.ReadInt16();
                    Players = reader.ReadByte();
                    MaxPlayers = reader.ReadByte();
                    Bots = reader.ReadByte();
                    ServerType = (ServerType) reader.ReadByte();
                    Environment = (ServerEnvironment) reader.ReadByte();
                    Visibility = (Visibility) reader.ReadByte();
                    Vac = (VacStatus) reader.ReadByte();
                    Version = reader.ReadNullTerminatedString();

                    Port = -1;
                    SteamId = -1;
                    SourceTvPort = -1;
                    SourceTvName = String.Empty;
                    Keywords = String.Empty;
                    GameId = -1;
                    
                    try
                    {
                        // EDF
                        byte edf = reader.ReadByte();

                        if ((edf & 0x80) != 0)
                            Port = reader.ReadInt16();
                        if ((edf & 0x10) != 0)
                            SteamId = reader.ReadInt64();
                        if ((edf & 0x40) != 0)
                        {
                            SourceTvPort = reader.ReadInt16();
                            SourceTvName = reader.ReadNullTerminatedString();
                        }
                        if ((edf & 0x20) != 0)
                            Keywords = reader.ReadNullTerminatedString();
                        if ((edf & 0x01) != 0)
                            GameId = reader.ReadInt64();
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                }
            }
        }
    }

    public enum ServerType
    {
        Dedicated = 0x64,
        NonDedicated = 0x6C,
        SourceTv = 0x70
    }

    public enum ServerEnvironment
    {
        Linux = 0x6C,
        Windows = 0x77,
        MacLegacy = 0x6D,
        Mac = 0x6F
    }

    public enum Visibility
    {
        Public,
        Private
    }

    public enum VacStatus
    {
        Unsecured,
        Secured
    }
}