using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyUtils
{
    public class ListOfAlbumInfo
    {
        public List<AlbumInfo> AlbumInfo { get; set; }
        [JsonConstructor]

        public ListOfAlbumInfo(List<AlbumInfo> albums)
        {
            AlbumInfo = albums;
        }
    }
    public class AlbumInfo
    {
        public string ReleaseDate { get; set; }
        public string Label { get; set; }
        [JsonConstructor]
        public AlbumInfo(string release_date, string label)
        {
            ReleaseDate = release_date;
            Label = label;
        }

    }
}
