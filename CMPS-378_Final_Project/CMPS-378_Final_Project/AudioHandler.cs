using Discord;
using Discord.Audio;
using Discord.WebSocket;
using System.Diagnostics;


namespace DiscordBot
{
    public struct SongInfo
    {
        public string title { get; }
        public string videoURL { get; }
        public string thumbnailURL { get; }
        public string duration { get; }

        public SongInfo(string title, string videoURL, string thumbnailURL, string duration) : this()
        {
            this.title = title;
            this.videoURL = videoURL;
            this.thumbnailURL = thumbnailURL;
            this.duration = duration;
        }
    }

    public class AudioHandler
    {
        private readonly DiscordSocketClient discord;
        private IAudioClient voiceClient;
        private Queue<SongInfo> songQueue = new Queue<SongInfo>();
        private bool isPlaying = false;

        public AudioHandler(DiscordSocketClient discord)
        {
            discord = discord;
        }

        public async Task JoinChannel(IVoiceChannel channel)
            => voiceClient = await channel.ConnectAsync();

        public async Task LeaveChannel()
        {
            await voiceClient.StopAsync();
            songQueue.Clear();
        }

        public async Task Play(string url, IVoiceChannel channel)
        {
            /* General function to play songs.
             * Smartly joins the voice channel, starts playing audio, and then disconnects.
             * If called while already playing audio, this function will add songs to the queue instead.
             * 
             * Args:
             *      url (string): can either be the youtube link or a search term
             *      channel (IVoiceChannel): the voice channel of the user invoking this command
             *      context (SocketCommandContext): used for sending messages to discord
             */

            if (voiceClient == null || voiceClient.ConnectionState == ConnectionState.Disconnected) await JoinChannel(channel);
            songQueue.Enqueue(GetSongInfo(url));
            if (isPlaying) return;
            isPlaying = true;
            AudioOutStream voice = null;

            while (songQueue.Count > 0)
            {
                var songInfo = songQueue.Dequeue();
                ProcessStartInfo cmd = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $@"/C youtube-dl --no-check-certificate --no-playlist -f bestaudio --default-search ytsearch -o - ""{songInfo.videoURL}"" | ffmpeg -i pipe:0 -f s16le -ar 48000 -ac 2 pipe:1",
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

        public SongInfo GetSongInfo(string url)
        {
            /* Gets basic information about the song being queued
             *      Title
             *      Video ID
             *      Thumbnail URL
             *      Duration
             *      
             * Args:
             *      url (string): the video URL or search term to be queued
             *      
             * Returns:
             *      (SongInfo): a basic struct containing the song information
             */

            ProcessStartInfo youtube = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $@"/C youtube-dl --get-title --get-id --get-thumbnail --get-duration --no-playlist --skip-download --default-search ytsearch ""{url}""",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            var yt = Process.Start(youtube);
            string title = yt.StandardOutput.ReadLine();
            string videoID = yt.StandardOutput.ReadLine();
            string thumbnailURL = yt.StandardOutput.ReadLine();
            string duration = yt.StandardOutput.ReadLine();
            return new SongInfo(title, "https://www.youtube.com/watch?v=" + videoID, thumbnailURL, duration);
        }

        public async Task test()
        {
            Console.WriteLine(songQueue.Count);
        }
    }
}