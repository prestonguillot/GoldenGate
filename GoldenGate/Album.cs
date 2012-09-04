using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GoldenGate
{
    public class Album : WebControl
    {
        public String AlbumName { get; set; }
        public String ThumbNailUrl { get; set; }
        public int ItemsCount { get; set; }
        public AlbumType Type { get; set; }
        private string _cssId;
        public String CssId
        {
            get
            {
                return String.IsNullOrEmpty(_cssId) ? String.Format("album{0}", Guid.NewGuid()) : _cssId;
            }
            set { _cssId = value; }
        }
        private string ItemCountText
        {
            get
            {
                var contentName = String.Empty;

                switch (Type)
                {
                    case AlbumType.Movie:
                        contentName =  ItemsCount > 1 ? "Videos" : "Video";
                        break;
                    case AlbumType.Photo:
                        contentName = ItemsCount > 1 ? "Photos" : "Photo";
                        break;
                }

                return String.Format("{0} {1}", ItemsCount, contentName);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            var htmlOutput = String.Format(
            @"<div id='{0}' class='albumContainer'>
                 <div class='albumImg'>
                     <img src='{1}' />
                 </div>
                 <div class='albumTitle'>
                     {2}
                 </div>
                 <div class='albumItems'>
                    {3}
                 </div>
             </div>", CssId, ThumbNailUrl, AlbumName, ItemCountText);

            writer.Write(htmlOutput);
        }

        public enum AlbumType
        {
            Photo,
            Movie
        }
    }
}