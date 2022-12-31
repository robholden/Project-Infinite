import { Injectable } from '@angular/core';

import { obj2QueryString } from '@shared/functions';
import { CustomError, PagedList, PageRequest, Post, PostSearch } from '@shared/models';

import { HttpApiService } from '../';

@Injectable({
    providedIn: 'root',
})
export class PostService {
    constructor(private api: HttpApiService) {}

    /**
     * Get a post by its id
     *
     * @param id The post's id
     */
    async getById(id: string): Promise<Post | CustomError> {
        return await this.api.get<Post>(`/content/post/${id}`);
    }

    /**
     * Searches posts by given parameters
     *
     * @param route The api route to use for the endpoint
     * @param search The search criteria
     * @param pageRequest Page options
     */
    async lookup(route: string, search: PostSearch, pageRequest: PageRequest): Promise<PagedList<Post> | CustomError> {
        return await this.api.get(`/content/post${route || ''}?` + obj2QueryString(search, pageRequest), {
            toastError: false,
        });
    }
}