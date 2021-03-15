const users_endpoint = 'http://localhost:5001/api/Users';

function getUser(userId) {
    return fetch(users_endpoint + '/' + userId)
        .then(response => response.ok ? response.json() : response.json().then(str => Promise.reject(str)));
}

function updateUser(userId, userName) {
    return fetch(users_endpoint + '/' + userId, {
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            method: 'PUT',
            body: JSON.stringify({
                nickname: userName
            })
        }
    ).then(response => response.ok ? response.json() : response.json().then(Promise.reject));
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
        .then(response => response.ok ? response.json() : response.json().then(Promise.reject))
        .then(user => user.id);
}

// ==================================================
// Debug only
// ==================================================
// Deletes all existing users
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
