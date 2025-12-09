using LoanApplication.Application.Common.Contracts.Abstractions;
using LoanApplication.Domain.Entities;
using LoanApplication.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanApplication.Infrastructure.Persistence.Configurations;

public class KycVerificationConfiguration(
    IEncryptionProvider encryptionProvider) : IEntityTypeConfiguration<KycVerification>
{
    public void Configure(EntityTypeBuilder<KycVerification> builder)
    {
        builder.HasIndex(k => k.UserId).IsUnique();
        builder.HasIndex(k => k.BvnHash).IsUnique();
        builder.HasIndex(k => k.NinHash).IsUnique();
        builder.Property(k => k.BvnCipher).HasConversion(new EncryptedConverter(encryptionProvider));
        builder.Property(k => k.NinCipher).HasConversion(new EncryptedConverter(encryptionProvider));
    }
}