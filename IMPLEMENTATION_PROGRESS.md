# FamilyPills Implementation Progress

Last updated: 2026-06-01

## Current Status

The backend and frontend are partially integrated for Feature 1 Authentication. The database setup script has been expanded so a fresh local MySQL database can support the current app and the next feature work.

## Database

### Done

- Added `users` table for authentication and profile data.
- Added `refresh_tokens` table for future refresh-token persistence and logout invalidation.
- Added `medicines` table with optional `user_id` foreign key support.
- Added `medicine_images` table for uploaded image metadata.
- Added `medicine_inventory_logs` table for future quantity/history tracking.
- Added `user_settings` table for future per-user thresholds.
- Made seed medicine inserts idempotent by barcode.
- Added helper procedures in `create-database.sql` so indexes/columns can be added safely when the script is re-run.
- Backend startup now creates the core tables if they are missing, which helps local development when MySQL CLI is not available.

### Still Needed

- Decide whether to use EF Core migrations or keep manual SQL scripts. Do not mix both casually.
- If using existing databases, verify foreign keys on old `medicines` tables because `CREATE TABLE IF NOT EXISTS` will not retrofit constraints on an already-created table.
- Add `user_id` assignment in medicine APIs after Feature 2 auth scoping is implemented.

## Feature 1: Authentication

### Backend Done

- `User` model.
- `Users` DbSet and unique email mapping.
- BCrypt password hashing.
- JWT token generation.
- Auth endpoints:
  - `POST /api/auth/register`
  - `POST /api/auth/login`
  - `POST /api/auth/logout`
  - `POST /api/auth/refresh`
- Swagger JWT bearer configuration.
- Local HTTP development flow for Android by removing HTTPS redirect from the dev API path.

### Frontend Done

- `AuthRepository`.
- `AuthViewModel`.
- Login screen calls backend API.
- Register screen calls backend API.
- JWT saved in `SharedPreferences`.
- Session guard in `MainActivity`.
- Logout clears saved session.
- Retrofit supports:
  - Emulator: `http://10.0.2.2:5000/`
  - Physical device via USB reverse: `http://127.0.0.1:5000/`

### Still Needed

- Improve Vietnamese UI/error strings. Some existing XML text appears mojibake/encoding-corrupted.
- Show loading indicators instead of only disabling buttons.
- Parse backend error bodies when HTTP status is not 2xx; Retrofit currently only reads `response.body()`.
- Decide token expiry policy. Current backend uses `Jwt:ExpiresInMinutes = 60`, while the API spec mentions 24 hours.
- Persist refresh tokens if real refresh/logout invalidation is required.

## Feature 2: Cabinet / Medicine Management

### Backend Done

- Require JWT authorization on medicine endpoints.
- Scope medicine queries by authenticated `user_id`.
- Add pagination response shape: `items`, `totalCount`, `pageNumber`, `pageSize`, `totalPages`.
- Add search by name/barcode.
- Add filters: all, running low, expired.
- Check barcode uniqueness per user.
- `GET /api/medicines/stats` — per-user stats (total, running low, expired, expiring soon within 30 days).
- `GET /api/medicines/validate-barcode/{barcode}` — check if barcode exists for current user.
- `POST /api/medicines/upload-image` — upload medicine image (max 5MB, saved to `wwwroot/uploads/medicines/`).
- `app.UseStaticFiles()` enabled in `Program.cs` for serving uploaded images.

### Frontend Done

- `MedicineRepository` wraps all medicine API calls including CRUD, stats, barcode validation, and image upload.
- `CabinetFragment` loads medicine list from API with correct English filter constants (`all`, `runningLow`, `expired`).
- All three filter buttons wired: "Đang dùng" (all), "Sắp hết" (runningLow), "Hết hạn" (expired).
- Loading/error state observation in `CabinetFragment` (ProgressBar + Toast).
- `onResume()` reload in `CabinetFragment` so newly added/edited medicines appear immediately.
- Delete medicine calls API and refreshes list.
- Edit medicine navigates to `AddMedicineActivity` with `medicine_id` extra.

## Feature 3: Add/Edit Medicine, Image, Barcode

### Backend Done

- `POST /api/medicines/upload-image` endpoint with file validation (max 5MB, image/* content type).
- `GET /api/medicines/validate-barcode/{barcode}` endpoint.
- Image storage under `wwwroot/uploads/medicines/`.

### Frontend Done

- `AddMedicineInfoFragment` form collects name, barcode, quantity, unit, expiry date, and image.
- Save button calls `addMedicine()` API with validation (name required, quantity >= 0).
- Edit mode loads existing medicine data from `getMedicineById()` API and populates form.
- Edit save calls `updateMedicine()` API.
- Button disabled during API call, re-enabled on response.
- CameraX barcode scanning via ML Kit returns result to form.
- CameraX photo capture returns image path to form.

### Still Needed

- Wire captured image upload to `uploadMedicineImage()` API before saving medicine.
- Wire barcode scan result to `validateBarcode()` API for duplicate warning.

## Feature 4: Home / Dashboard

### Backend Done

- `GET /api/medicines/stats` endpoint returns per-user totals.
- Calculates total medicines, running low count, expired count.
- Calculates expiring-soon count (ExpiryDate within 30 days, parsed with multiple date formats).

### Frontend Done

- `HomeViewModel.loadDashboardData()` calls `getStats()` API for real dashboard numbers.
- Recent medicines list loaded from `getAllMedicines()` API (top 4 items).
- `onResume()` reload in `HomeFragment` so dashboard updates after adding/editing medicines.
- Hardcoded mock data replaced with live API data.

## Feature 5: User Profile

### Currently Present

- Android Profile UI/ViewModel/Repository skeleton exists.
- User profile DTO exists in backend.

### Still Needed

- Backend `GET /api/users/profile`.
- Backend `PUT /api/users/profile`.
- Backend `POST /api/users/change-password`.
- Profile should return authenticated user data and medicine count.
- Change password should verify current password and hash the new password.
- Update `last_login` on successful login if profile should display it.

## Local Test Notes

### Backend

Run backend on HTTP port 5000:

```powershell
dotnet run --project FamilyPillsAPI\FamilyPillsAPI.csproj --launch-profile http
```

### Android Emulator

The app should call:

```text
http://10.0.2.2:5000/
```

### Physical Android Device via USB

Run this after plugging in the phone or reconnecting ADB:

```powershell
D:\AppData\Local\Android\Sdk\platform-tools\adb.exe reverse tcp:5000 tcp:5000
```

The app should call:

```text
http://127.0.0.1:5000/
```

## Immediate Next Steps

1. Run `create-database.sql` against local MySQL to make sure all tables exist.
2. Retest register/login from physical device.
3. Test full Cabinet flow: add medicine → verify in list → edit → delete.
4. Test Home dashboard shows real stats from API.
5. Implement Feature 5 profile endpoints next, because Profile UI already calls those APIs after login.
6. Wire image upload and barcode validation into AddMedicine flow (currently API endpoints exist but not called from the form).
