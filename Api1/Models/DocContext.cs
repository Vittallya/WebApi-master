using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Api1.Models
{
    public class DocContext : DbContext
    {
        public DocContext(DbContextOptions<DocContext> options)
            : base(options)
        {
        }
        //для файлов
        public DbSet<Doc> Docs { get; set; }
        public DbSet<Version> Versions { get; set; }
    }
}
