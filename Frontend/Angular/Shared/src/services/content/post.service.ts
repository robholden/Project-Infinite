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

    /**
     * Creates a new post
     *
     * @param post The post to create
     */
    async create(post: Partial<Post>): Promise<Post | CustomError> {
        return await this.api.post(`/content/post`, post, {
            toastError: false,
        });
    }

    /**
     * Updates a given post
     *
     * @param id The post id
     * @param post The new post data
     */
    async update(id: string, post: Partial<Post>): Promise<Post | CustomError> {
        return await this.api.post(`/content/post/${id}`, post, {
            toastError: false,
        });
    }

    /**
     * Deletes a post
     *
     * @param id The post id
     */
    async delete(id: string): Promise<boolean> {
        const resp = await this.api.delete(`/content/post/${id}`);
        return !(resp instanceof CustomError);
    }
}
