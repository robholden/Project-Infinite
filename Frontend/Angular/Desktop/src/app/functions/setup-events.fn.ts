import { Injector } from '@angular/core';
import { Router } from '@angular/router';

import { CustomEvent } from '@shared/enums';
import { SMap } from '@shared/models';
import { EventService } from '@shared/services';
import { AuthService } from '@shared/services/identity';

import { RegisterExternalComponent } from '@app/components/auth/register-external/register-external.component';
import { AuthModal } from '@app/modals/auth-modal/auth.modal';
import { ModalController } from '@app/shared/controllers/modal';

function logout(router: Router, authService: AuthService, modalCtrl: ModalController) {
    return async () => {
        await authService.logout();

        modalCtrl.dismiss();
        router.navigateByUrl('/');
    };
}

function revokeSession(router: Router, authService: AuthService) {
    return async () => {
        // Logout user
        await authService.logout(true);

        // Only redirect when user is on an authorized page
        const url = router.url;
        router.navigateByUrl('/', { skipLocationChange: true }).then(() => setTimeout(() => router.navigateByUrl(url), 5000));
    };
}

function openModal(modalCtrl: ModalController) {
    return async (id: string, component: any, componentProps: any, dismiss?: string) => {
        if (dismiss) modalCtrl.dismiss(dismiss);
        if (!componentProps) componentProps = {};

        const modal = modalCtrl.add(id, component, componentProps);
        return await modal.present();
    };
}

export function setupEvents(injector: Injector) {
    const router = injector.get<Router>(Router);
    const events = injector.get<EventService>(EventService);
    const modalCtrl = injector.get<ModalController>(ModalController);
    const authService = injector.get<AuthService>(AuthService);

    events.register(CustomEvent.Login, async (params) => await openModal(modalCtrl)('login-modal', AuthModal, { type: 'login', ...(params || {}) }));
    events.register(
        CustomEvent.Register,
        async (params) => await openModal(modalCtrl)('register-modal', AuthModal, { type: 'register', ...(params || {}) })
    );
    events.register(
        CustomEvent.RegisterExternal,
        async (params) => await openModal(modalCtrl)('register-external-modal', RegisterExternalComponent, params)
    );
    events.register(CustomEvent.Logout, async () => await logout(router, authService, modalCtrl)());
    events.register(CustomEvent.Revoked, async () => await revokeSession(router, authService)());

    const eventsToRun: SMap<(ev: MouseEvent) => any> = {};
    document.body.addEventListener('click', (ev: MouseEvent) => {
        Object.values(eventsToRun).forEach((fn) => fn(ev));
    });

    events.register(CustomEvent.BodyClick, (params) => {
        if (!params.fn) delete eventsToRun[params.id];
        else eventsToRun[params.id] = params.fn;
    });
}

export function preferDarkMode(): boolean {
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)');
    return prefersDark && prefersDark.matches;
}
