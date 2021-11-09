using System.IO;

namespace SteamServerQuery
{
    internal static class Extensions
    {
        internal static string ReadNullTerminatedString(this BinaryReader reader)
        {
            string dataString = "";
            char data = reader.ReadChar();

            while (data != 0x00)
            {
                dataString += data;
                data = reader.ReadChar();
            }

            return dataString;
        }
    }
}