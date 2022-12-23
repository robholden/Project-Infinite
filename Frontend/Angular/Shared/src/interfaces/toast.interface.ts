import { Trx } from '@shared/models';

export interface Toast {
    showError(trx: Trx | string, duration?: number): Promise<void>;
    showMessage(trx: Trx | string, duration?: number): Promise<void>;
}
