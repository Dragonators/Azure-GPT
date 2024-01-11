(async function () {
    await fetch(`remote/Chat/GetChatModel`, {
        method: 'GET',
        headers: {
            "X-CSRF": "1"
        }
    }).then(response => response.json())
        .then(data => {
            let modelList = document.querySelector('.dropdown-menu');
           // data = JSON.parse(data);
            data.forEach(model => {
                let li = document.createElement('li');
                li.innerHTML = `<a class="dropdown-item">${model}</a>`;
                modelList.appendChild(li);
            });
        })
        .then(() => {
            let modelbtn = document.querySelector('.dropdown-toggle');
            let items = document.querySelectorAll('.dropdown-item');
            items.forEach(item => {
                item.addEventListener("click", function () {
                    modelbtn.textContent = item.textContent;
                    modelbtn.value = item.textContent;
                    console.log(`innerHTML: ${modelbtn.textContent}, value: ${modelbtn.value}`);
                });
            });
        })
        .catch(error => console.error('Error:', error));
})();
