import { FingerprintAIO } from '@ionic-native/fingerprint-aio/ngx';
import { Storage } from '@ionic/storage-angular';

import { stringFormat } from '@shared/functions';

export interface TouchIdResult {
    enabled: boolean;
    passed?: boolean;
    key?: string;
}

export function removeTouchId(storage: Storage) {
    return new Promise<void>(async (res) => {
        let touchIdName: string;

        try {
            touchIdName = await storage.get('touch-id-name');
            if (touchIdName) await storage.remove('touch-id-name');
        } catch (err) {}

        res();
    });
}

export function requireTouchId(opts: { title?: string; message: string }, fingerprint: FingerprintAIO, storage: Storage) {
    return new Promise<TouchIdResult>(async (res) => {
        let touchIdName: string;

        try {
            touchIdName = await storage.get('touch-id-name');
            if (!touchIdName) return res({ enabled: false });

            await fingerprint.isAvailable();
        } catch (err) {
            return res({ enabled: false });
        }

        try {
            const key = await fingerprint.loadBiometricSecret({
                title: opts.title,
                description: stringFormat(opts.message, touchIdName),
                disableBackup: false,
            });
            res({ enabled: true, passed: !!key, key });
        } catch (err) {
            res({ enabled: true, passed: false });
        }
    });
}
