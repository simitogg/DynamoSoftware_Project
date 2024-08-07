'use client';
import { useEffect, useState, useRef } from 'react';
import * as signalR from '@microsoft/signalr';

const useSignalR = (url, eventHandlers) => {
    const [connection, setConnection] = useState(null);
    const connectionRef = useRef(null);

    useEffect(() => {
        const connect = async () => {
            if (connectionRef.current) return; // Prevent multiple connections

            const conn = new signalR.HubConnectionBuilder()
                .withUrl(url)
                .configureLogging(signalR.LogLevel.Information)
                .withAutomaticReconnect()
                .build();

            // Set timeouts
            conn.serverTimeoutInMilliseconds = 120000; // 120 seconds
            conn.handshakeTimeoutInMilliseconds = 120000; // 120 seconds

            Object.entries(eventHandlers).forEach(([event, handler]) => {
                conn.on(event, handler);
            });

            conn.onclose(async (error) => {
                if (error) {
                    console.error('SignalR connection closed with error:', error);
                    alert('SignalR connection closed with error:' + error);
                } else {
                    console.log('SignalR connection closed.');
                }
                // Optionally try to reconnect or notify the user
                setTimeout(() => connect(), 5000); // Retry connection after 5 seconds
            });

            try {
                await conn.start();
                console.log('SignalR connected.');
            } catch (err) {
                console.error('SignalR Connection Error:', err);
                alert('SignalR Connection Error:' + err);
            }

            connectionRef.current = conn;
            setConnection(conn);
        };

        connect();

        return () => {
            if (connectionRef.current) {
                connectionRef.current.stop();
                connectionRef.current = null;
            }
        };
    }, [url, eventHandlers]); // Ensure dependencies are correctly set

    return connection;
};

export default useSignalR;
