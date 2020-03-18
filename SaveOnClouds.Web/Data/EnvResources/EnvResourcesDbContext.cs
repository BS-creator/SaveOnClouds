using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace SaveOnClouds.Web.Data.EnvResources
{
    public class EnvResourcesDbContext : DbContext
    {
        public EnvResourcesDbContext()
        {
        }

        public EnvResourcesDbContext(DbContextOptions<EnvResourcesDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CloudAccounts> CloudAccounts { get; set; }
        public virtual DbSet<CloudResources> CloudResources { get; set; }
        public virtual DbSet<Tags> Tags { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            var configureNamedOptions = new ConfigureNamedOptions<ConsoleLoggerOptions>("", null);
            var optionsFactory = new OptionsFactory<ConsoleLoggerOptions>(new[] {configureNamedOptions},
                Enumerable.Empty<IPostConfigureOptions<ConsoleLoggerOptions>>());
            var optionsMonitor = new OptionsMonitor<ConsoleLoggerOptions>(optionsFactory,
                Enumerable.Empty<IOptionsChangeTokenSource<ConsoleLoggerOptions>>(),
                new OptionsCache<ConsoleLoggerOptions>());
            var loggerFactory = new LoggerFactory(new[] {new ConsoleLoggerProvider(optionsMonitor)},
                new LoggerFilterOptions {MinLevel = Microsoft.Extensions.Logging.LogLevel.Trace});

            optionsBuilder.UseLoggerFactory(loggerFactory)
                .EnableSensitiveDataLogging();
#endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CloudAccounts>(entity =>
            {
                entity.HasIndex(e => e.AwsaccountNumber)
                    .HasName("IX_CloudAccounts_AWSAccount");

                entity.HasIndex(e => new {e.Id, e.AccountName, e.AccountType, e.CreatorUserId})
                    .HasName("IX_CloudAccounts_GetAllCloudAccounts");

                entity.HasIndex(e => new {e.CreatorUserId, e.AccountType, e.AccountName, e.AwsaccountNumber, e.Id})
                    .HasName("IX_CloudAccounts_Id");

                entity.Property(e => e.AccountName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.AccountType).HasDefaultValueSql("((1))");

                entity.Property(e => e.AwsaccountNumber)
                    .HasColumnName("AWSAccountNumber")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.AwsregionName)
                    .HasColumnName("AWSRegionName")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.AwsroleArn)
                    .HasColumnName("AWSRoleArn")
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDateTimeUtc)
                    .HasColumnName("CreatedDateTimeUTC")
                    .HasColumnType("datetime");

                entity.Property(e => e.CreatorUserId)
                    .IsRequired()
                    .HasColumnName("CreatorUserID")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.ExternalId)
                    .IsRequired()
                    .HasColumnName("ExternalID")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.SourceAccountNumber)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CloudResources>(entity =>
            {
                entity.HasIndex(e => new
                    {
                        e.Id, e.Name, e.Location, e.InstanceType, e.Cost, e.State, e.StateReason, e.CloudResourceId,
                        e.ResourceType, e.IsMemberOfScalingGroup, e.CloudAccountId
                    })
                    .HasName("IX_CloudResource_CloudAccountId");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CloudResourceId)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Cost).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.InstanceType)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Location)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.ResourceType)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.State)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.StateReason)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(e => e.CloudAccount)
                    .WithMany(e => e.CloudResources)
                    .HasForeignKey(e => e.CloudAccountId);
            });

            modelBuilder.Entity<Tags>(entity =>
            {
                entity.ToTable("tags");

                entity.HasIndex(e => new {e.Id, e.Key, e.Value, e.Cloudresourceid})
                    .HasName("IX_Tags");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Cloudresourceid).HasColumnName("cloudresourceid");

                entity.Property(e => e.Key)
                    .IsRequired()
                    .HasColumnName("key")
                    .HasMaxLength(200);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("value")
                    .HasColumnType("nvarchar(max)");

                entity.HasOne(e => e.CloudResource)
                    .WithMany(e => e.Tags)
                    .HasForeignKey(e => e.Cloudresourceid);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        private void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            throw new NotImplementedException();
        }
    }
}