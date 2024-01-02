var chatBox = document.querySelector('.col-md-10.chat');
var textBox = document.getElementById('textarea');
var baseHeight = textBox.offsetHeight;
function resize() {
    chatBox.style.setProperty('height', window.innerHeight - 78 - textBox.offsetHeight + baseHeight);
};
new ResizeObserver(resize).observe(textBox);
window.addEventListener('resize', resize);