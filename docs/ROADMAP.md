# Roadmap

The app does one thing and does it now: set App Manager as the device owner on a connected device. This
file tracks **future** work only — completed work lives in git history and the code, not here.

## Possible future work

| Item | Notes |
| --- | --- |
| Settings UI | Expose the persisted theme/density and the `adb` path override (already in `ISettingsService`) in the desktop UI. None of it is editable in-app yet. |
| Remove device owner | A "clear device owner" action (`adb shell dpm remove-active-admin <component>`) to undo during testing. |
| Device owner status | Show whether App Manager is *already* the device owner on the selected device. |

When one of these lands, delete its row.
