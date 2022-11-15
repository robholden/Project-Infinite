using System.ComponentModel;

namespace Library.Core;

public enum ErrorCode
{
    [Description("An unexpected error has occurred")]
    Default,

    [Description("Cannot find {0}")]
    MissingEntity,

    [Description("Either your username or password is incorrect")]
    IncorrectUsernameOrPassword,

    [Description("Your password is incorrect")]
    IncorrectPassword,

    [Description("Your account has been disabled")]
    AccountDisabled,

    [Description("Your account has been locked")]
    AccountLocked,

    [Description("Your session has expired or been revoked")]
    SessionHasExpired,

    [Description("Your email must be confirmed to do this")]
    EmailConfirmationRequired,

    [Description("Two-factor verification failed")]
    TwoFactorAttemptFailed,

    [Description("The provided code is invalid")]
    ProvidedCodeInvalid,

    [Description("The provided key has been used, expired or is invalid")]
    ProvidedKeyInvalid,

    [Description("The provided key has been used, expired or is invalid")]
    KeyExpiredOrInvalid,

    [Description("A code can only be used once")]
    CannotReUseCode,

    [Description("This account has Two Factor authentication already enabled")]
    TwoFactorAlreadyEnabled,

    [Description("This email address is already in use")]
    EmailInUse,

    [Description("This username has been taken")]
    UsernameTaken,

    [Description("This username is invalid")]
    InvalidUsername,

    [Description("The given two factor type is unset")]
    TwoFactorTypeUnset,

    [Description("This email address has already been register by an external provider")]
    EmailRegisteredByExternalProvider,

    [Description("Invalid 'UserIdentifier' claims")]
    InvalidUserClaims,

    [Description("You do not have permission to perform this action")]
    MissingPermissions,

    [Description("Missing or invalid: {0}")]
    InvalidModelRequest,

    [Description("ReCaptcha validation failed")]
    ReCaptchaFailed,

    [Description("Cannot not find entity")]
    EntityNotFound,

    [Description("Cannot update entity with another one")]
    EntitiesAreNotTheSame,

    [Description("You are not permitted to upload any picture at this time")]
    UploadNotAllowed,

    [Description("You have reached the daily upload limit")]
    UploadLimitReached,

    [Description("Duplicate picture detected")]
    DuplicatePicture,

    [Description("Dangerous picture detected")]
    DangerousPicture,

    [Description("Failed to find location")]
    NoLocationFound,

    [Description("Picture has been modified. Refresh to get the latest version")]
    PictureModified,

    [Description("Picture has already been submitted")]
    PictureAlreadySubmitted,

    [Description("Picture has already been published")]
    PictureAlreadyLive,

    [Description("Picture has already been processed")]
    PictureAlreadyProcessed,

    [Description("The given path does not exist")]
    PathNotFound,

    [Description("You can only have {0} drafts at any one time")]
    DraftLimitReached,

    [Description("You only have {0} draft uploads remaining")]
    UploadWouldExceedDraftLimit,

    [Description("Please upload a picture")]
    PictureRequired,

    [Description("Failed to read geodata")]
    FailedToDecodeJson,

    [Description("Failed to read geodata")]
    FailedToLookupCoords,

    [Description("Failed to read geodata")]
    FailedToGetCountry,

    [Description("An SMS has already been sent")]
    SmsAlreadySent,

    [Description("Failed to send SMS: {0}")]
    SmsFailedToSend,

    [Description("Report already actioned")]
    ReportAlreadyActioned,

    [Description("Picture extension is invalid")]
    InvalidPictureExtension,

    [Description("Picture size must not exceed {0}mb")]
    PictureTooBig,

    [Description("Picture dimensions are too small")]
    PictureDimensionsTooSmall,

    [Description("No location could found in picture metadata")]
    MissingExifLocation,

    [Description("No timestamp could found in picture metadata")]
    MissingExifTimestamp,

    [Description("Token invalid, access not available")]
    TokenInvalid,

    [Description("This account is connected with a different platform")]
    WrongExternalProvider,

    [Description("Failed to login with {0}")]
    ExternalProviderLoginFailed,

    [Description("A verified email is required")]
    ExternalProviderUnverifiedEmail,

    [Description("Your session token is invalid")]
    SessionTokenInvalid
}