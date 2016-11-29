document.getElementById('addField').addEventListener('click', function (e) {
    e.preventDefault();
    var id = parseInt(document.getElementById('addField').getAttribute('name'));
    var string = '<br /><div id="input-group-' + parseInt(id) + '" class="input-group" style="margin-left:auto;margin-right:auto;min-width:200px;max-width:400px;">';
    string = string + '<span class="input-group-addon">';
    string = string + '<input type="checkbox" aria-label="..."name="item.Checkbox" id="enableDisable' + parseInt(id) + '">';
    string = string + '</span>';
    string = string + '<input type="text" class="form-control" name="item.Text" id="Question-' + parseInt(id) + '">';
    string = string + '<input type="text" class="form-control" name="item.CorrectAnswer" id="Answer-' + parseInt(id) + '">';
    //string = string + "<input name='item.Text' type='text'><input name='item.CorrectAnswer' ";
    //string = string + "id='Answer-" + parseInt(id);
    //string = string + "' type='text'>";
    string = string + '</div>';
    var newInput = string;
    $('div#input-group-' + (id - 1)).after(newInput);
    id = parseInt(1 + id);
    document.getElementById('addField').setAttribute('name', id);

},
    false);


$(document).ready(function () {
    $("#formDiv").click(function (e) {
        var target = e.target;
        if (target !== null && target.classList.contains('removeRow')) {
            $('#input-group-' + target.id).remove()

        }

    })
});