using LoanApplication.Application.Common.Contracts.Abstractions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LoanApplication.Infrastructure.Persistence.Converters;

public class EncryptedConverter(
    IEncryptionProvider encryptionProvider) : ValueConverter<string, string>(
    v => encryptionProvider.Encrypt(v),
    v => encryptionProvider.Decrypt(v))
{
}