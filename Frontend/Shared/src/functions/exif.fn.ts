import { ExifReader, ExifTag } from '@shared/helpers';
import { ErrorCode } from '@shared/models';

import { base64toBlob, getTrueBase64 } from './base64.fn';

export function getExifDataFromFile(base64: string) {
    const b64 = getTrueBase64(base64);
    const exif = new ExifReader();
    const data = exif.readFromBinaryFile(base64toBlob(b64));

    return data;
}

export function validateFileGeoLocation(base64: string): ErrorCode {
    const data = getExifDataFromFile(base64);
    if (!data) return ErrorCode.MissingExifLocation;

    const gpsFlags = [ExifTag.GPSLatitudeRef, ExifTag.GPSLatitude, ExifTag.GPSLongitudeRef, ExifTag.GPSLongitude];
    const missing_gps_flags = gpsFlags.filter((flag) => !(flag in data) || !data[flag]);

    if (missing_gps_flags.length > 0) {
        return ErrorCode.MissingExifLocation;
    }

    if (!data[ExifTag.DateTimeOriginal]) {
        return ErrorCode.MissingExifTimestamp;
    }

    return null;
}
