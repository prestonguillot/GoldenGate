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
            try
            {
                if (ConfigIsValid)
                {
                    //LOL SANDBOX ...
                    this.Controls.Add(
                        new LiteralControl(String.Format(@"<link rel='stylesheet' type='text/css' href='{0}' />",
                                                         CustomCss)));
                    this.Controls.Add(
                        new LiteralControl(String.Format(@"<script src='{0}' type='text/javascript'></script>",
                                                         CustomScript)));
                    CreatePhotoGallery();
                }
                else
                {
                    this.EmitErrorMessage("Web Part Configuration Is Invalid");
                }
            }
            catch(Exception ex)
            {
                this.Controls.Add(new LiteralControl("Something is horribly wrong: " + ex));
            }
        }

        private void CreatePhotoGallery()
        {
            var pictureLibrary = SPContext.Current.Web.Lists[PictureLibraryName];
            SPFolder selectedAlbum;

            if (String.IsNullOrEmpty(SelectedAlbumName) || !TryGetSelectedAlbum(pictureLibrary, out selectedAlbum))
            {
                AddAlbumControls(pictureLibrary);
                this.Controls.Add(new LiteralControl(
                @"<div class='albumHeader recent'>Recent Photos</div>"));
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

            foreach (var curAlbum in albumLibrary.GetItems(albumsQuery).Cast<SPListItem>().GroupBy(x => x["Start Date"] == null ? "Timeless" : x.GetFormattedValue("Album Year")).OrderBy(x => x.Key)) //TODO: handle empty album year, format string value here
            {
                var albumGroup = new AlbumGroup() { GroupName = curAlbum.Key.Replace(",", String.Empty) };

                foreach(var album in curAlbum)
                {
                    //THANKS SHAREPOINT
                    var thumbNailUrlField = album["Album Thumbnail URL"];
                    var thumbNailUrl = thumbNailUrlField != null
                                           ? new SPFieldUrlValue(thumbNailUrlField.ToString()).Url
                                           : "http://sharepointdev/SiteAssets/Folder.png"; //TODO: make this URL relative
                    
                    albumGroup.AddAlbum(new Album
                    {
                        AlbumName = album.Folder.Name,
                        ThumbNailUrl = thumbNailUrl,
                        Type = Album.AlbumType.Photo,
                        ItemsCount = album.Folder.ItemCount, //This will include sub-folders in the count, but they shouldn't be there.
                    });
                }

                Controls.Add(albumGroup);
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