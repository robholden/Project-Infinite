import { FingerprintAIO } from '@ionic-native/fingerprint-aio/ngx';
import { AlertController } from '@ionic/angular';
import { Storage } from '@ionic/storage-angular';

import { PasswordRequest } from '@shared/models';
import { AuthState } from '@shared/storage';
import { AppColour } from '@shared/types/colour.type';

import { requireTouchId } from './touch-id.fn';
import { isPasswordValid } from './validators.fn';

export function promptForPassword(
    opts: { title?: string; touchId: { message: string }; password: { message: string; submitText?: string; submitColor?: 'primary' | 'danger' } },
    alertCtrl: AlertController,
    fingerprint: FingerprintAIO,
    storage: Storage,
    authState: AuthState
) {
    return new Promise<PasswordRequest>(async (res) => {
        // Prompt touch id if enabled
        const touchIdResult = await requireTouchId({ title: opts.title, message: opts.touchId?.message }, fingerprint, storage);
        if (touchIdResult.passed) return res({ touchIdKey: touchIdResult.key });
        else if (touchIdResult.enabled) return res(null);

        // If external provider, do not ask for password as we can't validate this
        if (!!authState.provider) {
            return res({ password: '' });
        }

        const alert = await alertCtrl.create({
            header: opts.title,
            subHeader: opts.password?.message,
            inputs: [
                {
                    name: 'password',
                    type: 'password',
                },
            ],
            buttons: [
                {
                    text: 'Cancel',
                    role: 'cancel',
                    cssClass: 'color-medium',
                    handler: () => res(null),
                },
                {
                    text: opts.password.submitText || 'Submit',
                    cssClass: `color-${opts.password?.submitColor || 'primary'}`,
                    handler: async (inputs) => res(isPasswordValid(inputs.password) ? { password: inputs.password } : null),
                },
            ],
        });
        await alert.present();
    });
}
