﻿using System;
using System.Linq;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Serialization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebPartPages;
using WebPart = System.Web.UI.WebControls.WebParts.WebPart;

namespace GoldenGate.GoldenGatePhotoGallery
{
    [ToolboxItemAttribute(false)]
    public class GoldenGatePhotoGallery : WebPart
    {
        #region Web Part Properties

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

        [WebBrowsable]
        [Personalizable(PersonalizationScope.Shared)]
        [SPWebCategoryName("Picture Library Configuration")]
        [WebDisplayName("Default Album Image")]
        [XmlElement("DefaultAlbumImage")]
        public string DefaultAlbumImage { get; set; }

        [WebBrowsable]
        [Personalizable(PersonalizationScope.Shared)]
        [SPWebCategoryName("Picture Library Configuration")]
        [WebDisplayName("Default Albums Per Group")]
        [XmlElement("AlbumsPerGroup")]
        public int AlbumsPerGroup { get; set; }

        [WebBrowsable]
        [Personalizable(PersonalizationScope.Shared)]
        [SPWebCategoryName("Picture Library Configuration")]
        [WebDisplayName("Images Per Page")]
        [XmlElement("ImagesPerPage")]
        public int ImagesPerPage { get; set; }

        [WebBrowsable]
        [Personalizable(PersonalizationScope.Shared)]
        [SPWebCategoryName("Picture Library Configuration")]
        [WebDisplayName("Enable Image Lazy Loading")]
        [XmlElement("LazyLoadingEnabled")]
        public bool LazyLoadingEnabled { get; set; }

        #endregion

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
                @"<div class='albumHeader recent'>Photos Posted to the News Feed</div>"));
                AddAlbumItemControls(pictureLibrary);
            }
            else
            {
                //TODO: stop being lazy
                var createdBy = new SPFieldUserValue(SPContext.Current.Web, selectedAlbum.Item["ows_Author"].ToString()).User.Name;
                var modifiedDate = DateTime.Parse(selectedAlbum.Item["ows_Modified"].ToString()).ToString("MM/dd/yyyy");

                var upLoadControlHtml = GenerateUploadControlHtml(pictureLibrary, selectedAlbum);
                var editAlbumControlHtml = GenerateEditAlbumControlHtml(pictureLibrary, selectedAlbum);
                var albumBackLink = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path);
                //HACK: When using HTTPS this returns a url in the form of "http://mysite:443" instead of "https://mysite" which causes mixed content warnings, the following block repairs this, poorly
                if(albumBackLink.StartsWith("http://") && albumBackLink.Contains(":443/"))
                {
                    albumBackLink = albumBackLink.Replace("http://", "https://").Replace(":443/", "/");
                }
                this.Controls.Add(new LiteralControl(String.Format(
                @"<div class='albumDetailHeader'>
                    <a href='{0}' id='albumBack' class='selectable'>&lt; Back to Albums</a>
                    {1}
                    {2}
                    <h2 id='albumTitle'>{3}</h2>
                    <ul>
                        <li>{4} Photos</li>
                        <li>Created by: {5}</li>
                        <li>Last Updated On: {6}</li>
                    </ul>
                  </div>
                  <div class='albumClear'></span>", albumBackLink, upLoadControlHtml, editAlbumControlHtml, selectedAlbum.Name, selectedAlbum.ItemCount, createdBy, modifiedDate)));
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

            var albumGroups =
                albumLibrary.GetItems(albumsQuery).Cast<SPListItem>().GroupBy(x => x["Start Date"] == null ? "Timeless" : x.GetFormattedValue("Album Year"))
                            .OrderByDescending(x => x.Key).ToList();

            //HACK: Get the albums with no date value at the end of the the list instead of the begining.
            var timelessPhotos = albumGroups.FirstOrDefault(x => x.Key == "Timeless");
            if(timelessPhotos != null)
            {
                albumGroups.Remove(timelessPhotos);
                albumGroups.Add(timelessPhotos);
            }

            var uploadControlHtml = GenerateUploadControlHtml(albumLibrary);
            var addAlbumControlHtml = GenerateAddAlbumControHtml(albumLibrary);
            var topHtml = String.Format(@"<div class='titleRow'>
                                            <span id='albumTitle'>Photos</span>
                                            {0}
                                            {1}
                                          </div>", uploadControlHtml, addAlbumControlHtml);
            Controls.Add(new LiteralControl(topHtml));

            var headerHtml = new StringBuilder(String.Format(@"<div class='albumHeader' data-albums-visible-for-groups='{0}'>Albums: ", AlbumsPerGroup), 250);
            foreach(var albumGroup in albumGroups.Select(x => x.Key))
            {
                headerHtml.AppendFormat(@"<span class='albumNav groupName selectable'>{0}</span>", albumGroup.Replace(",", String.Empty));
            }
            headerHtml.Append("</div>");
            Controls.Add(new LiteralControl(headerHtml.ToString()));

            foreach (var curAlbum in albumGroups)
            {
                var albumGroup = new AlbumGroup { GroupName = curAlbum.Key.Replace(",", String.Empty) };

                foreach(var album in curAlbum)
                {
                    //THANKS SHAREPOINT
                    var thumbNailUrlField = album["Album Thumbnail URL"];
                    var thumbNailUrl = thumbNailUrlField != null
                                           ? new SPFieldUrlValue(thumbNailUrlField.ToString()).Url
                                           : DefaultAlbumImage ?? String.Empty;
                    
                    albumGroup.AddAlbum(new Album
                    {
                        AlbumName = album.Folder.Name,
                        ThumbNailUrl = thumbNailUrl,
                        LazyImageLoadEnabled = this.LazyLoadingEnabled,
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

            var itemCount = 0;
            foreach (SPListItem curPicture in albumLibrary.GetItems(picturesQuery))
            {
                Controls.Add(new AlbumItem
                {
                    ThumbNailUrl = curPicture["ows_EncodedAbsThumbnailUrl"].ToString(),
                    LazyImageLoadEnabled = this.LazyLoadingEnabled,
                    Type = AlbumItem.AlbumItemType.Photo,
                    //ItemUrl = curPicture["ows_EncodedAbsUrl"].ToString()
                    ItemUrl = curPicture["ows_EncodedAbsWebImgUrl"].ToString()
                });
                itemCount++;
            }

            Controls.Add(new SimplePager { PageSize = ImagesPerPage > 0 ? (uint)ImagesPerPage : 0, TotalItems = (uint)itemCount});
        }

        private static string GenerateUploadLink(SPList albumLibrary, SPFolder destinationFolder)
        {
            var layoutsUrl = albumLibrary.ParentWeb.ServerRelativeUrl + (albumLibrary.ParentWeb.ServerRelativeUrl.EndsWith("/") ? string.Empty : "/") + "_layouts";
            var formLink = String.Format("{0}/Upload.aspx?List={1}&RootFolder={2}&Source={3}", layoutsUrl, albumLibrary.ID, destinationFolder.ServerRelativeUrl, HttpContext.Current.Request.Url);
            formLink = SPEncode.ScriptEncode(formLink);
            var javascriptActionLink = String.Format(@"EditItem2(event, '{0}')", formLink);
            return javascriptActionLink;
        }

        private static string GenerateUploadControlHtml(SPList albumLibrary)
        {
            return GenerateUploadControlHtml(albumLibrary, albumLibrary.RootFolder);
        }

        private static string GenerateUploadControlHtml(SPList albumLibrary, SPFolder destinationFolder)
        {
            var javaScriptActionLink = GenerateUploadLink(albumLibrary, destinationFolder);
            return String.Format(@"<span class='albumButton' onClick=""{0}"">Add Photos</span>", javaScriptActionLink);
        }

        private static string GenerateAddAlbumLink(SPList albumLibrary)
        {
            var layoutsUrl = albumLibrary.ParentWeb.ServerRelativeUrl + (albumLibrary.ParentWeb.ServerRelativeUrl.EndsWith("/") ? string.Empty : "/") + "_layouts";
            var contentTypeId = String.Empty;
            foreach(SPContentType contentType in albumLibrary.ContentTypes)
            {
                if(contentType.Name == "Photo Album")
                {
                    contentTypeId = contentType.Id.ToString();
                    break;
                }
            }
            var formLink = String.Format("{0}/listform.aspx?ListId={1}&PageType=8&RootFolder={2}&ContentTypeId={3}", layoutsUrl, albumLibrary.ID, albumLibrary.RootFolder.ServerRelativeUrl, contentTypeId);
            formLink = SPEncode.ScriptEncode(formLink);
            var javascriptActionLink = String.Format(@"NewItem2(event, '{0}')", formLink);
            return javascriptActionLink;
        }

        private static string GenerateAddAlbumControHtml(SPList albumLibrary)
        {
            var javascriptActionLink = GenerateAddAlbumLink(albumLibrary);
            return String.Format(@"<span class='albumButton' onClick=""{0}"">Add Album</span>", javascriptActionLink);
        }


        private static string GenerateEditAlbumLink(SPList albumLibrary, SPFolder albumFolder)
        {
            var layoutsUrl = albumLibrary.ParentWeb.ServerRelativeUrl + (albumLibrary.ParentWeb.ServerRelativeUrl.EndsWith("/") ? string.Empty : "/") + "_layouts";
            var formLink = String.Format("{0}/listform.aspx?ListId={1}&PageType=6&Id={2}&RootFolder={3}", layoutsUrl, albumLibrary.ID, albumFolder.Item.ID, albumLibrary.RootFolder.ServerRelativeUrl);
            formLink = SPEncode.ScriptEncode(formLink);
            var javascriptActionLink = String.Format(@"EditItem2(event, '{0}')", formLink);
            return javascriptActionLink;
        }

        private static string GenerateEditAlbumControlHtml(SPList albumLibrary, SPFolder albumFolder)
        {
            var javascriptActionLink = GenerateEditAlbumLink(albumLibrary, albumFolder);
            return String.Format(@"<span class='albumButton' onClick=""{0}"">Edit Album</span>", javascriptActionLink);
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