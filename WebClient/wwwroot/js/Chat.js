var sendButton = document.getElementById("sendbutton");
var stopButton = document.getElementById("stopbutton");
var newchatButton = document.getElementById("newchatbutton");
var backButton = document.getElementById("backbutton");
var ensureEditButton =document.getElementById("ensureEdit");

var editModal = new bootstrap.Modal(document.getElementById('editModal'), {
    keyboard: false
});
var initDictionary = {};//某个navId对应的tab-pane是否已经获得详细数据

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
backButton.addEventListener("click", function (event) {
    window.location.href = "https://localhost:5002/";
    event.preventDefault();
});
ensureEditButton.addEventListener("click", function (event) {
    //获取当前选择的navlink的navId
    let navId = document.querySelector('[aria-selected="true"]').getAttribute('data-bs-target').substring(1);
    let form = document.forms["ReNameForm"];
    let formdata = new FormData(form);
    formdata.set('navId', navId);
    fetch(`remote/Chat/UpdateNavNameAsync`, {
        method: 'PUT',
        headers: {
            "X-CSRF": "1"
        },
        body: formdata
    })
        .then(response => response.text())
        .then(data => {
            let intData = parseInt(data);
            if (intData == 1) {
                let navlink = document.querySelector(`a[data-bs-target="#${navId}"]`);
                navlink.innerHTML = `<p>${formdata.get("navName")}</p>`;
                editModal.hide();
            }
        })
        .catch(error => console.error('Error:', error));
    event.preventDefault();
});

function sendText() {
    //获取当前显示的tab-pane
    let messageList = document.querySelector('.tab-content').querySelector('.active');

    if (messageList == null) return;//刚进入页面，没有选择tab-pane时，不发送请求

    let form = document.forms["ChatForm"];
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return;
    }

    sendButton.disabled = true;
    stopButton.disabled = false;

    let httpRequest = new XMLHttpRequest();
    let formdata = new FormData(form);

    let gptCardElement = createGPTCardElement();
    let userCardElement = createUserCardElement();
    //let cursorElement = gptCardElement.querySelector(".cursor");//末尾光标，暂时无法对转markdown后<p>使用
    let gptTextElement = gptCardElement.querySelector(".text");//gpt文本区域，修改其innerHTML展示流式对话

    formdata.set('navId', messageList.id);
    formdata.set('model', document.querySelector('.dropdown-toggle').value);

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
            xhr.open('POST', `remote/Chat/CancelOperation/${formdata.get("userId")}`, true);
            xhr.setRequestHeader('X-CSRF', '1');
            xhr.send();
            xhr.onloadend = function () {
                if (xhr.status == 200) console.log(xhr.statusText);
                if (xhr.status == 500) console.log(xhr.statusText);
            };
        }
    };

    httpRequest.open("POST", "remote/Chat/ChatAsStreamAsync", true);
    httpRequest.setRequestHeader('X-CSRF', '1');
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
    let div = document.createElement('div');
    let Navlink = document.createElement('a');
    let delbtn = document.createElement('button');
    let editbtn=document.createElement('button');

    Navlink.classList.add('nav-link');
    Navlink.setAttribute('data-bs-toggle', 'pill');
    Navlink.setAttribute('data-bs-target', `#${guid}`);
    Navlink.setAttribute('type', 'button');
    Navlink.textContent = text;

    delbtn.classList.add("btn", "delbtn");
    delbtn.innerHTML = `<i class="bi bi-trash3-fill"></i>`;
    //配置删除按钮相关事件
    delbtn.addEventListener('click', async function (event) {
        await fetch(`remote/Chat/DeleteNavAsync/${guid}`, {
            method: 'DELETE',
            headers: {
                "X-CSRF": "1"
            }
        }).then(response => response.text())
            .then(data => {
                let intData = parseInt(data);
                if (intData == 1) {
                    let divlink = document.querySelector(`a[data-bs-target="#${guid}"]`).parentElement;
                    let tabpane = document.getElementById(guid);
                    //删除navlink对应的group-title
                    if (divlink.previousElementSibling.className == "group-title" &&
                       (divlink.nextElementSibling == null || divlink.nextElementSibling.className != "nav-class"))
                       divlink.previousElementSibling.remove();

                    divlink.remove();
                    tabpane.remove();
                }
            })
            .catch(error => console.error('Error:', error));

        event.preventDefault();
    });

    editbtn.classList.add("btn", "editbtn");
    editbtn.innerHTML = `<i class="bi bi-pen-fill"></i>`;
    editbtn.addEventListener("click",async function (event) {
        editModal.show();
        event.preventDefault();
    });

    div.classList.add("nav-class");
    div.appendChild(Navlink);
    div.appendChild(delbtn);
    div.appendChild(editbtn);
    return div;
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

        //隐藏上一个navlink的edit与del图标，显示当前navlink的edit与del图标，重新排版内部文本格式适配右侧按钮
        if (event.relatedTarget !== null) {
            event.relatedTarget.parentElement.lastElementChild.style.display = "none";
            event.relatedTarget.parentElement.lastElementChild.previousElementSibling.style.display = "none";
            event.relatedTarget.innerHTML = event.relatedTarget.textContent;
        }
        event.target.parentElement.lastElementChild.style.display = "block";
        event.target.parentElement.lastElementChild.previousElementSibling.style.display = "block";
        event.target.innerHTML = `<p>${event.target.textContent}</p>`;

        scrolldiv.scrollTop = scrolldiv.scrollHeight;
    });
    //显示新的tab-pane
    new bootstrap.Tab(Navlink.firstChild).show();
}
//获取navlink-tabpane之间的唯一标识符
async function getGuid() {
    let guid;
    await fetch(`remote/Chat/CreateNavIdAsync/${document.forms["ChatForm"].userId.value}`, {
        method: 'POST',
        headers: {
            "X-CSRF": "1"
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
    await fetch(`remote/Chat/GetNavAsync?userId=${document.forms["ChatForm"].userId.value}`, {
        method: 'GET',
        headers: {
            "X-CSRF": "1"
        }
    })
        .then(response => response.json())
        .then(data => {
            //data = JSON.parse(data);
            if (data !== null) {
                data.forEach(item => {
                    date = new Date(item.latestAt);
                    diff = now.getTime() - date.getTime();
                    navlink = createNavlinkElement(item.navId, item.navName);

                    //按照日期将navlink插入到对应的group-title中
                    if (diff <= 1 * 24 * 60 * 60 * 1000) {
                        messageHistory.insertBefore(navlink, groupY);
                        array[0] += 1;
                    } else if (diff <= 2 * 24 * 60 * 60 * 1000) {
                        messageHistory.insertBefore(navlink, groupP7);
                        array[1] += 1;
                    } else if (diff <= 7 * 24 * 60 * 60 * 1000) {
                        messageHistory.insertBefore(navlink, groupP30);
                        array[2] += 1;
                    } else if (diff <= 30 * 24 * 60 * 60 * 1000) {
                        messageHistory.insertBefore(navlink, groupL);
                        array[3] += 1;
                    } else {
                        messageHistory.appendChild(navlink);
                        array[4] += 1;       
                    }
                    //设置navId对应的tab-pane尚未初始化
                    initDictionary[`${item.navId}`] = false;

                    //延迟获取详细历史记录，避免页面卡顿
                    document.querySelector(`a[data-bs-target="#${item.navId}"]`).addEventListener('shown.bs.tab', async event => {
                        let messageList = document.getElementById(item.navId);

                        //隐藏上一个navlink的edit与del图标，显示当前navlink的edit与del图标，重新排版内部文本格式适配右侧按钮
                        if (event.relatedTarget !== null) {
                            event.relatedTarget.parentElement.lastElementChild.style.display = "none";
                            event.relatedTarget.parentElement.lastElementChild.previousElementSibling.style.display = "none";
                            event.relatedTarget.innerHTML = event.relatedTarget.textContent;
                        }
                        event.target.parentElement.lastElementChild.style.display = "block";
                        event.target.parentElement.lastElementChild.previousElementSibling.style.display = "block";
                        event.target.innerHTML = `<p>${event.target.textContent}</p>`;

                        if (initDictionary[`${item.navId}`] === false) {
                            await fetch(`remote/Chat/GetHisAsync?navId=${item.navId}`, {
                                method: 'GET',
                                headers: {
                                    "X-CSRF": "1"
                                }
                            })
                                .then(response => response.json())
                                .then(data => {
                                    //data = JSON.parse(data);
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