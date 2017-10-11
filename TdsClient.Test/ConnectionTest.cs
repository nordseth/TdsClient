using System;
using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TdsClient.Test
{
    [TestClass]
    public class ConnectionTest
    {
        private const string connectionString = "Data Source=192.168.0.25;Port=5000;User=sa;Pass=password;db=pubs2";

        [TestMethod]
        public void Connection_SimpleSelect()
        {
            string sqlQuery = "SELECT top 10 * from titles";

            using (IDbConnection connection = new TdsConnection(connectionString, ConsoleLogger.LoggerFactory))
            {
                var command = connection.CreateCommand();
                command.CommandText = sqlQuery;

                connection.Open();

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        Console.Write($" {reader.GetName(i)} : {reader[i]}");
                    }

                    Console.WriteLine();
                }

                reader.Close();
            }
        }

        [TestMethod]
        public void Connection_SelectAll_Tables_Pubs2()
        {
            var sqlQuery = new[]
            {
                "SELECT TOP 10 * FROM publishers",
                "SELECT TOP 10 * FROM authors",
                "SELECT TOP 10 * FROM titles",
                "SELECT TOP 10 * FROM titleauthor",
                "SELECT TOP 10 * FROM salesdetail",
                "SELECT TOP 10 * FROM sales",
                "SELECT TOP 10 * FROM stores",
                "SELECT TOP 10 * FROM roysched",
                "SELECT TOP 10 * FROM discounts",
                "SELECT TOP 10 * FROM blurbs",
                "SELECT TOP 1 * FROM au_pix",
            };

            using (IDbConnection connection = new TdsConnection(connectionString, ConsoleLogger.LoggerFactory))
            {
                connection.Open();

                foreach (var q in sqlQuery)
                {
                    var result = connection.Query(q);
                    foreach (var r in result)
                    {
                        Console.WriteLine($" {r}");
                    }
                }
            }
        }

        [TestMethod]
        public void Connection_SelectAll_Tables_Pubs3()
        {
            var sqlQuery = new[]
            {
                "SELECT TOP 10 * FROM publishers",
                "SELECT TOP 10 * FROM authors",
                "SELECT TOP 10 * FROM titles",
                "SELECT TOP 10 * FROM titleauthor",
                "SELECT TOP 10 * FROM salesdetail",
                "SELECT TOP 10 * FROM sales",
                "SELECT TOP 10 * FROM stores",
                "SELECT TOP 10 * FROM store_employees",
                "SELECT TOP 10 * FROM roysched",
                "SELECT TOP 10 * FROM discounts",
                "SELECT TOP 10 * FROM blurbs",
            };

            using (IDbConnection connection = new TdsConnection(connectionString, ConsoleLogger.LoggerFactory))
            {
                connection.Open();
                connection.ChangeDatabase("pubs3");

                foreach (var q in sqlQuery)
                {
                    var result = connection.Query(q);
                    foreach (var r in result)
                    {
                        Console.WriteLine($" {r}");
                    }
                }
            }
        }

        [TestMethod]
        public void Connection_QueryWithParamTest_2()
        {
            string sqlQuery = "SELECT @par";

            string paramValue = "test123";

            using (IDbConnection connection = new TdsConnection(connectionString, ConsoleLogger.LoggerFactory))
            {
                var command = connection.CreateCommand();
                command.CommandText = sqlQuery;
                var param = command.CreateParameter();
                param.ParameterName = "@par";
                param.Value = paramValue;
                param.DbType = DbType.String;
                command.Parameters.Add(param);

                connection.Open();

                var reader = command.ExecuteReader();
                while (reader != null && reader.Read())
                {
                    Console.WriteLine("\t{0}", reader[0]);
                }

                reader.Close();
            }
        }

        [TestMethod]
        public void Connection_QueryWithParamTest()
        {
            string sqlQuery =
                "SELECT @string as c1, @char as c2, @int as c3, @double as c4, @decimal as c5, @datetime as c6";

            string strParam = "test";
            char charParam = 't';
            int intParam = 123141;
            double doubleParam = 5.55;
            decimal decimalParam = 3.02m;
            var dt = DateTime.Now.AddSeconds(123000);
            DateTime dtParam = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Day, dt.Second);

            using (IDbConnection connection = new TdsConnection(connectionString, ConsoleLogger.LoggerFactory))
            {
                var command = connection.CreateCommand();
                command.CommandText = sqlQuery;

                var param = command.CreateParameter();
                param.ParameterName = "@string";
                param.Value = strParam;
                command.Parameters.Add(param);

                param = command.CreateParameter();
                param.ParameterName = "@char";
                param.Value = charParam;
                command.Parameters.Add(param);

                param = command.CreateParameter();
                param.ParameterName = "@int";
                param.Value = intParam;
                command.Parameters.Add(param);

                param = command.CreateParameter();
                param.ParameterName = "@double";
                param.Value = doubleParam;
                command.Parameters.Add(param);

                param = command.CreateParameter();
                param.ParameterName = "@decimal";
                param.Value = decimalParam;
                command.Parameters.Add(param);

                param = command.CreateParameter();
                param.ParameterName = "@datetime";
                param.Value = dtParam;
                command.Parameters.Add(param);

                connection.Open();

                var reader = command.ExecuteReader();
                while (reader != null && reader.Read())
                {
                    Console.WriteLine($"\"{reader[0]}\", \"{reader[1]}\", \"{reader[2]}\", \"{reader[3]}\", \"{reader[4]}\", \"{reader[5]}\"");
                    Assert.AreEqual(strParam, reader[0]);
                    Assert.AreEqual(charParam.ToString(), reader[1]);
                    Assert.AreEqual(intParam, reader[2]);
                    Assert.AreEqual(doubleParam, reader[3]);
                    Assert.AreEqual(decimalParam, reader[4]);
                    Assert.AreEqual(dtParam, reader[5]);
                }

                reader.Close();
            }
        }

        [TestMethod]
        public void Connection_Dapper_WithParams()
        {
            using (IDbConnection connection = new TdsConnection(connectionString, ConsoleLogger.LoggerFactory))
            {
                connection.Open();
                connection.ChangeDatabase("pubs3");

                var result = connection.Query("SELECT * FROM titles where title_id = @Id", new { Id = "PS3333" });
                foreach (var r in result)
                {
                    Console.WriteLine($" {r}");
                }
            }
        }

        [TestMethod]
        public void Connection_Dapper()
        {
            string sqlQuery = "exec sp_help 'titles'";
            var result = new TdsConnection(connectionString, ConsoleLogger.LoggerFactory).Query(sqlQuery);
            foreach (var r in result)
            {
                Console.WriteLine($" {r}");
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            ConsoleLogger.Dispose();
        }
    }
}
