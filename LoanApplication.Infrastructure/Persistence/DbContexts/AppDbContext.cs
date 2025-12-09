using System.Reflection;
using LoanApplication.Application.Common.Contracts.Abstractions;
using LoanApplication.Domain.Attributes;
using LoanApplication.Domain.Entities;
using LoanApplication.Infrastructure.Persistence.Comparators;
using LoanApplication.Infrastructure.Persistence.Configurations;
using LoanApplication.Infrastructure.Persistence.Converters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace LoanApplication.Infrastructure.Persistence.DbContexts;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Address> Addresses { get; set; }
    public DbSet<KycVerification> KycVerifications { get; set; }
    public DbSet<Domain.Entities.LoanApplication> LoanApplications { get; set; }
    public DbSet<LoanApplicationHistory> LoanApplicationHistories { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        var encryptionProvider = this.GetService<IEncryptionProvider>();
        modelBuilder.ApplyConfiguration(new KycVerificationConfiguration(encryptionProvider));

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType != typeof(string)) continue;

                var memberInfo = property.PropertyInfo ?? (MemberInfo)property.FieldInfo;
                if (memberInfo == null || !Attribute.IsDefined(memberInfo, typeof(EncryptedAttribute))) continue;

                property.SetValueConverter(new EncryptedConverter(encryptionProvider));
                property.SetValueComparer(new EncryptedConverterComparer());
            }
        }
    }

}