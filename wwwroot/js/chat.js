
const chatConn = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/chat')
    .withAutomaticReconnect()
    .build();

async function startChatConn(){
    try { await chatConn.start(); console.log('Chat connected'); }
    catch(e){ console.error('Chat connect failed', e); setTimeout(startChatConn, 2000); }
}
startChatConn();

chatConn.on('ReceiveMessage', (fromUserId, message, sentAtUtc) => {
    let input = document.querySelector(`#msg-${fromUserId}`) || document.querySelector(`#msg-${fromUserId.replace(/[^a-zA-Z0-9_-]/g,'')}`);
    if(!input){
        const hist = document.querySelector('.msg-history');
        if(hist){ appendMessage(hist, fromUserId, message, false, sentAtUtc); }
        return;
    }
    const card = input.closest('.list-group-item') || input.parentElement;
    const hist = card ? card.querySelector('.msg-history') : null;
    if(hist){ appendMessage(hist, fromUserId, message, false, sentAtUtc); }
});

chatConn.on('UserOnline', (userId, isOnline) => {
    // Expect a status element like span.status-dot on the person's row (best-effort)
    const row = document.querySelector(`[data-user-id="${userId}"]`) || document.getElementById(`row-${userId}`);
    if(!row) return;
    const dot = row.querySelector('.status-dot');
    if(!dot) return;
    dot.classList.toggle('online', !!isOnline);
    dot.classList.toggle('offline', !isOnline);
});

window.sendMessageToUser = async function(toUserId){
    const input = document.querySelector(`#msg-${toUserId}`);
    if(!input) { alert('Message box not found'); return; }
    const text = input.value.trim();
    if(!text) return;
    try{
        await chatConn.invoke('SendMessage', toUserId, text);
        const card = input.closest('.list-group-item') || input.parentElement;
        const hist = card ? card.querySelector('.msg-history') : null;
        if(hist){ appendMessage(hist, 'You', text, true, new Date().toISOString()); }
        input.value = '';
    }catch(e){
        console.error(e);
        alert('Failed to send message');
    }
}

function appendMessage(container, from, text, isSelf, sentAtUtc){
    const time = new Date(sentAtUtc || Date.now()).toLocaleTimeString();
    const item = document.createElement('div');
    item.className = `d-flex ${isSelf ? 'justify-content-end' : 'justify-content-start'} mb-1`;
    const bubble = document.createElement('div');
    bubble.className = `p-2 rounded ${isSelf ? 'bg-primary text-white' : 'bg-light border'}`;
    bubble.innerText = `${isSelf ? '' : from + ': '} ${text}  (${time})`;
    item.appendChild(bubble);
    container.appendChild(item);
    container.scrollTop = container.scrollHeight;
}


