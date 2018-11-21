class ConnectionInfo {
    connectionStatus: ConnectionStatus;
    server: string;
    connectionType: ConnectionType;

    constructor(connectionStatus: ConnectionStatus, server: string, connectionType: ConnectionType) {
        this.connectionStatus = connectionStatus;
        this.server = server;
        this.connectionType = connectionType;
    }

    toString() {
        return "ConnectionStatus: " + this.connectionStatus + ", Server: " + this.server + ", Type: " + this.connectionType;
    }
}