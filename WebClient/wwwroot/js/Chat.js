﻿var sendButton = document.getElementById("sendbutton");
var stopButton = document.getElementById("stopbutton");
var newchatButton = document.getElementById("newchatbutton");
var accessToken = getAccessToken();
var initDictionary = {};

prepareNavlink();

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
newchatButton.addEventListener("click", function (event) {
    createNewChatList();
    event.preventDefault();
});

function getAccessToken() {
    let returnVal = "";

    let xhr = new XMLHttpRequest();
    xhr.open("GET", "https://localhost:5002/AccessToken/UserToken", false);
    xhr.send();
    returnVal = xhr.responseText;

    return returnVal;
}
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

    //获取当前显示的tab-pane
    let messageList = document.querySelector('.tab-content').querySelector('.active');

    let gptCardElement = createGPTCardElement();
    let userCardElement = createUserCardElement();
    //let cursorElement = gptCardElement.querySelector(".cursor");//末尾光标，暂时无法对转markdown后<p>使用
    let gptTextElement = gptCardElement.querySelector(".text");//gpt文本区域，修改其innerHTML展示流式对话

    formdata.set('navId', messageList.id);

    httpRequest.onloadstart = function (e) {
        //cursorElement.style.display = 'inline-block';
        userCardElement.querySelector(".text").innerHTML = marked.parse(formdata.get("message"));
        messageList.appendChild(userCardElement);
        messageList.appendChild(gptCardElement);
    }
    httpRequest.onprogress = function (e) {
        //处理响应数据
        let markdown = e.target.responseText;
        let html = marked.parse(markdown);
        let scrolldiv = messageList.parentElement.parentElement;
        gptTextElement.innerHTML = html;
        scrolldiv.scrollTop = scrolldiv.scrollHeight;

    };
    httpRequest.onloadend = function (e) {
        sendButton.disabled = false;
        stopButton.disabled = true;
        //cursorElement.style.display = 'none';
    };
    httpRequest.onreadystatechange = function () {
        if (stopRequest) {
            //取消时使用abort，会触发onloadend内部逻辑
            this.abort();
            stopRequest = false;
            let xhr = new XMLHttpRequest();
            xhr.open('POST', `https://localhost:7001/Chat/CancelOperation/${formdata.get("userId")}`, true);
            xhr.setRequestHeader('Authorization', `Bearer ${accessToken}`);
            xhr.send();
            xhr.onloadend = function () {
                if (xhr.status == 200) console.log(xhr.statusText);
                if (xhr.status == 500) console.log(xhr.statusText);
            };
        }
    };

    httpRequest.open("POST", "https://localhost:7001/Chat/ChatAsStreamAsync", true);
    httpRequest.setRequestHeader('Authorization', `Bearer ${accessToken}`);
    httpRequest.responseType = "text";
    httpRequest.send(formdata);
}
function createGPTCardElement() {
    let cardDiv = document.createElement("div");
    cardDiv.classList.add("chat-message");
    cardDiv.innerHTML = `
            <div class="sender">ChatGPT</div>
            <div class="text"></div>
    `;
    return cardDiv;
}
function createUserCardElement() {
    let cardDiv = document.createElement("div");
    cardDiv.classList.add("chat-message");
    cardDiv.innerHTML = `
            <div class="sender">You</div>
            <div class="text"></div>
    `;
    return cardDiv;
}
function createNavlinkElement(guid, text) {
    let Navlink = document.createElement('a');
    Navlink.classList.add('nav-link');
    Navlink.setAttribute('data-bs-toggle', 'pill');
    Navlink.setAttribute('data-bs-target', `#${guid}`);
    Navlink.setAttribute('type', 'button');
    Navlink.textContent = text;
    return Navlink;
}
function createGroupTitleElement(text) {
    groupTitle = document.createElement("div");
    groupTitle.classList.add("group-title");
    groupTitle.textContent = text;
    groupTitle.id = text.replace(/\s*/g, "");
    return groupTitle;
}
//创建新的会话框架
async function createNewChatList() {
    let messageHistory = document.getElementById('messagehistory');
    let groupTitle = document.getElementById('Today');

    //获取navlink-tabpane之间的唯一标识符
    let guid = await getGuid();

    //创建 navlink item
    let Navlink = createNavlinkElement(guid, "New Chat");

    //添加 navlink item
    if (groupTitle == null || groupTitle.textContent !== "Today") {
        groupTitle = createGroupTitleElement("Today");
        messageHistory.insertBefore(Navlink, messageHistory.firstChild.nextSibling);
        messageHistory.insertBefore(groupTitle, messageHistory.firstChild.nextSibling);
    }
    else {
        //firstChild was #text
        messageHistory.insertBefore(Navlink, groupTitle.nextSibling);
    }
    //创建并添加navlink对应的tab-pane
    AddTabPane(guid);

    Navlink.addEventListener('shown.bs.tab', event => {
        let messageList = document.getElementById(guid);
        let scrolldiv = messageList.parentElement.parentElement;
        scrolldiv.scrollTop = scrolldiv.scrollHeight;
    });
    //显示新的tab-pane
    new bootstrap.Tab(Navlink).show();
}
//获取navlink-tabpane之间的唯一标识符
async function getGuid() {
    let guid;
    await fetch(`https://localhost:7001/Chat/CreateNavId/${document.forms["ChatForm"].userId.value}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${accessToken}`
        }
    })
        .then(response => response.text())//用text()而不是json()，因为返回的是一个字符串
        .then(data => {
            //console.log(data);
            guid = data;
        })
        .catch(error => {
            console.error(error);
        });
    return 'gpt' + guid;//数字不能作为ID选择器开头
}
//创建并添加navlink对应的tab-pane
function AddTabPane(guid) {
    let chatContainerDiv = document.createElement("div");
    chatContainerDiv.classList.add("chat-container", "tab-pane", "fade");
    chatContainerDiv.id = guid;
    chatContainerDiv.innerHTML = `
            <div class="chat-header">
                ChatGPT 3.5
            </div>
    `;
    let tabContent = document.querySelector('.tab-content');
    tabContent.insertBefore(chatContainerDiv, tabContent.lastElementChild);
}
/**
* 初始化navlink表与基础tab - pane结构
*/
async function prepareNavlink() {
    let messageHistory = document.getElementById('messagehistory');
    let now = new Date();
    let date;
    let diff;
    let navlink;
    let array = [0, 0, 0, 0, 0];;

    messageHistory.style.display = "none";

    messageHistory.appendChild(createGroupTitleElement("Today"));
    messageHistory.appendChild(createGroupTitleElement("Yesterday"));
    messageHistory.appendChild(createGroupTitleElement("Previous 7 Days"));
    messageHistory.appendChild(createGroupTitleElement("Previous 30 Days"));
    messageHistory.appendChild(createGroupTitleElement("Long ago"));

    let groupT = document.getElementById("Today");
    let groupY = document.getElementById("Yesterday");
    let groupP7 = document.getElementById("Previous7Days");
    let groupP30 = document.getElementById("Previous30Days");
    let groupL = document.getElementById("Longago");

    //获取navlink list
    await fetch(`https://localhost:7001/Chat/GetNavAsync?userId=${document.forms["ChatForm"].userId.value}`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${accessToken}`
        }
    })
        .then(response => response.json())
        .then(data => {
            data = JSON.parse(data);
            if (data !== null) {
                data.forEach(item => {
                    date = new Date(item.createAt);
                    diff = now.getTime() - date.getTime();
                    navlink = createNavlinkElement(item.navId, item.navName);

                    //按照日期将navlink插入到对应的group-title中
                    if (diff <= 1 * 24 * 60 * 60 * 1000) {
                        messageHistory.insertBefore(navlink, groupY);
                        array[0] = 1;
                    } else if (diff <= 2 * 24 * 60 * 60 * 1000) {
                        messageHistory.insertBefore(navlink, groupP7);
                        array[1] = 1;
                    } else if (diff <= 7 * 24 * 60 * 60 * 1000) {
                        messageHistory.insertBefore(navlink, groupP30);
                        array[2] = 1;
                    } else if (diff <= 30 * 24 * 60 * 60 * 1000) {
                        messageHistory.insertBefore(navlink, groupL);
                        array[3] = 1;
                    } else {
                        messageHistory.appendChild(navlink);
                        array[4] = 1;       
                    }
                    //设置navId对应的tab-pane尚未初始化
                    initDictionary[`${item.navId}`] = false;

                    //延迟获取详细历史记录，避免页面卡顿
                    document.querySelector(`a[data-bs-target="#${item.navId}"]`).addEventListener('shown.bs.tab', async event => {
                        let messageList = document.getElementById(item.navId);
                        if (initDictionary[`${item.navId}`] === false) {
                            await fetch(`https://localhost:7001/Chat/GetHisAsync?navId=${item.navId}`, {
                                method: 'GET',
                                headers: {
                                    'Authorization': `Bearer ${accessToken}`
                                }
                            })
                                .then(response => response.json())
                                .then(data => {
                                    data = JSON.parse(data);
                                    if (data !== null) {


                                        data.forEach(item => {
                                            let markdown = item.message;
                                            let html = marked.parse(markdown);
                                            if (item.role == "user") {
                                                let userCardElement = createUserCardElement();
                                                userCardElement.querySelector(".text").innerHTML = html;
                                                messageList.appendChild(userCardElement);
                                            }
                                            else if (item.role == "assistant") {
                                                let gptCardElement = createGPTCardElement();
                                                gptCardElement.querySelector(".text").innerHTML = html;
                                                messageList.appendChild(gptCardElement);
                                            }
                                        });
                                    }
                                })
                                .catch(error => console.error('Error:', error));
                            initDictionary[`${item.navId}`] = true;
                        }
                        let scrolldiv = messageList.parentElement.parentElement;
                        scrolldiv.scrollTop = scrolldiv.scrollHeight;
                    });
                    //create and add tab-pane
                    AddTabPane(item.navId);

                });
                //只保留合法的group-title
                if (array[0] == 0) groupT.remove();
                if (array[1] == 0) groupY.remove();
                if (array[2] == 0) groupP7.remove();
                if (array[3] == 0) groupP30.remove();
                if (array[4] == 0) groupL.remove();
            }
        })
        .catch(error => console.error('Error:', error));
    messageHistory.style.display = "block";
}