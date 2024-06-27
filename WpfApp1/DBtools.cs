using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace WpfApp1.DBtools
{
    public class Database
    {
        private const string DatabaseFileName = "filters.db";

        public static List<string> InitialiseDatabase()
        {
            string databasePath = Path.Combine(Environment.CurrentDirectory, DatabaseFileName);

            // Create database file if it doesn't exist
            bool exists = File.Exists(databasePath);

            if (!exists)
            {
                SQLiteConnection.CreateFile(DatabaseFileName);
                Console.WriteLine("Created database");
            }

            // Create tables
            CreateTables();

            // Retrieve existing filter table names (without "FilterData_" prefix)
            List<string> filterTableNames = GetFilterTableNames();

            return filterTableNames;
        }

        private static void CreateTables()
        {
            string connectionString = $"Data Source={DatabaseFileName};Version=3;";

            // Create main Filters table
            string createFiltersTableSql = @"
                CREATE TABLE IF NOT EXISTS Filters (
                    FilterId INTEGER PRIMARY KEY AUTOINCREMENT,
                    FilterName TEXT NOT NULL UNIQUE
                );";

            ExecuteNonQuery(connectionString, createFiltersTableSql);

            // Example: No specific tables created here; they are created dynamically
        }

        public static void AddFilter(string filterName, List<(double wavelength, double transmission)> filters)
        {
            string connectionString = $"Data Source={DatabaseFileName};Version=3;";
            string filterDataTableName = filterName.Replace(" ", string.Empty);

            // Insert filter name into Filters table if it doesn't exist
            int filterId = InsertFilterName(connectionString, filterName);

            // Insert filter data into the corresponding filter-specific table
            if (filterId != -1)
            {
                CreateFilterDataTable(connectionString, filterDataTableName);
                InsertFilterData(connectionString, filterDataTableName, filterId, filters);
            }
        }

        private static int InsertFilterName(string connectionString, string filterName)
        {
            string insertFilterNameSql = @"
                INSERT OR IGNORE INTO Filters (FilterName) VALUES (@FilterName);
                SELECT FilterId FROM Filters WHERE FilterName = @FilterName;";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(insertFilterNameSql, connection))
                {
                    command.Parameters.AddWithValue("@FilterName", filterName);
                    int filterId = Convert.ToInt32(command.ExecuteScalar());
                    return filterId;
                }
            }
        }

        private static void CreateFilterDataTable(string connectionString, string tableName)
        {
            string createFilterDataTableSql = @"
                CREATE TABLE IF NOT EXISTS ""@TableName"" (
                    DataId INTEGER PRIMARY KEY AUTOINCREMENT,
                    FilterId INTEGER NOT NULL,
                    Wavelength REAL NOT NULL,
                    Transmission REAL NOT NULL,
                    FOREIGN KEY(FilterId) REFERENCES Filters(FilterId)
                );";

            createFilterDataTableSql = createFilterDataTableSql.Replace("@TableName", tableName);

            ExecuteNonQuery(connectionString, createFilterDataTableSql);
        }

        private static void InsertFilterData(string connectionString, string tableName, int filterId, List<(double wavelength, double transmission)> filters)
        {
            string insertFilterDataSql = @"
                INSERT INTO ""@TableName"" (FilterId, Wavelength, Transmission) VALUES (@FilterId, @Wavelength, @Transmission);";

            insertFilterDataSql = insertFilterDataSql.Replace("@TableName", tableName);

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                foreach (var filter in filters)
                {
                    using (SQLiteCommand command = new SQLiteCommand(insertFilterDataSql, connection))
                    {
                        command.Parameters.AddWithValue("@FilterId", filterId);
                        command.Parameters.AddWithValue("@Wavelength", filter.wavelength);
                        command.Parameters.AddWithValue("@Transmission", filter.transmission);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
        
        public static List<(double Wavelength, double Transmission)> GetFilterData(string filterName)
        {
            List<(double Wavelength, double Transmission)> filterData = new List<(double Wavelength, double Transmission)>();
    
            string connectionString = $"Data Source={DatabaseFileName};Version=3;";

            // Normalize the filter table name (assuming it's FilterData_{FilterName})
            string filterDataTableName = $"{filterName.Replace(" ", string.Empty)}";

            // SQL command to select data from the filter-specific table
            string selectFilterDataSql = @"
                SELECT Wavelength, Transmission
                FROM ""@FilterDataTableName"";";

            selectFilterDataSql = selectFilterDataSql.Replace("@FilterDataTableName", filterDataTableName);

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(selectFilterDataSql, connection))
                {
                    SQLiteDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        double wavelength = Convert.ToDouble(reader["Wavelength"]);
                        double transmission = Convert.ToDouble(reader["Transmission"]);
                        filterData.Add((wavelength, transmission));
                    }

                    reader.Close();
                }

                connection.Close();
            }

            return filterData;
        }


        private static List<string> GetFilterTableNames()
        {
            List<string> tableNames = new List<string>();

            string connectionString = $"Data Source={DatabaseFileName};Version=3;";
            string sql = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name NOT LIKE 'Filters';";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string tableName = reader.GetString(0);
                            tableNames.Add(tableName);
                        }
                    }
                }

                connection.Close();
            }

            return tableNames;
        }
        
        public static void DeleteFilter(string filterName)
        {
            string connectionString = $"Data Source={DatabaseFileName};Version=3;";

            // Normalize the table name
            string filterDataTableName = $"{filterName.Replace(" ", string.Empty)}";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Delete from the filter-specific table
                        string deleteFilterDataSql = @"
                            DROP TABLE IF EXISTS ""@FilterDataTableName"";";
                
                        deleteFilterDataSql = deleteFilterDataSql.Replace("@FilterDataTableName", filterDataTableName);
                
                        using (SQLiteCommand deleteFilterDataCommand = new SQLiteCommand(deleteFilterDataSql, connection, transaction))
                        {
                            deleteFilterDataCommand.ExecuteNonQuery();
                        }

                        // Delete from the Filters table
                        string deleteFilterSql = @"
                            DELETE FROM Filters WHERE FilterName = @FilterName;";
                
                        using (SQLiteCommand deleteFilterCommand = new SQLiteCommand(deleteFilterSql, connection, transaction))
                        {
                            deleteFilterCommand.Parameters.AddWithValue("@FilterName", filterName);
                            deleteFilterCommand.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Error deleting filter {filterName}: {ex.Message}");
                    }
                }
            }
        }

        public static void RenameFilter(string oldName, string newName)
        {
            string connectionString = $"Data Source={DatabaseFileName};Version=3;";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string renameTableSql = @"
                ALTER TABLE ""@OldName""
                RENAME TO ""@NewName"";";

                renameTableSql = renameTableSql.Replace("@OldName", oldName).Replace("@NewName", newName);

                using (var command = new SQLiteCommand(renameTableSql, connection))
                {
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }


        private static void ExecuteNonQuery(string connectionString, string sql)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
