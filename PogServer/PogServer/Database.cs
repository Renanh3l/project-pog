using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Data.SQLite;
using Dapper;

namespace PogServer
{

    public class Database
    {
        public string databaseFile;
        public string connectionString;

        public Database()
        {
            databaseFile = "PogDB.sqlite";
            connectionString = $"Data Source={databaseFile};Version=3;";

            if (!File.Exists(databaseFile))
            {
                Console.WriteLine("Database file missing. Creating new one...");
                SQLiteConnection.CreateFile(databaseFile);
                SeedDatabase();
            }
        }

        public List<accounts> LoadAccounts() 
        {
            using (IDbConnection conn = new SQLiteConnection(connectionString))
            {
                var output = conn.Query<accounts>("SELECT * FROM accounts", new DynamicParameters());
                return output.ToList();
            }
        }

        public bool TryCreateAccount(string _account, string _email, string _password)
        {
            if (!string.IsNullOrWhiteSpace(_account) && !string.IsNullOrWhiteSpace(_password))
            {
                using (IDbConnection conn = new SQLiteConnection(connectionString))
                {
                    List<accounts> accountsOutput = conn.Query<accounts>($"SELECT * FROM accounts WHERE email='{_email}'").ToList();
                    bool alreadyExists = accountsOutput.Count > 0;

                    if (!alreadyExists)
                    {
                        accounts account = new accounts();
                        account.username = _account;
                        account.email = _email;
                        account.password = _password;

                        conn.Execute(@"INSERT INTO accounts (username, email, password) VALUES (@username, @email, @password)", account);
                        Console.WriteLine($"Account {_account} from {_email} created.");
                        return true;
                    }
                    
                    Console.WriteLine($"Account creation failed: {_email} already exists.");
                }
            }

            return false;
        }

        private void SeedDatabase()
        {
            using (IDbConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Execute(@"CREATE TABLE IF NOT EXISTS [accounts] (
                    [id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    [username] NVARCHAR(64) NOT NULL,
                    [email] NVARCHAR(128) NOT NULL,
                    [password] NVARCHAR(128) NOT NULL,
                    [created] TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    [lastlogin] TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    [banned] BIT NOT NULL DEFAULT 0
                )");

                // Insert admin user
                conn.Execute(@"INSERT INTO accounts (username, email, password) VALUES ('admin', 'nan.pp@hotmail.com', 'admin')");
            }
        }

        //public bool TryLogin(string _account, string _password)
        //{
        //    if (!string.IsNullOrWhiteSpace(_account) && !string.IsNullOrWhiteSpace(_password))
        //    {
        //        // check account name, password, banned status
        //        if (connection.FindWithQuery<accounts>("SELECT * FROM accounts WHERE name=? AND password=? and banned=0", _account, _password) != null)
        //        {
        //            // save last login time and return true
        //            connection.Execute("UPDATE accounts SET lastlogin=? WHERE name=?", DateTime.UtcNow, _account);
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        //public void CharacterSave(Player _player, bool _online, bool _useTransaction = true)
        //{
        //    // only use a transaction if not called within SaveMany transaction
        //    if (_useTransaction) connection.BeginTransaction();

        //    connection.InsertOrReplace(new characters
        //    {
        //        name = _player.username,
        //        x = _player.position.X,
        //        y = _player.position.Y,
        //        online = _online,
        //        lastsaved = DateTime.UtcNow
        //    });

        //    if (_useTransaction) connection.Commit();
        //}

        public class accounts
        {
            public string username { get; set; }
            public string email { get; set; }
            public string password { get; set; }
            public DateTime created { get; set; }
            public DateTime lastlogin { get; set; }
            public bool banned { get; set; }
        }

        class characters
        {
            public string name { get; set; }
            public string account { get; set; }
            public float x { get; set; }
            public float y { get; set; }
            public int level { get; set; }

            public bool online { get; set; }
            public DateTime lastsaved { get; set; }
        }
    }
}
