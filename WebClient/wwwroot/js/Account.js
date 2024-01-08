let userClaims = null;
let loginbtn = document.getElementById("login");
let logoutbtn = document.getElementById("logout");
let profilebtn = document.getElementById("profile");
let chatgptbtn = document.getElementById("chatgpt");
let hellotext = document.getElementById("currentuser");
loginbtn.addEventListener("click", login, false);
logoutbtn.addEventListener("click", logout, false);
(async function () {
    var req = new Request("/bff/user", {
        headers: new Headers({
            "X-CSRF": "1",
        }),
    });

    try {
        var resp = await fetch(req);
        if (resp.ok) {
            userClaims = await resp.json();
            loginbtn.parentElement.style.display = "none";
            logoutbtn.parentElement.style.display = "block";
            profilebtn.parentElement.style.display = "block";
            chatgptbtn.parentElement.style.display = "block";
            hellotext.display = "block";
            console.log(userClaims);
            hellotext.innerText = "Hi , " + userClaims.find(
                (claim) => claim.type === "Nick Name"
            ).value;
        } else if (resp.status === 401) {
            logoutbtn.parentElement.style.display = "none";
            profilebtn.parentElement.style.display = "none";
            chatgptbtn.parentElement.style.display = "none";
            hellotext.display = "none";
            loginbtn.parentElement.style.display = "block";
        }
        //After render
        document.body.style.display="block";
    } catch (e) {
        console.log(e);
    }
})();
function logout() {
    if (userClaims) {
        var logoutUrl = userClaims.find(
            (claim) => claim.type === "bff:logout_url"
        ).value;
        window.location = logoutUrl;
    } else {
        window.location = "/bff/logout";
    }
}
function login() {
    window.location = "/bff/login";
}