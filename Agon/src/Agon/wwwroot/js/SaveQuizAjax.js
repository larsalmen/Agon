$(document).ready(function () {

    $("#saveQuiz").click(function () {
        //$.get("/Quiz/SaveQuiz");
        var name = $("#quizName").val();
        $.ajax({
            url: '/Quiz/SaveQuiz',
            data: {
                quizName: name
            }
        })
    });
});
