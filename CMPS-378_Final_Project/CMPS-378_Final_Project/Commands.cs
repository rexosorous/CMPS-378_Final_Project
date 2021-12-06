using Discord.Commands;
using Discord;
using Discord.Audio;
using CliWrap;

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

        [Command("play", RunMode = RunMode.Async)]
        public async Task Play(string url, IVoiceChannel channel = null)
        {
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await Context.Channel.SendMessageAsync("User must be in a voice channel.");
                return;
            }
            var guildId = channel.Guild.Id;
            var audioClient = await channel.ConnectAsync();

            AudioHandler audio = new AudioHandler();
            var stream = await audio.getSongByURL(url);

            var memoryStream = new MemoryStream();
            await Cli.Wrap("ffmpeg")
                .WithArguments(" -hide_banner -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1")
                .WithStandardInputPipe(PipeSource.FromStream(stream))
                .WithStandardOutputPipe(PipeTarget.ToStream(memoryStream))
                .ExecuteAsync();

            using (var discord = audioClient.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await discord.WriteAsync(memoryStream.ToArray(), 0, (int)memoryStream.Length); }
                finally { await discord.FlushAsync(); }
            }
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