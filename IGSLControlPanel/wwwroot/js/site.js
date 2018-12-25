function SelectSingleItem(parentJquery, div, color1, color2) {
    var elements = parentJquery.children();
    $.each(elements, function () {
        if (this !== div) {
            this.style.backgroundColor = color1;
        }
    });
    if (div.style.backgroundColor === color1 || div.style.backgroundColor === "") {
        div.style.backgroundColor = color2;
    } else {
        div.style.backgroundColor = color1;
    }
}

function GoBack() {
    javascript: history.go(-1);
}

function hideSelectedFolders(div) {
    var childrens = div.children();
    for (var i = 0; i < childrens.length; i++) {
        $("#" + childrens[i].id).show();
    };

    $.get("/Tariffs/GetSelectedFoldersIds", function (data) {
        data.forEach(function (guid) {
            var item = $("#" + guid);
            if (item) {
                item.hide();
            }
        });
    });
}