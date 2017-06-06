namespace BeepBoopBot
{
    /// <summary>
    /// The enum used to specify permission levels. A lower
    /// number means less permissions than a higher number.
    /// </summary>
    public enum ServerAccessLevel
    {
        Blocked,
        User,
        ServerMod,
        ServerAdmin,
        ServerOwner
    }
}
