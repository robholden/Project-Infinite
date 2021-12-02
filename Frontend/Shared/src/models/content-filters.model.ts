import { Location } from './location.model';
import { Tag } from './tag.model';

export interface ContentFilters {
    locations: Location[];
    tags: Tag[];
}
