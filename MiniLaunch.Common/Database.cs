﻿using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
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
            var createAccountsTable =
@"CREATE TABLE Accounts (
    Username TEXT PRIMARY KEY,
    Password TEXT NOT NULL,
    Ticket TEXT NOT NULL
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

        public static async Task AddSessionToDatabase(Session session, string password)
        {
            await AddAccount(session.Username, password, session.Ticket);

            foreach (var subscription in session.Subscriptions)
            {
                await AddSubscription(subscription, session.Username);
            }
        }

        public static Task AddAccount(string username, string password, string ticket)
        {
            var addAccount = @"INSERT INTO Accounts VALUES ($username, $password, $ticket)";

            var addAccountCmd = new SqliteCommand(addAccount, DbConnection);
            addAccountCmd.Parameters.AddWithValue("$username", username);
            addAccountCmd.Parameters.AddWithValue("$password", password);
            addAccountCmd.Parameters.AddWithValue("$ticket", ticket);
            return addAccountCmd.ExecuteNonQueryAsync();
        }

        public static Task AddSubscription(Subscription sub, string username)
        {
            var addSubscription = @"INSERT INTO Subscriptions VALUES ($name, $username, $game, $description, $status)";

            var addSubscriptionCmd = new SqliteCommand(addSubscription, DbConnection);
            addSubscriptionCmd.Parameters.AddWithValue("$username", username);
            addSubscriptionCmd.Parameters.AddWithValue("$name", sub.Name);
            addSubscriptionCmd.Parameters.AddWithValue("$game", sub.Game);
            addSubscriptionCmd.Parameters.AddWithValue("$description", sub.Description);
            addSubscriptionCmd.Parameters.AddWithValue("$status", sub.Status);
            return addSubscriptionCmd.ExecuteNonQueryAsync();
        }

        public static async Task<Dictionary<string, List<Tuple<string, string>>>> GetSubscriptions()
        {
            var getSubscriptions = @"SELECT Name, Username, Description FROM Subscriptions";

            var getSubscriptionsCmd = new SqliteCommand(getSubscriptions, DbConnection);
            var reader = await getSubscriptionsCmd.ExecuteReaderAsync();

            var subscriptions = new Dictionary<string, List<Tuple<string, string>>>();

            while (await reader.ReadAsync())
            {
                var subId = reader[0].ToString();
                var username = reader[1].ToString();
                var subDesc = reader[2].ToString();

                if (!subscriptions.ContainsKey(username))
                {
                    subscriptions[username] = new List<Tuple<string, string>>();
                }

                subscriptions[username].Add(new(subId, subDesc));
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