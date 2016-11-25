document.getElementById('addField').addEventListener('click', function (e) {
    e.preventDefault();
    var id = parseInt(document.getElementById('addField').getAttribute('name'))
    var string1 = "<br/><input name='item.Text' type='text'><input name='item.CorrectAnswer' "; 
    var string2 = "id='Answer-"+ parseInt(id);
    var string3 = "' type='text'>";
    var newInput = string1 + string2 + string3;
    $('input#Answer-' + (id - 1)).after(newInput);
    id = parseInt(1+id);
    document.getElementById('addField').setAttribute('name',id)


},
    false);


