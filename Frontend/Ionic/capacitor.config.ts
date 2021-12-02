import { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
    appId: 'com.snowcapture',
    appName: 'Snow Capture',
    webDir: 'www',
    bundledWebRuntime: false,
    plugins: {
        SplashScreen: {
            launchShowDuration: 3000,
            launchAutoHide: false,
            backgroundColor: '#ffffffff',
            androidSplashResourceName: 'splash',
            androidScaleType: 'CENTER_CROP',
            showSpinner: false,
            androidSpinnerStyle: 'large',
            iosSpinnerStyle: 'small',
            spinnerColor: '#999999',
            splashFullScreen: true,
            splashImmersive: true,
        },
    },
};

export default config;
