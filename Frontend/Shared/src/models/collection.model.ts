export interface CollectionPicture {
    pictureId: string;
    path: string;
}

export class Collection implements Collection {
    collectionId: string;
    pictures: CollectionPicture[];
    username: string;

    constructor(public name: string = '') {}
}

export interface CollectionSearch {
    username?: string;
    includePictureId?: string;
}

export enum CollectionOrderBy {
    None,
    Name,
}
