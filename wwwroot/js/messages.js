const messages_endpoint ='http://localhost:5001/api/Messages';

function createMessage(newMessage){
    const body = {
        message: newMessage
    }
    return fetch(messages_endpoint, {
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        method: 'POST',
        body: JSON.stringify(body)
    })
        .then(response => response.ok ? response.json() : response.json().then(Promise.reject))
        .then(message => message.id)
}
