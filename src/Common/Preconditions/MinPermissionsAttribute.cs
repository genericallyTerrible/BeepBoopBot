using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BeepBoopBot.Preconditions
{
    /// <summary>
    /// Set the minimum permission required to use a module or command
    /// similar to how MinPermissions works in Discord.Net 0.9.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class MinPermissionsAttribute : PreconditionAttribute
    {
        private ServerAccessLevel serverLevel;
        private BotAccessLevel botLevel;
        private RequiredPreconditions requiredPreconditions;

        public MinPermissionsAttribute(ServerAccessLevel sLevel)
        {
            serverLevel = sLevel;
            requiredPreconditions = RequiredPreconditions.RequireAllPreconditions;
        }

        public MinPermissionsAttribute(BotAccessLevel bLevel)
        {
            botLevel = bLevel;
            requiredPreconditions = RequiredPreconditions.RequireAllPreconditions;
        }

        public MinPermissionsAttribute(ServerAccessLevel sLevel, BotAccessLevel bLevel)
        {
            serverLevel = sLevel;
            botLevel = bLevel;
            requiredPreconditions = RequiredPreconditions.RequireAllPreconditions;
        }

        public MinPermissionsAttribute(ServerAccessLevel sLevel, BotAccessLevel bLevel, RequiredPreconditions rPreconditions)
        {
            serverLevel = sLevel;
            botLevel = bLevel;
            requiredPreconditions = rPreconditions;
        }


        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            ServerAccessLevel serverAccess = GetServerPermission(context);            // Get the acccesslevel for this context
            BotAccessLevel botAccess = GetBotPermission(context);
            if (requiredPreconditions == RequiredPreconditions.RequireAllPreconditions)
            {
                if ((serverAccess >= serverLevel && botAccess >= botLevel))// || botAccess == BotAccessLevel.BotMaster)          // If the user's access level is greater than the required level, return success.
                    return Task.FromResult(PreconditionResult.FromSuccess());
                else
                    return Task.FromResult(PreconditionResult.FromError("Insufficient permissions."));
            }
            else if(requiredPreconditions == RequiredPreconditions.RequireAnyPrecondition)
            {
                if ((serverAccess >= serverLevel || botAccess >= botLevel))
                    return Task.FromResult(PreconditionResult.FromSuccess());
                else
                    return Task.FromResult(PreconditionResult.FromError("Insufficient permissions."));
            }
            else
            {
                return Task.FromResult(PreconditionResult.FromError("Insufficient permissions."));
            }
        }

        public ServerAccessLevel GetServerPermission(ICommandContext c)
        {
            // Check if the context is in a guild.
            if (c.User is SocketGuildUser user)
            {
                if (c.Guild.OwnerId == user.Id)                  // Check if the user is the guild owner.
                    return ServerAccessLevel.ServerOwner;

                if (user.GuildPermissions.Administrator)         // Check if the user has the administrator permission.
                    return ServerAccessLevel.ServerAdmin;

                if (user.GuildPermissions.ManageMessages ||      // Check if the user can ban, kick, or manage messages.
                    user.GuildPermissions.BanMembers ||
                    user.GuildPermissions.KickMembers)
                    return ServerAccessLevel.ServerMod;
            }

            return ServerAccessLevel.User;                             // If nothing else, return a default permission.
        }

        public BotAccessLevel GetBotPermission(ICommandContext c)
        {
            Configuration config = Configuration.Load();
            if (c.User.IsBot) //Identify bots
            {
                return BotAccessLevel.Bot;
            }
            else if (c.User.Id == config.BotMaster)     // Give the master even more special access.
            {
                return BotAccessLevel.BotMaster;
            }
            else if (config.Owners.Contains(c.User.Id)) // Give configured owners special access.
            {
                return BotAccessLevel.BotOwner;
            }
            else
            {
                return BotAccessLevel.User;
            }
        }
    }
}
