import { Inject, Injectable, isDevMode } from '@angular/core';

import { INJ_ENV } from '@shared/injectors';
import { Environment } from '@shared/interfaces';
import { SMap } from '@shared/models';
import { AuthStore } from '@shared/storage';

import { HttpApiService } from './http-api.service';
import { StorageService } from './storage.service';

import * as signalR from '@microsoft/signalr';

interface HubState {
    conn: signalR.HubConnection;
    methods: SMap<HubMethod[]>;
}

interface HubMethod {
    name: string;
    fn: (...args: any[]) => void;
}

@Injectable({
    providedIn: 'root',
})
export class SocketService {
    private state: HubState;

    constructor(@Inject(INJ_ENV) private env: Environment, private storage: StorageService<AuthStore>, private http: HttpApiService) {}

    async connect() {
        const [token, authToken] = await Promise.all([this.storage.getFromStorage<string>('token'), this.storage.getFromStorage<string>('refresh_token')]);
        if (!token || !authToken || this.state?.conn) return;

        const endpoint = this.env.gateway + '/sockets/hubs/hub';
        const conn = new signalR.HubConnectionBuilder()
            .withUrl(endpoint, {
                accessTokenFactory: async () => {
                    await this.http.refreshToken(endpoint);
                    return await this.storage.getFromStorage<string>('token');
                },
                transport: signalR.HttpTransportType.WebSockets,
            })
            .withAutomaticReconnect()
            .configureLogging(isDevMode() ? signalR.LogLevel.None : signalR.LogLevel.None)
            .build();

        try {
            await conn.start();
        } catch (err) {
            console.log('Failed to connect to web socket :/');
            return;
        }

        if (this.state) {
            this.state.conn = conn;
            this.connectMethods(conn);
        } else {
            this.state = {
                conn,
                methods: {},
            };
        }
    }

    async invoke(method: string, value: any) {
        let state = this.state;
        if (!state || state.conn.state !== signalR.HubConnectionState.Connected) return;

        await this.state.conn.invoke(method, value);
    }

    on(method: string, name: string, newMethod: (...args: any[]) => void): void {
        let state = this.state;
        if (!state) {
            const methods = {};
            methods[method] = [{ name, fn: newMethod }];
            this.state = { conn: null, methods };
            return;
        }

        const methodI = (state.methods[method] || []).findIndex((m) => m.name === name);
        if (methodI >= 0) {
            state.methods[method][methodI].fn = newMethod;
            return;
        }

        if (state.methods[method]) {
            state.methods[method].push({ name, fn: newMethod });
            return;
        }

        state.methods[method] = [{ name, fn: newMethod }];
        this.connectMethod(method);
    }

    off(method: string, name: string) {
        const index = this.state?.methods[method]?.findIndex((m) => m.name === name);
        if (index >= 0) this.state.methods[method].splice(index, 1);

        return this;
    }

    stop() {
        const state = this.state;
        if (!state?.conn) return;

        const methods = Object.keys(this.state.methods || {});
        methods.forEach((method) => state.conn.off(method));

        state.conn.stop();
        state.conn = null;
    }

    private connectMethods(conn: signalR.HubConnection) {
        if (!conn) return;

        const methods = Object.keys(this.state.methods || {});
        methods.forEach((method) => this.connectMethod(method));
    }

    private connectMethod(method: string) {
        const conn = this.state?.conn;
        if (!conn) return;

        const that = this;
        conn.on(method, function () {
            if (isDevMode) console.log('[DEBUG]', method, ...arguments);

            if (!that.state || !(that.state.methods || {})[method]) {
                return;
            }

            for (const fn of that.state.methods[method]) {
                fn.fn(...arguments);
            }
        });
    }
}
