# Loan Application Service

A REST API for loan origination: users sign up, complete KYC (BVN, NIN, address), and apply for loans; loan admins review pending applications and approve or reject them, with the applicant notified by email.

Built on .NET 10 minimal APIs with a Clean Architecture layout, CQRS via MediatR, PostgreSQL, and JWT (RS256) authentication.

## Tech stack

| Concern | Choice |
| --- | --- |
| Runtime | .NET 10, ASP.NET Core minimal APIs |
| Database | PostgreSQL — EF Core (writes, migrations) + Dapper (reads) |
| Identity & auth | ASP.NET Core Identity, JWT bearer signed with RSA (RS256), refresh tokens |
| Application layer | MediatR (CQRS), FluentValidation pipeline behaviour |
| Email | MailKit over SMTP (StartTLS), sent from an in-process background queue |
| API docs | OpenAPI + Scalar UI, URL-segment API versioning (`Asp.Versioning`) |
| Logging | Serilog (configured from app settings) + SerilogTimings |
| Packaging | Central package management (`Directory.Packages.props`), Dockerfile |

## Solution layout

```
LoanApplication.sln
├── LoanApplication.Domain          Entities, enums, role constants. No dependencies.
├── LoanApplication.Application     Use cases (Features/*), contracts, abstractions, validation.
├── LoanApplication.Infrastructure  EF Core/Dapper persistence, migrations, JWT, email, security services.
└── LoanApplication.Presentation    Minimal API endpoints, exception handling, Program.cs, Dockerfile.
```

Dependencies point inward: `Presentation → Infrastructure → Application → Domain`.

Each use case lives in one file under `LoanApplication.Application/Features/<Area>/{Commands,Queries}` containing its `Command`/`Query`, `Handler`, and `Validator`. Endpoint classes implement `IEndpoints` and are discovered and mapped automatically at startup.

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL
- An SMTP account (e.g. a Gmail app password) for outgoing email
- An RSA key pair for signing JWTs

### 1. Configure

No `appsettings.json` is committed. Create `LoanApplication.Presentation/appsettings.Development.json` (or supply the same keys through user secrets / environment variables) with this shape:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=loan_application;Username=postgres;Password=<password>"
  },
  "Security": {
    "ClientId": "<client-id>",
    "Authentication": {
      "MaxFailedAttempts": 3,
      "BaseLockoutMinutes": 5,
      "LockoutMultiplier": 2,
      "MaxLockoutMinutes": 60
    },
    "Jwt": {
      "Issuer": "<issuer>",
      "Audience": "<audience>",
      "PrivateKey": "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----",
      "PublicKey": "-----BEGIN PUBLIC KEY-----\n...\n-----END PUBLIC KEY-----",
      "ExpireMinutes": 15,
      "RefreshTokenExpireDays": 7
    },
    "Encryption": {
      "Algorithm": "AES-256-GCM",
      "Keys": [
        { "KeyId": "enc-v1", "Secret": "<base64 32-byte key>", "IsActive": true }
      ]
    },
    "Hashing": {
      "Algorithm": "HMAC-SHA256",
      "Keys": [
        { "KeyId": "hash-v1", "Secret": "<base64 key>", "IsActive": true }
      ]
    }
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "SenderName": "Loan Applications",
    "SenderEmail": "<sender@example.com>",
    "AppPassword": "<smtp-app-password>",
    "ConfirmEmailEndpoint": "https://<frontend>/confirm-email?"
  },
  "ApiEndpoints": {
    "ResetPasswordEndpoint": "https://<frontend>/reset-password?"
  },
  "LoanTenure": {
    "InterestRate": 5
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [ { "Name": "Console" } ]
  }
}
```

Notes:

- `ConfirmEmailEndpoint` and `ResetPasswordEndpoint` must end with `?` (or `&`) — the service appends `email=...&token=...` directly to them.
- Exactly one key in each `Keys` array must have `IsActive: true`; `KeyId`s must be unique. Encryption keys must decode to 32 bytes.
- `LoanTenure:InterestRate` is a percentage shown in the loan approval email.

Generate the key material with:

```bash
# JWT signing key pair
openssl genrsa -out private.pem 2048
openssl rsa -in private.pem -pubout -out public.pem

# Encryption / hashing secrets
openssl rand -base64 32
```

Keep real secrets out of source control — `.env` files are git-ignored, but `appsettings*.json` is not.

### 2. Run

```bash
dotnet restore
dotnet run --project LoanApplication.Presentation --launch-profile https
```

On startup the app applies pending EF Core migrations and seeds roles and test accounts, so the database only needs to exist and be reachable.

| | URL |
| --- | --- |
| API (https profile) | `https://localhost:7279` |
| API (http profile) | `http://localhost:5205` |
| Scalar API reference | `/scalar` |
| OpenAPI documents | `/openapi/v1.json`, `/openapi/v2.json` |

### Seeded accounts

Created on first run for local development — change or remove them before deploying anywhere real.

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@example.com` | `Admin@123` |
| LoanAdmin | `loanadmin@example.com` | `LoanAdmin@123` |
| User | `user@example.com` | `User@123` |

### Docker

Build from the repository root (the Dockerfile expects the root as its context):

```bash
docker build -f LoanApplication.Presentation/Dockerfile -t loan-application-api .

docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=<db-host>;Database=loan_application;Username=postgres;Password=<password>" \
  -e Security__Jwt__Issuer="<issuer>" \
  loan-application-api
```

Pass the remaining settings the same way, using `__` as the section separator (for example `Email__Host`, `Security__Encryption__Keys__0__Secret`).

## API

All routes are versioned in the URL: `/api/v{version}/...`. Protected routes expect `Authorization: Bearer <access token>`.

### Auth — `/api/v1/auth` (also served under `/api/v2/auth`)

| Method | Route | Access | Description |
| --- | --- | --- | --- |
| POST | `/sign-up` | Anonymous | Register a user; sends a confirmation email |
| POST | `/confirm-email` | Anonymous | Confirm email with the emailed token |
| POST | `/sign-in` | Anonymous | Returns access and refresh tokens plus the user's roles |
| POST | `/refresh-token` | Anonymous | Exchange an (expired) access token + refresh token for a new pair |
| POST | `/change-password` | Authenticated | Change the signed-in user's password |
| POST | `/forgot-password` | Anonymous | Email a password reset link |
| POST | `/reset-password` | Anonymous | Reset the password with the emailed token |

### KYC — `/api/v1/kyc`

| Method | Route | Access | Description |
| --- | --- | --- | --- |
| POST | `/add-bvn` | Authenticated | Attach an 11-digit BVN to the user's profile |
| POST | `/add-nin` | Authenticated | Attach a NIN to the user's profile |
| POST | `/add-address` | Authenticated | Attach an address to the user's profile |

### Loan application — `/api/v1/loan-application`

| Method | Route | Access | Description |
| --- | --- | --- | --- |
| POST | `/` | Authenticated | Apply for a loan (`amount` ≥ 5,000, `tenure` > 0, `purpose` ≤ 500 chars) |

### Loan application admin — `/api/v1/loan-application-admin`

| Method | Route | Access | Description |
| --- | --- | --- | --- |
| GET | `/get-pending-loan-applications` | `LoanAdmin` | Paginated pending applications. Query: `pageNumber`, `pageSize`, `startDate`, `endDate` |
| POST | `/review-loan-application` | `LoanAdmin` | Approve or reject an application; a comment is required when rejecting. Emails the applicant |

### Example

```bash
# Sign in
curl -s http://localhost:5205/api/v1/auth/sign-in \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"User@123"}'

# Apply for a loan
curl -s http://localhost:5205/api/v1/loan-application \
  -H "Authorization: Bearer <accessToken>" \
  -H "Content-Type: application/json" \
  -d '{"amount":50000,"tenure":6,"purpose":"Working capital"}'
```

### Responses and errors

Successful calls return an envelope:

```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "Completed successfully",
  "errors": [],
  "data": {}
}
```

Failures are returned as [RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807) `application/problem+json` by the global exception handler: validation failures as `ValidationProblemDetails` (400), domain errors (`ApiException`) with their own status code, and anything else as a generic 500.

## Security behaviour

- **Passwords** — minimum 8 characters with upper case, lower case, digit, and special character.
- **Progressive lockout** — after `MaxFailedAttempts` failed sign-ins the account locks for `BaseLockoutMinutes × LockoutMultiplier^(lockouts − 1)`, capped at `MaxLockoutMinutes`.
- **Tokens** — access tokens are RS256-signed with zero clock skew; refresh tokens are random 32-byte values stored in the database.
- **Roles** — `Admin`, `LoanAdmin`, `User`. New sign-ups get `User`.
- **KYC data** — BVN/NIN values are encrypted with AES-256-GCM before they are stored, alongside the id of the key used so keys can be rotated. An HMAC-SHA256 hash of each value is stored too, so duplicates can be detected through unique indexes without decrypting. A masking service is available for display.

## Database migrations

Migrations live in `LoanApplication.Infrastructure/Migrations` and are applied automatically at startup. To add one:

```bash
dotnet ef migrations add <Name> \
  --project LoanApplication.Infrastructure \
  --startup-project LoanApplication.Presentation
```

This needs the EF Core CLI (`dotnet tool install --global dotnet-ef`) and a valid configuration, since the startup project is used to build the `DbContext`.

## Known gaps

- Refresh tokens are not yet marked used/revoked when exchanged.
- The v2 auth endpoints currently mirror v1.
- `weather-forecast` is a leftover template endpoint.
- There is no test project yet.
