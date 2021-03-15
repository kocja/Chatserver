const users_endpoint = 'http://localhost:5001/api/Users';

let users = [];

function initUsers() {
    fetch(users_endpoint)
        .then(response => response.json())
        .then(users => users.forEach(addUsers))
        .catch(console.error.bind(console))
}

/* !! DEBUG !!
function deleteUsers() {
    fetch(users_endpoint)
        .then(response => response.json())
        .then(users => users.forEach(user => fetch(users_endpoint + '/' + user.id, {
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            method: 'DELETE'
        })));
}
deleteUsers();
*/

function getUser(userId) {
    return fetch(users_endpoint + '/' + userId)
        .then(response => response.json());
}

function createUser(nickName) {
    const body = {
        nickname: nickName
    };

    return fetch(users_endpoint, {
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        method: 'POST',
        body: JSON.stringify(body)
    })
        .then(response => response.json())
        .then(user => user.id);
}

function addUsers(user) {
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


