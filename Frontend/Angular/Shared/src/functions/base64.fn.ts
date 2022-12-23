export function base64toBlob(base64: any) {
    const binary_string = atob(base64);
    let len = binary_string.length;
    let bytes = new Uint8Array(len);
    for (let i = 0; i < len; i++) {
        bytes[i] = binary_string.charCodeAt(i);
    }
    return bytes.buffer;
}

export function getTrueBase64(base64: string) {
    const startI = base64.indexOf('base64,');
    return base64.substring(startI + 7, base64.length);
}

export function base64FileSizeInMb(base64: string) {
    base64 = getTrueBase64(base64);
    var sizeInBytes = 4 * Math.ceil(base64.length / 3) * 0.5624896334383812;
    return sizeInBytes / 1000 / 1000;
}
