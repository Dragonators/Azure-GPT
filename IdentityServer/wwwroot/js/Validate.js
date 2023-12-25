// 获取表单元素
var forms = document.querySelectorAll('.needs-validation');
//表单id的索引
var index = 0;
var index_ = 0;
forms.forEach(function (form) {
    if (form.id == 'UpdateRoleForm-' + index_.toString()) {
        let select = document.getElementById('validationTooltiprole-' + index_.toString());
        let btnadd = form.parentElement.querySelector('button.btn-success');
        let btndel = form.parentElement.querySelector('button.btn-secondary');

        select.addEventListener('blur', blurHandlerClient, false);
        btnadd.addEventListener('click', async function (event) {
            if (!await RoleAddHandlerServer(btnadd)) {
                form.querySelector("input[type='hidden'][name='Button']").value = "Add";
                form.submit();
            }
            else {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
        btndel.addEventListener('click', async function (event) {
            if (!await RoleDelHandlerServer(btndel)) {
                form.querySelector("input[type='hidden'][name='Button']").value = "Remove";
                form.submit();
            }
            else {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
        index_++;
    }
    else {
        form.addEventListener('submit', function (event) {
            // 如果表单无效，阻止提交
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            // 添加was-validated类，以显示验证反馈
            form.classList.add('was-validated');
        }, false);
        //为每个input元素添加虚焦客户端检查
        let inputs = form.querySelectorAll('input');
        inputs.forEach(function (input) {
            input.addEventListener('blur', blurHandlerClient, false);
        });
        //只有在UpdateForm-表单中才需要对select元素和用户名input进行监听
        if (form.id == 'UpdateForm-' + index.toString()) {
            //为select元素添加虚焦客户端检查
            let select = document.getElementById('validationTooltipsex-' + index.toString());
            select.addEventListener('blur', blurHandlerClient, false);

            //为用户名的input元素添加服务端检查
            let inputname = document.getElementById('name-' + index.toString());
            //输入内容时实时重名检查，与其余input项的实时格式检查类似
            inputname.addEventListener('input', blurHandlerServer);
            //失去焦点时进行重名检查
            inputname.addEventListener('blur', blurHandlerServer, false);

            //准备下一个表单的id
            index++;
        }
    }
});
//客户端格式检查
function blurHandlerClient() {
    this.parentElement.classList.add('was-validated');
}
//服务端重名检查
function blurHandlerServer() {
    //如果输入框为空，不进行检查
    if (this.value == "") {
        return;
    }
    //获取当前用户的id
    let id = this.parentElement.parentElement.parentElement.querySelector("input[type='hidden']");
    //根据id与用户名，构造请求url
    fetch('/Account/Admin/?handler=validusername', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            ///提供防伪令牌
            RequestVerificationToken: $('input:hidden[name="__RequestVerificationToken"]').val()
        },
        body: JSON.stringify({
            username: this.value,
            id: id.value
        })
    })
        //将响应转换为json对象
        .then(response => response.json())
        // 处理json对象
        .then(data => {
            //获取bootstrap validation错误信息的div元素
            let div = this.parentElement.querySelector("div.invalid-feedback.order-1");
            //如果data为false，表示用户名已存在，显示错误信息
            if (!data) {
                //更改验证错误提示文本
                div.innerText = "Username already exists"
                //以下文本没有实际意义，仅用于设置验证失败的状态
                this.setCustomValidity("用户名已存在");
            } else {
                //如果data为true，表示非重名错误，恢复默认错误信息，交给bootstrap处理
                div.innerText = "Please provide a correct Username."
                //清除错误状态
                this.setCustomValidity("");
            }
        })
        .catch(error => {
            console.error(error);
        });
}
//服务端添加角色检查
async function RoleAddHandlerServer(btn) {
    let id = btn.parentElement.parentElement.querySelector("input[type='hidden'][name='id_']");//当前用户的id
    let role = btn.parentElement.parentElement.querySelector('select');//当前选择的角色
    let addable;
    //如果选择框为空，不进行检查
    if (role.value == "") {
        return;
    }
    //向服务端发送请求，检查是否已经属于该角色
    await fetch('/Account/Admin/?handler=validrole', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            //提供防伪令牌
            RequestVerificationToken: $('input:hidden[name="__RequestVerificationToken"]').val()
        },
        body: JSON.stringify({
            role: role.value,
            id: id.value
        })
    })
        .then(response => response.json())
        .then(data => {
            //获取bootstrap validation错误信息的div元素
            var div = role.parentElement.querySelector("div.invalid-feedback");
            //如果data为false，表示已经属于该角色，显示错误信息
            if (data) {
                //更改验证错误提示文本
                div.innerText = " User already in this Role"
                //以下文本没有实际意义，仅用于设置验证失败的状态
                role.setCustomValidity("已属于该角色");
                addable = true;
            } else {
                //如果data为true，表示非重复身份错误，恢复默认错误信息，交给bootstrap处理
                div.innerText = "Please select a Role."
                //清除错误状态
                role.setCustomValidity("");
                addable = false;
            }
        })
        .catch(error => {
            console.error(error);
        });
    return addable;
}
//服务端删除角色检查
async function RoleDelHandlerServer(btn) {
    let id = btn.parentElement.parentElement.querySelector("input[type='hidden'][name='id_']");//当前用户的id
    let role = btn.parentElement.parentElement.querySelector('select');//当前选择的角色
    let delable;
    //如果选择框为空，不进行检查
    if (role.value == "") {
        return;
    }
    //向服务端发送请求，检查是否已经属于该角色
    await fetch('/Account/Admin/?handler=validrole', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            //提供防伪令牌
            RequestVerificationToken: $('input:hidden[name="__RequestVerificationToken"]').val()
        },
        body: JSON.stringify({
            role: role.value,
            id: id.value
        })
    })
        .then(response => response.json())
        .then(data => {
            //获取bootstrap validation错误信息的div元素
            var div = role.parentElement.querySelector("div.invalid-feedback");
            //如果data为true，表示不属于该角色无法移除，显示错误信息
            if (!data) {
                //更改验证错误提示文本
                div.innerText = " User haven't this Role"
                //以下文本没有实际意义，仅用于设置验证失败的状态
                role.setCustomValidity("不属于该角色");
                delable = true;
            } else {
                //如果data为true，表示非无身份错误，恢复默认错误信息，交给bootstrap处理
                div.innerText = "Please select a Role."
                //清除错误状态
                role.setCustomValidity("");
                delable = false;
            }
        })
        .catch(error => {
            console.error(error);
        });
    return delable;
}