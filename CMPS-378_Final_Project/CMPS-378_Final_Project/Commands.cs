using Discord.Commands;

namespace DiscordBot
{
    // Modules must be public and inherit from an IModuleBase
    public class CommandModule : ModuleBase<SocketCommandContext>
    {
        [Command("test")]
        public async Task Test()
        {
            Console.WriteLine("test executed");
            await ReplyAsync("hello world");
            // lorem ipsum dolor sit amet
            // CMPS 378 Project
        }


        [Command("ping")]
        [Alias("pong", "hello")]
        public Task PingAsync()
            => ReplyAsync("pong!");]
    }
}