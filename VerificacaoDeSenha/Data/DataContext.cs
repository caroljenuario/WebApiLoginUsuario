using Pomelo.EntityFrameworkCore.MySql;
using Microsoft.EntityFrameworkCore;



namespace VerificacaoDeSenha.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }

    }





}
