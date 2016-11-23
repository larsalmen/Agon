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
        public List<PlaylistItem> Items { get; set; }

        [JsonConstructor]
        public ListOfPlaylists(List<PlaylistItem> items)
        {
            Items = items;
        }
    }

    public class PlaylistItem
    {
        public string Name { get; set; }
        public Tracks Tracks { get; set; }

        [JsonConstructor]
        public PlaylistItem(string name, Tracks tracks)
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
            var temp = href.Substring(0, href.LastIndexOf("/"));
            temp = temp.Substring(temp.LastIndexOf("/") + 1);

            Href = temp;
        }
    }
}
