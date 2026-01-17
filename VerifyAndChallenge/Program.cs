using System.Security.Cryptography;
using System.Text;

var bytes = RandomNumberGenerator.GetBytes(64);
var verifier = Convert.ToBase64String(bytes)
   .Replace("+","-").Replace("/","_").Replace("=","");

var hash = SHA256.HashData(Encoding.ASCII.GetBytes(verifier));
var challenge = Convert.ToBase64String(hash)
   .Replace("+","-").Replace("/","_").Replace("=","");

Console.WriteLine("Verifier");
Console.WriteLine(verifier);
Console.WriteLine("Challenge:");
Console.WriteLine(challenge);