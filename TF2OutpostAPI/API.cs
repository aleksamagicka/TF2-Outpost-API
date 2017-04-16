// Copyright 2016-2017 Aleksa Savić
// Contact: GitHub@AleksaSavic.com
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TF2OutpostAPI.Models;

// ReSharper disable once CheckNamespace
namespace TF2Outpost
{
    [SuppressMessage("ReSharper", "TooWideLocalVariableScope")]
    public class API
    {
        private readonly HttpClient Client;
        private readonly HttpClient SearchClient;

        /// <summary>
        /// Initializes TF2 Outpost API with a preconfigured HttpClient.
        /// </summary>
        public API()
        {
            Client = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            });

            SearchClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            });

            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:52.0) Gecko/20100101 Firefox/52.0");
            Client.DefaultRequestHeaders.Add("Accept","text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            Client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            Client.DefaultRequestHeaders.Add("Connection", "keep-alive");

            SearchClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:52.0) Gecko/20100101 Firefox/52.0");
            SearchClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            SearchClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            SearchClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
        }

        /// <summary>
        /// Parses a trade, given its ID.
        /// </summary>
        /// <param name="tradeID"></param>
        /// <returns>A Trade instance filled with extracted data.</returns>
        public async Task<Trade> ParseTrade(ulong tradeID)
        {
            if (tradeID <= 0) throw new ArgumentOutOfRangeException(nameof(tradeID));

            var tf2OutpostTrade = new HtmlDocument();

            tf2OutpostTrade.LoadHtml(await Client.GetStringAsync(new Uri("http://www.tf2outpost.com/trade/" + tradeID)));

            return await ParseTrade(tradeID, tf2OutpostTrade);
        }

        /// <summary>
        /// Parses a trade, given its ID and HtmlDocument. You most likely won't use this method directly.
        /// </summary>
        /// <param name="tradeID"></param>
        /// <param name="tf2OutpostTrade"></param>
        /// <returns>A Trade instance filled with extracted data.</returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public async Task<Trade> ParseTrade(ulong tradeID, HtmlDocument tf2OutpostTrade)
        {
            if (tf2OutpostTrade.DocumentNode.InnerHtml.Contains("does not exist"))
                throw new ArgumentException("Trade with number " + tradeID + " does not exist!");

            var theirItems = await EnumerateTradeItems(tf2OutpostTrade, @"//div[contains(@class,'trade-has col-md-6')]");
            var wantedItems = await EnumerateTradeItems(tf2OutpostTrade, @"//div[contains(@class,'trade-wants col-md-6')]");

            var posterObject =
                tf2OutpostTrade.DocumentNode.SelectSingleNode(@"//div[contains(@class,'trade-post-user col-md-6')]");
            var poster = WebUtility.HtmlDecode(posterObject.ChildNodes[1].InnerText);

            var userID = posterObject.ChildNodes[1].Attributes[0].Value.Replace("/user/", "");

            var timestamp =
                DateTime.Parse(
                    tf2OutpostTrade.DocumentNode.SelectSingleNode(@"//div[contains(@class,'trade-post-time col-md-6')]")
                        .ChildNodes[3].Attributes[0].Value);

            var messageNode =
                tf2OutpostTrade.DocumentNode.SelectSingleNode(
                    @"//div[contains(@class, 'trade-notes-content summary-padded summary-light')]");

            var message = messageNode.InnerText ??
                          "Could not fetch trade notes. Reason: " + nameof(messageNode) + " is null.";

            var status =
                tf2OutpostTrade.DocumentNode.SelectSingleNode("//li[contains(@class,'trade-status')]")?.ChildNodes[2]?
                    .InnerText;
            var tradeStatus = status.DetermineTradeStatus();

            var closedTradeObject =
                tf2OutpostTrade.DocumentNode.SelectSingleNode(
                    "//div[contains(@class, 'widget widget-message widget-error')]")?.ChildNodes;

            string closedTradeNotification = null, reasonForClosing = null, closedBy = null;
            DateTime? timeOfClosing = null;
            bool isClosed = false;

            if (closedTradeObject != null)
            {
                isClosed = true;
                closedTradeNotification = closedTradeObject[3]?.InnerText;

                if (closedTradeNotification != "This trade was closed, but the reason is unknown.")
                {
                    reasonForClosing = closedTradeObject[5]?.InnerText;
                    closedBy = closedTradeObject[3]?.ChildNodes[1]?.InnerText;
                    string time =
                        closedTradeObject[3]?.ChildNodes[3].InnerText.Replace("on ", "")
                            .Replace("1st", "1")
                            .Replace("th", "")
                            .Replace("nd", "")
                            .Replace("rd", "");

                    if (time != null) timeOfClosing = DateTime.Parse(time);
                }

                closedTradeNotification += " " + reasonForClosing;
            }

            var tradeStats = tf2OutpostTrade.DocumentNode.SelectSingleNode("//div[contains(@class, 'trade-post-stats col-md-6')]");

            ulong numberOfViews = ulong.Parse(tradeStats.ChildNodes[1].ChildNodes[3].InnerText.Replace(",", "").Replace(".", ""));
            ulong numberOfStars = ulong.Parse(tradeStats.ChildNodes[3].ChildNodes[3].InnerText.Replace(",", "").Replace(".", ""));

            var tradePosts =
                tf2OutpostTrade.DocumentNode.SelectSingleNode("//div[contains(@class, 'trade-posts')]")?
                    .ChildNodes?.Where(p => p.Name != "#text")
                    .ToList();

            List<Message> messages = new List<Message>();

            if (tradePosts != null)
            {
                foreach (var post in tradePosts)
                {
                    Message startMsg = new Message();
                    Message replyMsg = new Message();

                    if (!post.OuterHtml.Contains("trade-post-replies"))
                    {
                        startMsg = await ExtractMessage(post, startMsg, false);
                        if (startMsg == null) continue;

                        startMsg.UserID = post.Attributes.FirstOrDefault(x => x.Name == "data-userid").Value;
                        startMsg.PostID = post.Attributes.FirstOrDefault(x => x.Name == "data-postid").Value;

                        startMsg.Status = startMsg.Contents.DetermineMessageStatus();
                        startMsg.Contents = Utilities.CleanMessage(startMsg.Contents);

                        messages.Add(startMsg);
                    }
                    else
                    {
                        string forPost = post.Attributes.FirstOrDefault(x => x.Name == "data-forpost").Value;

                        replyMsg = await ExtractMessage(post, replyMsg, true);
                        if (replyMsg == null) continue;

                        replyMsg.Status = replyMsg.Contents.DetermineMessageStatus();
                        replyMsg.Contents = Utilities.CleanMessage(replyMsg.Contents);

                        messages.Find(m => m.PostID == forPost).Replies.Add(replyMsg);
                    }
                }
            }

            Trade trade = new Trade
            {
                Number = tradeID,
                TheirItems = theirItems,
                WantedItems = wantedItems,
                Poster = poster,
                UserID = userID,
                NumberOfViews = numberOfViews,
                NumberOfStars = numberOfStars,
                TimeOfClosing = timeOfClosing,
                Notes = WebUtility.HtmlDecode(message),
                Timestamp = timestamp,
                TradeStatus = tradeStatus,
                ClosedNotification = closedTradeNotification,
                ClosedBy = closedBy,
                IsClosed = isClosed,
                ReasonForClosing = reasonForClosing,
                Messages = messages
            };

            return trade;
        }

        private async Task<Message> ExtractMessage(HtmlNode post, Message finalMsg, bool reply)
        {
            int divNodesCounter = 0;

            // Avatar
            var img =
                post.SelectSingleNode("//div[contains(@class, 'trade-post-avatar avatar st_online')]") ??
                post.SelectSingleNode("//div[contains(@class, 'trade-post-avatar avatar st_offline')]");

            /* 
               If the comment poster is banned, default avatar will show
               http://cdn.tf2outpost.com/img/avatars/default.jpg
            */

            try
            {
                img = img.ChildNodes.FirstOrDefault(x => x.Name == "a");
                finalMsg.UserImage = img.Attributes[0].Value.Replace("/user/", "");
            }
            catch (Exception e)
            {
                img = null;
            }           

            var divNode = post.ChildNodes.FirstOrDefault(n => n.Name == "div");
            if (divNode == null) return null;

            if (reply)
            {
                finalMsg.PostID = divNode.Attributes.FirstOrDefault(a => a.Name == "data-postid").Value;
                finalMsg.UserID = divNode.Attributes.FirstOrDefault(a => a.Name == "data-userid").Value;
            }

            var divNodes = reply
                ? post.ChildNodes.Where(n => n.Name == "div").ToList()[0].ChildNodes.Where(n => n.Name == "div")
                    .ToList()
                : post.ChildNodes.Where(n => n.Name == "div").ToList();

            finalMsg.UserImage =
                divNodes[divNodesCounter].ChildNodes.FirstOrDefault(n => n.Name == "a")
                    .ChildNodes.FirstOrDefault(n => n.Name == "img")
                    .Attributes.FirstOrDefault(n => n.Name == "src")
                    .Value;

            divNodesCounter++;

            // Nickname and time container
            var nicknameTimeNode =
                divNodes[divNodesCounter].ChildNodes.FirstOrDefault(n => n.Name == "div")
                    .ChildNodes.Where(n => n.Name == "div").ToList();

            // Nickname
            finalMsg.Poster = WebUtility.HtmlDecode(nicknameTimeNode[0].InnerText).Trim();

            // Time
            finalMsg.Time =
                DateTime.Parse(
                    nicknameTimeNode[1].ChildNodes.FirstOrDefault(n => n.Name == "time")
                        .Attributes.FirstOrDefault(n => n.Name == "datetime").Value);

            divNodesCounter++;

            // Message contents
            finalMsg.Contents = divNodes[divNodesCounter].InnerText.Trim();
            return finalMsg;
        }

        private async Task<List<Item>> EnumerateTradeItems(HtmlDocument tf2OutpostTrade, string XPath)
        {
            JObject itemAttributes;
            var TradeItems = new List<Item>(5);
            bool itemClassParseSuccess;

            var rawItemCollection = tf2OutpostTrade.DocumentNode.SelectSingleNode(XPath);
            var innerHtml = new HtmlDocument();
            innerHtml.LoadHtml(rawItemCollection.ChildNodes[3].InnerHtml);

            foreach (var node in innerHtml.DocumentNode.ChildNodes.Where(e => e.Name != "#text"))
            {
                itemAttributes = Utilities.NodeAttributesToJson(node.Attributes);
                itemClassParseSuccess = Enum.TryParse(itemAttributes["data-used_by"]?.ToString(), true, out Enums.EUsedByClass itemClass);
                string customAttributes = node.Attributes["data-attributes"]?.Value;
                customAttributes = customAttributes != null ? WebUtility.HtmlDecode(customAttributes) : String.Empty;

                TradeItems.Add(new Item
                {
                    Id = itemAttributes["data-id"]?.ToString(),
                    Name = WebUtility.HtmlDecode(itemAttributes["data-name"]?.ToString()),
                    Hash = itemAttributes["data-hash"]?.ToString(),
                    Subtitle = WebUtility.HtmlDecode(itemAttributes["data-subtitle"]?.ToString()),
                    BackgroundImage = itemAttributes["style"]?.ToString().ExtractBackgroundImage(),
                    Deleted = itemAttributes["class"]?.ToString().Contains("item-deleted"),
                    Quantity = itemAttributes["data-key"]?.ToString().ExtractItemQuantity(),
                    Level = itemAttributes["data-subtitle"]?.ToString().ExtractItemLevel(),
                    UsedBy = itemClassParseSuccess ? itemClass : Enums.EUsedByClass.Unknown,
                    CustomAttributes = customAttributes
                });
            }
            return TradeItems;
        }

        /// <summary>
        /// Parses a trade listing page.
        /// </summary>
        /// <param name="pageID">Page number to start parsing from.</param>
        /// <param name="userPage">Set this to true if you wish to parse a user page.</param>
        /// <param name="parseUntilEnd">Set this to true if you wish to brute force all possible pages.</param>
        /// <param name="stopAfter">Maximum number of pages to parse. There are 50 trades on a page by default.</param>
        /// <returns>A List filled with parsed trades.</returns>
        public async Task<List<Trade>> ParsePage(uint pageID, bool userPage, bool parseUntilEnd, uint stopAfter = uint.MaxValue)
        {
            if (parseUntilEnd && stopAfter != uint.MaxValue)
                throw new ArgumentException(nameof(parseUntilEnd) + "cannot be used in conjuction with " + nameof(stopAfter));

            HtmlDocument doc = new HtmlDocument();
            List<Trade> trades = new List<Trade>();

            string pageSuffix = userPage ? "user" : "recent";
            string URL = "http://www.tf2outpost.com/" + pageSuffix + "/";
            uint pageCounter = 1;

            do
            {
                doc.LoadHtml(await Client.GetStringAsync(new Uri(URL + pageID + "," + pageCounter)));

                if (userPage && doc.DocumentNode.InnerText.Contains("never made a trade"))
                    break;

                var tradeNodes = doc.DocumentNode.SelectNodes(@"//div[contains(@class,'trade widget widget-trade')]");

                foreach (var trade in tradeNodes)
                {
                    string temporaryTradeID = trade.Attributes.FirstOrDefault(a => a.Name == "data-tradeid").Value;
                    if (ulong.TryParse(temporaryTradeID, out ulong currentTradeID))
                    {
                        trades.Add(await ParseTrade(currentTradeID));
                    }
                }

                pageCounter++;
            }
            while (parseUntilEnd || pageCounter <= stopAfter);

            return trades;
        }

        /// <summary>
        /// Executes a search given a specified keyword.
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns>A SearchResult instance filled with extracted results.</returns>
        public async Task<SearchResults> Search(string keyword)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://www.tf2outpost.com/api/core");

            request.Content = new StringContent($"query={keyword}&action=header.search&hash=");
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            request.Headers.Host = "www.tf2outpost.com";

            HttpResponseMessage response = await SearchClient.SendAsync(request);

            /*
             Returned JSON can contain Unicode characters, such as emoticons

             We can't do response.Content.ReadAsStringAsync(), it throws because of incorrect formatting
            */

            Stream receiveStream = await response.Content.ReadAsStreamAsync();
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            string json = readStream.ReadToEnd();

            var result = JsonConvert.DeserializeObject<SearchResults>(json);

            if (result.Meta.Code != 200)
            {
                throw new HttpRequestException(nameof(result.Meta.Code) + " was different than 200 OK.");
            }

            return result;
        }
    }
}