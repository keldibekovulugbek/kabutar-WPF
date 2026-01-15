# WPF Kabutar - Complete Refactor Plan

## PHASE 0: Current State Analysis

### ✅ What EXISTS and WORKS:
```
kabutar-WPF/
├── Core/
│   ├── ObservableObject.cs      ✅ Clean MVVM base class
│   ├── RelayCommand.cs          ✅ ICommand implementation
│   └── NavigationService.cs     ✅ Page navigation service
├── Converters/
│   └── BoolToVisibilityConverter.cs  ✅ Value converter
├── Views/                        ✅ Empty folder (ready)
├── App.xaml                      ✅ Configured with themes
├── App.xaml.cs                   ✅ Clean entry point
└── Kabutar-WPF.csproj           ✅ Has SignalR + Newtonsoft packages
```

### ❌ What's MISSING:
- Models/ folder and all DTOs
- Services/ folder (ApiService, AuthService)
- ViewModels/ folder (all ViewModels)
- Resources/ folder (Themes, Styles)
- All Views (Login, Register, VerifyEmail, ForgotPassword)

### 🎯 Backend API Endpoints (Confirmed Running):
```
POST /api/account/register
  Body: { firstname, lastname, email, username, password }

POST /api/account/login
  Body: { usernameOrEmail, password }
  Response: { "token": "JWT_TOKEN" }

POST /api/account/verify-email
  Body: { email, code }

POST /api/account/send-code
  Body: { email }

POST /api/account/reset-password
  Body: { email, code, newPassword }
```

---

## REFACTOR STRATEGY

### ✅ KEEP (No Changes Needed):
1. `Core/ObservableObject.cs` - Perfect MVVM implementation
2. `Core/RelayCommand.cs` - Clean ICommand with generic support
3. `Core/NavigationService.cs` - Singleton navigation
4. `Converters/BoolToVisibilityConverter.cs` - Standard converter
5. `App.xaml` - Already configured correctly
6. `Kabutar-WPF.csproj` - Has all required packages

### ⚠️ MODIFY:
1. `App.xaml.cs` - Add global exception handling

### ➕ CREATE (NEW):

#### Models/
```
Models/
├── Auth/
│   ├── LoginRequest.cs
│   ├── LoginResponse.cs
│   ├── RegisterRequest.cs
│   ├── VerifyEmailRequest.cs
│   ├── SendCodeRequest.cs
│   └── ResetPasswordRequest.cs
└── User.cs
```

#### Services/
```
Services/
├── ApiClient.cs           # HttpClient wrapper with JWT
├── IAuthService.cs        # Auth interface
└── AuthService.cs         # Auth implementation
```

#### ViewModels/
```
ViewModels/
├── Auth/
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   ├── VerifyEmailViewModel.cs
│   └── ForgotPasswordViewModel.cs
└── MainViewModel.cs       # Post-auth placeholder
```

#### Views/
```
Views/
├── Auth/
│   ├── LoginView.xaml/.cs
│   ├── RegisterView.xaml/.cs
│   ├── VerifyEmailView.xaml/.cs
│   └── ForgotPasswordView.xaml/.cs
└── MainView.xaml/.cs      # Post-auth placeholder
```

#### Resources/
```
Resources/
├── Themes/
│   ├── LightTheme.xaml
│   └── DarkTheme.xaml
└── Styles/
    ├── ButtonStyles.xaml
    └── TextBoxStyles.xaml
```

### ❌ DELETE:
- `tmpclaude-*-cwd` files (temporary)
- `App.xaml.backup`
- `nul` file
- `WPF_REBUILD_PLAN.md` (superseded by this)

---

## IMPLEMENTATION ORDER

### PHASE 1: Clean & Setup (15 min)
1. Delete temporary files
2. Create folder structure
3. Create resource dictionaries (themes, styles)
4. Ensure project builds

### PHASE 2: Infrastructure (30 min)
1. Create all Model classes (DTOs)
2. Implement ApiClient with JWT support
3. Implement AuthService
4. Add token storage (Settings or SecureStorage)
5. Test backend connectivity

### PHASE 3: Login Flow (45 min)
1. Create LoginViewModel with validation
2. Create LoginView.xaml (Figma design)
3. Wire up login → backend → token storage
4. Test login success → navigate to Main
5. Test login failure → show error

### PHASE 4: Register Flow (45 min)
1. Create RegisterViewModel with validation
2. Create RegisterView.xaml
3. Wire up register → verify email navigation
4. Create VerifyEmailViewModel
5. Create VerifyEmailView.xaml
6. Test full register → verify → login flow

### PHASE 5: Password Reset (30 min)
1. Create ForgotPasswordViewModel
2. Create ForgotPasswordView.xaml
3. Wire up forgot password → send code → reset → login
4. Test full reset flow

### PHASE 6: Polish & Testing (30 min)
1. Add loading indicators
2. Add error messages
3. Add input validation
4. Test all edge cases
5. Test navigation in all directions

---

## TECHNICAL DECISIONS

### Token Storage:
- Use `Application.Current.Properties["AuthToken"]` for simplicity
- Alternative: Windows Credential Manager (overkill for diploma)

### Navigation Pattern:
- Window-based navigation (new window per screen)
- Close previous window on navigation
- Justification: Simpler than Page/Frame for auth flow

### Error Handling:
- Try/catch in ViewModels
- Display errors in UI via ErrorMessage property
- No global error dialog (better UX)

### Validation:
- Client-side validation in ViewModels
- Backend validation errors also displayed
- Real-time validation on property change

### Theme Toggle:
- Light/Dark theme files ready
- Toggle button in title bar
- Persisted theme preference

---

## CONFIRMATION REQUIRED

**FILES TO DELETE:**
- `tmpclaude-*-cwd` (28 files)
- `App.xaml.backup`
- `nul`
- `WPF_REBUILD_PLAN.md`

**FOLDERS TO CREATE:**
- Models/Auth/
- Services/
- ViewModels/Auth/
- Views/Auth/
- Resources/Themes/
- Resources/Styles/

**PROCEED WITH PLAN?**
- ✅ YES → Execute PHASE 1
- ❌ NO → Adjust plan based on feedback

---

**Estimated Total Time:** 3-4 hours
**Deliverable:** Fully working auth flow (Login, Register, Verify, Reset)
**Stop Condition:** User successfully logs in and reaches MainView
