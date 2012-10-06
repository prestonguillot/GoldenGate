using System;
// ReSharper disable RedundantUsingDirective
using System.Linq;
// ReSharper restore RedundantUsingDirective
using System.Collections.Specialized;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint.Utilities;

namespace GoldenGate
{
    public class Album : WebControl
    {
        public String AlbumName { get; set; }
        public String ThumbNailUrl { get; set; }
        public int ItemsCount { get; set; }
        public AlbumType Type { get; set; }
// ReSharper disable InconsistentNaming
        private string _cssId;
// ReSharper restore InconsistentNaming
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
                        contentName =  ItemsCount != 1 ? "Videos" : "Video";
                        break;
                    case AlbumType.Photo:
                        contentName = ItemsCount != 1 ? "Photos" : "Photo";
                        break;
                }

                return String.Format("{0} {1}", ItemsCount, contentName);
            }
        }
        private string AlbumLinkText
        {
            get
            {
                const string queryStringParam = "album";
                var encodedAlbumName = SPEncode.UrlEncode(AlbumName);
                var queryStringValue = queryStringParam + "=" + encodedAlbumName;

                if(!Page.Request.QueryString.HasKeys())
                {
                    return "?" + queryStringValue;
                }
                
                if(Page.Request.QueryString[queryStringParam] == null)
                {
                    return "?" + Page.Request.QueryString + "&" + queryStringValue;
                }

                //I hate you for making me do this Microsoft.
                var editableQueryString = new NameValueCollection(Page.Request.QueryString);
                editableQueryString[queryStringParam] = encodedAlbumName;
                var sb = new StringBuilder();
                var first = true;
                foreach(var curKey in editableQueryString.AllKeys)
                {
                    sb.AppendFormat(first ? "?{0}={1}" : "&{0}={1}", curKey, editableQueryString[curKey]);
                    first = false;
                }

                return sb.ToString();
            }
        }
        public bool LazyImageLoadEnabled { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            var htmlOutput = String.Format(
            @"<div id='{0}' class='albumContainer'>
                 <a href='{1}'>
                     <div class='albumImg'>
                         <img src='{2}' data-image-source='{3}' />
                     </div>
                     <div class='albumTitle selectable'>
                         {4}
                     </div>
                     <div class='albumItems'>
                        {5}
                     </div>
                 </a>
             </div>", CssId, AlbumLinkText, LazyImageLoadEnabled ? "/_layouts/images/ImagePreviewHH.PNG" : ThumbNailUrl, ThumbNailUrl, AlbumName, ItemCountText);

            writer.Write(htmlOutput);
        }

        public enum AlbumType
        {
            Photo,
            Movie
        }
    }
}