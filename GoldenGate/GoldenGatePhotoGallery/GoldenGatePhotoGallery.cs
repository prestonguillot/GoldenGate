using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Serialization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.WebPartPages;
using WebPart = System.Web.UI.WebControls.WebParts.WebPart;
using System.Linq;

namespace GoldenGate.GoldenGatePhotoGallery
{
    [ToolboxItemAttribute(false)]
    public class GoldenGatePhotoGallery : WebPart
    {
        #region Web Part Configuration

        [WebBrowsable]
        [Personalizable(PersonalizationScope.Shared)]
        [SPWebCategoryName("Picture Library Configuration")]
        [WebDisplayName("Picture Library Name")]
        [XmlElement("PictureLibraryName")]
        public string PictureLibraryName { get; set; }

        #endregion

        #region Web Part Life Cycle

        protected override void CreateChildControls()
        {
            if(ConfigIsValid)
            {
                CreatePhotoGallery();
            }
            else
            {
                this.EmitErrorMessage("Web Part Configuration Is Invalid");
            }
        }

        #endregion

        private bool ConfigIsValid
        {
            get { return SPContext.Current.Web.ListExists(PictureLibraryName); }
        }

        private void CreatePhotoGallery()
        {
            var albumsQuery = new SPQuery()
                                  {
                                      Query = QueryResources.AlbumsQueryText,
                                  };

            var pictureLibrary = SPContext.Current.Web.Lists[PictureLibraryName];

            foreach(var curAlbum in pictureLibrary.GetItems(albumsQuery).Cast<SPListItem>().Select(x => x.Folder))
            {
                Controls.Add(new Album()
                                 {
                                     AlbumName = curAlbum.Name,
                                     ThumbNailUrl = "http://sharepointdev/_layouts/images/siteIcon.png",
                                     Type = Album.AlbumType.Photo,
                                     ItemsCount = curAlbum.ItemCount //This will include sub-folders in the count, but they shouldn't be there.
                                 });
            }

            var picturesQuery = new SPQuery()
                                    {
                                        Query = QueryResources.TopLevelPhotosQueryText,
                                    };

            foreach(SPListItem curPicture in pictureLibrary.GetItems(picturesQuery))
            {
                Controls.Add(new LiteralControl(String.Format(
                @"<div class='photo'>
                    <img src='{0}' />
                  </div>", curPicture["ows_EncodedAbsThumbnailUrl"])));
            }
        }

        private static class QueryResources
        {
            public const string AlbumsQueryText =
                @"<Where>
                    <Eq>
                        <FieldRef Name='ContentType' />
                        <Value Type='Computed'>Folder</Value>
                    </Eq>
                  </Where>
                  <OrderBy>
                    <FieldRef Name='Created' Ascending='False' />
                  </OrderBy>";

            public const string TopLevelPhotosQueryText =
                @"<Where>
                    <Eq>
                        <FieldRef Name='ContentType' />
                        <Value Type='Computed'>Picture</Value>
                    </Eq>
                  </Where>
                  <OrderBy>
                    <FieldRef Name='Created' Ascending='False' />
                  </OrderBy>";
        }
    }
}