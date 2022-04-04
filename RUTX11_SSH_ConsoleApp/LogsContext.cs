using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RUTX11_SSH_ConsoleApp
{
    public class LogsContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Attempt> Attempts { get; set; }
        public DbSet<Error> Errors { get; set; }

        public string DbPath { get; }

        public LogsContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = System.IO.Path.Join(path, "instance2.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }

    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Password { get; set; }
        public string? Ip { get; set; }
        public int Port { get; set; }
        public List<Attempt> Attempts { get; set; } = new();
    }

    public class Attempt
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public List<Error> Errors { get; } = new();
    }

    public class Error
    {
        public int Id { get; set; }
        public string? Message { get; set; }
    }
}
