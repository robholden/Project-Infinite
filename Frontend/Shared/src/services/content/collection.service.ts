import { Injectable } from '@angular/core';

import { obj2QueryString } from '@shared/functions';
import { Collection, CollectionSearch, CustomError, PagedList, PageRequest } from '@shared/models';

import { HttpApiService } from '../http-api.service';

@Injectable({
    providedIn: 'root',
})
export class CollectionService {
    constructor(private api: HttpApiService) {}

    /**
     * Returns a picture specific collection
     *
     * @param id The collection id
     */
    async get(id: string): Promise<Collection | CustomError> {
        return await this.api.get('/content/collection/' + id);
    }

    /**
     * Searches collections by given parameters
     *
     * @param route The api route to use for the endpoint
     * @param search The search criteria
     * @param pageRequest Page options
     */
    async lookup(route: string, search: CollectionSearch, pageRequest: PageRequest): Promise<PagedList<Collection> | CustomError> {
        return await this.api.get<PagedList<Collection>>(`/content/collection${route || ''}?` + obj2QueryString(search, pageRequest), {
            toastError: false,
        });
    }

    /**
     * Either adds or removes a picture to/from the collection
     *
     * @param collectionId The collection id to add to
     * @param pictureId The picture id to add
     */
    async addRemovePicture(collectionId: string, pictureId: string): Promise<void | CustomError> {
        return await this.api.put(`/content/collection/${collectionId}/${pictureId}`, {});
    }

    /**
     * Creates a collection
     *
     * @param collection The collection object to create
     */
    async create(collection: Collection): Promise<Collection | CustomError> {
        return await this.api.post('/content/collection', collection);
    }

    /**
     * Updates a collection
     *
     * @param id The collection id
     * @param collection The updated collection object
     */
    async update(id: string, collection: Collection): Promise<Collection | CustomError> {
        return await this.api.put('/content/collection/' + id, collection);
    }

    /**
     * Deletes a collection
     *
     * @param id The collection id
     */
    async delete(id: string): Promise<void | CustomError> {
        return await this.api.delete('/content/collection/' + id);
    }
}
