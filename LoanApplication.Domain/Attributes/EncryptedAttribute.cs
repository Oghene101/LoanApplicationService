namespace LoanApplication.Domain.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class EncryptedAttribute : Attribute
{
}