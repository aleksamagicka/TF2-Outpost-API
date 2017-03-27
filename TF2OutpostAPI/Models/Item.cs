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

namespace TF2OutpostAPI.Models
{
    public class Item
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Subtitle { get; set; }
        public int? Level { get; set; }
        public bool? Deleted { get; set; }
        public int? Quantity { get; set; }
        public string Hash { get; set; }
        public Enums.EUsedByClass UsedBy { get; set; }
        public string BackgroundImage { get; set; }
        public string CustomAttributes { get; set; }
    }
}