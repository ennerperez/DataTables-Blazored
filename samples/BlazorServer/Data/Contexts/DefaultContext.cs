using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BlazorServer.Data.Entities;
using BlazorServer.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace BlazorServer.Data.Contexts
{
    public class DefaultContext : DbContext
    {
        private readonly DbContextOptions _options;

        public DbContextOptions Options => _options;
        
        public DefaultContext()
        {
        }

        public DefaultContext(DbContextOptions<DefaultContext> options) : base(options)
        {
            _options = options;
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DefaultContext).Assembly, m => m.GetCustomAttributes(typeof(DbContextAttribute), true).OfType<DbContextAttribute>().Any(a => a.ContextType == typeof(DefaultContext)));
        }
        
#if DEBUG
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.EnableSensitiveDataLogging();
            if (_options == null)
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
#if DEBUG
                    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
#endif
                    .AddEnvironmentVariables()
                    .Build();

                UseDbEngine(optionsBuilder, config);
            }
        }
#endif
        
        public DbSet<Entities.Employee> Employees { get; set; }
        
        public static void UseDbEngine(DbContextOptionsBuilder optionsBuilder, IConfiguration config)
        {
            var migrationsHistoryTableName = "__EFMigrationsHistory";
            var connectionString = config.GetConnectionString(nameof(DefaultContext));
            DbConnectionStringBuilder csb;
            csb = new SqliteConnectionStringBuilder() { ConnectionString = connectionString };
            var dbPath = Regex.Match(csb.ConnectionString.ToLower(), "(data source ?= ?)(.*)(;?)").Groups[2].Value;
            var dbPathExpanded = Environment.ExpandEnvironmentVariables(dbPath);
            csb.ConnectionString = csb.ConnectionString.Replace(dbPath, dbPathExpanded);
            optionsBuilder.UseSqlite(csb.ConnectionString, x => x.MigrationsHistoryTable(migrationsHistoryTableName));
            
        }
    }
}
