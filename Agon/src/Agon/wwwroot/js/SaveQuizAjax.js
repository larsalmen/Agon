$(document).ready(function () {

    $("#saveQuiz").click(function () {
        $.get("/Quiz/SaveQuiz");
    });
});