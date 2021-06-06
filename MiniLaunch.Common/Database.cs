using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MiniLaunch.Common
{
    public class Database
    {
        public static readonly SqliteConnection DbConnection;

        static Database()
        {
            DbConnection = new SqliteConnection("data source=\"" + Config.DatabaseFilePath + "\";");
            DbConnection.Open();
        }

        public static async Task CreateDatabase()
        {
            File.Delete(Path.Combine(Config.DataFolder, "database.dat"));

            var createAccountsTable =
@"CREATE TABLE Accounts (
    Username TEXT PRIMARY KEY,
    Password TEXT NOT NULL
);";
            var createAccountsTableCmd = new SqliteCommand(createAccountsTable, DbConnection);
            await createAccountsTableCmd.ExecuteNonQueryAsync();

            var createSubscriptionsTable =
@"CREATE TABLE Subscriptions (
    Name TEXT PRIMARY KEY,
    Username TEXT NOT NULL,
    Game INTEGER NOT NULL,
    Description TEXT NOT NULL,
    Status TEXT NOT NULL,
    FOREIGN KEY(Username) REFERENCES Accounts(Username)
);";
            var createSubscriptionsTableCmd = new SqliteCommand(createSubscriptionsTable, DbConnection);
            await createSubscriptionsTableCmd.ExecuteNonQueryAsync();
        }

        public static async Task AddSessionToDatabase(Account account)
        {
            await AddAccount(account.Username, account.Password);

            foreach (var subscription in account.Subscriptions)
            {
                await AddSubscription(subscription, account.Username);
            }
        }

        public static Task AddAccount(string username, string password)
        {
            var addAccount =
@"INSERT INTO Accounts VALUES ($username, $password)
    ON CONFLICT (Username) DO UPDATE SET Password=$password";

            var addAccountCmd = new SqliteCommand(addAccount, DbConnection);
            addAccountCmd.Parameters.AddWithValue("$username", username);
            addAccountCmd.Parameters.AddWithValue("$password", password);
            return addAccountCmd.ExecuteNonQueryAsync();
        }

        public static Task AddSubscription(Subscription sub, string username)
        {
            var addSubscription =
@"INSERT INTO Subscriptions VALUES ($name, $username, $game, $description, $status)
    ON CONFLICT (Name) DO UPDATE SET Description=$description, Status=$status";

            var addSubscriptionCmd = new SqliteCommand(addSubscription, DbConnection);
            addSubscriptionCmd.Parameters.AddWithValue("$username", username);
            addSubscriptionCmd.Parameters.AddWithValue("$name", sub.Name);
            addSubscriptionCmd.Parameters.AddWithValue("$game", sub.Game);
            addSubscriptionCmd.Parameters.AddWithValue("$description", sub.Description);
            addSubscriptionCmd.Parameters.AddWithValue("$status", sub.Status);
            return addSubscriptionCmd.ExecuteNonQueryAsync();
        }

        public static async Task DeleteSubscription(string subName, string username)
        {
            var deleteSubscription = @"DELETE FROM Subscriptions WHERE Name = $name;";

            var deleteSubscriptionCmd = new SqliteCommand(deleteSubscription, DbConnection);
            deleteSubscriptionCmd.Parameters.AddWithValue("$name", subName);

            await deleteSubscriptionCmd.ExecuteNonQueryAsync();

            var readSubs = @"SELECT * FROM Subscriptions WHERE Username = $username";

            var readSubsCmd = new SqliteCommand(readSubs, DbConnection);
            readSubsCmd.Parameters.AddWithValue("$username", username);

            var reader = await readSubsCmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                return;
            }

            var deleteAccount = @"DELETE FROM Accounts WHERE Username = $username";

            var deleteAccountCmd = new SqliteCommand(deleteAccount, DbConnection);
            deleteAccountCmd.Parameters.AddWithValue("$username", username);

            await deleteAccountCmd.ExecuteNonQueryAsync();
        }

        public static async Task<List<Tuple<string, string, string>>> GetSubscriptions()
        {
            var getSubscriptions = @"SELECT Name, Username, Description FROM Subscriptions";

            var getSubscriptionsCmd = new SqliteCommand(getSubscriptions, DbConnection);
            var reader = await getSubscriptionsCmd.ExecuteReaderAsync();

            var subscriptions = new List<Tuple<string, string, string>>();

            while (await reader.ReadAsync())
            {
                var subId = reader[0].ToString();
                var username = reader[1].ToString();
                var subDesc = reader[2].ToString();

                subscriptions.Add(new(username, subDesc, subId));
            }

            return subscriptions;
        }

        public static async Task<string> GetPassword(string username)
        {
            var getPassword = @"SELECT Password FROM Accounts WHERE Username=$username";
            var getPasswordCmd = new SqliteCommand(getPassword, DbConnection);
            getPasswordCmd.Parameters.AddWithValue("$username", username);
            var reader = await getPasswordCmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                return reader[0].ToString();
            }

            throw new Exception();
        }
    }
}