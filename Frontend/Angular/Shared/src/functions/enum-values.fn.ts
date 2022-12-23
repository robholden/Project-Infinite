import { pascalToScores } from './pascal-to-scores.fn';

export function enumValues(enumParam: any, ...ignoreValues: number[]): string[] {
    const values: string[] = [];
    const to_ignore: string[] = (ignoreValues || []).map((v) => enumParam[v]);

    for (let entry in enumParam) {
        if (!isNaN(parseInt(entry))) continue;
        if (to_ignore.includes(entry)) continue;

        values.push(pascalToScores(entry));
    }

    return values;
}
