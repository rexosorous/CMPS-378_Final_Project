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
        }


        [Command("ping")]
        [Alias("pong", "hello")]
        public Task PingAsync()
            => ReplyAsync("pong!");


        [Command("youtube", RunMode=RunMode.Async)]
        public async Task Youtube([Remainder] string url)
        {
            AudioHandler yt = new AudioHandler();
            await yt.getSongBySearch(url);
        }
    }
}