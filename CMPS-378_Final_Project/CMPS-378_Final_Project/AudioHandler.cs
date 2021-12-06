using CliWrap;
using Discord;
using Discord.Audio;
using Discord.WebSocket;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;

namespace DiscordBot
{
    public class AudioHandler
    {
        /* Class that handles all interactions with youtube using YoutubeExplode https://github.com/Tyrrrz/YoutubeExplode */

        private readonly DiscordSocketClient discord;
        private IAudioClient voiceClient;

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
            if (voiceClient == null || voiceClient.ConnectionState == ConnectionState.Disconnected) await JoinChannel(channel);

            var stream = await getSongByURL(url);

            var memoryStream = new MemoryStream();
            await Cli.Wrap("ffmpeg")
                .WithArguments(" -hide_banner -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1")
                .WithStandardInputPipe(PipeSource.FromStream(stream))
                .WithStandardOutputPipe(PipeTarget.ToStream(memoryStream))
                .ExecuteAsync();

            using (var voice = voiceClient.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await voice.WriteAsync(memoryStream.ToArray(), 0, (int)memoryStream.Length); }
                finally { await voice.FlushAsync(); }
            }

            await LeaveChannel();
        }

        public async Task<System.IO.Stream> getSongByURL(string url)
        {
            /* Fetches an audio stream from youtube from it's URL
             * 
             * Arguments:
             *      url (string): the url of the video to get the stream from
             *      
             * Returns:
             *      stream (System.IO.Stream): the audio stream that can be played by ffmpeg in Discord.Net
             */
            YoutubeClient youtube = new YoutubeClient();
            /* var videoData = await youtube.Videos.GetAsync(url);
            Console.WriteLine(videoData.Title);
            Console.WriteLine(videoData.Id); */

            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
            var streamInfo = streamManifest.GetAudioOnlyStreams().OrderBy(s => s.Bitrate).First();
            System.IO.Stream stream = await youtube.Videos.Streams.GetAsync(streamInfo);
            return stream;
        }

        public async Task<System.IO.Stream> getSongBySearch(string searchPhrase)
        {
            /* Searches youtube and pulls the audio stream from the video at the top of the results
             * After searching and getting the top result, calls getSongByURL()
             * 
             * Arguments:
             *      searchPhrase (string): the phrase used to search for
             *      
             * Returns:
             *      (System.IO.Stream): the audio stream that can be played by ffmpeg in Discord.Net
             */
            YoutubeClient youtube = new YoutubeClient();
            var searchResults = await youtube.Search.GetVideosAsync(searchPhrase);
            return await getSongByURL(searchResults[0].Url);
        }
    }
}