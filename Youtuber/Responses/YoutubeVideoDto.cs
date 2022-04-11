using System;

namespace Youtuber.Responses
{
    public class YoutubeVideoDto
    {
        public string VideoId { get; set; }
        public string Title { get; set; }
        public DateTime? PublishedDate { get; set; }
        public string ChannelId { get; set; }
        public string ChannelTitle { get; set; }
    }
}
