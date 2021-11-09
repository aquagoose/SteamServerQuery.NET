using System;

namespace SteamServerQuery
{
    public struct PlayerInfo
    {
        public string Name { get; }
        
        public int Score { get; }
        
        public TimeSpan Duration { get; }

        internal PlayerInfo(string name, int score, float duration)
        {
            Name = name;
            Score = score;
            Duration = TimeSpan.FromSeconds(duration);
        }
    }
    
}