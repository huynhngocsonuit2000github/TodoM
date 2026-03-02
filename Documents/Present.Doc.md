# Problem

- Multiple apps -> multiple login
  - each app need handles:
    - passwords
    - sessions
    - user storage
  - scaling = sync users, sessions, security logic
  - High risk
  - handle user pool sync
- Risk of security with password storing
- Hard to manage user & roles

# Simple SSO Concept

- Login once -> access many apps
- IdP does the authentication

# SSO architecture

- User
- App
- IdP (Azure Entra ID/ B2C)
- Token: ID Token, Access Token

# Common SSO flows (important)

- Authorization Code Flow (most common for web apps)
- Client Credentials Flow (service → service)
- On-Behalf-Of (OBO) (API calls API)

# Real-world use cases

- Internal apps (employees)
- Microservice (machine to machine)

# End with benefits

- Better UX
- Centralized security
- Easier scaling
