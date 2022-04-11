using System.Collections.Generic;

namespace Youtuber.Responses
{
    public class GetChannelVideoResponse
    {
        public IEnumerable<YoutubeVideoDto> Videos { get; set; }
        public string PrevPageToken { get; set; }
        public string NextPageToken { get; set; }

    }
}