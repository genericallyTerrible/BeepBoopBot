using System;
using System.Collections.Generic;
using System.Text;

namespace BeepBoopBot.Services
{
    public class Localization
    {
        public static string LoadCommandString(string key)
        {
            string retVal = Resources.CommandStrings.ResourceManager.GetString(key);
            return string.IsNullOrWhiteSpace(retVal) ? key : retVal;
        }
    }
}
