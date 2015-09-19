using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeCamp2015.Domain;

namespace CodeCamp2015.Data
{
    public class ClinicalDbContext : DbContext
    {
        public ClinicalDbContext()
        {
            // this is essentially just for Add-Migration
        }

        public ClinicalDbContext(string connectionString) : base(connectionString)
        {
            
        }

        public ClinicalDbContext(DbConnection connection) : base(connection, true)
        {

        }

        public virtual DbSet<Patient> Patients { get; set; }
        public virtual DbSet<Physician> Physicians { get; set; }
        public virtual DbSet<Visit> Visits { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("demo1");

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
