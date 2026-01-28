# CafePOS (Windows WPF, Offline-First)

## Prerequisites
- Windows 10/11
- .NET 8 SDK
- Visual Studio 2022 (optional, for WPF designer)
- SQLite (bundled via EF Core)

## Run
```bash
# from repo root
# restore/build
 dotnet restore
 dotnet build

# run WPF app
 dotnet run --project src/CafePos.Presentation
```

On first run the app seeds a default Store/Terminal/Payment methods and shows a **first-time setup** screen to create the initial Manager account.

## Create the First Manager User
- Launch the app.
- If no users exist, the **First-time Setup** screen appears.
- Enter username, display name, and password.
- After creation, log in normally.

## Database & Migrations
The SQLite database file is created as `pos.db` in the app directory (same folder as the executable).

To manage migrations (recommended):
```bash
 dotnet tool install --global dotnet-ef
 dotnet ef migrations add InitialCreate -p src/CafePos.Infrastructure -s src/CafePos.Presentation
 dotnet ef database update -p src/CafePos.Infrastructure -s src/CafePos.Presentation
```

## Receipt Printer
Configure printer name in `src/CafePos.Presentation/appsettings.json`:
```json
{
  "Printer": {
    "Name": "Your Printer Name"
  }
}
```
If printing fails, receipts are saved to a `receipts` folder in the app directory as plain text.

## Backup
- Close the app.
- Copy `pos.db` to your backup location.
- To restore, replace `pos.db` with a backup file.

## Notes / MVP Coverage
- Core flows implemented: login, open shift, start order, add items, take payment, print receipt, close shift, view basic reports.
- CSV export buttons write files to an `exports` folder in the app directory.
- Menu setup, settings, and user management screens are placeholder UI for this MVP and can be expanded.
- Modifier selection UI is not yet implemented; the domain supports modifiers for future expansion.

## Tests
```bash
 dotnet test
```
