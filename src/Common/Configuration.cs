using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace BeepBoopBot
{
    /// <summary> 
    /// A file that contains information you either don't want public
    /// or will want to change without having to compile another bot.
    /// </summary>
    public class Configuration
    {

        #region Properties

        [JsonIgnore]
        public static string ClassName { get; private set; } = typeof(Configuration).Name;

        /// <summary> The location and name of your bot's configuration file. </summary>
        [JsonIgnore]
        public static string FileName { get; private set; } = "config/configuration.json";

        /// <summary> Your bot's login token. </summary>
        public string Token { get; set; } = "";

        /// <summary> Your bot's command prefix. </summary>
        public string Prefix { get; set; } = "";

        /// <summary> String to store the bot's master's Id. Used only for Json serialization. </summary>
        [JsonProperty(PropertyName = "BotMaster")]
        private string botMaster = "";
        /// <summary> Id of the most privelaged owner of the bot. Cannot be modified by bot owners. </summary>
        [JsonIgnore]
        public ulong BotMaster
        {
            get => ulong.Parse(botMaster);
            protected set => botMaster = value.ToString();
        }

        /// <summary> String list to store the Id's of users with owner access. Used only for Json serialization. </summary>
        [JsonProperty(PropertyName = "Owners")]
        private List<string> owners = new List<string>();

        [JsonIgnore]
        /// <summary> Ids of users who will have owner access to the bot. </summary>
        public ObservableCollection<ulong> Owners
        {
            get
            {
                //Only returns values that successfully parse
                ulong ownerId = 0;
                ObservableCollection<ulong> ObservableOwners =  new ObservableCollection<ulong>(
                    owners
                        .Where(ownerIdString => ulong.TryParse(ownerIdString, out ownerId))
                        .Select(ownerIdString => ownerId)
                        .ToList()
                );
                ObservableOwners.CollectionChanged += Owners_CollectionChanged;
                return ObservableOwners;
            }
            protected set => owners = value.Select(i => i.ToString()).ToList();
            //set => new HashSet<double>(value.Select(i => (double)i).ToArray());
        }

        [JsonIgnore]
        public static Color DefaultEmbedColor = new Color(114, 137, 218);
        /// <summary> The integer value of a hex color. Used only for Json serialization. </summary>
        [JsonProperty(PropertyName = "EmbedColor")]
        private uint embedColor = DefaultEmbedColor.RawValue;
        /// <summary> The default color to be used when embedding. </summary>
        [JsonIgnore]
        public Color EmbedColor
        {
            get => new Color(embedColor);
            set => embedColor = value.RawValue;
        }

        /// <summary> Boolean flag as to whether or not to prompt for settings changes on consecutive boots. </summary>
        public bool AutoStart { get; set; } = false;

        /// <summary> The severity you want the Discord API to log in the console. </summary>
        public LogSeverity LoggingSeverity { get; set; } = LogSeverity.Verbose;

        [JsonIgnore]
        private static readonly string ErrorMessage = "Sorry, I didn't quite catch that.";

        #endregion

        #region Methods

        public Configuration()
        {
            Owners.CollectionChanged += Owners_CollectionChanged;
        }

        /// <summary> Notify the Owners list when a change occurs </summary>
        private void Owners_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {                                                           // Make sure the data in Owners and owners are in sync
            owners = ((ObservableCollection<ulong>)sender).Select(i => i.ToString()).ToList();
        }

        /// <summary> Used to ensure the a configuration file. </summary>
        public static void EnsureExists()
        {
            BeepBoopBot.Logger.Log(ClassName, "Loading Configuration...");

            string file = Path.Combine(AppContext.BaseDirectory, FileName);
            if (!File.Exists(file))                                 // Check if the configuration file exists.
            {
                BeepBoopBot.Logger.Log(ClassName, "Configuration not found", LogSeverity.Warning);
                Console.WriteLine();

                string path = Path.GetDirectoryName(file);          // Create config directory if doesn't exist.
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                Configuration config = new Configuration()          // Create a new configuration object.
                {
                    Token = GetNewToken(),                          // Get the bot token from the user.
                    Prefix = GetNewPrefix(),                        // Get the bot prefix from the user.
                    BotMaster = GetBotMaster()                      // Get the bot master's id from the user.
                };
                config.SaveJson();                                  // Save the new configuration object to file.

                Console.WriteLine();
            }

            BeepBoopBot.Logger.Log(ClassName, "Configuration Loaded");
        }

        /// <summary> Uses to console to ask the user if they want to change bot settings. </summary>
        public Configuration PromptForChanges()
        {
            Console.WriteLine();
            Console.WriteLine("Would you like to change my settings? (y/n): ");
            string input = Console.ReadLine().ToLower();        // Collect user input.
            if (input.Length > 0 && input[0] == 'y')            // Check if input is long enough and uesr said yes.
            {
                ChangeBotSettings();                            // Otherwise, assume no.
            }
            Console.WriteLine();
            return this;
        }

        /// <summary> Uses the console to allow a user to change bot settings. </summary>
        private void ChangeBotSettings()
        {
            bool changeSettings = true;
            bool listSettingsCommands = true;

            while (changeSettings)                                  // Loop for continued execution until the user explicitly quits.
            {
                if (listSettingsCommands)                           // Only show the commands on the first loop or if the user asks for them again.
                {
                    Console.WriteLine("Here's what you can do:\n" +
                        "Type \'t\' (Token)\tto change my token\n" +
                        "Type \'p\' (Prefix)\tto change my command prefix\n" +
                        "Type \'m\' (Master)\tto change my master\n" +
                        "Type \'c\' (Color)\tto change my embeding color\n" +
                        "Type \'a\' (AutoStart)\tto toggle AutoStart\n" +
                        "Type \'l\' (Logging)\tto change my current logging level\n" +
                        "Type \'s\' (Settings)\tto list my current settings\n" +
                        "Type \'q\' (Quit)\t\tto quit changing my settings\n" +
                        "Type \'h\' (Help)\t\tto see my current commands again"
                        );
                    listSettingsCommands = false;
                }

                Console.WriteLine("What would you like me to do? ");
                string input = Console.ReadLine().ToLower();        // Collect user input and ToLower() to avoid errors.

                if (input.Length > 0)                               // Make sure the input was long enough.
                {
                    char charIn = input[0];                         // Only look at the first character.
                    switch (charIn)                                 // Attempt to do what the user asked.
                    {
                        case 't':                                   // Change token.
                            SetToken();
                            Console.WriteLine("My token is now:\n" + Token);
                            break;

                        case 'm':
                            SetBotMaster();
                            Console.WriteLine("My recognized master's Id is now:\n" + BotMaster);
                            break;

                        case 'p':                                   // Change Prefix.
                            SetPrefix();
                            Console.WriteLine("My recognized prefix is now:\n" + Prefix);
                            break;

                        case 'c':                                   // Change embed color
                            SetEmbedColor();
                            Console.WriteLine("My embed color is now:\n" + EmbedColor.ToString().ToUpper());
                            break;

                        case 'a':                                   // Toggle AutoStart.
                            AutoStart = !AutoStart;
                            SaveJson();
                            string status = AutoStart ? "Enabled" : "Disabled";
                            Console.WriteLine("Autostart is now:\n" + status);
                            break;

                        case 'l':                                   // Change logging severity.
                            SetLoggingSeverity();
                            Console.WriteLine("My logging severity is now:\n" + LoggingSeverity);
                            break;

                        case 's':                                   // List settings.
                            Console.WriteLine();
                            PrintConfiguration(this);
                            Console.WriteLine();
                            break;

                        case 'q':                                   // Quit changing settings.
                            changeSettings = false;
                            Console.WriteLine("Exiting settings . . .");
                            break;

                        case 'h':                                   // Show the user bot settings commands again.
                            listSettingsCommands = true;
                            break;

                        default:                                    // Unrecognized command.
                            Console.WriteLine("I'm sorry, I don't know what you mean by that.");
                            break;
                    }
                }
                else                                                // User's command was malformed (empty string).
                {
                    Console.WriteLine("I didn't quite catch that.");
                }
            }
        }

        /// <summary> uses the console to promp the user for a new token. </summary>
        private static string GetNewToken()
        {
            string token = "";
            while (String.IsNullOrEmpty(token))                     // Loop until an acceptable token is input
            {
                Console.WriteLine("Please enter my token: ");
                token = Console.ReadLine();
            }
            return token;
        }

        /// <summary> Uses the console to prompt the user for a new token. </summary>
        private void SetToken()
        {
            string token = "";
            while (String.IsNullOrEmpty(token))                     // Loop until an acceptable token is input
            {
                Console.WriteLine("Please enter my token: ");
                token = Console.ReadLine();
            }
            Token = token;
            SaveJson();                                             // Save the changes.
        }

        /// <summary> Uses the console to prompt the user for a new token. </summary>
        private static string GetNewPrefix()
        {
            string prefix = "";
            while (String.IsNullOrEmpty(prefix))                    // Loop until an acceptable prefix is input
            {
                Console.WriteLine("Please enter the prefix you want me to recognize: ");
                prefix = Console.ReadLine();
            }
            return prefix;
        }

        /// <summary> Uses the console to prompt the user for a new token. </summary>
        private void SetPrefix()
        {
            string prefix = "";
            while (String.IsNullOrEmpty(prefix))                    // Loop until an acceptable prefix is input
            {
                Console.WriteLine("Please enter the prefix you want me to recognize: ");
                prefix = Console.ReadLine();
            }
            Prefix = prefix;
            SaveJson();                                             // Save the changes
        }

        /// <summary> Uses the console to prompt the user for their id. </summary>
        private static ulong GetBotMaster()
        {
            ulong botMaster = new ulong();
            bool ownerSet = false;
            while (!ownerSet)
            {
                try
                {
                    Console.WriteLine("Please enter your Id to claim me as your own: ");
                    botMaster = ulong.Parse(Console.ReadLine());
                    ownerSet = true;
                }
                catch
                {
                    Console.WriteLine(ErrorMessage);
                }
            }
            return botMaster;
        }

        /// <summary> Uses the console to prompt the user for their id. </summary>
        private void SetBotMaster()
        {
            ulong botMaster = new ulong();
            bool ownerSet = false;
            while (!ownerSet)
            {
                try
                {
                    Console.WriteLine("Please enter your Id to claim me as your own: ");
                    botMaster = ulong.Parse(Console.ReadLine());
                    ownerSet = true;
                }
                catch
                {
                    Console.WriteLine(ErrorMessage);
                }
            }
            BotMaster = botMaster;
            SaveJson();
        }

        /// <summary> Uses the console to prompt the user for a new embed color in hex. </summary>
        private void SetEmbedColor()
        {
            bool getNewColor = true;
            while (getNewColor)
            {
                Console.WriteLine("Please input a new hexadecimal color to use for embeds:");
                Console.Write('#');
                try
                {
                    uint newVal = uint.Parse(Console.ReadLine(), System.Globalization.NumberStyles.HexNumber);
                    if (newVal <= new Color(255, 255, 255).RawValue)
                    {
                        embedColor = newVal;
                        SaveJson();
                        getNewColor = false;
                    }
                    else
                    {
                        throw new OverflowException();
                    }
                }
                catch
                {
                    Console.WriteLine(ErrorMessage);
                }
            }
        }

        /// <summary> Uses the console to allow the user to change the bot's logging severity. </summary>
        private void SetLoggingSeverity()
        {
            // Convert the LogSeverity Enum into an iterable list.
            Array severitiesArray = Enum.GetValues(typeof(LogSeverity));
            List<LogSeverity> severitiesList = severitiesArray.Cast<LogSeverity>().ToList();

            StringBuilder sb = new StringBuilder();                 // Build and store to save on Console writes.
            for (int i = 0; i < severitiesList.Count; i++)          // Get all possible logging severities.
            {
                LogSeverity x = severitiesList[i];
                sb.Append($"{i} {x.ToString()}\n");
            }
            string loggingSeverities = sb.ToString();               // Save the results for later use.


            int newLogLevelIndex = -1;                                   // Initialize to an unacceptable input.
            bool getUserInput = true;
            while (getUserInput)                                    // Loop until an acceptable int is input.
            {
                Console.WriteLine("What logging level would you like to use?");
                Console.Write(loggingSeverities);
                Console.WriteLine("A lower number means less information will be logged in this console.");
                Console.WriteLine("Please input the integer number of the logging severity you want to use:");

                try
                {
                    newLogLevelIndex = int.Parse(Console.ReadLine());    // Try to parse the user input.
                    if (0 <= newLogLevelIndex && newLogLevelIndex < severitiesList.Count)
                    {                                               // Make sure the input is within the range of possible severity indexes.
                        getUserInput = false;                       // User input was good, we can stop the loop.
                    }
                    else                                            // User input wasn't good. Tell the user and continue the loop.
                    {
                        Console.WriteLine(@"Sorry, the number has to be between {0} and {1}", 0, (severitiesList.Count - 1));
                    }
                }
                catch                                               // Input probably wasn't a proper int. Loop again to try for proper input.
                {
                    Console.WriteLine(ErrorMessage);
                }
            }
            LoggingSeverity = severitiesList[newLogLevelIndex];          // Update the logging severity.
            SaveJson();                                                  // Save the changes.
        }

        /// <summary> Returns a string enumerating the bot's current configuration. </summary>
        public static string ListConfiguration()
        {
            Configuration config = Configuration.Load();
            return ListConfiguration(config);
        }

        /// <summary> Returns a string enumerating the provided Configuration's settings. </summary>
        public static string ListConfiguration(Configuration config)
        {
            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine("My current token is:\n" + config.Token);

            sb.AppendLine("My current prefix is:\n" + config.Prefix);

            sb.AppendLine("My current master is:\n" + config.BotMaster);

            sb.AppendLine("My current embed color is:\n" + config.EmbedColor.ToString().ToUpper());

            sb.AppendLine("Autostart is currently:");
            string status = config.AutoStart ? "Enabled" : "Disabled";
            sb.AppendLine(status);

            sb.AppendLine("Logging severity is currently:\n" + config.LoggingSeverity.ToString());
            return sb.ToString();
        }

        /// <summary> Prints the current bot configuration to the console using Client_Log_Multiple </summary>
        public static void PrintConfiguration()
        {
            PrintConfiguration(Configuration.Load());
        }

        /// <summary> Prints the specified bot configuration to the console using Client_Log_Multiple </summary>
        public static void PrintConfiguration(Configuration config)
        {
            LogSeverity severity = LogSeverity.Info;
            string[] sources = new string[6];
            string[] messages = new string[6];

            sources[0] = "Token";
            messages[0] = config.Token;

            sources[1] = "Prefix";
            messages[1] = config.Prefix;

            sources[2] = "Bot master";
            messages[2] = $"{config.BotMaster}";

            sources[3] = "Embed color";
            messages[3] = $"{config.EmbedColor}".ToUpper();

            sources[4] = "Autostart";
            messages[4] = $"{config.AutoStart}";

            sources[5] = "Log severity";
            messages[5] = $"{config.LoggingSeverity}";

            int leftWidth = 0;
            foreach (string src in sources)
                if (src.Length > leftWidth)
                    leftWidth = src.Length;
            leftWidth++;
            // My first named argument
            BeepBoopBot.Logger.LogMultiple(severity, ClassName, sources, messages, leftWidth: leftWidth, trimCenter: false);
        }

        /// <summary> Save the configuration to the path specified in FileName. </summary>
        public void SaveJson()
        {
            string file = Path.Combine(AppContext.BaseDirectory, FileName);
            File.WriteAllText(file, ToJson());
        }

        /// <summary> Load the configuration from the path specified in FileName. </summary>
        public static Configuration Load()
        {
            string file = Path.Combine(AppContext.BaseDirectory, FileName);
            return JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(file));
        }

        /// <summary> Convert the configuration to a json string. </summary>
        public string ToJson()
            => JsonConvert.SerializeObject(this, Formatting.Indented);

        #endregion
    }
}
