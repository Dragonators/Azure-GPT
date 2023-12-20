// 获取表单元素
var forms = document.querySelectorAll('.needs-validation');
//表单id的索引
var index = 0;
forms.forEach(function (form) {
    // 监听表单的提交事件
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
    var inputs = form.querySelectorAll('input');
    inputs.forEach(function (input) {
        input.addEventListener('blur', blurHandlerClient, false);
    });
    //只有在UpdateForm-表单中才需要对select元素和用户名input进行监听
    if (form.id == 'UpdateForm-' + index.toString())
    {
        //为select元素添加虚焦客户端检查
        var select = document.getElementById('validationTooltipsex-' + index.toString());
        select.addEventListener('blur', blurHandlerClient, false);

        //为用户名的input元素添加服务端检查
        var inputname = document.getElementById('name-' + index.toString());
        //输入内容时实时重名检查，与其余input项的实时格式检查类似
        inputname.addEventListener('input', blurHandlerServer);
        //失去焦点时进行重名检查
        inputname.addEventListener('blur', blurHandlerServer, false);

        //准备下一个表单的id
        index++;
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
    var id = this.parentElement.parentElement.parentElement.querySelector("input[type='hidden']");
    //根据id与用户名，构造请求url
    fetch('/Account/Admin/?handler=validusername', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            username: this.value,
            id: id.value
        })
    })
        //将响应转换为json对象
        .then(response => response.json())
        //处理json对象
        .then(data => {
            //获取bootstrap validation错误信息的div元素
            var div = this.parentElement.querySelector("div.invalid-feedback.order-1");
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