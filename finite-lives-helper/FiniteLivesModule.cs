using System;
using System.Xml;
using System.Collections.Generic;
using Monocle;

namespace Celeste.Mod.FiniteLives
{
    public class FiniteLivesModule : EverestModule
    {
        private static FiniteLivesModule Instance;
        public Dictionary<string, Chapter> chapters { get; }
        private int lifeCount = 1;
        private bool infiniteLives = true;
        private bool shouldRestart = false;
        private bool enabled = false;
        private FiniteLivesDisplay display;

        public override Type SessionType => typeof(FiniteLivesSession);
        public static FiniteLivesSession Session => (FiniteLivesSession)Instance._Session;

        /// <summary>
        /// Default constructor. Initialize instance var and chapter dictionary.
        /// </summary>
        public FiniteLivesModule()
        {
            Instance = this;
            chapters = new Dictionary<string, Chapter>();
        }

        /// <summary>
        /// Load module. Add event handlers.
        /// </summary>
        public override void Load()
        {
            Everest.Events.Player.OnDie += OnPlayerDeath;
            Everest.Events.Level.OnLoadLevel += OnLevelLoad;
            Everest.Events.Level.OnExit += OnLevelExit;
            Everest.Events.Level.OnEnter += OnLevelEnter;

            On.Celeste.LevelLoader.LoadingThread += (orig, self) =>
            {
                orig(self);
                self.Level.Add(display = new FiniteLivesDisplay(self.Level));
            };
        }

        /// <summary>
        /// Unload module. Remove event handlers.
        /// </summary>
        public override void Unload()
        {
            Everest.Events.Player.OnDie -= OnPlayerDeath;
            Everest.Events.Level.OnLoadLevel -= OnLevelLoad;
            Everest.Events.Level.OnExit -= OnLevelExit;
            Everest.Events.Level.OnEnter -= OnLevelEnter;
        }

        /// <summary>
        /// Load all finitelives.xml files available.
        /// </summary>
        /// <param name="firstLoad">Whether the mod is loading for the first time.</param>
        public override void LoadContent(bool firstLoad)
        {
            Log("Finding all finitelives.xml's");
            // Keep track of visited (cached) file paths, don't want to read the same file twice
            HashSet<string> visited = new HashSet<string>();

            // Read all finitelives.xml's from every mod (as long as they're available)
            Dictionary<string, ModAsset> maps = Everest.Content.Map;
            lock (maps)
            {
                foreach (ModAsset modAsset in maps.Values)
                {
                    // Read XML file if it is a "finitelives" file that hasn't been visited yet
                    if ((modAsset?.Type == typeof(AssetTypeXml)) && modAsset.PathVirtual.EndsWith("finitelives") && !visited.Contains(modAsset.GetCachedPath()))
                    {
                        Log($"Found xml: {modAsset.PathVirtual} => {modAsset.GetCachedPath()}");
                        ReadXMLFile(modAsset.GetCachedPath());
                        visited.Add(modAsset.GetCachedPath());
                    }
                }
            }
        }

        /// <summary>
        /// Decrement number of player's lives when the player dies.
        /// </summary>
        /// <param name="player">Player object.</param>
        private void OnPlayerDeath(Player player)
        {
            // Don't do anything if fany of the conditions meet
            if (infiniteLives || HasGoldenBerry(player) || !enabled)
                return;

            // Decrement life count if player does not have infinite lives
            Log($"OnPlayerDeath: lifeCount={Math.Max(0, --lifeCount)}");
            SaveSessionData();
            display.SetDisplayText(lifeCount.ToString());

            // Restart chapter if out of lives
            if (lifeCount == 0 && !shouldRestart)
            {
                Log("OnPlayerDeath: Restarting chapter");
                shouldRestart = true;
            }
        }

        /// <summary>
        /// Checks how many lives should be given to the player when a level is loaded.
        /// </summary>
        /// <param name="level">Level object.</param>
        /// <param name="playerIntro">Intro type (how the player entered the level).</param>
        /// <param name="isFromLoader">Whether level entry is from after file select or not.</param>
        private void OnLevelLoad(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            // Check if chapter name is in the dictionary, return if otherwise
            enabled = chapters.TryGetValue(level.Session.MapData.Filename, out Chapter c);
            display.SetEnabled(enabled);
            if (!enabled)
                return;

            // Check if should restart chapter, do so if needed
            if (shouldRestart)
            {
                Action restartChapter = delegate { Engine.Scene = new LevelExit(LevelExit.Mode.Restart, level.Session); };
                if (isFromLoader)
                    level.DoScreenWipe(false, restartChapter);
                else
                    restartChapter();
                shouldRestart = false;
                return;
            }

            // Check if player has not visited this level before, return if otherwise
            if (!level.NewLevel)
            {
                display.SetDisplayText(lifeCount.ToString());
                return;
            }

            int? newLives = c.GetLives(level.Session.LevelData.Name);

            // Do not try to change life count if non-existent new life count
            if (!newLives.HasValue)
                return;

            lifeCount = Math.Max(lifeCount, newLives.Value);  // Don't punish player for being too good
            infiniteLives = newLives.Value <= 0;
            SaveSessionData();
            display.SetDisplayText(infiniteLives ? "inf" : lifeCount.ToString());

            Log($"OnLevelLoad: map={level.Session.MapData.Filename}, level: {level.Session.LevelData.Name}, lifeCount={lifeCount}, infiniteLives={infiniteLives}");
        }

        /// <summary>
        /// Reset values if player exits or restarts chapter.
        /// </summary>
        /// <param name="level">Level object.</param>
        /// <param name="exit">Level exit object.</param>
        /// <param name="mode">Level exit mode.</param>
        /// <param name="session">Current session.</param>
        /// <param name="snow">???</param>
        private void OnLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            Log($"OnLevelExit: mode={mode}");
            switch (mode)
            {
                case LevelExit.Mode.GoldenBerryRestart:
                case LevelExit.Mode.SaveAndQuit:
                    break;
                default:
                    // Reset player life count
                    lifeCount = 1;
                    infiniteLives = true;
                    break;
            }
        }

        /// <summary>
        /// Set initial values when player enters a chapter.
        /// </summary>
        /// <param name="session">Current session.</param>
        /// <param name="fromSaveData">Whether level entry is from after file select or not.</param>
        private void OnLevelEnter(Session session, bool fromSaveData)
        {
            if (fromSaveData)
            {
                // Get number of lives from save data
                Log($"OnLevelEnter: Entering from save data");
                if (Session.Checksum != Session.HashCode())
                {
                    // User is cheating, restart chapter
                    shouldRestart = true;
                    Log($"OnLevelEnter: Cheat detection - user-modified session data");
                    Log($"OnLevelEnter: Restarting chapter");
                }
                else
                {
                    // User is not cheating, set values accordingly
                    lifeCount = --Session.LifeCount;
                    infiniteLives = Session.InfiniteLives;
                    SaveSessionData();
                    Log($"OnLevelEnter: Session data is unmodified");

                    // Restart chapter if out of lives
                    if (lifeCount == 0)
                    {
                        shouldRestart = true;
                        Log($"OnLevelEnter: Restarting chapter");
                    }
                }
            }
            else
            {              
                // Reset player values
                lifeCount = 1;
                infiniteLives = true;
                shouldRestart = false;
                Log($"OnLevelEnter: Entering from chapter select");
            }           
        }

        /// <summary>
        /// Check whether player is holding the golden berry.
        /// </summary>
        /// <param name="player">Player object.</param>
        /// <returns></returns>
        private bool HasGoldenBerry(Player player)
        {
            foreach (Follower current in player.Leader.Followers)
            {
                if (current.Entity is Strawberry && (current.Entity as Strawberry).Golden && !(current.Entity as Strawberry).Winged)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Parse an XML file to get number of available lives for each level for each chapter.
        /// </summary>
        /// <param name="filename">File name.</param>
        public void ReadXMLFile(string filename)
        {
            // Read XML file
            using (XmlReader xml = XmlReader.Create(filename))
            {
                Log($"FiniteLines: Reading file {filename}");
                Chapter c = null;
                KeyValuePair<string, int> l = default;
                while (xml.Read())
                {
                    switch (xml.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (xml.Name == NODE_CHAPTER)
                            {
                                // Don't try to init another chapter when one currently exists
                                if (c != null)
                                    break;

                                // Init chapter if name attr exists
                                string name = xml.GetAttribute(ATTR_NAME);
                                if (name != null)
                                {
                                    // Check if chapter w/ name is already in dict, break if so
                                    if (chapters.ContainsKey(name))
                                        break;

                                    c = new Chapter(name);
                                    Log($"Initializing chapter {name}");
                                }
                            }
                            else if (xml.Name == NODE_LEVEL)
                            {
                                // Don't try to init another level when one currently exists
                                if (c == null || l.Key != null)
                                    break;

                                // Init level pair if name and lives attrs exist
                                string name = xml.GetAttribute(ATTR_NAME);
                                string lives = xml.GetAttribute(ATTR_LIVES);
                                if (name != null && lives != null)
                                {
                                    int.TryParse(lives, out int lives_i);
                                    l = new KeyValuePair<string, int>(name, lives_i);
                                    Log($"FiniteLines: Initializing level {name} with {lives_i} live(s)");
                                }
                            }
                            break;
                        case XmlNodeType.EndElement:
                            if (xml.Name == NODE_CHAPTER)
                            {
                                // Check if c is initialized, want to prepare for another chapter
                                if (c == null)
                                    break;

                                // Add chapter to dictionary, then nullify c
                                chapters.Add(c.name, c);
                                Log($"Adding chapter {c.name} to dictionary");
                                c = null;                               
                            }
                            else if (xml.Name == NODE_LEVEL)
                            {
                                // Check if c and l are initialized, can't have levels w/o chapter
                                if (c == null || l.Key == null)
                                    break;

                                // Add level to chapter's dict, then reset l
                                c.AddOrUpdateLevel(l.Key, l.Value);
                                Log($"FiniteLines: Adding level {l.Key} to {c.name}'s dictionary");
                                l = default;
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Store current values to session data.
        /// </summary>
        private void SaveSessionData()
        {
            Session.LifeCount = lifeCount;
            Session.InfiniteLives = infiniteLives;
            Session.Checksum = Session.HashCode();
        }

        /// <summary>
        /// Write message to log (verbose level).
        /// </summary>
        /// <param name="msg">Message.</param>
        private void Log(string msg)
        {
            Logger.Log("FiniteLives", msg);
        }

        private const string NODE_CHAPTER = "chapter";       
        private const string NODE_LEVEL = "level";
        private const string ATTR_NAME = "name";
        private const string ATTR_LIVES = "lives";
    }

    public class Chapter
    {
        /// <summary>
        /// Chapter name.
        /// </summary>
        public string name { get; }

        /// <summary>
        /// Level dictionary. (level name -> number of available lives)
        /// </summary>
        private Dictionary<string, int?> levels { get; }

        /// <summary>
        /// Initialize chapter with given name.
        /// </summary>
        /// <param name="s">Chapter name.</param>
        public Chapter(string s)
        {
            name = s;
            levels = new Dictionary<string, int?>();
        }

        /// <summary>
        /// Add or update lives number to level.
        /// </summary>
        /// <param name="s">Level name.</param>
        /// <param name="i">Number of lives. Non-positive numbers represent infinity.</param>
        public void AddOrUpdateLevel(string s, int i)
        {
            levels[s] = Math.Max(0, i);
        }

        /// <summary>
        /// Get number of available lives in given level.
        /// </summary>
        /// <param name="s">Level name.</param>
        /// <returns></returns>
        public int? GetLives(string s) => levels.ContainsKey(s) ? levels[s] : null;
    }
}

