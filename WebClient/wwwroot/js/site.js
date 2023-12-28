// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

fetch('/ChatGPT/?handler=testmessage',{
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        ///提供防伪令牌
        RequestVerificationToken: $('input:hidden[name="__RequestVerificationToken"]').val()
    },
    //body: JSON.stringify({
    //    username: this.value,
    //    id: id.value
    //})
})
    .then(response => response.text())
    .then(data => {
        givenText = data;
    });

}