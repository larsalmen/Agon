using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyUtils
{
    public class ListOfSongs
    {
        public List<SongItem> Items { get; set; }

        [JsonConstructor]
        public ListOfSongs(List<SongItem> items)
        {
            Items = items;
        }
    }

    public class SongItem
    {
        public Track Track { get; set; }

        [JsonConstructor]
        public SongItem(Track track)
        {
            Track = track;
        }
    }

    public class Track
    {
        public string Name { get; set; }
        public Album Album { get; set; }

        [JsonConstructor]
        public Track(string name)
        {
            Name = name;
        }
    }
    public class Album
    {
        public string Name { get; set; }
        public string Href { get; set; }

        [JsonConstructor]
        public Album(string name, string href)
        {
            Name = name;
            var temp = href.Substring(href.LastIndexOf("/") + 1);
            Href = temp;
        }
    }
}
