using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeepBoopBot.Services.Database
{

    public class BotContextFactory : IDesignTimeDbContextFactory<BotContext>
    {
        /// <summary> Used for migrations </summary>
        public BotContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlite("Filename=./data/BeepBoopBot.db");
            return new BotContext(optionsBuilder.Options);
        }
    }


    public class BotContext : DbContext
    {

        public BotContext() : base()
        {
        }

        public BotContext(DbContextOptions options) : base(options)
        {
        }


    }
}
