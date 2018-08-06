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

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TF2Outpost.Models
{
    public class Enums
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum EUsedByClass
        {
            Unknown,
            Scout,
            Soldier,
            Pyro,
            Demoman,
            Heavy,
            Engineer,
            Medic,
            Sniper,
            Spy
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum EItemType
        {
            CraftItem,
            Apparel,
            Mask,
            FacialHair,
            Gift,
            Tool,
            Key,
            Satchel,
            ToolBelt,
            Wildcard
            // TODO: Add others
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ETradeStatus
        {
            Unknown,
            Open,
            Completed,
            Expired,
            ClosedByStaff
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum EMessageStatus
        {
            Shown,
            HiddenByUser,
            HiddenReplyByUser,
            HiddenByTradeOwner
            // TODO: I bet there is one more, when a user is banned and had their posts hidden
        }
    }
}