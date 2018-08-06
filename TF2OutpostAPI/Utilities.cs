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
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using TF2Outpost.Models;

// ReSharper disable once CheckNamespace
namespace TF2Outpost
{
    internal static class Utilities
    {
        internal static JObject NodeAttributesToJson(HtmlAttributeCollection attributeCollection)
        {
            var json = new JObject();
            foreach (var attribute in attributeCollection)
            {
                json.Add(attribute.Name, attribute.Value);
            }
            return json;
        }

        internal static string ExtractBackgroundImage(this string style)
        {
            if (String.IsNullOrWhiteSpace(style)) return String.Empty;
            return style.Replace("background-image:url('", "").Replace("');", "");
        }

        internal static int ExtractItemLevel(this string subtitle)
        {
            // From http://stackoverflow.com/a/19169192
            Regex onlyNumbers = new Regex(@"[^\d]+");
            int level;
            if (int.TryParse(onlyNumbers.Replace(subtitle, ""), out level))
                return level;
            return -1;
        }

        internal static int ExtractItemQuantity(this string key)
        {
            return key.Contains("has") || key.Contains("") ? ExtractItemLevel(key) : -1;
        }

        internal static Enums.ETradeStatus DetermineTradeStatus(this string status)
        {
            switch (status)
            {
                case "Completed":
                    return Enums.ETradeStatus.Completed;
                case "Closed by staff":
                    return Enums.ETradeStatus.ClosedByStaff;
                case "Expired due to inactivity":
                    return Enums.ETradeStatus.Expired;
                case null:
                    return Enums.ETradeStatus.Open;
                default:
                    return Enums.ETradeStatus.Unknown;
            }
        }

        internal static Enums.EMessageStatus DetermineMessageStatus(this string message)
        {
            if (message.StartsWith("This user hid their own post."))
            {
                return Enums.EMessageStatus.HiddenByUser;
            }

            if (message.StartsWith("This post was hidden by the trade owner."))
            {
                return Enums.EMessageStatus.HiddenByTradeOwner;
            }

            if (message.StartsWith("This user hid their own reply."))
            {
                return Enums.EMessageStatus.HiddenReplyByUser;
            }

            // TODO: Investigate http://www.tf2outpost.com/trade/893
            return Enums.EMessageStatus.Shown;
        }

        internal static string CleanMessage(string messageToClean)
        {
            string message = messageToClean;

            if (messageToClean.StartsWith("This user hid their own post."))
            {
                message = messageToClean.ReplaceFirst("This user hid their own post.", "");
            }

            if (messageToClean.StartsWith("This post was hidden by the trade owner."))
            {
                message = messageToClean.ReplaceFirst("This post was hidden by the trade owner.", "");
            }

            if (messageToClean.StartsWith("This user hid their own reply."))
            {
                message = messageToClean.ReplaceFirst("This user hid their own reply.", "");
            }

            return message;
        }

        // From StackOverflow: http://stackoverflow.com/a/8809437/5504421
        private static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search, StringComparison.Ordinal);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }
}