using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;

namespace DiscordBot
{
    public class AudioHandler
    {
        /* Class that handles all interactions with youtube using YoutubeExplode https://github.com/Tyrrrz/YoutubeExplode */

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
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
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