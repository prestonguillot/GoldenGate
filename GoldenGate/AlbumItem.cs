using System;
// ReSharper disable RedundantUsingDirective
using System.Linq;
// ReSharper restore RedundantUsingDirective
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
                    <a href='javascript:OpenPopUpPage(""{2}"");'>
                        <img src='{1}' />
                        <input type='hidden' value='{2}' />
                    </a>
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