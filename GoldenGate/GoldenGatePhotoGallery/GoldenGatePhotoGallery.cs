using System;
using System.Linq;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Serialization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebPartPages;
using WebPart = System.Web.UI.WebControls.WebParts.WebPart;

namespace GoldenGate.GoldenGatePhotoGallery
{
    [ToolboxItemAttribute(false)]
    public class GoldenGatePhotoGallery : WebPart
    {
        [WebBrowsable]
        [Personalizable(PersonalizationScope.Shared)]
        [SPWebCategoryName("Picture Library Configuration")]
        [WebDisplayName("Picture Library Name")]
        [XmlElement("PictureLibraryName")]
        public string PictureLibraryName { get; set; }

        [WebBrowsable]
        [Personalizable(PersonalizationScope.Shared)]
        [SPWebCategoryName("Picture Library Configuration")]
        [WebDisplayName("Custom CSS File")]
        [XmlElement("CustomCss")]
        public string CustomCss { get; set; }

        [WebBrowsable]
        [Personalizable(PersonalizationScope.Shared)]
        [SPWebCategoryName("Picture Library Configuration")]
        [WebDisplayName("Custom Script File")]
        [XmlElement("CustomScript")]
        public string CustomScript { get; set; }

        private string SelectedAlbumName
        {
            get { return Page.Request.QueryString["album"]; }
        }

        private bool ConfigIsValid
        {
            get { return SPContext.Current.Web.ListExists(PictureLibraryName); }
        }

        protected override void CreateChildControls()
        {
            if(ConfigIsValid)
            {
                //LOL SANDBOX ...
                this.Controls.Add(new LiteralControl(String.Format(@"<link rel='stylesheet' type='text/css' href='{0}' />", CustomCss)));
                this.Controls.Add(new LiteralControl(String.Format(@"<script src='{0}' type='text/javascript'></script>", CustomScript)));
                CreatePhotoGallery();
            }
            else
            {
                this.EmitErrorMessage("Web Part Configuration Is Invalid");
            }
        }

        private void CreatePhotoGallery()
        {
            var pictureLibrary = SPContext.Current.Web.Lists[PictureLibraryName];
            SPFolder selectedAlbum;

            if (String.IsNullOrEmpty(SelectedAlbumName) || !TryGetSelectedAlbum(pictureLibrary, out selectedAlbum))
            {
                AddAlbumControls(pictureLibrary);
                AddAlbumItemControls(pictureLibrary);
            }
            else
            {
                AddAlbumItemControls(pictureLibrary, selectedAlbum);
            }
        }

        private bool TryGetSelectedAlbum(SPList albumLibary, out SPFolder folder)
        {
            var foldersQuery = new SPQuery
            {
                Query = QueryResources.AlbumsQueryText
            };

            folder = albumLibary.GetItems(foldersQuery)
                                .Cast<SPListItem>()
                                .Select(x => x.Folder)
                                .FirstOrDefault(x => x.Name.Equals(SelectedAlbumName, StringComparison.CurrentCultureIgnoreCase));

            return folder != null;
        }

        private void AddAlbumControls(SPList albumLibrary)
        {
            var albumsQuery = new SPQuery
            {
                Query = QueryResources.AlbumsQueryText,
            };

            foreach (var curAlbum in albumLibrary.GetItems(albumsQuery).Cast<SPListItem>().Select(x => x.Folder))
            {
                Controls.Add(new Album
                {
                    AlbumName = curAlbum.Name,
                    ThumbNailUrl = "http://sharepointdev/_layouts/images/siteIcon.png", //TODO: Get the album thumbnail from a new content type field (?)
                    Type = Album.AlbumType.Photo,
                    ItemsCount = curAlbum.ItemCount, //This will include sub-folders in the count, but they shouldn't be there.
                    //TODO: Add album year/some kind of grouping mechanism for output (albums collection class?)
                });
            }
        }

        private void AddAlbumItemControls(SPList albumLibrary)
        {
            AddAlbumItemControls(albumLibrary, null);
        }

        private void AddAlbumItemControls(SPList albumLibrary, SPFolder fromFolder)
        {
            var picturesQuery = new SPQuery
            {
                Query = QueryResources.TopLevelPhotosQueryText,
            };

            if(fromFolder != null)
            {
                picturesQuery.Folder = fromFolder;
            }

            foreach (SPListItem curPicture in albumLibrary.GetItems(picturesQuery))
            {
                Controls.Add(new AlbumItem
                {
                    ThumbNailUrl = curPicture["ows_EncodedAbsThumbnailUrl"].ToString(), //TODO: does this column always exist?
                    Type = AlbumItem.AlbumItemType.Photo,
                    ItemUrl = String.Empty
                    //TODO: Populate full picture URL for jQuery magic to display the big image
                });
            }
        }

        private static class QueryResources
        {
            public const string AlbumsQueryText =
                @"<Where>
                    <BeginsWith>
                        <FieldRef Name='ContentTypeId' />
                        <Value Type='ContentTypeId'>0x0120</Value>
                    </BeginsWith>
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