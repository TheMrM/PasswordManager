using System.Data;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;

namespace PasswordManager.Database
{
    public static class DatabaseHelper
    {
        private const string DatabasePath = "UserDatabase.db";
        private static string ConnectionString = $"Data Source={DatabasePath};Version=3;";

        public static void InitializeDatabase()
        {
            try
            {
                // Create database file if it doesn't exist
                if (!File.Exists(DatabasePath))
                {
                    SQLiteConnection.CreateFile(DatabasePath);
                }

                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    // Drop existing table to recreate it with the correct structure
                    string dropTable = "DROP TABLE IF EXISTS Users;";
                    using (var command = new SQLiteCommand(dropTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // Create users table with all required columns
                    string createTable = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT UNIQUE NOT NULL,
                    Password TEXT NOT NULL,
                    Role TEXT NOT NULL DEFAULT 'basic'
                );";

                    using (var command = new SQLiteCommand(createTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // Create StoredPasswords table
                    string createStoredPasswordsTable = @"
                CREATE TABLE IF NOT EXISTS StoredPasswords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER NOT NULL,
                    Website TEXT NOT NULL,
                    Username TEXT NOT NULL,
                    Password TEXT NOT NULL,
                    FOREIGN KEY(UserId) REFERENCES Users(Id)
                );";

                    using (var command = new SQLiteCommand(createStoredPasswordsTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // Create default admin user with hashed password
                    string adminPassword = HashPassword("admin123");
                    string createAdmin = @"
                INSERT OR IGNORE INTO Users (Username, Password, Role) 
                VALUES ('admin', @password, 'admin');";

                    using (var command = new SQLiteCommand(createAdmin, connection))
                    {
                        command.Parameters.AddWithValue("@password", adminPassword);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error initializing database: {ex.Message}");
            }
        }

        private static void CreateTables()
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var command = new SQLiteCommand(
                            "CREATE TABLE IF NOT EXISTS Users (Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                            "Username TEXT NOT NULL UNIQUE, " +
                            "Password TEXT NOT NULL, " +
                            "Role TEXT NOT NULL DEFAULT 'basic', " +
                            "CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP);", connection);
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static void CreateDefaultAdmin()
        {
            if (!UserExists("admin"))
            {
                AddUser("admin", "admin123", "admin");
            }
        }

        public static bool ValidateUser(string username, string password, out string role)
        {
            role = "basic"; // default role
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return false;

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                var command = new SQLiteCommand(
                    "SELECT Role FROM Users WHERE Username = @username AND Password = @password",
                    connection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", HashPassword(password));

                try
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            role = reader["Role"].ToString();
                            return true;
                        }
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error validating user: {ex.Message}");
                }
            }
        }

        public static bool UserExists(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                var sql = "SELECT COUNT(*) FROM Users WHERE Username=@username";
                var command = new SQLiteCommand(sql, conn);
                command.Parameters.AddWithValue("@username", username);

                try
                {
                    var result = (long)command.ExecuteScalar();
                    return result > 0;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error checking user existence: {ex.Message}");
                }
            }
        }

        public static bool AddUser(string username, string password, string role = "basic")
        {
            try
            {
                // Input validation
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    throw new ArgumentException("Username and password cannot be empty");

                // Check if user exists
                if (UserExists(username))
                    throw new Exception("Username already exists");

                using (var conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            var sql = "INSERT INTO Users (Username, Password, Role) VALUES (@username, @password, @role)";
                            var command = new SQLiteCommand(sql, conn);
                            command.Parameters.AddWithValue("@username", username.Trim());
                            command.Parameters.AddWithValue("@password", HashPassword(password));
                            command.Parameters.AddWithValue("@role", role.ToLower().Trim());

                            int rowsAffected = command.ExecuteNonQuery();
                            transaction.Commit();

                            return rowsAffected > 0;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception($"Error adding user: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add user: {ex.Message}");
            }
        }

        public static List<(string Username, string Role)> GetAllUsers()
        {
            var users = new List<(string Username, string Role)>();
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT Username, Role FROM Users";

                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add((
                                reader["Username"].ToString(),
                                reader["Role"].ToString()
                            ));
                        }
                    }
                }
            }
            return users;
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public static bool UpdateUser(string username, string newPassword, string newRole)
        {
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var sql = "UPDATE Users SET Password = @password, Role = @role WHERE Username = @username";
                        var command = new SQLiteCommand(sql, conn);
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", HashPassword(newPassword));
                        command.Parameters.AddWithValue("@role", newRole);
                        var rowsAffected = command.ExecuteNonQuery();
                        transaction.Commit();
                        return rowsAffected > 0;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public static bool DeleteUser(string username)
        {
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var sql = "DELETE FROM Users WHERE Username = @username";
                        var command = new SQLiteCommand(sql, conn);
                        command.Parameters.AddWithValue("@username", username);
                        var rowsAffected = command.ExecuteNonQuery();
                        transaction.Commit();
                        return rowsAffected > 0;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public static int GetUserId(string username)
        {
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                var command = new SQLiteCommand(
                    "SELECT Id FROM Users WHERE Username = @username",
                    conn);
                command.Parameters.AddWithValue("@username", username);
                var result = command.ExecuteScalar();
                return Convert.ToInt32(result);
            }
        }

        public static void AddStoredPassword(int userId, string website, string username, string password)
        {
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var command = new SQLiteCommand(
                            "INSERT INTO StoredPasswords (UserId, Website, Username, Password) " +
                            "VALUES (@userId, @website, @username, @password)",
                            conn);
                        command.Parameters.AddWithValue("@userId", userId);
                        command.Parameters.AddWithValue("@website", website);
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", EncryptPassword(password));
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public static List<(int Id, string Website, string Username, string Password)> GetStoredPasswords(int userId)
        {
            var passwords = new List<(int Id, string Website, string Username, string Password)>();
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                var command = new SQLiteCommand(
                    "SELECT Id, Website, Username, Password FROM StoredPasswords WHERE UserId = @userId",
                    conn);
                command.Parameters.AddWithValue("@userId", userId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        passwords.Add((
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.GetString(2),
                            DecryptPassword(reader.GetString(3))
                        ));
                    }
                }
            }
            return passwords;
        }

        public static void DeleteStoredPassword(int passwordId)
        {
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var command = new SQLiteCommand(
                            "DELETE FROM StoredPasswords WHERE Id = @id",
                            conn);
                        command.Parameters.AddWithValue("@id", passwordId);
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static string EncryptPassword(string password)
        {
            byte[] key = new byte[32] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32 };
            byte[] iv = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(password);
                    }

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        private static string DecryptPassword(string encryptedPassword)
        {
            byte[] key = new byte[32] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32 };
            byte[] iv = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

            byte[] cipherText = Convert.FromBase64String(encryptedPassword);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }
    }
}
