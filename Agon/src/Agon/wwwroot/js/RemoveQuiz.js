$(document).ready(function () {

    $(".RemoveQuiz").click(function (e) {
        e.preventDefault();
        var target = e.target;
        $.ajax({
            url: '/Quiz/RemoveQuiz',
            data: {
                id: target.name
            },
            success: function (response) {
                $("#" + target.name).remove();

                //var elements = document.getElementsByClassName("RemoveQuiz");
                //for (var i = 0; i < elements.length; i++) {
                //    elements[i].setAttribute("name", i);

                //var elementsDiv = document.getElementsByClassName("RemoveDivQuiz");
                //for (var j = 0; j < elementsDiv.length; j++) {
                //    elementsDiv[j].setAttribute("id", j);
            }
        }
        )
    })
})