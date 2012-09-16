using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GoldenGate
{
    public class AlbumGroup : WebControl
    {
        public String GroupName { get; set; }

        public void AddAlbum(Album album)
        {
            this.Controls.Add(album);
        }

        public void AddAlbums(IEnumerable<Album> albums)
        {
            albums.ToList().ForEach(x => this.Controls.Add(x));
        }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write(
            @"<div class='albumGroup'>
                <div class='albumGroupHeader'>
                    <span class='albumGroupName'>{0}</span>
                </div>
                <div class='albumGroupContent'>", GroupName);

            this.RenderChildren(writer);

            writer.Write(
            @"  </div>
              </div>");
        }
    }
}