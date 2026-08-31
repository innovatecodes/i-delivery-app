namespace IDelivery.Application.Abstractions.Security;

public interface ISecureTokenGenerator
{
    string Generate(int length = 32);
}