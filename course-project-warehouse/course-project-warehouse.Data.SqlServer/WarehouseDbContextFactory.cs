using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace course_project_warehouse.Data.SqlServer
{
    public class WarehouseDbContextFactory : IDesignTimeDbContextFactory<WarehouseDbContext>
    {
        public WarehouseDbContext CreateDbContext(string[] args)
        {
            // Простая строка подключения (можно заменить на свою)
            var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=WarehouseDB_Ivanov;Integrated Security=True;TrustServerCertificate=True;";

            var optionsBuilder = new DbContextOptionsBuilder<WarehouseDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new WarehouseDbContext(optionsBuilder.Options);
        }
    }
}