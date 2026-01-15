using System.Security.Cryptography;
using System.Text.RegularExpressions;
using CarRentalApi.BuildingBlocks;
using CarRentalApi.Modules.Customers.Domain.Errors;

namespace CarRentalApi.Modules.Customers.Domain.ValueObjects;

public sealed record Credentials {

   public string PasswordHash { get; private set; } = string.Empty;
   public string PasswordSalt { get; private set; } = string.Empty;

   private const int SaltBytes = 16;
   private const int HashBytes = 32;
   private const int FixedIterations = 120_000;

   // length: 6..25
   private const int MinLen = 6;
   private const int MaxLen = 25;

   // ✅ at least: 1 lower, 1 upper, 1 digit, 1 special, and only "reasonable" chars
   // Special = anything that is not letter/digit (from allowed set below)
   private static readonly Regex PasswordRegex = new(
      pattern: @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9])[A-Za-z\d[^ \t\r\n]]+$",
      options: RegexOptions.Compiled
   );

   // EF Core
   private Credentials() { }

   private Credentials(string hash, string salt) {
      PasswordHash = hash;
      PasswordSalt = salt;
   }

   // ------------------------------------------------------------
   // Factory (from plain password)
   // ------------------------------------------------------------
   // Register: create new Credentials from plain password
   public static Result<Credentials> Create(string plainPassword) {
      if (string.IsNullOrWhiteSpace(plainPassword))
         return Result<Credentials>.Failure(CredentialsErrors.PasswordEmpty);

      if (!IsValidPassword(plainPassword))
         return Result<Credentials>.Failure(CredentialsErrors.PasswordPolicyViolation);

      
      var saltBytes = RandomNumberGenerator.GetBytes(SaltBytes);
      var hashBytes = Hash(plainPassword, saltBytes, FixedIterations);
      
      return Result<Credentials>.Success(
         new Credentials(
            Convert.ToBase64String(hashBytes),
            Convert.ToBase64String(saltBytes)
         )
      );
   }

   // Login: Verify password
   public Result VerifyPassword(string plainPassword) {
      if (string.IsNullOrWhiteSpace(plainPassword))
         return Result.Failure(CredentialsErrors.PasswordEmpty);

      var saltBytes = Convert.FromBase64String(PasswordSalt);
      var expectedHash = Convert.FromBase64String(PasswordHash);
      var actualHash = Hash(plainPassword, saltBytes, FixedIterations);

      return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash)
         ? Result.Success()
         : Result.Failure(CredentialsErrors.InvalidCredentials);
   }

   // Change password
   public Result<Credentials> ChangePassword(string newPlainPassword) =>
      Create(newPlainPassword);

   private static bool IsValidPassword(string password) {
      if (password.Length < MinLen || password.Length > MaxLen)
         return false;

      // Reject whitespace explicitly (common policy)
      if (password.Any(char.IsWhiteSpace))
         return false;

      // At least one of each required category.
      // (We enforce via regex, length via code.)
      return PasswordRegex.IsMatch(password);
   }

   //--- Hashing with PBKDF2-HMAC-SHA256 ---
   private static byte[] Hash(string password, byte[] salt, int iterations) {
      return Rfc2898DeriveBytes.Pbkdf2(
         password: password,
         salt: salt,
         iterations: iterations,
         hashAlgorithm: HashAlgorithmName.SHA256,
         outputLength: HashBytes
      );
   }
}
