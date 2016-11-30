
$(document).ready(function () {

    $(".RemoveSong").click(function (e) {
        e.preventDefault();
        var target = e.target;
        $.ajax({
            url: '/Quiz/RemoveSongFromQuiz',
            data: {
                index: target.name
            },
            success: function (response) {
                $("#" + target.name).remove();

                var elements = document.getElementsByClassName("RemoveSong");
                for (var i = 0; i < elements.length; i++) {
                    elements[i].setAttribute("name", i);
                }
                var elementsDiv = document.getElementsByClassName("RemoveDiv");
                for (var j = 0; j < elementsDiv.length; j++) {
                    elementsDiv[j].setAttribute("id", j);
                }
            }
        })
    })
})