var sendButton = document.getElementById("sendbutton");
var stopButton = document.getElementById("stopbutton");
var newchatButton = document.getElementById("newchatbutton");
var accessToken = getAccessToken();
var initDictionary = {};
function getAccessToken() {
    let returnVal = "";

    let xhr = new XMLHttpRequest();
    xhr.open("GET", "https://localhost:5002/AccessToken/UserToken", false);
    xhr.send();
    returnVal = xhr.responseText;



    console.log(returnVal);////////


    return returnVal;
}

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

prepareNavlink();

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
        let scrolldiv=messageList.parentElement.parentElement;
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
//创建新的会话UI框架
function createNavlinkElement(guid,text) {
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
async function createNewChatList() {
    let messageHistory = document.getElementById('messagehistory');
    let groupTitle = document.getElementById('Today');

    //获取navlink-tabpane之间的唯一标识符
    let guid = await getGuid();

    //创建 navlink item
    let Navlink = createNavlinkElement(guid,"New Chat");

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
            guid=data;
        })
        .catch(error => {
            console.error(error);
        });
    return 'gpt'+guid;//数字不能作为ID选择器开头
}
//创建并添加navlink对应的tab-pane
function AddTabPane(guid) {
    let chatContainerDiv = document.createElement("div");
    chatContainerDiv.classList.add("chat-container", "tab-pane", "fade");
    chatContainerDiv.id=guid;                                                                                                                                                        
    chatContainerDiv.innerHTML = `
            <div class="chat-header">
                ChatGPT 3.5
            </div>
    `;
    let tabContent = document.querySelector('.tab-content');
    tabContent.insertBefore(chatContainerDiv, tabContent.lastElementChild);
}
//async function PrepareFramework() {
//    prepareNavlink();
//    prepareTabPane();
//}

//prepareNavlink and basic tab-pane
async function prepareNavlink() {
    let messageHistory = document.getElementById('messagehistory');
    let now = new Date();
    let date;
    let diff;
    let navlink;
    let longest = 0;

    messageHistory.style.display="none";

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
                //create and add navlink item
                date = new Date(item.createAt);
                diff = now.getTime() - date.getTime();
                navlink = createNavlinkElement(item.navId, item.navName);

                if (diff > longest) longest = diff;

                if (diff <= 1 * 24 * 60 * 60 * 1000) {
                    messageHistory.insertBefore(navlink, groupY);
                } else if (diff <= 2 * 24 * 60 * 60 * 1000) {
                    messageHistory.insertBefore(navlink, groupP7);
                } else if (diff <= 7 * 24 * 60 * 60 * 1000) {
                    messageHistory.insertBefore(navlink, groupP30);
                } else if (diff <= 30 * 24 * 60 * 60 * 1000) {
                    messageHistory.insertBefore(navlink, groupL);
                } else {
                    messageHistory.appendChild(navlink);
                }

                initDictionary[`${item.navId}`]=false;

                document.querySelector(`a[data-bs-target="#${item.navId}"]`).addEventListener('shown.bs.tab', async event => {
                    if (initDictionary[`${item.navId}`] === false) { 
                    let messageList = document.getElementById(item.navId);
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
                    let scrolldiv = messageList.parentElement.parentElement;
                        scrolldiv.scrollTop = scrolldiv.scrollHeight;
                        initDictionary[`${item.navId}`] = true;
                    }
                });

                AddTabPane(item.navId);

            });
            if (longest <= 30 * 24 * 60 * 60 * 1000) {
                groupL.remove();
                if (longest <= 7 * 24 * 60 * 60 * 1000) {
                    groupP30.remove();
                    if (longest <= 2 * 24 * 60 * 60 * 1000) {
                        groupP7.remove();
                        if (longest <= 1 * 24 * 60 * 60 * 1000) {
                            groupY.remove();
                            if (longest == 0) groupT.remove();
                        }
                    }
                }
                }
            }
        })
        .catch(error => console.error('Error:', error));
    messageHistory.style.display = "block";
}

//prepare specific tab-pane
async function prepareTabPane() {
    //console.log(event.target);
    console.log(1); // 输出data-bs-target属性的值

    //let messageList = document.querySelector('.tab-content').querySelector('.active');
    //let tabPane;
    //let navlink;
    //let longest = 0;

    //tabContent.style.display = "none";

    //await fetch(`https://localhost:7001/Chat/GetHisAsync?navId=${document.forms["ChatForm"].userId.value}`, {
    //    method: 'GET',
    //    headers: {
    //        'Authorization': `Bearer ${accessToken}`
    //    }
    //})
    //    .then(response => response.json())
    //    .then(data => {
    //        data = JSON.parse(data);
    //        data.forEach(item => {
    //            //create and add navlink item
    //            tabPane = document.createElement('div');
    //            tabPane.classList.add('chat-container', 'tab-pane', 'fade');
    //            tabPane.id = item.navId;
    //            tabPane.innerHTML = `
    //                <div class="chat-header">
    //                    ChatGPT 3.5
    //                </div>
    //            `;
    //            tabContent.appendChild(tabPane);
    //        });
    //    })
    //    .catch(error => console.error('Error:', error));
    //tabContent.style.display = "block";
}