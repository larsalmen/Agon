$(document).ready(function () {

    $("#actuallyStartQuiz").click(function () {
        var val = $('#actuallyStartQuiz').val();
        $.get("/Quiz/DropPin/" + val);
        $('#pinHeadline').hide("slow");
        $('#startQuizInfo').hide("slow");
        $('#actuallyStartQuiz').hide("slow");
        $('#actualQuiz').show("slow");
    });
});