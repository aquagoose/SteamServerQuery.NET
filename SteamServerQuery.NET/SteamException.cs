﻿using System;

namespace SteamServerQuery
{
    public class SteamException : Exception
    {
        public SteamException() { }
        public SteamException(string message) : base(message) { }
        public SteamException(string message, Exception innerException) : base(message, innerException) { }
    }
}