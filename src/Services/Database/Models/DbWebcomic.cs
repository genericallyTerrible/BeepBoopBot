using System;
using System.Collections.Generic;
using System.Text;

namespace BeepBoopBot.Services.Database.Models
{
    public class DbWebcomic : DbEntity
    {
        public string ComicName { get; set; } = "";

        public List<ComicChapter> Chapters { get; set; } = new List<ComicChapter>();
        public HashSet<Reader> Readers { get; set; } = new HashSet<Reader>();
    }

    public class ComicChapter : DbEntity
    {
        public string Chaptername { get; set; } = "";

        public List<ComicPanel> Panels { get; set; } = new List<ComicPanel>();
    }

    public class ComicPanel : DbEntity
    {
        public string Title { get; set; } = "";

        public string ContentUrl { get; set; } = "";
        public bool IsImage { get; set; } = false;

        public string Text { get; set; } = "";

    }

    public class Author : DbEntity
    {
        public ulong UserId { get; set; }
        public string DisplayName { get; set; }
    }

    public class Reader : DbEntity
    {
        public ulong UserId { get; set; }
        public uint CurrentChapter { get; set; } = 0;
        public uint CurrentPanel { get; set; } = 0;
    }
}
