using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using System.Diagnostics;


namespace DiscordBot
{
    public struct SongInfo
    {
        /* A struct containing the basic information about a song
         * Information should be gathered from AudioHandler.GetSongInfo()
         */

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
        private List<SongInfo> songQueue = new List<SongInfo>();
        private bool isPlaying = false;
        private AudioOutStream voice = null;
        private CancellationTokenSource cancelTokenSource = new CancellationTokenSource();


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



        public async Task Play(string url, IVoiceChannel channel, SocketCommandContext context)
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
            SongInfo newSongInfo = GetSongInfo(url);
            songQueue.Add(newSongInfo);
            if (isPlaying)
            {   // don't immediately play songs if something is already playing, instead, add that new song to the queue
                await sendAddQueue(newSongInfo, context);
                return;
            }
            isPlaying = true;

            while (songQueue.Count > 0)
            {   // playes through all the songs in the queue
                var songInfo = songQueue[0];
                songQueue.RemoveAt(0);
                ProcessStartInfo cmd = new ProcessStartInfo // gets the audio stream from youtube
                {
                    FileName = "cmd.exe",
                    Arguments = $@"/C youtube-dl --no-check-certificate --no-playlist -f bestaudio --default-search ytsearch -o - ""{songInfo.videoURL}"" | ffmpeg -i pipe:0 -f s16le -ar 48000 -ac 2 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                var ffmpeg = Process.Start(cmd);
                var output = ffmpeg.StandardOutput.BaseStream;
                await sendNowPlaying(songInfo, context); // displays message to discord about what's about to start playing
                if (voice == null) voice = voiceClient.CreatePCMStream(AudioApplication.Mixed, 96000);
                try { await output.CopyToAsync(voice, cancelTokenSource.Token); } // starts playing music
                catch (OperationCanceledException) { cancelTokenSource = new CancellationTokenSource(); } // used for skipping and stopping music
                finally { await voice.FlushAsync(); }
            }

            isPlaying = false;
            await LeaveChannel();
        }



        public SongInfo GetSongInfo(string url)
        {
            /* Gets basic information about the song being queued via youtube-dl
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



        public async Task sendAddQueue(SongInfo songInfo, SocketCommandContext context)
        {
            /* Builds an embed message for discord containing the information about the song being added to the queue
             * 
             * Args:
             *      songInfo (SongInfo): all the information about the new song in a struct
             *      context (SocketCommandContext): used for sending messages to discord
             */

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithThumbnailUrl(songInfo.thumbnailURL);
            embed.WithUrl(songInfo.videoURL);
            embed.WithColor(Color.Green);
            embed.WithAuthor("Added to Queue");
            embed.WithTitle(songInfo.title);
            embed.AddField("Duration", songInfo.duration);
            embed.WithFooter($"Position #{songQueue.Count} in Queue");
            await context.Channel.SendMessageAsync(embed: embed.Build());
        }



        public async Task sendNowPlaying(SongInfo songInfo, SocketCommandContext context)
        {
            /* Builds an embed message for discord containing the information about what song is currently playing
             * 
             * Args:
             *      songInfo (SongInfo): all the information about the currently playing song in a struct
             *      context (SocketCommandContext): used for sending messages to discord
             */

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithThumbnailUrl(songInfo.thumbnailURL);
            embed.WithUrl(songInfo.videoURL);
            embed.WithColor(Color.Green);
            embed.WithAuthor("Now Playing");
            embed.WithTitle(songInfo.title);
            embed.AddField("Duration", songInfo.duration);
            if (songQueue.Count > 0)
            {
                SongInfo nextSongInfo = songQueue[0];
                embed.AddField("Up next:", $"[{nextSongInfo.title}]({nextSongInfo.videoURL})");
            }

            await context.Channel.SendMessageAsync(embed: embed.Build());
        }



        public async Task checkQueue(SocketCommandContext context)
        {
            /* Builds an embed message for discord containing the information about all the songs in the queue
             * 
             * Args:
             *      context (SocketCommandContext): used for sending messages to discord
             */

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(Color.Green);
            string songList = "";
            for (int i = 0; i < songQueue.Count; i++)
            {
                songList += $"#{(i + 1).ToString()}: [{songQueue[i].title}]({songQueue[i].videoURL})\n";
            }
            if (songList == "") songList = "There are no songs in the Queue";
            embed.AddField("Queue", songList);
            await context.Channel.SendMessageAsync(embed: embed.Build());
        }



        public async Task clearSongQueue(SocketCommandContext context)
        { 
            songQueue.Clear();
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(Color.Green);
            embed.WithAuthor("Queue Cleared");
            await context.Channel.SendMessageAsync(embed: embed.Build());
        }



        public void skipSong()
            => cancelTokenSource.Cancel();
    }
}