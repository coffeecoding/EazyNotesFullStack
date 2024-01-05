using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace EazyNotesAPI.Library.Internal.DataAccess
{
    internal class SqlDataAccess : IDisposable
    {
        public static IConfiguration AppSettings { get; private set; }
        public const string ConnectionStringName = "MainCn";

        public SqlDataAccess()
        {
            var x = Environment.CurrentDirectory;
            var y = AppDomain.CurrentDomain.BaseDirectory;
            if (AppSettings == null) { 
                AppSettings = new ConfigurationBuilder()
                    .SetBasePath(Environment.CurrentDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();
            }
        }

        public string GetConnectionString(string name) => AppSettings.GetConnectionString(name);

        // According to Tim, this generic method covers about 98% of use cases for retrieving data
        public List<T> LoadData<T, U>(string storedProcedure, U parameters, string connectionStringName)
        {
            string connectionString = GetConnectionString(connectionStringName);

            using IDbConnection connection = new MySqlConnection(connectionString);
            List<T> rows = connection.Query<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure).ToList();
            return rows;
        }

        // Tim uses return value void, but returning the saved object if successfull should be better, to indicate failure
        // by returning null
        public T SaveData<T>(string storedProcedure, T parameters, string connectionStringName)
        {
            string connectionString = GetConnectionString(connectionStringName);

            using IDbConnection connection = new MySqlConnection(connectionString);
            // TODO: eventually do this asynchronously
            //connection.ExecuteAsync
            int affectedRows = connection.Execute(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            if (affectedRows == 0)
                return default;
            else return parameters;
        }

        /// <summary>
        ///  Returns Id of the inserted data row.
        /// </summary>
        public dynamic SaveDataSingle<T>(string storedProcedure, T parameters, string connectionStringName)
        {
            string connectionString = GetConnectionString(connectionStringName);

            using IDbConnection connection = new MySqlConnection(connectionString);
            var id = connection.QuerySingleOrDefault<dynamic>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            return id;
        }

        public async Task SaveDataAsync<T>(string storedProcedure, T parameters, string connectionStringName)
        {
            string connectionString = GetConnectionString(connectionStringName);

            using IDbConnection connection = new MySqlConnection(connectionString);
            await connection.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
        }

        private IDbConnection _connection;
        private IDbTransaction _transaction;

        public void BeginTransaction    (string cnStringName)
        {
            string connectionString = GetConnectionString(cnStringName);

            _connection = new MySqlConnection(connectionString);
            _connection.Open();

            _transaction = _connection.BeginTransaction();
        }

        public T SaveDataInTransaction<T>(string storedProcedure, T parameters)
        {
            int affectedRows = _connection.Execute(storedProcedure, parameters, 
                commandType: CommandType.StoredProcedure, transaction: _transaction);
            if (affectedRows == 0)
                return default;
            else return parameters;
        }

        public List<T> LoadDataInTransaction<T, U>(string storedProcedure, U parameters)
        {
            List<T> rows = _connection.Query<T>(storedProcedure, parameters, 
                commandType: CommandType.StoredProcedure, transaction: _transaction).ToList();
            return rows;
        }

        public void CommitTransaction()
        {
            _transaction.Commit();
            _transaction = null;
            _connection.Close();
            _connection = null;
        }

        public void RollbackTransaction()
        {
            _transaction?.Rollback();
            _transaction = null;
            _connection.Close();
            _connection = null;
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
