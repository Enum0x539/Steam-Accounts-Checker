using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Steam_Accounts_Checker
{
    internal static class Program
    {
        private static Accounts accounts = new Accounts();

        private static void Main()
        {
            string content = "";
            if (!File.Exists("Accounts.txt"))
            {
                Console.WriteLine("Put your accounts in Accounts.txt, I'll open this file for you.\n\nPress any key to exit.");
                File.WriteAllText("Accounts.txt", "");
                Process.Start("notepad.exe", "Accounts.txt");
                Console.ReadKey();
                return;
            }
            else
            {
                content = File.ReadAllText("Accounts.txt");
                if (String.IsNullOrEmpty(content) || String.IsNullOrWhiteSpace(content))
                {
                    Console.WriteLine("Put your accounts in Accounts.txt, I'll open this file for you.\n\nPress any key to exit.");
                    Process.Start("notepad.exe", "Accounts.txt");
                    Console.ReadKey();
                    return;
                }
            }

            List<string> combos = new List<string>(File.ReadAllText("Accounts.txt").Split(Environment.NewLine.ToCharArray()));
            combos = combos.Distinct().ToList();
            combos.RemoveAll(i => String.IsNullOrEmpty(i) || String.IsNullOrWhiteSpace(i) || !i.Contains(":"));
            if (combos.Count == 0)
            {
                Console.WriteLine("Put your accounts (as combos) in Accounts.txt, I'll open this file for you.\n\nPress any key to exit.");
                Process.Start("notepad.exe", "Accounts.txt");
                Console.ReadKey();
                return;
            }

            Checker checker = new Checker();
            checker.OnAccountChecked += Checker_OnAccountChecked;
            checker.OnCheckerDone += Checker_OnCheckerDone;
            checker.CheckAccounts(ref combos);
        }

        private static void Checker_OnCheckerDone(TimeSpan time)
        {
            string ValidContent = "";
            string InValidContent = "";
            string SteamGuardProtectedContent = "";

            accounts.Vaild.ForEach(i =>
            {
                ValidContent += $"\n\n~~~~~~~~~~~~~~~~~~~~~\n" +
                                $"\nUsername: {i.Username}" +
                                $"\nPassword: {i.Password}" +
                                $"\nSteamID: {i.Id}";
            });

            accounts.InVaild.ForEach(i =>
            {
                InValidContent += $"\n\n~~~~~~~~~~~~~~~~~~~~~\n" +
                                  $"\nUsername: {i.Username}" +
                                  $"\nPassword: {i.Password}";
            });

            accounts.SteamGuardProtected.ForEach(i =>
            {
                SteamGuardProtectedContent += $"\n\n~~~~~~~~~~~~~~~~~~~~~\n" +
                                              $"\nUsername: {i.Username}" +
                                              $"\nPassword: {i.Password}" +
                                              $"\nSteamID: {i.Id}";
            });

            Console.WriteLine($"\nDone; Invaild Accounts: {accounts.InVaild.Count} | Vaild Accounts: {accounts.Vaild.Count} | Protected Accounts: {accounts.SteamGuardProtected.Count}\nSaved the accounts into \"{Application.StartupPath}\\Output\\\"");
            if (!Directory.Exists("Output"))
                Directory.CreateDirectory("Output");

            File.WriteAllText("Output\\Vaild.txt", ValidContent);
            File.WriteAllText("Output\\InVaild.txt", InValidContent);
            File.WriteAllText("Output\\Protected.txt", SteamGuardProtectedContent);

            Thread.Sleep(-1);
        }

        private static void Checker_OnAccountChecked(int Index, int Count, Account Account)
        {
            switch (Account.Type)
            {
                case Status.Fail:
                    accounts.InVaild.Add(Account);
                    break;

                case Status.Success:
                    accounts.Vaild.Add(Account);
                    break;

                case Status.SteamGuardProtected:
                    accounts.SteamGuardProtected.Add(Account);
                    break;
            }

            Console.WriteLine($"Checked {Index}/{Count}");
        }
    }
}