using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using EasySaveLogs;
using EasySave.Core.Utils;

namespace EasySave.Core.Services
{
    public class EncryptionService
    {
        /// <summary>
        /// Chiffre ou déchiffre un fichier en utilisant un XOR bit par bit.
        /// </summary>
        /// <param name="filePath">Chemin du fichier à crypter</param>
        /// <param name="key">Clé de cryptage (minimum 64 bits, soit 8 caractères)</param>
        /// <returns>Temps de cryptage en ms (>0) ou -1 en cas d'erreur</returns>
        public static int EncryptFile(string filePath, string key)
        {
            try
            {
                // Vérifier si l'extension du fichier est autorisée
                List<string> extensionsAutorisees = Configuration.GetCryptoExtensions();
                if (!extensionsAutorisees.Contains(Path.GetExtension(filePath).ToLower()))
                {
                    return 0; // Pas de cryptage nécessaire
                }

                // Vérifier la longueur de la clé (64 bits = 8 caractères minimum)
                if (string.IsNullOrEmpty(key) || key.Length < 8)
                {
                    throw new ArgumentException("La clé de cryptage doit faire au moins 64 bits (8 caractères).");
                }

                var stopwatch = Stopwatch.StartNew();

                // Lire le fichier
                byte[] fileBytes = File.ReadAllBytes(filePath);
                byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(key);

                // Appliquer le chiffrement XOR
                byte[] encryptedBytes = XorEncrypt(fileBytes, keyBytes);

                // Sauvegarder le fichier crypté
                File.WriteAllBytes(filePath, encryptedBytes);

                stopwatch.Stop();
                int encryptionTimeMs = (int)stopwatch.ElapsedMilliseconds;

                // Logger le succès
                Logger.GetInstance(Configuration.GetLogDirectory(), Configuration.GetLogFormat()).LogAction(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    BackupName = "Cryptage",
                    SourceFilePath = filePath,
                    TargetFilePath = "N/A",
                    FileSize = new FileInfo(filePath).Length,
                    TransferTimeMs = 0,
                    EncryptionTimeMs = encryptionTimeMs,
                    Status = "Fichier crypté avec succès",
                    Level = Logger.LogLevel.Info
                });

                return encryptionTimeMs;
            }
            catch (Exception ex)
            {
                // Logger l'erreur
                Logger.GetInstance(Configuration.GetLogDirectory(), Configuration.GetLogFormat()).LogAction(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    BackupName = "Erreur Cryptage",
                    SourceFilePath = filePath,
                    TargetFilePath = "N/A",
                    FileSize = 0,
                    TransferTimeMs = 0,
                    EncryptionTimeMs = -1,
                    Status = $"Erreur de cryptage : {ex.Message}",
                    Level = Logger.LogLevel.Error
                });

                return -1;
            }
        }

        /// <summary>
        /// Applique un XOR bit par bit entre les données et la clé.
        /// </summary>
        /// <param name="data">Données du fichier</param>
        /// <param name="key">Clé de chiffrement</param>
        /// <returns>Données chiffrées/déchiffrées</returns>
        private static byte[] XorEncrypt(byte[] data, byte[] key)
        {
            byte[] output = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                output[i] = (byte)(data[i] ^ key[i % key.Length]);
            }
            return output;
        }
    }
}
