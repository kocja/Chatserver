// Deletes all existing users
function deleteAllUsers() {
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

// Deletes all existing messages
function deleteAllMessages() {
    fetch(messages_endpoint)
        .then(response => response.json())
        .then(messages => messages.forEach(message => fetch(messages_endpoint + '/' + message.id, {
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            method: 'DELETE'
        })))
}
