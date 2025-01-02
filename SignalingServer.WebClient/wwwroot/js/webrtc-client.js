class WebRTCClient {
    constructor() {
        this.localVideo = document.getElementById('localVideo');
        this.remoteVideo = document.getElementById('remoteVideo');
        this.localStream = null;
        this.peerConnection = null;
        this.remoteStream = null;
        this.dataChannel = null;
        this.servers = null; // 定义您的STUN/TURN服务器
        this.signalingServer = null; // 信令服务器实例
        this.configuration = {
            iceServers: [
                { urls: 'stun:stun.l.google.com:19302' },
                { urls: 'stun:stun.cloudflare.com:3478' }
            ],
            iceCandidatePoolSize: 10
        };
        this.isConnected = false;
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 3;
    }

    // 初始化信令服务器
    initSignalingServer(signalingServerUrl) {
        this.signalingServer = new WebSocket(signalingServerUrl);
        this.signalingServer.onmessage = (message) => this.handleSignalingMessage(message);
        this.signalingServer.onerror = (error) => console.error('信令服务器错误:', error);
    }

    // 获取本地媒体流
    async start() {
        try {
            const constraints = {
                video: {
                    width: { ideal: 1280 },
                    height: { ideal: 720 },
                    frameRate: { ideal: 30 }
                },
                audio: {
                    echoCancellation: true,
                    noiseSuppression: true,
                    autoGainControl: true
                }
            };
            
            this.localStream = await navigator.mediaDevices.getUserMedia(constraints);
            this.localVideo.srcObject = this.localStream;
            console.log('本地媒体流获取成功');
            return true;
        } catch (error) {
            console.error('获取本地媒体流失败:', error);
            throw error;
        }
    }

    async initializePeerConnection() {
        if (this.peerConnection) {
            await this.cleanupPeerConnection();
        }

        this.peerConnection = new RTCPeerConnection(this.configuration);
        this._setupPeerConnectionListeners();
        this._addLocalStreamTracks();
        
        return this.peerConnection;
    }

    _setupPeerConnectionListeners() {
        this.peerConnection.oniceconnectionstatechange = () => {
            console.log('ICE连接状态:', this.peerConnection.iceConnectionState);
            switch (this.peerConnection.iceConnectionState) {
                case 'disconnected':
                    this._handleDisconnection();
                    break;
                case 'failed':
                    this._attemptReconnection();
                    break;
                case 'connected':
                    this.isConnected = true;
                    this.reconnectAttempts = 0;
                    break;
            }
        };

        this.peerConnection.onconnectionstatechange = () => {
            console.log('连接状态:', this.peerConnection.connectionState);
        };

        this.peerConnection.onicegatheringstatechange = () => {
            console.log('ICE收集状态:', this.peerConnection.iceGatheringState);
        };

        this.peerConnection.onicecandidate = event => this.onIceCandidate(event);
        this.peerConnection.ontrack = event => this.onTrack(event);
    }

    async _handleDisconnection() {
        if (this.isConnected) {
            this.isConnected = false;
            console.log('连接已断开，尝试重新建立连接...');
            await this._attemptReconnection();
        }
    }

    async _attemptReconnection() {
        if (this.reconnectAttempts >= this.maxReconnectAttempts) {
            console.error('重连次数超过最大限制，停止重连');
            this.handleRemoteHangup();
            return;
        }

        this.reconnectAttempts++;
        console.log(`尝试重连 (${this.reconnectAttempts}/${this.maxReconnectAttempts})`);
        
        try {
            await this.initializePeerConnection();
            await this.call();
        } catch (error) {
            console.error('重连失败:', error);
        }
    }

    async cleanupPeerConnection() {
        if (this.peerConnection) {
            this.peerConnection.onicecandidate = null;
            this.peerConnection.ontrack = null;
            this.peerConnection.oniceconnectionstatechange = null;
            this.peerConnection.onconnectionstatechange = null;
            this.peerConnection.close();
            this.peerConnection = null;
        }

        if (this.dataChannel) {
            this.dataChannel.close();
            this.dataChannel = null;
        }
    }

    // 发起呼叫
    async call() {
        this.peerConnection = new RTCPeerConnection(this.servers);
        this.peerConnection.onicecandidate = event => this.onIceCandidate(event);
        this.peerConnection.ontrack = event => this.onTrack(event);

        this.localStream.getTracks().forEach(track => {
            this.peerConnection.addTrack(track, this.localStream);
        });

        this.dataChannel = this.peerConnection.createDataChannel('chat');
        this.dataChannel.onopen = () => console.log('数据通道已打开');
        this.dataChannel.onmessage = event => console.log('收到消息:', event.data);

        try {
            const offer = await this.peerConnection.createOffer();
            await this.peerConnection.setLocalDescription(offer);
            this.sendSignalingMessage({ type: 'offer', sdp: offer.sdp });
            console.log('发送offer:', offer.sdp);
        } catch (error) {
            console.error('创建offer失败:', error);
        }
    }

    // 处理ICE候选者
    async onIceCandidate(event) {
        if (event.candidate) {
            this.sendSignalingMessage({ type: 'candidate', candidate: event.candidate });
            console.log('发送ICE候选者:', event.candidate);
        }
    }

    // 处理远程媒体流
    onTrack(event) {
        if (!this.remoteStream) {
            this.remoteStream = new MediaStream();
            this.remoteVideo.srcObject = this.remoteStream;
        }
        this.remoteStream.addTrack(event.track);
        console.log('收到远程媒体流');
    }

    // 挂断通话
    async hangup() {
        this.peerConnection.close();
        this.peerConnection = null;
        this.sendSignalingMessage({ type: 'hangup' });
        console.log('挂断通话');
    }

    // 静音
    mute() {
        this.localStream.getAudioTracks().forEach(track => track.enabled = false);
        console.log('麦克风已静音');
    }

    // 取消静音
    unmute() {
        this.localStream.getAudioTracks().forEach(track => track.enabled = true);
        console.log('麦克风已取消静音');
    }

    // 关闭摄像头
    closeCamera() {
        this.localStream.getVideoTracks().forEach(track => track.enabled = false);
        console.log('摄像头已关闭');
    }

    // 打开摄像头
    openCamera() {
        this.localStream.getVideoTracks().forEach(track => track.enabled = true);
        console.log('摄像头已打开');
    }

    // 发送数据通道消息
    sendMessage() {
        const message = document.getElementById('dataChannelInput').value;
        this.dataChannel.send(message);
        console.log('发送消息:', message);
    }

    // 发送信令消息
    sendSignalingMessage(message) {
        this.signalingServer.send(JSON.stringify(message));
        console.log('发送信令消息:', message);
    }

    // 处理信令消息
    async handleSignalingMessage(message) {
        const data = JSON.parse(message.data);
        switch (data.type) {
            case 'offer':
                await this.handleOffer(data.sdp);
                break;
            case 'answer':
                await this.handleAnswer(data.sdp);
                break;
            case 'candidate':
                await this.handleCandidate(data.candidate);
                break;
            case 'hangup':
                this.handleRemoteHangup();
                break;
            default:
                console.error('未知的信令消息类型:', data.type);
        }
    }

    // 处理offer
    async handleOffer(sdp) {
        if (!this.peerConnection) {
            this.peerConnection = new RTCPeerConnection(this.servers);
            this.peerConnection.onicecandidate = event => this.onIceCandidate(event);
            this.peerConnection.ontrack = event => this.onTrack(event);

            this.localStream.getTracks().forEach(track => {
                this.peerConnection.addTrack(track, this.localStream);
            });

            this.peerConnection.ondatachannel = event => {
                this.dataChannel = event.channel;
                this.dataChannel.onopen = () => console.log('数据通道已打开');
                this.dataChannel.onmessage = event => console.log('收到消息:', event.data);
            };
        }

        await this.peerConnection.setRemoteDescription(new RTCSessionDescription({ type: 'offer', sdp }));
        const answer = await this.peerConnection.createAnswer();
        await this.peerConnection.setLocalDescription(answer);
        this.sendSignalingMessage({ type: 'answer', sdp: answer.sdp });
        console.log('发送answer:', answer.sdp);
    }

    // 处理answer
    async handleAnswer(sdp) {
        await this.peerConnection.setRemoteDescription(new RTCSessionDescription({ type: 'answer', sdp }));
        console.log('收到answer:', sdp);
    }

    // 处理ICE候选者
    async handleCandidate(candidate) {
        await this.peerConnection.addIceCandidate(new RTCIceCandidate(candidate));
        console.log('添加ICE候选者:', candidate);
    }

    // 处理远程挂断
    handleRemoteHangup() {
        this.peerConnection.close();
        this.peerConnection = null;
        console.log('远程挂断通话');
    }

    // 带宽管理方法
    async manageBandwidth() {
        const sender = this.peerConnection.getSenders().find(s => s.track.kind === 'video');
        const parameters = sender.getParameters();
        if (!parameters.encodings) {
            parameters.encodings = [{}];
        }
        parameters.encodings[0].maxBitrate = 500000; // 设置最大比特率为500kbps
        await sender.setParameters(parameters);
        console.log('设置最大比特率为500kbps');
    }

    // 改进的媒体控制方法
    async toggleAudio(enabled) {
        this.localStream.getAudioTracks().forEach(track => {
            track.enabled = enabled;
        });
        return enabled;
    }

    async toggleVideo(enabled) {
        this.localStream.getVideoTracks().forEach(track => {
            track.enabled = enabled;
        });
        return enabled;
    }

    async changeVideoQuality(quality) {
        const videoTrack = this.localStream.getVideoTracks()[0];
        if (!videoTrack) return;

        const constraints = {
            low: { width: 640, height: 480, frameRate: 15 },
            medium: { width: 1280, height: 720, frameRate: 30 },
            high: { width: 1920, height: 1080, frameRate: 30 }
        };

        try {
            await videoTrack.applyConstraints(constraints[quality]);
            console.log(`视频质量已更改为: ${quality}`);
        } catch (error) {
            console.error('更改视频质量失败:', error);
            throw error;
        }
    }

    dispose() {
        this.cleanupPeerConnection();
        if (this.localStream) {
            this.localStream.getTracks().forEach(track => {
                track.stop();
            });
            this.localStream = null;
        }
        if (this.signalingServer) {
            this.signalingServer.close();
            this.signalingServer = null;
        }
    }
}

// 初始化WebRTC客户端
const webRTCClient = new WebRTCClient();

// 防止全局变量污染
window.WebRTCClient = new WebRTCClient();
