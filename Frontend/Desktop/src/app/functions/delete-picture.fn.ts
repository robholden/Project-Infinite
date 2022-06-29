import { Injector } from '@angular/core';

import { PictureStatus } from '@shared/enums';
import { CustomError, Picture } from '@shared/models';
import { PictureService } from '@shared/services/content';
import { AuthState } from '@shared/storage';

import { AlertController } from '@app/shared/controllers/alert';
import { ToastController } from '@app/shared/controllers/toast';

export function deletePicture(injector: Injector, picture: Picture) {
    const alertCtrl = injector.get(AlertController);
    const toastCtrl = injector.get(ToastController);
    const pictureService = injector.get(PictureService);
    const authState = injector.get(AuthState);

    return new Promise<boolean>(async (res) => {
        await alertCtrl.confirm({
            title: 'Delete Picture',
            message: 'Are you sure you want to permanently delete this picture?',
            confirmBtn: {
                text: 'Delete',
                colour: 'danger',
            },
            focusFirst: true,
            dismissWhen: async (confirmed: boolean) => {
                if (!confirmed) {
                    res(false);
                    return true;
                }

                // Call api to delete picture
                const resp = await pictureService.delete(picture.pictureId);

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
                    return false;
                }

                toastCtrl.add('Picture has been successfully deleted', 'success').present(5000);

                res(true);
                return true;
            },
        });
    });
}
