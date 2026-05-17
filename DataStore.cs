using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutopartsSystemBD
{
    public static class DataStore
    {
        private static readonly string ConnectionString =
            "Host=localhost;Port=5432;Database=autoparts_db;Username=postgres;Password=your_password";

        public static List<User> Users { get; private set; } = new();
        public static List<Supplier> Suppliers { get; private set; } = new();
        public static List<Part> Parts { get; private set; } = new();
        public static List<SuppliedPart> SuppliedParts { get; private set; } = new();
        public static List<Purchase> Purchases { get; private set; } = new();
        public static List<PriceHistory> PriceHistory { get; private set; } = new();

        public static void Load()
        {
            RefreshAll();
        }

        public static void Save()
        {
            RefreshAll();
        }

        public static void RefreshAll()
        {
            LoadUsers();
            LoadSuppliers();
            LoadParts();
            LoadSuppliedParts();
            LoadPurchases();
            LoadPriceHistory();
        }

        private static NpgsqlConnection GetConnection()
        {
            var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        public static User? Login(string login, string password)
        {
            RefreshAll();

            return Users.FirstOrDefault(x =>
                x.Login.Equals(login, StringComparison.OrdinalIgnoreCase) &&
                x.Password == password
            );
        }

        public static void AddSupplier(string name, string address, string phone)
        {
            using var connection = GetConnection();
            using var command = new NpgsqlCommand(
                "INSERT INTO suppliers (name, address, phone) VALUES (@name, @address, @phone)",
                connection
            );

            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("address", address);
            command.Parameters.AddWithValue("phone", phone);
            command.ExecuteNonQuery();

            RefreshAll();
        }

        public static void AddPart(string name, string article)
        {
            using var connection = GetConnection();
            using var command = new NpgsqlCommand(
                "INSERT INTO parts (name, article) VALUES (@name, @article)",
                connection
            );

            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("article", article);
            command.ExecuteNonQuery();

            RefreshAll();
        }

        public static void AddSuppliedPart(int supplierId, int partId, decimal currentPrice, DateTime startDate)
        {
            using var connection = GetConnection();
            using var transaction = connection.BeginTransaction();

            using var command = new NpgsqlCommand(
                "INSERT INTO supplied_parts (supplier_id, part_id, current_price) VALUES (@supplier_id, @part_id, @current_price) RETURNING id",
                connection,
                transaction
            );

            command.Parameters.AddWithValue("supplier_id", supplierId);
            command.Parameters.AddWithValue("part_id", partId);
            command.Parameters.AddWithValue("current_price", currentPrice);

            int suppliedPartId = Convert.ToInt32(command.ExecuteScalar());

            using var historyCommand = new NpgsqlCommand(
                "INSERT INTO price_history (supplied_part_id, new_price, notification_date, start_date) VALUES (@supplied_part_id, @new_price, @notification_date, @start_date)",
                connection,
                transaction
            );

            historyCommand.Parameters.AddWithValue("supplied_part_id", suppliedPartId);
            historyCommand.Parameters.AddWithValue("new_price", currentPrice);
            historyCommand.Parameters.AddWithValue("notification_date", DateTime.Today);
            historyCommand.Parameters.AddWithValue("start_date", startDate);

            historyCommand.ExecuteNonQuery();

            transaction.Commit();
            RefreshAll();
        }

        public static void AddPurchase(int suppliedPartId, int userId, DateTime purchaseDate, int quantity)
        {
            decimal? price = GetPriceForDate(suppliedPartId, purchaseDate);

            if (price == null)
            {
                throw new Exception("Для выбранной даты цена не найдена");
            }

            decimal totalSum = price.Value * quantity;

            using var connection = GetConnection();
            using var command = new NpgsqlCommand(
                "INSERT INTO purchases (supplied_part_id, user_id, purchase_date, quantity, unit_price, total_sum) " +
                "VALUES (@supplied_part_id, @user_id, @purchase_date, @quantity, @unit_price, @total_sum)",
                connection
            );

            command.Parameters.AddWithValue("supplied_part_id", suppliedPartId);
            command.Parameters.AddWithValue("user_id", userId);
            command.Parameters.AddWithValue("purchase_date", purchaseDate);
            command.Parameters.AddWithValue("quantity", quantity);
            command.Parameters.AddWithValue("unit_price", price.Value);
            command.Parameters.AddWithValue("total_sum", totalSum);

            command.ExecuteNonQuery();
            RefreshAll();
        }

        public static void AddPriceChange(int suppliedPartId, decimal newPrice, DateTime notificationDate, DateTime startDate)
        {
            using var connection = GetConnection();
            using var transaction = connection.BeginTransaction();

            using var command = new NpgsqlCommand(
                "INSERT INTO price_history (supplied_part_id, new_price, notification_date, start_date) VALUES (@supplied_part_id, @new_price, @notification_date, @start_date)",
                connection,
                transaction
            );

            command.Parameters.AddWithValue("supplied_part_id", suppliedPartId);
            command.Parameters.AddWithValue("new_price", newPrice);
            command.Parameters.AddWithValue("notification_date", notificationDate);
            command.Parameters.AddWithValue("start_date", startDate);

            command.ExecuteNonQuery();

            if (startDate.Date <= DateTime.Today)
            {
                using var updateCommand = new NpgsqlCommand(
                    "UPDATE supplied_parts SET current_price = @price WHERE id = @id",
                    connection,
                    transaction
                );

                updateCommand.Parameters.AddWithValue("price", newPrice);
                updateCommand.Parameters.AddWithValue("id", suppliedPartId);
                updateCommand.ExecuteNonQuery();
            }

            transaction.Commit();
            RefreshAll();
        }

        public static void ClearAll()
        {
            using var connection = GetConnection();

            using var command = new NpgsqlCommand(
                "TRUNCATE TABLE purchases, price_history, supplied_parts, parts, suppliers RESTART IDENTITY CASCADE",
                connection
            );

            command.ExecuteNonQuery();
            RefreshAll();
        }

        public static decimal? GetPriceForDate(int suppliedPartId, DateTime purchaseDate)
        {
            using var connection = GetConnection();
            using var command = new NpgsqlCommand(
                @"SELECT new_price 
                  FROM price_history
                  WHERE supplied_part_id = @supplied_part_id
                    AND start_date <= @purchase_date
                  ORDER BY start_date DESC
                  LIMIT 1",
                connection
            );

            command.Parameters.AddWithValue("supplied_part_id", suppliedPartId);
            command.Parameters.AddWithValue("purchase_date", purchaseDate);

            var result = command.ExecuteScalar();

            if (result == null)
            {
                return null;
            }

            return Convert.ToDecimal(result);
        }

        private static void LoadUsers()
        {
            Users.Clear();

            using var connection = GetConnection();
            using var command = new NpgsqlCommand("SELECT id, login, password, role FROM users ORDER BY id", connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Users.Add(new User
                {
                    Id = reader.GetInt32(0),
                    Login = reader.GetString(1),
                    Password = reader.GetString(2),
                    Role = reader.GetString(3)
                });
            }
        }

        private static void LoadSuppliers()
        {
            Suppliers.Clear();

            using var connection = GetConnection();
            using var command = new NpgsqlCommand("SELECT id, name, address, phone FROM suppliers ORDER BY id", connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Suppliers.Add(new Supplier
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Address = reader.GetString(2),
                    Phone = reader.GetString(3)
                });
            }
        }

        private static void LoadParts()
        {
            Parts.Clear();

            using var connection = GetConnection();
            using var command = new NpgsqlCommand("SELECT id, name, article FROM parts ORDER BY id", connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Parts.Add(new Part
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Article = reader.GetString(2)
                });
            }
        }

        private static void LoadSuppliedParts()
        {
            SuppliedParts.Clear();

            using var connection = GetConnection();
            using var command = new NpgsqlCommand("SELECT id, supplier_id, part_id, current_price FROM supplied_parts ORDER BY id", connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                SuppliedParts.Add(new SuppliedPart
                {
                    Id = reader.GetInt32(0),
                    SupplierId = reader.GetInt32(1),
                    PartId = reader.GetInt32(2),
                    CurrentPrice = reader.GetDecimal(3)
                });
            }
        }

        private static void LoadPurchases()
        {
            Purchases.Clear();

            using var connection = GetConnection();
            using var command = new NpgsqlCommand(
                "SELECT id, supplied_part_id, user_id, purchase_date, quantity, unit_price, total_sum FROM purchases ORDER BY id",
                connection
            );
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Purchases.Add(new Purchase
                {
                    Id = reader.GetInt32(0),
                    SuppliedPartId = reader.GetInt32(1),
                    UserId = reader.GetInt32(2),
                    PurchaseDate = reader.GetDateTime(3),
                    Quantity = reader.GetInt32(4),
                    UnitPrice = reader.GetDecimal(5),
                    TotalSum = reader.GetDecimal(6)
                });
            }
        }

        private static void LoadPriceHistory()
        {
            PriceHistory.Clear();

            using var connection = GetConnection();
            using var command = new NpgsqlCommand("SELECT id, supplied_part_id, new_price, notification_date, start_date FROM price_history ORDER BY id", connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                PriceHistory.Add(new PriceHistory
                {
                    Id = reader.GetInt32(0),
                    SuppliedPartId = reader.GetInt32(1),
                    NewPrice = reader.GetDecimal(2),
                    NotificationDate = reader.GetDateTime(3),
                    StartDate = reader.GetDateTime(4)
                });
            }
        }
    }
}
