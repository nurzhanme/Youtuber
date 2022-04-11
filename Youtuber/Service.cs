using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;

using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using Youtuber.Responses;

namespace Youtuber
{
    public class Service
    {

        private IClientService _clientService;
        private const string SNIPPET = "snippet";

        private readonly YoutubeOptions _options;

        public Service(YoutubeOptions options)
        {
            _options = options;
        }

        public async Task<(bool, string)> Auth(string applicationName, string accessToken = null)
        {
            var user = "user";

            var isAccessTokenUpdated = false;

            var scope = new[] { YouTubeService.Scope.Youtube, YouTubeService.Scope.YoutubeForceSsl, YouTubeService.Scope.Youtubepartner };

            var dataStore = new SimpleDataStore();

            var secrets = new ClientSecrets
            {
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret
            };

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(secrets, scope, user, CancellationToken.None, dataStore);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                isAccessTokenUpdated = true;
            }
            else
            {
                var oldToken = JsonConvert.DeserializeObject<TokenResponse>(accessToken);
                var newToken = await dataStore.GetAsync<TokenResponse>(user);

                //isAccessTokenUpdated = oldToken.IdToken == newToken.IdToken && oldToken.AccessToken == newToken.
            }

            _clientService = new YouTubeService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName,
            });

            return (isAccessTokenUpdated, isAccessTokenUpdated ? await dataStore.GetJsonAsync(user) : accessToken);
        }

        public async Task<string> GetMyChannelId()
        {
            var channelsResource = new ChannelsResource(_clientService);

            var request = channelsResource.List(SNIPPET);
            request.Mine = true;

            var result = await request.ExecuteAsync();

            return result.Items[0].Id;
        }

        public async Task VideosRate(string id)
        {
            var videosResource = new VideosResource(_clientService);
            var request = videosResource.Rate(id, VideosResource.RateRequest.RatingEnum.Like);

            await request.ExecuteAsync();
        }

        public async Task CommentThreadsInsert(string id, string channelId, string commentText)
        {
            var commentThreadsResource = new CommentThreadsResource(_clientService);
            var commentThread = new CommentThread
            {
                Snippet = new CommentThreadSnippet
                {
                    ChannelId = channelId,
                    VideoId = id,
                    TopLevelComment = new Comment
                    {
                        Snippet = new CommentSnippet
                        {
                            TextOriginal = commentText
                        }
                    }
                }
            };
            var request = commentThreadsResource.Insert(commentThread, SNIPPET);

            await request.ExecuteAsync();
        }

        public async Task SubscribtionsInsert(string channelId)
        {
            var subscriptionsResource = new SubscriptionsResource(_clientService);

            var subscription = new Subscription
            {
                Kind = "youtube#channel",
                Id = channelId
            };

            var request = subscriptionsResource.Insert(subscription, SNIPPET);
            await request.ExecuteAsync();
        }

        public async Task<string> GetChannelIdBy(string username)
        {
            var resource = new ChannelsResource(_clientService);
            var listRequest = resource.List("id");
            listRequest.ForUsername = username;

            var channelListResponse = await listRequest.ExecuteAsync();
            return channelListResponse.Items[0].Id;
        }

        public async Task<GetChannelVideoResponse> GetChannelVideo(string channelId, DateTime publishedAfter = default, string pageToken = default)
        {
            var resource = new SearchResource(_clientService);
            var searchListRequest = resource.List(SNIPPET);
            searchListRequest.ChannelId = channelId;
            searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
            searchListRequest.MaxResults = 50;
            searchListRequest.Type = "video";
            searchListRequest.PublishedAfter = publishedAfter;
            searchListRequest.PageToken = pageToken;

            var searchListResponse = await searchListRequest.ExecuteAsync();

            var videos = searchListResponse.Items.Select(x => new YoutubeVideoDto
            {
                ChannelId = channelId,
                ChannelTitle = x.Snippet.ChannelTitle,
                PublishedDate = x.Snippet.PublishedAt,
                Title = x.Snippet.Title,
                VideoId = x.Id.VideoId
            });
            
            return new GetChannelVideoResponse { Videos = videos, NextPageToken = searchListResponse.NextPageToken, PrevPageToken = searchListResponse.PrevPageToken };
        }
    }
}
