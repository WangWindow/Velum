namespace Velum.Core.Interfaces;

public interface IPasswordManager
{
    bool IsEncryptionEnabled { get; }
    string GetPublicKey();
    string ProcessPassword(string inputPassword);
    bool VerifyPassword(string inputPassword, string storedPassword);
}
