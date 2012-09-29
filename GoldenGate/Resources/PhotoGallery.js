var defaultAlbumsPerGroupDisplayed = 3;

$(function () {
    $(".albumGroup").slice(1).hide();

    $(".albumHeader > .albumNav:first").addClass("selected");

    $(".albumHeader > .albumNav").click(function () {
        toggleAlbumGroupDisplay($(this));
    });

    $(".albumGroup").each(function (index, albumGroup) {
        var albumGroupAlbums = $(albumGroup).find(".albumGroupContent > .albumContainer");
        if (albumGroupAlbums.length > defaultAlbumsPerGroupDisplayed) {
            albumGroupAlbums.slice(defaultAlbumsPerGroupDisplayed).hide();
            $(albumGroup).children(".albumGroupContent").append($("<div>").addClass("albumContentToggle more").click(function () {
                toggleAlbumsDisplay(albumGroupAlbums, $(this));
            }).text("Show More"));
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