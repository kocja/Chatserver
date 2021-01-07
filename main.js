let users = [];
let messages = [];

const addUser = (ev)=> {
    ev.preventDefault(); // Um das Abschicken des Formulars zu stoppen
    let user = {
        id: Date.now(),
        nickname: document.getElementById('nickname').value
    }
    users.push(user);
    document.forms[0].reset(); // Ausgefülltes Formular auf default zurücksetzen

    localStorage.setItem('UserList', JSON.stringify(users));
}
document.addEventListener('DOMContentLoaded', ()=> {
    document.getElementById('login').addEventListener('click', addUser);
})

function validateForm() {
    const x = document.forms["myForm"]["fname"].value;
    if (x === "") {
        alert("Name must be filled out");
        return false;
    }
}

const requestsArray = ["http://localhost:5001/api/Users", "http://localhost:5001/api/Messages"];

Promise.all(requestsArray.map((request) => {
    return fetch(request).then((response) => {
        return response.json();
    }).then((data) => {
        return data;
    });
})).then((values) => {
    initData(values[0], values[1])
}).catch(console.error.bind(console));

function initData(u, m) {
    users = u;
    messages = m;
    initUsers();
}

function initUsers() {
    for (let i = 0; i < users.length; i++) {
        addUser(users[i]);
    }
}

/*function addUser(user) {
    let li = document.createElement("li");
    li.setAttribute("id", user.id);

    let status = document.createElement();
    li.appendChild(status);

    let avatar = document.createElement("img");
    avatar.setAttribute("src", "images/avatar_icon_" + user.avatar + ".svg");
    avatar.setAttribute("height", "20px");
    avatar.setAttribute("width", "20px");
    li.appendChild(avatar);

    let nickname = document.createElement("span");
    nickname.appendChild(document.createTextNode(user.nickname));
    li.appendChild(nickname);

    document.getElementById("userList").appendChild(li);
}*/


src="https://code.jquery.com/jquery-3.5.1.slim.min.js"
integrity="sha384-DfXdz2htPH0lsSSs5nCTpuj/zy4C+OGpamoFVy38MVBnE+IbbVYUew+OrCXaRkfj"
crossorigin="anonymous"
src="https://cdn.jsdelivr.net/npm/bootstrap@4.5.3/dist/js/bootstrap.bundle.min.js"
integrity="sha384-ho+j7jyWK8fNQe+A12Hb8AhRq26LrZ/JpcUGGOn+Y7RsweNrtN/tE3MoK7ZeZDyx"
crossorigin="anonymous"
