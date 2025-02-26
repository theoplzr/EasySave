using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CryptoSoftLib
{
    /// <summary>
    /// Responsible for managing file operations such as
    /// checking file existence and performing XOR-based encryption.
    /// </summary>
    public class FileManager
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FileManager"/> with a file path and encryption key.
        /// </summary>
        /// <param name="path">The path of the file to be encrypted.</param>
        /// <param name="key">The encryption key to be used.</param>
        public FileManager(string path, string key)
        {
            FilePath = path;
            Key = key;
        }

        /// <summary>
        /// Gets the path of the file to be processed.
        /// </summary>
        private string FilePath { get; }

        /// <summary>
        /// Gets the encryption key.
        /// </summary>
        private string Key { get; }

        /// <summary>
        /// Orchestrates the XOR encryption process, measuring and returning
        /// the time taken in milliseconds. Returns -1 if the operation fails.
        /// </summary>
        /// <returns>The elapsed time in milliseconds, or -1 if encryption fails.</returns>
        public int TransformFile()
        {
            // Check if the file exists before proceeding
            if (!CheckFile()) 
                return -1;

            Console.WriteLine($"Starting encryption for file: {FilePath}");

            // Read all bytes from the original file
            byte[] originalBytes = File.ReadAllBytes(FilePath);
            Console.WriteLine($"File size before encryption: {originalBytes.Length} bytes");

            // Start measuring the encryption time
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Convert the key into bytes and XOR-encrypt the file data
            var keyBytes = ConvertToByte(Key);
            var encryptedBytes = XorMethod(originalBytes, keyBytes);

            // Write the encrypted data back to the original file
            File.WriteAllBytes(FilePath, encryptedBytes);

            // Stop the timer
            stopwatch.Stop();
            long elapsedTicks = stopwatch.ElapsedTicks;
            int elapsedMilliseconds = (int)stopwatch.ElapsedMilliseconds;

            // Validate that the file content changed after encryption
            byte[] newBytes = File.ReadAllBytes(FilePath);
            if (originalBytes.SequenceEqual(newBytes))
            {
                Console.WriteLine("Warning: The file appears unchanged after encryption!");
            }
            else
            {
                Console.WriteLine("The file has been successfully modified after encryption.");
            }

            Console.WriteLine($"File '{FilePath}' has been encrypted and saved.");
            Console.WriteLine($"Time measured in TransformFile(): {elapsedMilliseconds}ms ({elapsedTicks} ticks)");

            // Return at least 1ms to avoid returning 0 in fast operations
            return elapsedMilliseconds > 0 ? elapsedMilliseconds : 1; 
        }

        /// <summary>
        /// Checks if the specified file exists on the disk.
        /// </summary>
        /// <returns>True if the file exists, otherwise false.</returns>
        private bool CheckFile()
        {
            if (File.Exists(FilePath))
                return true;

            Console.WriteLine("File not found.");
            Thread.Sleep(1000); // Give the user a moment to see the message
            return false;
        }

        /// <summary>
        /// Converts a string to its corresponding byte array (UTF8 encoded).
        /// </summary>
        /// <param name="text">The input text to convert.</param>
        /// <returns>A byte array representing the UTF8-encoded string.</returns>
        private static byte[] ConvertToByte(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }

        /// <summary>
        /// XOR-based encryption method, applying the key to every byte of the file.
        /// </summary>
        /// <param name="fileBytes">The original file bytes.</param>
        /// <param name="keyBytes">The encryption key as bytes.</param>
        /// <returns>A new byte array containing the XOR-encrypted data.</returns>
        private static byte[] XorMethod(IReadOnlyList<byte> fileBytes, IReadOnlyList<byte> keyBytes)
        {
            var result = new byte[fileBytes.Count];

            for (var i = 0; i < fileBytes.Count; i++)
            {
                result[i] = (byte)(fileBytes[i] ^ keyBytes[i % keyBytes.Count]);
            }

            return result;
        }
    }
}
