using System;
using System.IO;
using System.Text;
using System.Threading;

namespace CryptoSoftLib
{
    /// <summary>
    /// Provides a high-level method to encrypt a file using a named mutex
    /// to prevent concurrent encryption operations.
    /// </summary>
    public static class CryptoSoft
    {
        // Named mutex to ensure that only one instance of this encryption process
        // can run at a time across the entire system.
        private static readonly Mutex mutex = new Mutex(false, "CryptoSoft_Mutex");

        /// <summary>
        /// Encrypts the specified file using the provided key.
        /// Utilizes XOR-based encryption and returns the time taken in milliseconds.
        /// </summary>
        /// <param name="filePath">The full path to the file that needs to be encrypted.</param>
        /// <param name="key">The encryption key (must be at least 8 characters long).</param>
        /// <returns>The elapsed encryption time in milliseconds, or -1 if encryption fails.</returns>
        public static int EncryptFile(string filePath, string key)
        {
            try
            {
                // Attempt to acquire the mutex without blocking.
                // If false, another instance is already running.
                if (!mutex.WaitOne(TimeSpan.Zero, true))
                {
                    Console.WriteLine("Another instance of CryptoSoft is already running.");
                    return -1; // Indicates that an instance is already active
                }

                // Validate that the encryption key is at least 8 characters (64 bits).
                if (string.IsNullOrEmpty(key) || key.Length < 8)
                {
                    throw new ArgumentException("Encryption key must be at least 64 bits (8 characters).");
                }

                Console.WriteLine($"Using encryption key: {key}");
                Console.WriteLine($"Attempting to encrypt file: {filePath}");

                // Create a FileManager to handle the file operations.
                var fileManager = new FileManager(filePath, key);
                int encryptionTime = fileManager.TransformFile();

                Console.WriteLine($"Time measured in EncryptFile(): {encryptionTime}ms");

                return encryptionTime;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CryptoSoft Error: {ex.Message}");
                return -1;
            }
            finally
            {
                // Release the mutex only if this process successfully owns it.
                if (mutex.WaitOne(0))
                {
                    mutex.ReleaseMutex();
                }
            }
        }
    }
}
