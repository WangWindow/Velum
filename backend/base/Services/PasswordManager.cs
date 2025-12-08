using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Velum.Core.Interfaces;

namespace Velum.Base.Services;

public class PasswordManager : IPasswordManager
{
    private readonly ILogger<PasswordManager> _logger;
    private readonly bool _isEncryptionEnabled;
    private readonly string? _publicKey;
    private readonly string? _privateKey;

    public bool IsEncryptionEnabled => _isEncryptionEnabled;

    public PasswordManager(IConfiguration configuration, ILogger<PasswordManager> logger)
    {
        _logger = logger;

        // 1. Check if encryption is explicitly enabled in config
        _isEncryptionEnabled = configuration.GetValue<bool>("Rsa:Enabled");

        if (!_isEncryptionEnabled)
        {
            _logger.LogWarning("RSA Encryption is DISABLED in configuration. Passwords will be processed as plain text.");
            return;
        }

        // 2. Try to load keys from config
        _publicKey = configuration["Rsa:PublicKey"];
        _privateKey = configuration["Rsa:PrivateKey"];

        // 3. If keys are missing, generate them
        if (string.IsNullOrWhiteSpace(_publicKey) || string.IsNullOrWhiteSpace(_privateKey))
        {
            _logger.LogWarning("RSA Keys are enabled but missing in configuration. Generating temporary keys...");

            using var rsa = RSA.Create(2048);
            _privateKey = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());
            _publicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());

            // 自动写入 appsettings.Local.json
            try
            {
                var localPath = Path.Combine(AppContext.BaseDirectory, "appsettings.local.json");
                string json = File.Exists(localPath) ? File.ReadAllText(localPath) : "{}";
                var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement.Clone();
                using var ms = new MemoryStream();
                using (var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = true }))
                {
                    var dic = root.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
                    dic["Rsa"] = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>($"{{\"Enabled\": true, \"PublicKey\": \"{_publicKey}\", \"PrivateKey\": \"{_privateKey}\"}}");
                    writer.WriteStartObject();
                    foreach (var kv in dic)
                    {
                        writer.WritePropertyName(kv.Key);
                        kv.Value.WriteTo(writer);
                    }
                    writer.WriteEndObject();
                }
                File.WriteAllText(localPath, System.Text.Encoding.UTF8.GetString(ms.ToArray()));
                Console.WriteLine($"RSA keys have been automatically written to {localPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to automatically write to appsettings.local.json: {ex.Message}");
            }

            // 4. Print keys to terminal for the developer
            var border = new string('=', 50);
            Console.WriteLine($"\n{border}");
            Console.WriteLine("RSA KEYS GENERATED (ACTION REQUIRED)");
            Console.WriteLine($"{border}");
            Console.WriteLine("Add these to your appsettings.json or Environment Variables to persist them.");
            Console.WriteLine("\n[Rsa:PublicKey]");
            Console.WriteLine(_publicKey);
            Console.WriteLine("\n[Rsa:PrivateKey]");
            Console.WriteLine(_privateKey);
            Console.WriteLine($"\n{border}\n");
        }
        else
        {
            _logger.LogInformation("RSA Encryption Enabled. Keys loaded from configuration.");
        }
    }

    public string GetPublicKey()
    {
        return _publicKey ?? string.Empty;
    }

    public string ProcessPassword(string inputPassword)
    {
        if (string.IsNullOrEmpty(inputPassword)) return inputPassword;

        if (!_isEncryptionEnabled)
        {
            return inputPassword;
        }

        try
        {
            return RsaDecrypt(inputPassword, _privateKey!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt password.");
            throw new InvalidOperationException("Password decryption failed.", ex);
        }
    }

    public bool VerifyPassword(string inputPassword, string storedPassword)
    {
        // 1. Process input password (decrypt if needed)
        string plainInput = ProcessPassword(inputPassword);

        // 2. Compare with stored password (which is plain text as per requirement)
        return plainInput == storedPassword;
    }

    private static string RsaDecrypt(string base64Cipher, string privateKeyPem)
    {
        using var rsa = RSA.Create();

        try
        {
            // Try treating as Base64 first (our generated format)
            var keyBytes = Convert.FromBase64String(privateKeyPem);
            rsa.ImportPkcs8PrivateKey(keyBytes, out _);
        }
        catch
        {
            // Fallback to PEM if user pasted PEM format
            if (privateKeyPem.Contains("-----"))
            {
                rsa.ImportFromPem(privateKeyPem.ToCharArray());
            }
            else
            {
                throw;
            }
        }

        var cipherBytes = Convert.FromBase64String(base64Cipher);
        var plainBytes = rsa.Decrypt(cipherBytes, RSAEncryptionPadding.Pkcs1);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
