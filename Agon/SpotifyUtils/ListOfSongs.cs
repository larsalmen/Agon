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
        public ListOfAlbumInfo AlbumInfo { get; set; }

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
        public List<Artist> Artists { get; set; }
        public string Href { get; set; }
        public string PreviewUrl { get; set; }

        [JsonConstructor]
        public Track(string name, List<Artist> artists, string href, string preview_url)
        {
            Name = name;
            Artists = artists;
            var temp = href.Substring(href.LastIndexOf("/") + 1);
            Href = temp;
            if (preview_url == "" || preview_url == null)
                PreviewUrl = @"https://p.scdn.co/mp3-preview/bd315ecee6f7f42f4c36f1c4ae8ab4a20c66bb90";
            else
                PreviewUrl = preview_url;
        }
    }

    public class Artist
    {
        public string Name { get; set; }

        [JsonConstructor]
        public Artist(string name)
        {
            Name = name;
        }
    }

    public class Album
    {
        public string Name { get; set; }
        public string Href { get; set; }
        public string PreviewUrl { get; set; }

        [JsonConstructor]
        public Album(string name, string href, string preview_url)
        {
            Name = name;
            var temp = href.Substring(href.LastIndexOf("/") + 1);
            Href = temp;
            PreviewUrl = preview_url;
        }
    }
}
