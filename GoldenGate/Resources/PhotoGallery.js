$(function () {
    $(".albumGroup").slice(1).hide();
});

$(function () {
    $(".albumGroup > .albumHeader").filter(":first")
        .append($("<span>", { id: "showMore" }).click(showMoreAlbums).append("| See More"))
        .append($("<span>", { id: "showLess"}).click(showLessAlbums).append("| See Less").hide());
});

function showMoreAlbums() {
    $("#showMore").hide();
    $("#showLess").show();
    $(".albumGroup").slice(1).show(1000);
}

function showLessAlbums() {
    $("#showMore").show();
    $("#showLess").hide();
    $(".albumGroup").slice(1).hide(1000);
}

$(function () {
    $(".albumItem").click(function () {
        alert("Don't click me yet");
    });
});