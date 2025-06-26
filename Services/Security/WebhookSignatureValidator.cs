using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace GiaNguyenCheck.Services.Security
{
    public class WebhookSignatureValidator
    {
        private readonly ILogger<WebhookSignatureValidator> _logger;

        public WebhookSignatureValidator(ILogger<WebhookSignatureValidator> logger)
        {
            _logger = logger;
        }

        public bool ValidateSignature(string payload, string signature, string secret, string algorithm = "sha256")
        {
            try
            {
                if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(secret))
                {
                    _logger.LogWarning("Invalid webhook signature parameters");
                    return false;
                }

                var computedSignature = ComputeSignature(payload, secret, algorithm);
                var isValid = CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(signature),
                    Encoding.UTF8.GetBytes(computedSignature));

                if (!isValid)
                {
                    _logger.LogWarning("Webhook signature validation failed. Expected: {Expected}, Received: {Received}", 
                        computedSignature, signature);
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating webhook signature");
                return false;
            }
        }

        public bool ValidateMoMoSignature(string payload, string signature, string secret)
        {
            try
            {
                // MoMo specific signature validation
                var rawSignature = $"{payload}&{secret}";
                var computedHash = ComputeMD5Hash(rawSignature);
                var isValid = string.Equals(computedHash, signature, StringComparison.OrdinalIgnoreCase);

                if (!isValid)
                {
                    _logger.LogWarning("MoMo webhook signature validation failed. Expected: {Expected}, Received: {Received}", 
                        computedHash, signature);
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating MoMo webhook signature");
                return false;
            }
        }

        public bool ValidateVNPAYSignature(string payload, string signature, string secret)
        {
            try
            {
                // VNPAY specific signature validation
                var rawSignature = $"{payload}&{secret}";
                var computedHash = ComputeSHA256Hash(rawSignature);
                var isValid = string.Equals(computedHash, signature, StringComparison.OrdinalIgnoreCase);

                if (!isValid)
                {
                    _logger.LogWarning("VNPAY webhook signature validation failed. Expected: {Expected}, Received: {Received}", 
                        computedHash, signature);
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating VNPAY webhook signature");
                return false;
            }
        }

        public bool ValidateStripeSignature(string payload, string signature, string secret)
        {
            try
            {
                // Stripe specific signature validation
                var timestamp = ExtractTimestampFromStripeSignature(signature);
                var signedPayload = $"{timestamp}.{payload}";
                var computedSignature = ComputeHMACSHA256(signedPayload, secret);
                var expectedSignature = $"t={timestamp},v1={computedSignature}";

                var isValid = string.Equals(signature, expectedSignature, StringComparison.OrdinalIgnoreCase);

                if (!isValid)
                {
                    _logger.LogWarning("Stripe webhook signature validation failed. Expected: {Expected}, Received: {Received}", 
                        expectedSignature, signature);
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Stripe webhook signature");
                return false;
            }
        }

        private string ComputeSignature(string payload, string secret, string algorithm)
        {
            return algorithm.ToLower() switch
            {
                "sha256" => ComputeHMACSHA256(payload, secret),
                "sha1" => ComputeHMACSHA1(payload, secret),
                "md5" => ComputeMD5Hash(payload + secret),
                _ => throw new ArgumentException($"Unsupported algorithm: {algorithm}")
            };
        }

        private string ComputeHMACSHA256(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(hashBytes).ToLower();
        }

        private string ComputeHMACSHA1(string data, string key)
        {
            using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(key));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(hashBytes).ToLower();
        }

        private string ComputeMD5Hash(string data)
        {
            using var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(hashBytes).ToLower();
        }

        private string ComputeSHA256Hash(string data)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(hashBytes).ToLower();
        }

        private string ExtractTimestampFromStripeSignature(string signature)
        {
            // Extract timestamp from Stripe signature format: "t=timestamp,v1=signature"
            var timestampMatch = System.Text.RegularExpressions.Regex.Match(signature, @"t=(\d+)");
            return timestampMatch.Success ? timestampMatch.Groups[1].Value : string.Empty;
        }

        public string GenerateSignature(string payload, string secret, string algorithm = "sha256")
        {
            try
            {
                return ComputeSignature(payload, secret, algorithm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating signature");
                return string.Empty;
            }
        }

        public bool ValidateTimestamp(string timestamp, int maxAgeSeconds = 300)
        {
            try
            {
                if (!long.TryParse(timestamp, out var timestampValue))
                {
                    return false;
                }

                var timestampDateTime = DateTimeOffset.FromUnixTimeSeconds(timestampValue);
                var currentTime = DateTimeOffset.UtcNow;
                var age = currentTime - timestampDateTime;

                if (age.TotalSeconds > maxAgeSeconds)
                {
                    _logger.LogWarning("Webhook timestamp is too old. Age: {Age}s, MaxAge: {MaxAge}s", 
                        age.TotalSeconds, maxAgeSeconds);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating timestamp");
                return false;
            }
        }
    }
} 