/// <summary>
/// Names shared between code, scenes and the Tag Manager.
/// Keeping them in one place means a renamed scene or tag only needs changing here.
/// </summary>
public static class GameConstants
{
    public static class Scenes
    {
        public const string MainMenu = "Main Menu";
        public const string LevelSelection = "LevelSelection";
        public const string LevelPrefix = "Level ";
    }

    public static class Tags
    {
        public const string Block = "Block";
        public const string Player = "Player";
    }

    public static class Objects
    {
        public const string MobileControls = "For Mobile";
        public const string VirtualCamera = "Virtual Camera";
    }

    public static class Prefs
    {
        public const string LevelsUnlocked = "levelsUnlocked";
    }
}
