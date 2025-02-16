using System;
using System.IO;
using System.Text;

namespace CryptoSoftLib
{
    public static class CryptoSoft
    {
        /// <summary>
        /// Encrypts or decrypts a file using XOR.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="key">Encryption key (at least 8 characters).</param>
        /// <returns>Encryption time in ms (>0) or -1 if an error occurs.</returns>
        public static int EncryptFile(string filePath, string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key) || key.Length < 8)
                {
                    throw new ArgumentException("Encryption key must be at least 64 bits (8 characters).");
                }

                Console.WriteLine($"🔑 Utilisation de la clé : {key}");
                Console.WriteLine($"📂 Tentative de cryptage du fichier : {filePath}");

                var fileManager = new FileManager(filePath, key);
                int encryptionTime = fileManager.TransformFile();

                // 🔍 Log pour vérifier si le temps de cryptage est bien mesuré
                Console.WriteLine($"⏳ Temps mesuré dans EncryptFile(): {encryptionTime}ms");

                if (encryptionTime == 0)
                {
                    Console.WriteLine("⚠️ Problème détecté : temps de cryptage à 0ms !");
                }

                return encryptionTime;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CryptoSoft Error: {ex.Message}");
                return -1;
            }
        }
    }
}
