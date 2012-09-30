$(function () {
    $(".albumGroup").slice(1).hide();

    var defaultAlbumsPerGroupDisplayed = $(".albumHeader").data("albumsVisibleForGroups");
    var firstAlbumGroup = $(".albumHeader > .albumNav:first");
    $(".albumHeader > .albumNav").click(function () {
        toggleAlbumGroupDisplay($(this), defaultAlbumsPerGroupDisplayed);
    });
    toggleAlbumGroupDisplay($(firstAlbumGroup), defaultAlbumsPerGroupDisplayed);
    $(firstAlbumGroup).addClass("selected");

    if (defaultAlbumsPerGroupDisplayed > 0) {
        $(".albumGroup").each(function (index, albumGroup) {
            var albumGroupAlbums = $(albumGroup).find(".albumGroupContent > .albumContainer");
            if (albumGroupAlbums.length > defaultAlbumsPerGroupDisplayed) {
                albumGroupAlbums.slice(defaultAlbumsPerGroupDisplayed).hide();
                $(albumGroup).children(".albumGroupContent").append($("<div>").addClass("albumContentToggle more").click(function () {
                    toggleAlbumsDisplay(albumGroupAlbums, $(this), defaultAlbumsPerGroupDisplayed);
                }).text("Show More"));
            }
        });
    }

    $(".galleryPager").each(function (index, albumPager) {
        var pageSize = $(albumPager).data("pageSize");
        var imagesToLoad = pageSize > 0 ? $(albumPager).siblings(".albumItem").slice(0, pageSize).find("img")
                                        : $(albumPager).siblings(".albumItem").find("img");
        $(albumPager).siblings(".albumItem").slice($(albumPager).data("pageSize")).hide();
        $(imagesToLoad).each(function () { swapImgSrc($(this)); });
    });

    $(".galleryPager > ul > li").click(function () {
        var pager = $(this).closest(".galleryPager");
        var currentPage = $(pager).data("currentPage");
        var selectedPage = $(this).text();
        switch (selectedPage) {
            case "Prev":
                selectedPage = currentPage - 1;
                break;
            case "Next":
                selectedPage = currentPage + 1;
                break;
            default:
                selectedPage = parseInt(selectedPage);
                break;
        }
        var totPages = Math.ceil($(pager).data("totalItems") / $(pager).data("pageSize"));
        if (selectedPage != currentPage && selectedPage > 0 && selectedPage <= totPages) {
            $(this).parent().children("li.selected").removeClass("selected");
            $($(this).parent().children("li").get(selectedPage)).addClass("selected");
            $(pager).data("currentPage", selectedPage);
            pageAlbumItems(pager, $(pager).siblings(".albumItem"));
        }
    });
});

function toggleAlbumGroupDisplay(toggleGroupLink, defaultAlbumsPerGroupDisplayed) {
    if (!toggleGroupLink.hasClass("selected")) {
        toggleGroupLink.siblings(".selected").removeClass("selected");
        toggleGroupLink.addClass("selected");
        $(".albumGroup").hide();
        var albumGroup = $("#albumGroup" + toggleGroupLink.text());
        $(albumGroup).fadeIn("fast");
        var albumImagesToLoad = defaultAlbumsPerGroupDisplayed > 0 ? $(albumGroup).find("img").slice(0, defaultAlbumsPerGroupDisplayed)
                                                                   : $(albumGroup).find("img");
        $(albumImagesToLoad).each(function () { swapImgSrc($(this)); });
    }
}

function toggleAlbumsDisplay(albumGroupAlbums, toggleLink, defaultAlbumsPerGroupDisplayed) {
    $(albumGroupAlbums).slice(defaultAlbumsPerGroupDisplayed).toggle("fast");
    $(albumGroupAlbums).find("img").each(function () { swapImgSrc($(this)); });

    if ($(toggleLink).hasClass("more")) {
        $(toggleLink).removeClass("more").addClass("less").text("Show Less");
    } else if ($(toggleLink).hasClass("less")) {
        $(toggleLink).removeClass("less").addClass("more").text("Show More");
    }
}

function pageAlbumItems(pager, albumItems) {
    var startIndex = ($(pager).data("currentPage") - 1) * $(pager).data("pageSize");
    var endIndex = startIndex + $(pager).data("pageSize");
    endIndex = endIndex > $(albumItems).length ? $(albumItems).length : endIndex;
    $(albumItems).hide();
    $(albumItems).slice(startIndex, endIndex).fadeIn("fast").find("img").each(function () {
        swapImgSrc($(this));
    });
}

function swapImgSrc(imageElement) {
    var currentSrc = $(imageElement).attr("src");
    var dataSrc = $(imageElement).data("imageSource");
    if (dataSrc != null && dataSrc != currentSrc) {
        $(imageElement).attr("src", dataSrc);
    }
}