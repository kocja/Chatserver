let users = [];
let messages = [];

const requestArray = ["http://localhost:5001/api/Users", "http://localhost:5001/api/Messages"]

function getAll() {
    Promise.all(requestArray.map((request) => {
        return fetch(request).then((response) => {
            return response.json();
        }).then((data) => {
            return data;
        })
    }));
}

function initData(users, messages) {
    users = this.users;
    messages = this.messages;
    initUsers();
    initMessages();
}

document.getElementById("submit").addEventListener("click", postMessage)

const addMessage = (ev) => {
    ev.preventDefault(); // Um das Abschicken des Formulars zu stoppen
    let message = {
        id: Date.now(),
        message: document.getElementById('message').value,
        date: Date.now(),
        user: Date.now()
    }
    messages.push(message);
    document.forms[0].reset(); // Ausgefülltes Formular auf default zurücksetzen

    localStorage.setItem('MessageList', JSON.stringify(messages));
}
document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('submit').addEventListener('click', addMessage);
})

function initUsers() {s
    for (let i = 0; i < users.length; i++) {
        addUser(users[i])
    }
}

function initMessages() {
    for (let i = 0; i < messages.length; i++) {
        addMessages(messages[i])
    }
}

function getUsersById(id) {
    for (let i = 0; i < users.length; i++) {
        if (users[i].id === id) {
            return users[i].nickname;
        }
    }

    const storagePath =  "/chat/user/local/";
    return localStorage.getItem(storagePath + "id");
}

function addMessages(message) {

    let user_id = "";

    let section = document.createElement("section");
    section.setAttribute("id", message.id);

    let userName = document.createElement("span");
    userName.appendChild(document.createTextNode(getUsersById(message.user_id)));
    console.log(message.user_id)
    section.appendChild(userName);

    let messageText = document.createElement("span");
    messageText.appendChild(document.createTextNode(message.message));
    section.appendChild(messageText);
    document.getElementById("messageList").appendChild(section);
}

function addUser(user) {

    let user_id = "";

    let li = document.createElement("li");
    li.setAttribute("id", user.id);

    let status = document.createElement();
    li.appendChild(status);

    let avatar = document.createElement("img")

    avatar.setAttribute("src", "img/avatar_icon_" + user.avatar + ".svg");
    avatar.setAttribute("height", "20px");
    avatar.setAttribute("width", "20px");
    li.appendChild(avatar);

    let nickname = document.createElement("span");

    nickname.appendChild(document.createTextNode(user.nickname));
    li.appendChild(nickname);
    document.getElementById("userList").appendChild(li);
}

function setNickname(nickname) {
    localStorage.setItem("nickname", nickname)
}

function getNickname() {
    return (localStorage.getItem("nickname"));
}

function newInput() {
    let input = document.createElement("input");
    input.setAttribute("type", "text");
    input.setAttribute("value", getNickname());
    document.getElementById("nicknameInput").appendChild(input)
}

function putUser(id, nickname) {
    fetch('http://localhost:5001/api/Users' + id, {
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        method: 'PUT',
        body: JSON.stringify({"nickname": nickname, "status": "online"})
    }).then(function (response) {
        if (response.status === 200) {
            return response.json();
        } else {
            console.log("nicht 200")
        }
    }).then(function (json) {
        setLocalUser({"id": json["id"], "nickname": nickname});
        loginSuccesfully();
    }).catch(err => console.log("exc", err));
}

function startWebSocket() {
    const ws = new WebSocket('http://localhost:5001/api/Users')

    ws.onerror = function (event) {
        console.error('WebSocket Error', event)
    }
    ws.onmessage = function (event) {
        handleMessage(event.data);
    }
    ws.onopen = function (event) {
        // Sende eine Nachricht an den Server, der Server wird diese danach einfach an alle verbundenen Clients zurückschicken (Echo).
        //ws.send('Hello World')
    }
    ws.onclose = function (event) {
        document.getElementById("pfooter").innerHTML = "Not connected!";
    }
    document.getElementById("pfooter").innerHTML = "Websocket connected!";
}


function updateUser(id) {
    id = getUsersById(id);
}

function handleMessage(input) {
    const jsonObject = JSON.parse(input);
    const action = jsonObject.action;
    if (action === "user_updated") {
        updateUser({
            "id": jsonObject.data.id,
            "status": jsonObject.data.status,
            "avatar": jsonObject.data.avatar,
            "nickname": jsonObject.data.nickname
        });
    } else if (action === "user_deleted") {
        console.log("user_deleted noch nicht implementiert")
    } else {
        console.error("websocket - unbekannte Action")
    }
}

startWebSocket();



