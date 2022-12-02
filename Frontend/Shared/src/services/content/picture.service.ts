import { Injectable } from '@angular/core';

import { obj2QueryString } from '@shared/functions';
import { CustomError, PagedList, PageRequest, Picture, PictureSearch, ReportPictureReason } from '@shared/models';

import { HttpApiService } from '../';
import { ApiOptions } from '../http-api.service';

@Injectable({
    providedIn: 'root',
})
export class PictureService {
    constructor(private api: HttpApiService) {}

    /**
     * Returns a picture specific picture
     *
     * @param id The picture id
     */
    async get(id: string, options: ApiOptions<Picture> = {}): Promise<Picture | CustomError> {
        return await this.api.get('/content/picture/' + id, { toastError: false, ...options });
    }

    /**
     * Searches pictures by given parameters
     *
     * @param route The api route to use for the endpoint
     * @param search The search criteria
     * @param pageRequest Page options
     */
    async lookup(route: string, search: PictureSearch, pageRequest: PageRequest): Promise<PagedList<Picture> | CustomError> {
        return await this.api.get<PagedList<Picture>>(`/content/picture${route || ''}?` + obj2QueryString(search, pageRequest), {
            toastError: false,
        });
    }

    /**
     * Returns a list of pictures that are nearby
     *
     * @param id The picture id
     */
    async nearby(id: string): Promise<Picture[]> {
        const resp = await this.api.get<Picture[]>(`/content/picture/${id}/nearby`, { toastError: false });
        return resp instanceof CustomError ? [] : resp;
    }

    /**
     * Returns a list of pictures that similar
     *
     * @param id The picture id
     */
    async matches(id: string): Promise<Picture[]> {
        const resp = await this.api.get<Picture[]>(`/content/picture/${id}/matches`, { toastError: false });
        return resp instanceof CustomError ? [] : resp;
    }

    /**
     * Submits a picture for processing
     *
     * @param id The picture id
     */
    async submit(id: string): Promise<Picture | CustomError> {
        return await this.api.post(`/content/picture/${id}/submit`, {});
    }

    /**
     * Updates a picture
     *
     * @param id The picture id
     * @param opts Values to update
     */
    async update(id: string, opts: { name?: string; tags: string[]; concealCoords: boolean; seed: string }): Promise<Picture | CustomError> {
        return await this.api.put('/content/picture/' + id, opts);
    }

    /**
     * Deletes a picture
     *
     * @param id The picture id
     */
    async delete(id: string): Promise<void | CustomError> {
        return await this.api.delete('/content/picture/' + id);
    }

    /**
     * Deletes a list of pictures
     *
     * @param ids The picture ids
     */
    async deleteMany(ids: string[]): Promise<void | CustomError> {
        return await this.api.post('/content/picture/delete-many', ids);
    }

    /**
     * Validates whether the user can upload x number of pictures
     *
     * @param uploads the number of uploads
     */
    async canUpload(uploads: number): Promise<void | CustomError> {
        const query = uploads === 1 ? '' : `?uploads=${uploads}`;
        const resp = await this.api.get(`/content/picture/upload/verify${query}`);
        if (resp instanceof CustomError) return resp;
    }

    /**
     * Upload and create a new picture
     *
     * @param file The image file
     * @param progress Callback returning progress of upload
     */
    async upload(file: File, progress: (percent: number) => void): Promise<{ picture: Picture; errors: CustomError[] } | CustomError> {
        return await this.api.upload('/content/picture', file, null, progress);
    }

    /**
     * Likes a picture
     *
     * @param id The picture id
     * @param liked Whether to like or unlike
     */
    async like(id: string, liked: boolean): Promise<void | CustomError> {
        return await this.api.put(`/content/picture/${id}/like`, { liked });
    }

    /**
     * Reports the given picture
     *
     * @param id The picture id
     * @param reason The reported reason
     */
    async report(id: string, reason: ReportPictureReason): Promise<void | CustomError> {
        return await this.api.post(`/content/picture/${id}/report`, { reason });
    }
}
