using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyUtils
{
    public class ListOfPlaylists
    {
        public List<Item> Items { get; set; }

        [JsonConstructor]
        public ListOfPlaylists(List<Item> items)
        {
            Items = items;
        }
    }

    public class Item
    {
        public string Name { get; set; }
        public Tracks Tracks { get; set; }

        [JsonConstructor]
        public Item(string name, Tracks tracks)
        {
            Name = name;
            Tracks = tracks;
        }
    }

    public class Tracks
    {
        public string Href { get; set; }

        [JsonConstructor]
        public Tracks(string href)
        {
            Href = href;
        }
    }
}
