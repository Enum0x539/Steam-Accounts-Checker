# 🎮 Steam Accounts Checker

    “Is it valid? Is it protected? Or is it just... sad?”
    A C# application that checks the status of a list of Steam accounts:
    
## 🧰 Tech Stack

- 🔧 **C# (.NET Framework)**
- 🧠 Powered by [SteamKit2](https://github.com/SteamRE/SteamKit), the official low-level Steam protocol library

---

## 🚀 How It Works

Steam accounts are checked via actual login attempts using the **SteamKit2** library. Each combo (username:password) is validated and categorized based on the Steam server’s response:

### 🔄 Login Flow

1. 🧠 **Connect to Steam**
2. 🔑 **Attempt login** with username and password only
3. 🧪 **Receive server response:**

| Result               | Meaning                            |
|----------------------|------------------------------------|
| `EResult.OK`         | ✅ Valid credentials               |
| `AccountLogonDenied` | 🔐 Steam Guard is enabled          |
| Anything else        | ❌ Invalid credentials             |

If Steam Guard is enabled, Steam will email the account owner a 2FA token — **but** this tool doesn't try to bypass it (because we do things the legal and ethical way 😇).

---

## 📁 File Input

- You must have a file named **`Accounts.txt`** in the app's root directory
- Each line should be in the format: `username:password`
- Duplicates and malformed lines are automatically filtered

---

## 📊 Output

After running the checker, results are saved to the `Output/` directory:

- ✅ **Valid.txt**
- 🔐 **Protected.txt** (Steam Guard)
- ❌ **Invalid.txt**

Each result includes:
- Username
- Password
- SteamID (when available)

---

## 🤓 How It REALLY Works (Advanced Explanation for the Brave)

SteamGuard is basically Valve’s way of saying, **“No bots allowed, human only zone!”** It enforces a **two-factor authentication (2FA)** scheme with some neat cryptographic wizardry behind the scenes:

1. **First-Time Logon with SteamGuard Enabled:**  
   When you try logging in with your username and password for the first time on a new device, Steam throws you a curveball — instead of letting you in, it shoots an **auth code** to your verified email.  
   This code is your golden ticket — a temporary token that acts as the **second factor** in the login process. But beware, it’s got an expiry timer ticking down like a game of Minesweeper.

2. **Sentry File Magic:**  
   After you punch in the auth code successfully, Steam’s servers generate a mysterious blob of bytes called the **sentry file** and hand it over to your client.  
   This sentry file is your VIP pass for all future logins from that device. It proves to Steam’s servers, “Hey, it’s me, the legit owner!” and skips the email code dance every time.

3. **Standard Login Flow:**  
   Here’s how the login saga usually unfolds:

   - **Connect** to Steam’s servers (like knocking on the door).  
   - **Send username & password** (your secret handshake).  
   - **Wait for Steam’s reply:**

     | Callback Result           | Meaning                                 | What You Do                |
     |--------------------------|-----------------------------------------|----------------------------|
     | `EResult.OK`             | Account creds legit, welcome aboard!   | Proceed with your session. |
     | `AccountLogonDenied`     | SteamGuard’s watching, code required. | Check your email, enter code. |
     | Anything else            | Credentials invalid or banned.          | Try another account, scrub! |

**Note:** The sentry file is essentially Valve’s way of telling bots: “You can’t touch this!” 🕶️💥

---

**TL;DR:** SteamGuard is like that overprotective friend who emails you a secret handshake every time you try to sneak into the party from a new phone. The sentry file is your backstage pass — keep it safe, or you gotta prove yourself all over again.

