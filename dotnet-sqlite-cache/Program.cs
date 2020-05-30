using McMaster.Extensions.CommandLineUtils;
using System;
using System.Data;
using System.Data.SQLite;
using System.Reflection;

namespace Contoso.Extensions.Caching.SqliteConfig.Tools
{
    public class Program
    {
        string _connectionString = null;
        string _tableName = null;
        readonly IConsole _console;

        public Program(IConsole console)
        {
            if (console == null)
                throw new ArgumentNullException(nameof(console));

            _console = console;
        }

        public static int Main(string[] args)
        {
            return new Program(PhysicalConsole.Singleton).Run(args);
        }

        public int Run(string[] args)
        {
            DebugHelper.HandleDebugSwitch(ref args);

            try
            {
                var app = new CommandLineApplication
                {
                    Name = "dotnet sqlite-cache",
                    FullName = "SQLite Cache Command Line Tool",
                    Description = "Creates table and indexes in SQLite database to be used for distributed caching",
                };

                app.HelpOption();
                app.VersionOptionFromAssemblyAttributes(typeof(Program).GetTypeInfo().Assembly);
                var verbose = app.VerboseOption();

                app.Command("create", command =>
                {
                    command.Description = app.Description;

                    var connectionStringArg = command.Argument("[connectionString]", "The connection string to connect to the database.");
                    var tableNameArg = command.Argument("[tableName]", "Name of the table to be created.");

                    command.HelpOption();

                    command.OnExecute(() =>
                    {
                        var reporter = CreateReporter(verbose.HasValue());
                        if (string.IsNullOrEmpty(connectionStringArg.Value)
                            || string.IsNullOrEmpty(tableNameArg.Value))
                        {
                            reporter.Error("Invalid input");
                            app.ShowHelp();
                            return 2;
                        }

                        _connectionString = connectionStringArg.Value;
                        _tableName = tableNameArg.Value;

                        return CreateTableAndIndexes(reporter);
                    });
                });

                // Show help information if no subcommand/option was specified.
                app.OnExecute(() =>
                {
                    app.ShowHelp();
                    return 2;
                });

                return app.Execute(args);
            }
            catch (Exception exception)
            {
                CreateReporter(verbose: false).Error($"An error occurred. {exception.Message}");
                return 1;
            }
        }

        IReporter CreateReporter(bool verbose) => new ConsoleReporter(_console, verbose, quiet: false);

        int CreateTableAndIndexes(IReporter reporter)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                var sqlQueries = new SqlQueries( _tableName);
                var command = new SQLiteCommand(sqlQueries.TableInfo, connection);

                using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                    if (reader.Read())
                    {
                        reporter.Warn($"Table with name '{_tableName}' already exists. Provide a different table name and try again.");
                        return 1;
                    }

                using (var transaction = connection.BeginTransaction())
                    try
                    {
                        command = new SQLiteCommand(sqlQueries.CreateTable, connection, transaction);

                        reporter.Verbose($"Executing {command.CommandText}");
                        command.ExecuteNonQuery();

                        command = new SQLiteCommand(sqlQueries.CreateIndexOnExpirationTime, connection, transaction);

                        reporter.Verbose($"Executing {command.CommandText}");
                        command.ExecuteNonQuery();

                        transaction.Commit();

                        reporter.Output("Table and index were created successfully.");
                    }
                    catch (Exception ex)
                    {
                        reporter.Error($"An error occurred while trying to create the table and index. {ex.Message}");
                        transaction.Rollback();

                        return 1;
                    }
            }

            return 0;
        }
    }
}