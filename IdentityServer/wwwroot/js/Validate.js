// 获取表单元素
var form = document.querySelector('.needs-validation');
// 获取表单元素中所有的input元素
var inputs = form.querySelectorAll('input');
var select = document.getElementById('validationTooltipsex')




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
// 为每个input元素添加监听事件
select.addEventListener('blur', function (event) {
    // 如果input元素失焦，添加was-validated类
    select.parentElement.classList.add('was-validated');
}, false);
inputs.forEach(function (input) {
    // 监听focusout事件
    input.addEventListener('blur', function (event) {
        // 如果input元素失焦，添加was-validated类
        input.parentElement.classList.add('was-validated');
    }, false);
});