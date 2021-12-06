using Discord.Commands;
using Discord;

namespace DiscordBot
{
    public class CommandModule : ModuleBase<SocketCommandContext>
    {
        public AudioHandler AudioHandler { get; set; }

        [Command("ping")]
        [Alias("pong", "hello")]
        public Task PingAsync()
            => ReplyAsync("pong!");

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
            await AudioHandler.JoinChannel(channel);
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
            await AudioHandler.LeaveChannel();
        }
        
        [Command("play", RunMode = RunMode.Async)]
        public async Task Play(string url, IVoiceChannel Channel = null)
        {
            Channel = Channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (Channel == null)
            {
                await Context.Channel.SendMessageAsync("User must be in a voice channel.");
                return;
            }
            await AudioHandler.Play(url, Channel);
        }
    }
}