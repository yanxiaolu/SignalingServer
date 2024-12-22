class WebRTCManager {
    constructor() {
        this.localStream = null;
        this.peerConnections = new Map();
        this.configuration = {
            iceServers: [
                { urls: 'stun:stun.cloudflare.com:3478' },
                { urls: 'stun:stun.l.google.com:19302' }
            ]
        };
    }

    async initialize() {
        try {
            this.localStream = await navigator.mediaDevices.getUserMedia({
                audio: true,
                video: true
            });
            
            const localVideo = document.getElementById('localVideo');
            if (localVideo) {
                localVideo.srcObject = this.localStream;
            }
            
            return true;
        } catch (error) {
            console.error('初始化WebRTC失败:', error);
            throw error;
        }
    }

    async createOffer(targetUserId) {
        try {
            const peerConnection = this._createPeerConnection(targetUserId);
            const offer = await peerConnection.createOffer();
            await peerConnection.setLocalDescription(offer);
            return JSON.stringify(offer);
        } catch (error) {
            console.error('创建offer失败:', error);
            throw error;
        }
    }

    async handleOffer(fromUserId, offerStr) {
        try {
            const offer = JSON.parse(offerStr);
            const peerConnection = this._createPeerConnection(fromUserId);
            await peerConnection.setRemoteDescription(offer);
            const answer = await peerConnection.createAnswer();
            await peerConnection.setLocalDescription(answer);
            return JSON.stringify(answer);
        } catch (error) {
            console.error('处理offer失败:', error);
            throw error;
        }
    }

    async handleAnswer(fromUserId, answerStr) {
        try {
            const answer = JSON.parse(answerStr);
            const peerConnection = this.peerConnections.get(fromUserId);
            if (!peerConnection) {
                throw new Error(`未找到与用户 ${fromUserId} 的连接`);
            }
            await peerConnection.setRemoteDescription(answer);
        } catch (error) {
            console.error('处理answer失败:', error);
            throw error;
        }
    }

    async handleIceCandidate(fromUserId, candidateStr) {
        try {
            const candidate = JSON.parse(candidateStr);
            const peerConnection = this.peerConnections.get(fromUserId);
            if (!peerConnection) {
                throw new Error(`未找到与用户 ${fromUserId} 的连接`);
            }
            await peerConnection.addIceCandidate(candidate);
        } catch (error) {
            console.error('处理ICE候选失败:', error);
            throw error;
        }
    }

    _createPeerConnection(userId) {
        if (this.peerConnections.has(userId)) {
            this.closePeerConnection(userId);
        }

        const peerConnection = new RTCPeerConnection(this.configuration);
        this.peerConnections.set(userId, peerConnection);

        // 添加本地流
        this.localStream.getTracks().forEach(track => {
            peerConnection.addTrack(track, this.localStream);
        });

        // 处理远程流
        peerConnection.ontrack = (event) => {
            const remoteVideo = document.querySelector(`#video-${userId}`);
            if (remoteVideo && event.streams[0]) {
                remoteVideo.srcObject = event.streams[0];
            }
        };

        // ICE候选处理
        peerConnection.onicecandidate = (event) => {
            if (event.candidate) {
                // 通过DotNet调用信令服务器发送ICE候选
                DotNet.invokeMethodAsync('SignalingServer.WebClient', 
                    'SendIceCandidate', 
                    userId, 
                    JSON.stringify(event.candidate)
                );
            }
        };

        // 连接状态变化监控
        peerConnection.onconnectionstatechange = () => {
            console.log(`连接状态 (${userId}):`, peerConnection.connectionState);
        };

        return peerConnection;
    }

    closePeerConnection(userId) {
        const peerConnection = this.peerConnections.get(userId);
        if (peerConnection) {
            peerConnection.close();
            this.peerConnections.delete(userId);
        }
    }

    dispose() {
        // 清理所有连接
        this.peerConnections.forEach((pc, userId) => {
            this.closePeerConnection(userId);
        });

        // 停止所有媒体轨道
        if (this.localStream) {
            this.localStream.getTracks().forEach(track => track.stop());
            this.localStream = null;
        }
    }
}

window.WebRTC = new WebRTCManager();