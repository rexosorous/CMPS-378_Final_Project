using Discord.Commands;
using Discord;
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

        [Command("youtube", RunMode = RunMode.Async)]
        public async Task Youtube([Remainder] string url)
        {
            AudioHandler yt = new AudioHandler();
            await yt.getSongBySearch(url);
        }

        [Command("join", RunMode = RunMode.Async)]
        [Alias("j")]
        public async Task JoinChannel(IVoiceChannel channel = null)
        {
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await Context.Channel.SendMessageAsync("User must be in a voice channel.");
                return;
            }
            var guildId = channel.Guild.Id;
            var audioClient = await channel.ConnectAsync();
        }

        [Command("disconnect", RunMode = RunMode.Async)]
        [Alias("dc")]
        public async Task LeaveChannel(IVoiceChannel Channel = null)
        {
            Channel = Channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (Channel == null)
            {
                await Context.Channel.SendMessageAsync("User must be in a voice channel.");
                return;
            }
            var guildId = Channel.Guild.Id;
            var audioClient = await Channel.ConnectAsync();
        }
    }
}