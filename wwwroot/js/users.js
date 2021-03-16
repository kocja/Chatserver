const users_endpoint = 'http://localhost:5001/api/Users';

function mapResponseIfNoError(response) {
    return response.ok ? response.json() : response.json().then(error => Promise.reject(error));
}

function getAllUsers() {
    return fetch(users_endpoint)
        .then(mapResponseIfNoError);
}

function getUserById(userId) {
    return fetch(users_endpoint + '/' + userId)
        .then(mapResponseIfNoError);
}

function createUser(nickName) {
    const body = {
        nickname: nickName,
        status: 'online'
    };
    return fetch(users_endpoint, {
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        method: 'POST',
        body: JSON.stringify(body)
    })
        .then(mapResponseIfNoError)
        .then(user => user.id);
}

function updateUser(userUpdate) {
    return fetch(users_endpoint + '/' + userUpdate.id, {
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            method: 'PUT',
            body: JSON.stringify({
                id: userUpdate.id,
                status: userUpdate.status,
                avatar: userUpdate.avatar,
                nickname: userUpdate.nickname
            })
        }
    ).then(mapResponseIfNoError);
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
