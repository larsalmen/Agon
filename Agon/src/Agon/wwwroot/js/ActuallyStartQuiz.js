$(document).ready(function () {

    $("#actuallyStartQuiz").click(function () {
        var val = $('#actuallyStartQuiz').val();
        $.get("/Quiz/DropPin/" + val);
    });
});