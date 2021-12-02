import { Injectable } from '@angular/core';

import { CustomError, Tag } from '@shared/models';

import { HttpApiService } from '../';

@Injectable({
    providedIn: 'root',
})
export class TagService {
    constructor(private api: HttpApiService) {}

    /**
     * Get all tags
     */
    async getAll(): Promise<Tag[] | CustomError> {
        return await this.api.get<Tag[]>('/content/tag', { toastError: false });
    }

    /**
     * Bulk save tags
     *
     * @param toAdd Tags to add
     * @param toUpdate Tags to update
     * @param toDelete Tags to delete
     */
    async save(toAdd: Tag[], toUpdate: Tag[], toDelete: Tag[]): Promise<void | CustomError> {
        return await this.api.post(`/content/tag`, { toAdd, toUpdate, toDelete });
    }

    /**
     * Deletes a tag
     *
     * @param id The id of the tag
     */
    async delete(id: number): Promise<void | CustomError> {
        return await this.api.delete(`/content/tag/${id}`);
    }
}
