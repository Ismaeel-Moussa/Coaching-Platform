# Joker Nutrition Coaching Platform – API Reference for Frontend Team

**API Base URL**: `https://localhost:7001`  
All endpoint URLs below are relative to this base. All request and response bodies use the `application/json` format.

---

## Authentication Endpoints (`/api/auth/*`)

### 1. Login
* **URL**: `/api/auth/login`
* **Method**: `POST`
* **Authentication**: None (Public)
* **Rate Limit**: Max 10 requests per minute
* **Request Body** (`LoginForm`):
  ```json
  {
    "email": "coach@jokernutrition.com", // Required, string, valid email
    "password": "Coach@Joker123!"         // Required, string, min 6 chars
  }
  ```
* **Response Body** (`200 OK` - `AuthTokenDto`):
  ```json
  {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "7c48f8695d52...",
    "tokenType": "Bearer",
    "expiresAt": "2026-06-18T11:18:45Z", // ISO UTC Timestamp
    "user": {
      "id": 2,
      "email": "coach@jokernutrition.com",
      "firstName": "Marcus",
      "lastName": "Steel",
      "role": "Coach", // "Admin" | "Coach" | "Athlete"
      "profilePictureUrl": null // string or null
    }
  }
  ```

---

### 2. Register User via Invitation Token
* **URL**: `/api/auth/register`
* **Method**: `POST`
* **Authentication**: None (Public)
* **Request Body** (`RegisterForm`):
  ```json
  {
    "invitationToken": "28f6c479e515446f9a3e2068d1...", // Required, Guid/string
    "firstName": "Test",                               // Required, string (max 100)
    "lastName": "Athlete",                             // Required, string (max 100)
    "password": "Athlete@Joker123!",                   // Required, string (min 8)
    "confirmPassword": "Athlete@Joker123!"              // Required, must match password
  }
  ```
* **Response Body** (`201 Created` - `AuthTokenDto`):
  Returns a logged-in session identical to the **Login** response (includes `accessToken`, `refreshToken`, and `user` object).

---

### 3. Refresh Access Token
* **URL**: `/api/auth/refresh`
* **Method**: `POST`
* **Authentication**: None (Public)
* **Request Body** (`RefreshTokenForm`):
  ```json
  {
    "refreshToken": "7c48f8695d52..." // Required, string
  }
  ```
* **Response Body** (`200 OK` - `AuthTokenDto`):
  Returns a newly rotated token pair identical to the **Login** response (includes new `accessToken` and rotated `refreshToken`).

---

### 4. Forgot Password
* **URL**: `/api/auth/forgot-password`
* **Method**: `POST`
* **Authentication**: None (Public)
* **Rate Limit**: Max 3 requests per hour
* **Request Body** (`ForgotPasswordForm`):
  ```json
  {
    "email": "athlete@jokernutrition.com" // Required, string, valid email
  }
  ```
* **Response Body** (`200 OK`):
  ```json
  {
    "message": "If the email exists, a reset link has been sent."
  }
  ```
  *(Note: For security, this endpoint does not reveal whether the email exists in the database).*

---

### 5. Reset Password
* **URL**: `/api/auth/reset-password`
* **Method**: `POST`
* **Authentication**: None (Public)
* **Request Body** (`ResetPasswordForm`):
  ```json
  {
    "token": "7c48f8695d52...",           // Required, token received in reset email
    "newPassword": "NewPassword123!",     // Required, string (min 8)
    "confirmPassword": "NewPassword123!"  // Required, must match newPassword
  }
  ```
* **Response Body** (`200 OK`):
  ```json
  {
    "message": "Password has been reset successfully."
  }
  ```

---

## Invitation Endpoints (`/api/invitations/*`)

> [!NOTE]
> All endpoints in this section (except **Validate Token**) require passing the JWT in the headers:  
> `Authorization: Bearer <accessToken>`

### 1. List Invitations (Paginated)
* **URL**: `/api/invitations`
* **Method**: `GET`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **Request Query Parameters**:
  - `page` (integer, optional, defaults to 1)
  - `pageSize` (integer, optional, defaults to 10)
* **Response Body** (`200 OK` - `PagedResult<InvitationDto>`):
  ```json
  {
    "items": [
      {
        "id": 1,
        "email": "testathlete@jokernutrition.com",
        "token": "28f6c479e515446f9a3e206...",
        "role": "Athlete", // "Athlete" | "Coach" | "Admin"
        "status": 0,       // Enum (see mapping below)
        "expiresAt": "2026-06-18T11:18:45Z",
        "createdAt": "2026-06-17T11:18:45Z",
        "inviteUrl": "http://localhost:5173/register?token=28f6c479e515..."
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 10
  }
  ```

---

### 2. Create and Send Invitation
* **URL**: `/api/invitations`
* **Method**: `POST`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **Request Body** (`CreateInvitationForm`):
  ```json
  {
    "email": "newuser@jokernutrition.com", // Required, string, valid email
    "role": "Athlete",                    // Required, "Athlete" | "Coach" | "Admin"
    "expiryHours": 72                     // Optional, integer, defaults to 72 (hours)
  }
  ```
* **Response Body** (`201 Created` - `InvitationDto`):
  ```json
  {
    "id": 2,
    "email": "newuser@jokernutrition.com",
    "token": "28f6c479e515446f9a3e206...",
    "role": "Athlete",
    "status": 0,
    "expiresAt": "2026-06-20T11:18:45Z",
    "createdAt": "2026-06-17T11:18:45Z",
    "inviteUrl": "http://localhost:5173/register?token=28f6c479e515..."
  }
  ```

---

### 3. Resend Invitation
Regenerates the token, resets status to Pending, and extends expiration by another 72 hours.
* **URL**: `/api/invitations/resend/{id}` (where `{id}` is the invitation's integer ID)
* **Method**: `POST`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **Request Body**: None
* **Response Body** (`200 OK` - `InvitationDto`):
  Returns the updated invitation object with the new token, reset status, and extended expiration date.

---

### 4. Revoke Invitation
Cancels the invitation immediately.
* **URL**: `/api/invitations/{id}` (where `{id}` is the invitation's integer ID)
* **Method**: `DELETE`
* **Authentication**: Required (`Coach` or `Admin` roles)
* **Request Body**: None
* **Response Body** (`204 No Content`): Empty response indicating success.

---

### 5. Validate Invitation Token
Used by the registration page to ensure the link is valid before displaying the registration form.
* **URL**: `/api/invitations/validate/{token}` (where `{token}` is the URL token string)
* **Method**: `GET`
* **Authentication**: None (Public)
* **Response Body** (`200 OK` - `InvitationDto`):
  ```json
  {
    "id": 2,
    "email": "newuser@jokernutrition.com",
    "token": "28f6c479e515446f9a3e206...",
    "role": "Athlete",
    "status": 0,
    "expiresAt": "2026-06-20T11:18:45Z",
    "createdAt": "2026-06-17T11:18:45Z",
    "inviteUrl": "http://localhost:5173/register?token=28f6c479e515..."
  }
  ```

---

## Data Definitions

### Invitation Status Enum Mapping
The invitation status is returned as an integer representation of the C# `enum`:
* `0` = **Pending** (Active and valid)
* `1` = **Accepted** (User has registered using this invite)
* `2` = **Expired** (Token lifetime has passed)
* `3` = **Revoked** (Cancelled by Coach/Admin)
