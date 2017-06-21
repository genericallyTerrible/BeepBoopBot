using System;
using System.Collections.Generic;
using System.Text;

namespace BeepBoopBot
{
    public enum SubmoduleInclusion
    {
        None,       // Do not include submodules
        Distinct,   // Include submodules as seperate modules
        Override    // Display submodule commands as commands in the current module
    }
}
