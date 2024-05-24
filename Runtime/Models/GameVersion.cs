namespace Models
{
    using UnityEngine;

    public class GameVersion
    {
        public static string Version { get; } = Application.version;
    }
}