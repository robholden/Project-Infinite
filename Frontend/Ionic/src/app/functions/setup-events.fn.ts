import { Injector } from '@angular/core';
import { Router } from '@angular/router';

import { LoginModal } from '@app/modals/login-modal/login.modal';
import { PrivacyPolicyModal } from '@app/modals/privacy-policy-modal/privacy-policy.modal';
import { RegisterModal } from '@app/modals/register-modal/register.modal';
import { TermsAndConditionsModal } from '@app/modals/terms-and-conditions-modal/terms-and-conditions.modal';

import { FingerprintAIO } from '@ionic-native/fingerprint-aio/ngx';
import { LoadingController, ModalController, NavController, ToastController } from '@ionic/angular';
import { Storage } from '@ionic/storage-angular';

import { CustomEvent } from '@shared/enums';
import { CustomError, SMap } from '@shared/models';
import { EventService } from '@shared/services';
import { AuthService } from '@shared/services/identity';

import { requireTouchId } from './touch-id.fn';

export interface ModalWithParams extends SMap<any> {
    presentingElement?: HTMLIonModalElement | HTMLIonRouterOutletElement;
}

function openModal(modalCtrl: ModalController) {
    return async (id: string, component: any, componentProps: ModalWithParams, dismiss?: string) => {
        if (dismiss && (await modalCtrl.getTop())) await modalCtrl.dismiss(null, null, dismiss);

        if (!componentProps) componentProps = {};
        if (!componentProps.presentingElement) componentProps.presentingElement = await modalCtrl.getTop();

        const modal = await modalCtrl.create({
            component,
            id,
            componentProps,
            swipeToClose: !!componentProps.presentingElement,
            presentingElement: componentProps.presentingElement,
        });

        // Open and wait for the modal to close
        await modal.present();
        const result = await modal.onDidDismiss();

        return result;
    };
}

function logout(navCtrl: NavController, authService: AuthService, loadingCtrl: LoadingController, toastCtrl: ToastController) {
    return async () => {
        const loading = await loadingCtrl.create({
            message: 'Logging you out',
            spinner: 'crescent',
        });
        await loading.present();

        await authService.logout();
        await loading.dismiss();
        await navCtrl.navigateRoot('/account');

        const toast = await toastCtrl.create({
            message: 'Goodbye 👋',
            color: 'primary',
            duration: 2500,
        });
        await toast.present();
    };
}

function login(
    params: SMap<any>,
    fingerprint: FingerprintAIO,
    router: Router,
    authService: AuthService,
    modalCtrl: ModalController,
    loadingCtrl: LoadingController,
    storage: Storage
) {
    return async () => {
        const result = await requireTouchId({ message: '{0}, scan your fingerprint to login' }, fingerprint, storage);
        if (result.passed === true) {
            const loading = await loadingCtrl.create({
                message: 'Logging you in',
                spinner: 'crescent',
            });
            await loading.present();

            const resp = await authService.loginWithTouchId(result.key);
            await loading.dismiss();

            if (!(resp instanceof CustomError)) {
                const url = 'ref' in (params || {}) ? params['ref'] : router.url;
                await router.navigateByUrl(url);
                return;
            }
        }

        await openModal(modalCtrl)('login', LoginModal, params, 'register');
    };
}

function revokeSession(navCtrl: NavController, modalCtrl: ModalController, router: Router, authService: AuthService) {
    return async () => {
        // Logout user
        authService.logout(true);

        // Only redirect when user is on an authorized page
        var currentRouteConfig = router.config.find((f) => f.path == router.url.substr(1));
        if (!currentRouteConfig || !currentRouteConfig.canActivate) return;

        const url = router.url;
        await navCtrl.navigateRoot('/account');

        await openModal(modalCtrl)('login', LoginModal, { ref: url }, 'register');
    };
}

export function setupEvents(injector: Injector) {
    const router = injector.get<Router>(Router);
    const navCtrl = injector.get<NavController>(NavController);
    const events = injector.get<EventService>(EventService);
    const modalCtrl = injector.get<ModalController>(ModalController);
    const authService = injector.get<AuthService>(AuthService);
    const loadingCtrl = injector.get<LoadingController>(LoadingController);
    const toastCtrl = injector.get<ToastController>(ToastController);
    const storage = injector.get<Storage>(Storage);
    const fingerprint = injector.get<FingerprintAIO>(FingerprintAIO);

    events.register(CustomEvent.TermsPolicy, async (params) => await openModal(modalCtrl)('terms', TermsAndConditionsModal, params));
    events.register(CustomEvent.PrivacyPolicy, async (params) => await openModal(modalCtrl)('privacy', PrivacyPolicyModal, params));
    events.register(CustomEvent.Register, async (params) => await openModal(modalCtrl)('register', RegisterModal, params, 'login'));
    events.register(CustomEvent.Login, async (params) => await login(params, fingerprint, router, authService, modalCtrl, loadingCtrl, storage)());
    events.register(CustomEvent.Logout, async (params) => await logout(navCtrl, authService, loadingCtrl, toastCtrl)());
    events.register(CustomEvent.Revoked, async (params) => await revokeSession(navCtrl, modalCtrl, router, authService)());
}
