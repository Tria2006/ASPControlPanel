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
