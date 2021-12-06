using Discord.Commands;
using Discord;

namespace DiscordBot
{
    public class CommandModule : ModuleBase<SocketCommandContext>
    {
        public AudioHandler AudioHandler { get; set; }


        [Command("disconnect", RunMode = RunMode.Async)]
        [Alias("dc", "stop", "leave")]
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
            await AudioHandler.Play(url, Channel, Context);
        }

        [Command("queue", RunMode = RunMode.Async)]
        public async Task Queue()
            => await AudioHandler.checkQueue(Context);

        [Command("skip")]
        [Alias("next")]
        public async Task Skip()
            => AudioHandler.skipSong();

        [Command("clear")]
        public async Task Clear()
            => AudioHandler.clearSongQueue(Context);
    }
}