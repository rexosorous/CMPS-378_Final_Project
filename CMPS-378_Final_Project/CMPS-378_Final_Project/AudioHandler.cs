using Discord;
using Discord.Audio;
using Discord.WebSocket;
using System.Diagnostics;


namespace DiscordBot
{
    public class AudioHandler
    {
        /* Class that handles all interactions with youtube using YoutubeExplode https://github.com/Tyrrrz/YoutubeExplode */

        private readonly DiscordSocketClient discord;
        private IAudioClient voiceClient;
        private Queue<string> songQueue = new Queue<string>();
        private bool isPlaying = false;

        public AudioHandler(DiscordSocketClient discord)
        {
            discord = discord;
        }

        public async Task JoinChannel(IVoiceChannel channel)
            => voiceClient =  await channel.ConnectAsync();

        public async Task LeaveChannel()
            => await voiceClient.StopAsync();

        public async Task Play(string url, IVoiceChannel channel)
        {
            /* General function to play songs.
             * Smartly joins the voice channel, starts playing audio, and then disconnects.
             * If called while already playing audio, this function will add songs to the queue instead.
             * 
             * Args:
             *      url (string): can either be the youtube link or a search term
             *      channel (IVoiceChannel): the voice channel of the user invoking this command
             */

            if (voiceClient == null || voiceClient.ConnectionState == ConnectionState.Disconnected) await JoinChannel(channel);
            songQueue.Enqueue(url);
            if (isPlaying) return;
            isPlaying = true;
            AudioOutStream voice = null;

            while (songQueue.Count > 0)
            {
                ProcessStartInfo cmd = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $@"/C youtube-dl --no-check-certificate -f bestaudio --default-search ytsearch -o - ""{songQueue.Dequeue()}"" | ffmpeg -i pipe:0 -f s16le -ar 48000 -ac 2 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                var ffmpeg = Process.Start(cmd);
                var output = ffmpeg.StandardOutput.BaseStream;
                if (voice == null) voice = voiceClient.CreatePCMStream(AudioApplication.Mixed, 96000);
                await output.CopyToAsync(voice);
                await voice.FlushAsync();
            }

            isPlaying = false;
            LeaveChannel();
        }

        public async Task test()
        {
            Console.WriteLine(songQueue.Count);
        }
    }
}