import { Injector } from '@angular/core';

import { AlertController } from '@app/shared/controllers/alert';
import { LoadingController } from '@app/shared/controllers/loading';
import { ToastController } from '@app/shared/controllers/toast';

import { PictureStatus } from '@shared/enums';
import { CustomError, Picture } from '@shared/models';
import { PictureService } from '@shared/services/content';
import { AuthState } from '@shared/storage';

export function deletePicture(injector: Injector, picture: Picture, btnId: string = 'delete-btn') {
    const alertCtrl = injector.get(AlertController);
    const toastCtrl = injector.get(ToastController);
    const loadingCtrl = injector.get(LoadingController);
    const pictureService = injector.get(PictureService);
    const authState = injector.get(AuthState);

    return new Promise<boolean>(async (res) => {
        const confirmed = await alertCtrl.confirm({
            title: 'Delete Picture',
            message: 'Are you sure you want to permanently delete this picture?',
            confirmBtn: {
                text: 'Delete',
                colour: 'danger',
            },
            focusFirst: true,
        });
        if (!confirmed) {
            res(false);
            return;
        }

        const loading = loadingCtrl.addBtn(btnId);
        loading.present();

        const resp = await pictureService.delete(picture.pictureId);
        loading.dismiss();

        // Update our stats
        if (authState.userStats) {
            switch (picture.status) {
                case PictureStatus.Draft:
                case PictureStatus.PendingApproval:
                    authState.userStats.drafts--;
                    break;

                case PictureStatus.Draft:
                    authState.userStats.published--;
                    break;
            }
        }

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            res(false);
            return;
        }

        toastCtrl.add('Picture has been successfully deleted', 'success').present(5000);

        res(true);
    });
}
