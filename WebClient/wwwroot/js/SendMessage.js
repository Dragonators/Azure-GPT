var sendButton = document.getElementById("sendbutton");
var stopButton = document.getElementById("stopbutton");

//停止响应信号
var stopRequest = false;
sendButton.addEventListener("click", function (event) {
    sendText();
    event.preventDefault();
});
stopButton.addEventListener("click", function (event) {
    stopRequest = true;
    event.preventDefault();
});
function sendText() {
    let form = document.forms["ChatForm"];
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return;
    }

    sendButton.disabled = true;
    stopButton.disabled = false;

    let httpRequest = new XMLHttpRequest();
    let formdata = new FormData(form);
    let messageList = document.getElementById("MessageList");
    let gptCardElement = createGPTCardElement();
    let userCardElement = createUserCardElement();
    let cursorElement = gptCardElement.querySelector(".cursor");//末尾光标
    let gptTextElement = gptCardElement.querySelector(".card-text");//gpt文本

    httpRequest.onloadstart = function (e) {
        cursorElement.style.display = 'inline-block';
        userCardElement.querySelector(".card-text").textContent = formdata.get("message");
        messageList.appendChild(userCardElement);
        messageList.appendChild(gptCardElement);
        messageList.appendChild(document.createElement("br"));
    }
    httpRequest.onprogress = function (e) {
        //处理响应数据
        gptTextElement.textContent = `${e.target.responseText}`;
    };
    httpRequest.onloadend=function(e) {
        sendButton.disabled = false;
        stopButton.disabled = true;
        cursorElement.style.display = 'none';
    };
    //取消时使用abort，会触发onloadend内部逻辑
    httpRequest.onreadystatechange = function () {
        if (stopRequest) {
             this.abort();//用httpRequest会出问题，和let有关系吗
            stopRequest = false;
            let xhr = new XMLHttpRequest();
            xhr.open('POST', `https://localhost:7001/Chat/CancelOperation/${formdata.get("userId")}`, true);
            xhr.send();
            xhr.onloadend = function () {
                if (xhr.status == 200) console.log(xhr.statusText);
                if (xhr.status == 500) console.log(xhr.statusText);
            };
        }
    };
    httpRequest.open("POST", "https://localhost:7001/Chat/ChatAsStreamAsync", true);
    httpRequest.responseType = "text";
    httpRequest.send(formdata);
}
function createGPTCardElement() {
    let cardDiv = document.createElement("div");
    cardDiv.classList.add("card","chat");
    cardDiv.style.width = "18rem";
    cardDiv.innerHTML = `
        <div class="card-body">
            <h5 class="card-title">ChatGPT</h5>
            <span class="card-text"></span><span class="cursor"></span>
        </div>
    `;
    return cardDiv;
}
function createUserCardElement() {
    let cardDiv = document.createElement("div");
    cardDiv.classList.add("card","chat");
    cardDiv.style.width = "18rem";
    cardDiv.innerHTML = `
        <div class="card-body">
            <h5 class="card-title">User</h5>
            <p class="card-text"></p>
        </div>
    `;
    return cardDiv;
}