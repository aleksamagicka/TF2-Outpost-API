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

namespace TF2OutpostAPI.Models
{
    public class Trade
    {
        public ulong Number { get; set; }
        public string Poster { get; set; }
        public string UserID { get; set; }
        public string Notes { get; set; }
        public Enums.ETradeStatus TradeStatus { get; set; }
        public bool IsClosed { get; set; }
        public string ReasonForClosing { get; set; }
        public DateTime? TimeOfClosing { get; set; }
        public string ClosedBy { get; set; }
        public string ClosedNotification { get; set; }
        public ulong NumberOfViews { get; set; }
        public ulong NumberOfStars { get; set; }
        public DateTime Timestamp { get; set; }
        public List<Item> TheirItems { get; set; }
        public List<Item> WantedItems { get; set; }
        public List<Message> Messages { get; set; }
    }
}
