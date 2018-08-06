using Newtonsoft.Json;

namespace TF2Outpost.Models
{
    public class SearchResults
    {
        public Meta Meta { get; set; }
        public Data Data { get; set; }
    }

    public class Meta
    {
        public int Code { get; set; }
    }

    public class Data
    {
        public string Type { get; set; }
        public Users Users { get; set; }
        public Items Items { get; set; }
    }

    public class Users
    {
        public UserData[] Data { get; set; }
        [JsonProperty(PropertyName = "too_many")]
        public bool TooMany { get; set; }
    }

    public class UserData
    {
        public string ID { get; set; }
        public string Nickname { get; set; }
        public string Avatar { get; set; }
        public string Status { get; set; }
    }

    public class Items
    {
        public ItemData[] Data { get; set; }
        [JsonProperty(PropertyName = "too_many")]
        public bool TooMany { get; set; }
    }

    public class ItemData
    {
        public int GameID { get; set; }
        public string GameName { get; set; }
        public int Index { get; set; }
        public int Quality { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Hash { get; set; }
        public string Icon { get; set; }
        public string Json { get; set; }
        public string _Class { get; set; }
    }
}