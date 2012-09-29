$(function () {
    $(".albumGroup").slice(1).hide();

    $(".albumHeader > .albumNav:first").addClass("selected");

    $(".albumHeader > .albumNav").click(function () {
        toggleAlbumGroupDisplay($(this));
    });

    var defaultAlbumsPerGroupDisplayed = $(".albumHeader").data("albumsVisibleForGroups");
    if (defaultAlbumsPerGroupDisplayed > 0) {
        $(".albumGroup").each(function(index, albumGroup) {
            var albumGroupAlbums = $(albumGroup).find(".albumGroupContent > .albumContainer");
            if (albumGroupAlbums.length > defaultAlbumsPerGroupDisplayed) {
                albumGroupAlbums.slice(defaultAlbumsPerGroupDisplayed).hide();
                $(albumGroup).children(".albumGroupContent").append($("<div>").addClass("albumContentToggle more").click(function() {
                    toggleAlbumsDisplay(albumGroupAlbums, $(this));
                }).text("Show More"));
            }
        });
    }

    $(".galleryPager").each(function (index, albumPager) {
        $(albumPager).siblings(".albumItem").slice($(albumPager).data("pageSize")).hide();
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

function toggleAlbumGroupDisplay(toggleGroupLink) {
    if (!toggleGroupLink.hasClass("selected")) {
        toggleGroupLink.siblings(".selected").removeClass("selected");
        toggleGroupLink.addClass("selected");
        $(".albumGroup").hide();
        $("#albumGroup" + toggleGroupLink.text()).fadeIn("fast");
    }
}

function toggleAlbumsDisplay(albumGroupAlbums, toggleLink) {
    $(albumGroupAlbums).slice(defaultAlbumsPerGroupDisplayed).toggle("fast");

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
    $(albumItems).slice(startIndex, endIndex).fadeIn("fast");
}