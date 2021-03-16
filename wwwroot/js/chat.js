let users = [];
let messages = [];

const storagePath = "/chat/user/local/";
const requestArray = ["http://localhost:5001/api/Users", "http://localhost:5001/api/Messages"]

function getAll() {
    Promise.all(requestArray.map((request) => {
        return fetch(request).then((response) => {
            return response.json();
        }).then((data) => {
            return data;
        });
    })).then((values) => {
        initData(values[0], values[1])
    }).catch(console.error.bind(console))
}

getAll();

function initUsers() {
    users.forEach(addUser);
}


function initMessages() {
    messages.forEach(addMessages)
}

function initData(_users, _messages) {
    users = _users;
    messages = _messages;
    initUsers();
    initMessages();
}
/*
const addMessage = (ev) => {
    ev.preventDefault(); // Um das Abschicken des Formulars zu stoppen
    let message = {
        message: document.getElementById('message').value,
        user: localStorage.getItem(storagePath + "id")
    }
    const b = JSON.stringify({
        'user_id': localStorage.getItem(storagePath + 'id'),
        message: document.getElementById('message').value
    });
    console.log(b);
    fetch('http://localhost:5001/api/Messages', {
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        method: 'POST',
        body: b
    }).then(data => console.log(data));

    messages.push(message);
    document.forms[0].reset(); // Ausgefülltes Formular auf default zurücksetzen

    localStorage.setItem('MessageList', JSON.stringify(messages));
}
document.addEventListener('DOMContentLoaded', () => {
    const element = document.getElementById('submit');
    if (element) {
        element.addEventListener('click', addMessage);
    }
});
*/
function startEventListener() {
    /*
    document.addEventListener('DOMContentLoaded', () => {
        document.getElementById('login').addEventListener('click', addUser)
    })
    */
}


function getUsersById(id) {
    for (let i = 0; i < users.length; i++) {
        if (users[i].id === id) {
            return users[i].nickname;
        }
    }


    return localStorage.getItem(storagePath + 'id');
}

function addMessages(message) {

    const box = document.createElement('div');
    box.setAttribute('id', message.id);
    box.setAttribute('class', 'card mb-2');

    const header = document.createElement("div");
    header.setAttribute('class', 'card-header lead d-flex justify-content-between');
    box.appendChild(header);

    const userName = document.createElement("strong");
    userName.appendChild(document.createTextNode(getUsersById(message.user_id)));
    header.appendChild(userName);

    const timeStamp = document.createElement("small");
    timeStamp.appendChild(document.createTextNode(message.timestamp));
    timeStamp.setAttribute('class', 'text-muted');
    header.appendChild(timeStamp);

    const messageText = document.createElement("span");
    messageText.appendChild(document.createTextNode(message.message));
    messageText.setAttribute('class', 'card-body')
    box.appendChild(messageText);

    document.getElementById("messageList").appendChild(box);
}

function setNickname(nickname) {
    console.log("ES FUNKTIONIERT")
    localStorage.setItem(storagePath + "nickname", nickname)
}

function getNickname() {
    return (localStorage.getItem(storagePath + "nickname"));
}

/*function putUser(id, nickname) {
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
}*/

function startWebSocket() {
    const ws = new WebSocket('ws://localhost:5001/ws');

    ws.onerror = function (event) {
        console.error('WebSocket Error', event)
    }
    ws.onmessage = function (event) {
        console.log('I handle the message:');
        handleMessage(event.data);
    };
    ws.onopen = function (event) {
        const element = document.getElementById('pfooter');
        if (element) {
            element.innerHTML = 'Websocket connected!';
        }
    };
    ws.onclose = function (event) {
        const element = document.getElementById('pfooter');
        if (element) {
            element.innerHTML = 'Not connected!';
        }
    };
}


function handleMessage(input) {
    const jsonObject = JSON.parse(input);
    const action = jsonObject.action;
    switch (action) {
        case 'message_added':
            console.log('Message added:');
            console.log(jsonObject.data);
            break;
        case 'user_updated':
            updateUser({
                "id": jsonObject.data.id,
                "status": jsonObject.data.status,
                "avatar": jsonObject.data.avatar,
                "nickname": jsonObject.data.nickname
            });
            break;
        case 'user_deleted':
            console.error("websocket - unbekannte Action")
            break;
    }
}

startWebSocket();


const FILTER_RED = 'invert(11%) sepia(67%) saturate(3947%) hue-rotate(353deg) brightness(94%) contrast(117%)';
const FILTER_GREEN = 'invert(21%) sepia(88%) saturate(3552%) hue-rotate(96deg) brightness(97%) contrast(103%)';
const FILTER_ORANGE = 'invert(56%) sepia(25%) saturate(6340%) hue-rotate(1deg) brightness(103%) contrast(105%)';

function get_filter_for_status(status) {
    switch (status) {
        case 'online':
            return FILTER_GREEN;
        case 'offline':
            return FILTER_RED;
        case 'away':
            return FILTER_ORANGE;
        default:
            return '';
    }
}


function addUser(user) {
    const li = document.createElement("li");
    li.setAttribute("id", user.id);
    if (localStorage.getItem('user-id') === user.id) {
        li.className = 'list-group-item d-flex active';
    } else {
        li.className = 'list-group-item d-flex';
    }

    const avatar = document.createElement("img")
    avatar.setAttribute("src", "images/avatar_icon_" + user.avatar + ".svg");
    avatar.setAttribute('alt', 'Avatar' + user.avatar);
    avatar.setAttribute('style', 'width: 24px; height: 24px; filter: ' + get_filter_for_status(user.status));
    avatar.className = 'mr-1';
    li.appendChild(avatar);

    const nickname = document.createElement("span");
    nickname.appendChild(document.createTextNode(user.nickname));
    li.appendChild(nickname);

    document.getElementById("userList").appendChild(li);
}

/*
function addUser(ev) {
    ev.preventDefault(); //Abschicken des Formulars
    const user = {
        user: document.getElementById('nickname').value
    }
    const b = JSON.stringify({
        nickname: document.getElementById('nickname').value
    })

    fetch('http://localhost:5001/api/Users', {
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        method: 'POST',
        body: b
    }).then(data => console.log(data));

    users.push(user);
    document.forms[0].reset();

    localStorage.setItem('UserList', JSON.stringify(user));
}
*/


