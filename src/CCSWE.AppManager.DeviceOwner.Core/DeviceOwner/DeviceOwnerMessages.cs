namespace CCSWE.AppManager.DeviceOwner.Core.DeviceOwner;

/// <summary>
/// User-facing copy for the device-owner pre-flight blockers and the runtime error mapper, kept in one place so
/// the two stay consistent and so the wording can be diffed against the ccswe.com guidance. Strings that mirror
/// the website are noted; the rest are app-authored where the site has no equivalent.
/// </summary>
public static class DeviceOwnerMessages
{
    // Mirrors ccswe.com.
    public const string AccountsPresent =
        "There are still accounts that need to be removed from the device. If you have already removed all your " +
        "Google accounts then check for device manufacturer accounts (e.g. Samsung) and temporarily remove those. " +
        "Keep removing accounts until you are successful.";

    // Reflects ccswe.com ("Device is already provisioned"); the most common cause is lingering accounts.
    public const string AlreadyProvisioned =
        "The device reports it is already provisioned. The most common cause is one or more accounts still on the " +
        "device — remove all accounts and try again.";

    // App-authored: the site has no "not installed" copy.
    public const string AppNotInstalled =
        "App Manager isn't installed on this device. Install App Manager from Google Play, then try again.";

    // Mirrors ccswe.com.
    public const string DeviceOwnerByOther =
        "There is already an application with device owner on the device. If this application is not App Manager " +
        "(Device Owner) then you will need to uninstall it before you can activate device owner.";

    // App-authored: the site has no "offline/unauthorized" copy.
    public const string NotConnected =
        "The device is offline or hasn't authorized USB debugging. Reconnect it, accept the debugging prompt on " +
        "the device, and try again.";

    // App-authored: rare profile-owner-on-system-user case.
    public const string ProfileOwner =
        "This user already has a profile owner, which prevents setting the device owner. Remove it and try again.";

    // App-authored, reflecting the ccswe.com Samsung Galaxy S8 timeout note.
    public const string Timeout =
        "The command didn't finish. Some devices don't report an error when non-Google accounts are present — " +
        "remove more accounts and try again.";

    // Mirrors ccswe.com.
    public const string Users =
        "An application on your device, such as Samsung's Secure Folder, has already created additional user " +
        "profiles. You will need to disable these applications prior to enabling device owner.";
}
