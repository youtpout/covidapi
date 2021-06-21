using covidlibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace coviddatabase
{
    public partial class CovidContext : DbContext
    {
        private string connectionString;

        public virtual DbSet<CitiesEntity> Cities { get; set; }
        public virtual DbSet<CountryEntity> Country { get; set; }
        public virtual DbSet<LogsEntity> Logs { get; set; }
        public virtual DbSet<NewsEntity> News { get; set; }
        public virtual DbSet<ProvinceEntity> Province { get; set; }
        public virtual DbSet<SerieEntity> Serie { get; set; }

        private IConfiguration configuration;

        public CovidContext()
        {
        }

        public CovidContext(DbContextOptions<CovidContext> options)
            : base(options)
        {
        }

        //public CovidContext(IConfiguration configuration)
        //{
        //    connectionString = configuration.GetConnectionString("CovidConnection");
        //}

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        optionsBuilder.UseNpgsql(configuration.GetConnectionString("CovidConnection"));
        //    }
        //}

        //public CovidContext(IConfiguration configuration)
        //{
        //    connectionString = configuration.GetConnectionString("CovidConnection");
        //}

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //    => optionsBuilder.UseNpgsql(connectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<CitiesEntity>(entity =>
            {
                entity.ToTable("cities");

                entity.HasIndex(e => new { e.CountryId, e.Latitude, e.Longitude })
                    .HasName("cities_country_id_latitude_longitude_key")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.City).HasColumnName("city");

                entity.Property(e => e.Confirmed).HasColumnName("confirmed");

                entity.Property(e => e.CountryId).HasColumnName("country_id");

                entity.Property(e => e.DateCreate)
                    .HasColumnName("date_create")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.DateUpdate)
                    .HasColumnName("date_update")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Deaths).HasColumnName("deaths");

                entity.Property(e => e.Extras).HasColumnName("extras");

                entity.Property(e => e.Latitude)
                    .HasColumnName("latitude")
                    .HasColumnType("numeric(12,6)");

                entity.Property(e => e.Longitude)
                    .HasColumnName("longitude")
                    .HasColumnType("numeric(12,6)");

                entity.Property(e => e.Recovered).HasColumnName("recovered");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.Cities)
                    .HasForeignKey(d => d.CountryId)
                    .HasConstraintName("cities_country_id_fkey");
            });

            modelBuilder.Entity<CountryEntity>(entity =>
            {
                entity.ToTable("country");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Code).HasColumnName("code");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.Three).HasColumnName("three");

                entity.Property(e => e.Two).HasColumnName("two");
            });

            modelBuilder.Entity<LogsEntity>(entity =>
            {
                entity.ToTable("logs");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Category).HasColumnName("category");

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.EventId).HasColumnName("event_id");

                entity.Property(e => e.Level).HasColumnName("level");

                entity.Property(e => e.Message).HasColumnName("message");

                entity.Property(e => e.Username).HasColumnName("username");
            });

            modelBuilder.Entity<NewsEntity>(entity =>
            {
                entity.ToTable("news");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Content).HasColumnName("content");

                entity.Property(e => e.DateCreate)
                    .HasColumnName("date_create")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.DateUpdate)
                    .HasColumnName("date_update")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Lang).HasColumnName("lang");

                entity.Property(e => e.Source).HasColumnName("source");

                entity.Property(e => e.TextSource).HasColumnName("text_source");

                entity.Property(e => e.Title).HasColumnName("title");
            });

            modelBuilder.Entity<ProvinceEntity>(entity =>
            {
                entity.ToTable("province");

                entity.HasIndex(e => new { e.Country, e.Province, e.Latitude, e.Longitude })
                    .HasName("province_country_province_latitude_longitude_key")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Country).HasColumnName("country");

                entity.Property(e => e.Latitude)
                    .HasColumnName("latitude")
                    .HasColumnType("numeric(12,6)");

                entity.Property(e => e.Longitude)
                    .HasColumnName("longitude")
                    .HasColumnType("numeric(12,6)");

                entity.Property(e => e.Province).HasColumnName("province");
            });

            modelBuilder.Entity<SerieEntity>(entity =>
            {
                entity.ToTable("serie");

                entity.HasIndex(e => new { e.ProvinceId, e.Date })
                    .HasName("serie_province_id_date_key")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Confirmed).HasColumnName("confirmed");

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("date");

                entity.Property(e => e.DateCreate)
                    .HasColumnName("date_create")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.DateUpdate)
                    .HasColumnName("date_update")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Deaths).HasColumnName("deaths");

                entity.Property(e => e.DontUpdate).HasColumnName("dont_update");

                entity.Property(e => e.ProvinceId).HasColumnName("province_id");

                entity.Property(e => e.Recovered).HasColumnName("recovered");

                entity.HasOne(d => d.Province)
                    .WithMany(p => p.Serie)
                    .HasForeignKey(d => d.ProvinceId)
                    .HasConstraintName("serie_province_id_fkey");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
