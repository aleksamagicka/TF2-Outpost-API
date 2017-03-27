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
    public class Message
    {
        public string Poster { get; set; }
        public string UserID { get; set; }
        public string PostID { get; set; }
        public string Contents { get; set; }
        public DateTime Time { get; set; }
        public string UserImage { get; set; }
        public Enums.EMessageStatus Status { get; set; }
        public List<Message> Replies { get; set; } = new List<Message>();
    }
}
