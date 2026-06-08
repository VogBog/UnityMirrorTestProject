namespace Game.Scripts.SceneManagement
{
    public static class StaticData
    {
        public static bool DisconnectedFromServer = false;
        public static bool DisconnectedByHimself = false;
        public static string SaveFileName = string.Empty;
        public static string PlayerName = string.Empty;

        public static void SetDefault()
        {
            DisconnectedFromServer = false;
            DisconnectedByHimself = false;
            SaveFileName = string.Empty;
            PlayerName = string.Empty;
        }
    }
}