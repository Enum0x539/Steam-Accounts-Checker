# Steam-Accounts-Checker
Steam accounts checker written in C#, determines if the account is valid, invalid or steam guard protected.

---
This project is using [SteamKit2](https://github.com/SteamRE/SteamKit).

## How its works
SteamGuard works by enforcing a two-factor authentication scheme
upon first logon to an account with SG enabled, the Steam server will email an auth code to the validated address of the account
this auth code token can be used as the second factor during logon, but the token has a limited period in which it is valid

after a client logs on using the auth code, the steam server will generate a blob of random data that the client stores called a "sentry file"
this sentry file is then used in all subsequent logons as the second factor
ownership of this file provides proof that the machine being used to logon is owned by the client in question

the usual login flow is this:
1. connect to the server
2. logon to account with only username and password
at this point, if the LoggedOnCallback callback is OK the account is valid if the callback is AccountLogonDenied the account is steam guard protected,
the server will disconnect the client and email the auth code; if the callback is not OK and not AccountLogonDenied the account is invalid;
