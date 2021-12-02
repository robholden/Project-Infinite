import { Location as CommonLocation } from '@angular/common';
import { Component, Injector, OnDestroy, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';

import { fade } from '@app/functions/animations.fn';
import { MapData, pictureMarkers } from '@app/functions/leaflet.fn';
import { replaceLoadingTitle } from '@app/functions/routing-titles.fn';

import { PictureStatus } from '@shared/enums';
import { waitThen } from '@shared/functions';
import { CustomError, Location, Picture } from '@shared/models';
import { SocketService } from '@shared/services';
import { PictureService } from '@shared/services/content';
import { AuthState } from '@shared/storage';

import { LatLngBounds } from 'leaflet';

@Component({
    selector: 'sc-picture',
    templateUrl: './picture.page.html',
    styleUrls: ['./picture.page.scss'],
    animations: [fade],
})
export class PicturePage implements OnInit, OnDestroy {
    loading: boolean = true;
    picture: Picture;
    author: boolean;
    reported: boolean;
    fromApprovals: boolean;

    nearby: Picture[];

    mapData: MapData;
    fitBounds: LatLngBounds;

    constructor(
        private injector: Injector,
        public authState: AuthState,
        private sockets: SocketService,
        private activatedRoute: ActivatedRoute,
        private location: CommonLocation,
        private title: Title,
        private router: Router,
        private pictureService: PictureService
    ) {
        this.activatedRoute.params.subscribe((params) => this.fetch(params['pictureId']));
        this.activatedRoute.queryParams.subscribe((params) => (this.fromApprovals = params['ref'] === 'approvals'));
    }

    ngOnInit(): void {}

    ngOnDestroy() {
        this.sockets.off('NewPictureLocation', 'picture_page').off('ModeratedPicture', 'picture_page');
    }

    async fetch(key: string) {
        // Verify the key with the server
        const resp = await this.pictureService.get(key);
        this.loading = false;

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            replaceLoadingTitle(this.title, ':(');
            return;
        }

        replaceLoadingTitle(this.title, resp.name || 'Draft');

        this.picture = resp;
        this.author = this.authState.user && this.authState.user.username === resp.username;

        // Listen for location updates
        if (this.author && this.picture.status === PictureStatus.Draft) {
            this.sockets.on('NewPictureLocation', 'picture_page', (payload: { pictureId: string; location: Location }) => {
                if (payload?.pictureId !== this.picture.pictureId) return;
                this.picture.location = payload.location;
            });
        }

        // Watch for moderation actions
        if (this.authState.is_mod && this.picture.status !== PictureStatus.Published) {
            this.sockets.on('ModeratedPicture', 'picture_page', async (pictureId: string, approved: boolean) => {
                if (key !== pictureId) return;

                if (!approved) return this.router.navigateByUrl('/');
                const resp = await this.pictureService.get(key);

                if (resp instanceof CustomError) return;
                else this.picture = resp;
            });
        }

        // Load nearby pictures
        if (this.authState.is_mod || this.picture.status === PictureStatus.Published) {
            this.nearby = await this.pictureService.nearby(this.picture.pictureId);
        }

        await waitThen(0, () => (this.mapData = pictureMarkers(this.injector, this.nearby, this.picture)));
    }

    deleted() {
        const ending = [PictureStatus.Draft, PictureStatus.PendingApproval].includes(this.picture.status) ? '/drafts' : '';
        this.router.navigateByUrl(`/user/${this.authState.user.username}${ending}`);
    }

    back() {
        this.location.back();
    }
}
