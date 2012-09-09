using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace GoldenGate
{
    public class AlbumItem : WebControl
    {
        public string ThumbNailUrl { get; set; }
        public string ItemUrl { get; set; }
        public AlbumItemType Type { get; set; }
        public new string CssClass
        {
            get
            {
                var result = "albumItem ";
                switch(Type)
                {
                    case AlbumItemType.Photo:
                        result += "photo";
                        break;
                    case AlbumItemType.Movie:
                        result += "movie";
                        break;
                }
                return result;
            }
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            var htmlOutput = String.Format(
            @"<div class='{0}'>
                    <img src='{1}' />
                    <input type='hidden' value='{2}' />
              </div>", CssClass, ThumbNailUrl, ItemUrl);

            writer.Write(htmlOutput);
        }

        public enum AlbumItemType
        {
            Photo,
            Movie
        }
    }
}