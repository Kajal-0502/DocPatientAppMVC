const videoConn = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/videocall')
    .withAutomaticReconnect()
    .build();

let pc = null;
let localStream = null;
let remoteUserId = null;
let callStartTime = null;
let callTimerInterval = null;
window._incomingOffer = null;

const ringtone = new Audio('/sounds/ringtone.mp3');
ringtone.loop = true;

// Start connection
videoConn.start().then(() => console.log("SignalR connected")).catch(e => console.error(e));

// Presence updates
videoConn.on('UserOnline', userId => updateStatusDot(userId, true));
videoConn.on('UserOffline', userId => updateStatusDot(userId, false));

// Messaging
videoConn.on('ReceiveMessage', (fromUserId, message) => {
    const row = document.querySelector(`[data-userid="${fromUserId}"]`);
    const time = new Date().toLocaleTimeString();
    const html = `<div class="small my-1"><strong>${fromUserId}</strong> <span class="text-muted" style="font-size:0.8em">(${time})</span><div>${escapeHtml(message)}</div></div>`;
    if (row?.querySelector('.msg-history')) row.querySelector('.msg-history').insertAdjacentHTML('afterbegin', html);
    else alert(`${fromUserId}: ${message}`);
});


// --- Messaging ---

function appendMessage(userId, sender, message) {
    const row = document.querySelector(`[data-userid="${userId}"]`);
    if (!row) return;

    const box = row.querySelector(".msg-history");
    if (!box) return;

    const time = new Date().toLocaleTimeString();
    const html = `
        <div class="small my-1 ${sender === "You" ? "text-primary" : ""}">
            <strong>${sender}</strong>
            <span class="text-muted" style="font-size:0.8em">(${time})</span>
            <div>${escapeHtml(message)}</div>
        </div>`;
    box.insertAdjacentHTML('beforeend', html);
    box.scrollTop = box.scrollHeight;
}

// ? Jab bhi message hub se aaye
videoConn.on("ReceiveMessage", (fromUserId, message) => {
    appendMessage(fromUserId, "Them", message);
});

// ? Jab doctor ya patient khud send kare
function sendMessageToUser(targetUserId) {
    const input = document.getElementById('msg-' + targetUserId);
    if (!input || !input.value.trim()) return;

    const text = input.value.trim();
    input.value = '';

    videoConn.invoke('SendMessage', targetUserId, text).catch(e => console.error(e));
    appendMessage(targetUserId, "You", text);
}


videoConn.on('UserNotOnline', targetUserId => alert("User not online: " + targetUserId));

// WebRTC signaling
videoConn.on('ReceiveOffer', async (fromUser, offer) => {
    remoteUserId = fromUser;
    window._incomingOffer = offer;
    showIncomingModal(fromUser);
});

videoConn.on('ReceiveAnswer', async (fromUser, answer) => { if (pc) await pc.setRemoteDescription(answer); });
videoConn.on('ReceiveIce', async (fromUser, candidate) => { if (pc) await pc.addIceCandidate(candidate).catch(() => { }); });

// --- Helpers ---
function escapeHtml(text) { return text.replace(/[&<>"']/g, m => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' })[m]); }
function updateStatusDot(userId, online) {
    const row = document.querySelector(`[data-userid="${userId}"]`);
    const dot = row?.querySelector('.status-dot');
    if (dot) { dot.classList.toggle('online', online); dot.classList.toggle('offline', !online); dot.title = online ? "Online" : "Offline"; }
}

// Messaging
function sendMessageToUser(targetUserId) {
    const input = document.getElementById('msg-' + targetUserId);
    if (!input || !input.value.trim()) return;
    const text = input.value.trim(); input.value = '';
    videoConn.invoke('SendMessage', targetUserId, text).catch(e => console.error(e));

    const row = document.querySelector(`[data-userid="${targetUserId}"]`);
    const box = row?.querySelector('.msg-history');
    if (box) {
        const time = new Date().toLocaleTimeString();
        const html = `<div class="small my-1 text-primary"><strong>You</strong> <span class="text-muted" style="font-size:0.8em">(${time})</span><div>${escapeHtml(text)}</div></div>`;
        box.insertAdjacentHTML('afterbegin', html);
    }
}

// Call
async function startCall(targetUserId) {
    try {
        const online = await videoConn.invoke('IsOnline', targetUserId);
        if (!online) { alert("User offline"); return; }
    } catch { }

    remoteUserId = targetUserId;
    await ensureMedia();

    pc = new RTCPeerConnection();
    pc.onicecandidate = e => { if (e.candidate) videoConn.invoke('SendIce', remoteUserId, e.candidate); }
    pc.ontrack = e => { document.getElementById('remoteVideo').srcObject = e.streams[0]; }
    localStream.getTracks().forEach(t => pc.addTrack(t, localStream));

    const offer = await pc.createOffer();
    await pc.setLocalDescription(offer);
    await videoConn.invoke('SendOffer', remoteUserId, offer);
    showCallModal(true);
}

// Accept call
async function acceptCall() {
    hideIncomingModal(); ringtone.pause(); ringtone.currentTime = 0;
    await ensureMedia();
    pc = new RTCPeerConnection();
    pc.onicecandidate = e => { if (e.candidate) videoConn.invoke('SendIce', remoteUserId, e.candidate); }
    pc.ontrack = e => { document.getElementById('remoteVideo').srcObject = e.streams[0]; }
    localStream.getTracks().forEach(t => pc.addTrack(t, localStream));

    await pc.setRemoteDescription(window._incomingOffer);
    const answer = await pc.createAnswer();
    await pc.setLocalDescription(answer);
    await videoConn.invoke('SendAnswer', remoteUserId, answer);
    window._incomingOffer = null;
    showCallModal(true);
}
// Reject call
function rejectCall() {
    hideIncomingModal();
    window._incomingOffer = null;
}

// End call
function endCall() {
    if (pc) { try { pc.getSenders().forEach(s => s.track?.stop()); } catch { } pc.close(); pc = null; }
    if (localStream) { try { localStream.getTracks().forEach(t => t.stop()); } catch { } localStream = null; }
    showCallModal(false);
}

// Media
async function ensureMedia() {
    if (!localStream) { localStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true }); document.getElementById('localVideo').srcObject = localStream; }
}

// Modal helpers
function showIncomingModal(fromUser) { const el = document.getElementById('incomingCallModal'); if (!el) return; el.querySelector('#incomingFrom').innerText = fromUser || 'Caller'; const modal = new bootstrap.Modal(el); modal.show(); ringtone.play(); }
function hideIncomingModal() { ringtone.pause(); ringtone.currentTime = 0; const el = document.getElementById('incomingCallModal'); if (!el) return; const modal = bootstrap.Modal.getInstance(el); if (modal) modal.hide(); }
function showCallModal(show) {
    const el = document.getElementById('callModal'); if (!el) return;
    if (show) {
        const modal = new bootstrap.Modal(el, { backdrop: 'static', keyboard: false }); modal.show();
        if (window.innerWidth < 768) el.querySelector('.modal-dialog').classList.add('modal-fullscreen');
        callStartTime = new Date();
        const timerEl = el.querySelector('#callTimer');
        callTimerInterval = setInterval(() => {
            if (!timerEl || !callStartTime) return;
            const diff = Math.floor((new Date() - callStartTime) / 1000);
            const min = String(Math.floor(diff / 60)).padStart(2, '0');
            const sec = String(diff % 60).padStart(2, '0');
            timerEl.innerText = `${min}:${sec}`;
        }, 1000);
    } else { const modal = bootstrap.Modal.getInstance(el); if (modal) modal.hide(); clearInterval(callTimerInterval); callStartTime = null; const timerEl = el.querySelector('#callTimer'); if (timerEl) timerEl.innerText = '00:00'; }
}
