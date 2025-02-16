using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CryptoSoftLib
{
    public class FileManager
    {
        private string FilePath { get; }
        private string Key { get; }

        public FileManager(string path, string key)
        {
            FilePath = path;
            Key = key;
        }

        /// <summary>
        /// Vérifie si le fichier existe
        /// </summary>
        private bool CheckFile()
        {
            if (File.Exists(FilePath))
                return true;

            Console.WriteLine("❌ Fichier introuvable.");
            Thread.Sleep(1000);
            return false;
        }

        /// <summary>
        /// Encrypts the file with XOR encryption
        /// </summary>
        public int TransformFile()
        {
            if (!CheckFile()) return -1;

            Console.WriteLine($"🔍 Début du chiffrement du fichier : {FilePath}");

            byte[] originalBytes = File.ReadAllBytes(FilePath);
            Console.WriteLine($"📏 Taille du fichier avant chiffrement : {originalBytes.Length} octets");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); // ⏳ Démarrer la mesure du temps AVANT toute opération

            var keyBytes = ConvertToByte(Key);
            var encryptedBytes = XorMethod(originalBytes, keyBytes);

            File.WriteAllBytes(FilePath, encryptedBytes);

            stopwatch.Stop();
            long elapsedTicks = stopwatch.ElapsedTicks; // 🔍 Mesurer en ticks pour plus de précision
            int elapsedMilliseconds = (int)stopwatch.ElapsedMilliseconds;

            // 🔍 Vérifie si le fichier a bien été modifié après chiffrement
            byte[] newBytes = File.ReadAllBytes(FilePath);
            if (originalBytes.SequenceEqual(newBytes))
            {
                Console.WriteLine("⚠️ Problème : Le fichier semble inchangé après cryptage !");
            }
            else
            {
                Console.WriteLine("✅ Le fichier a bien été modifié après cryptage.");
            }

            Console.WriteLine($"✅ Fichier {FilePath} chiffré et sauvegardé.");
            Console.WriteLine($"⏳ Temps mesuré dans TransformFile(): {elapsedMilliseconds}ms ({elapsedTicks} ticks)");

            return elapsedMilliseconds > 0 ? elapsedMilliseconds : 1; // ✅ Évite les valeurs à 0
        }

        /// <summary>
        /// Convertit une chaîne en tableau d'octets
        /// </summary>
        private static byte[] ConvertToByte(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }

        /// <summary>
        /// Algorithme XOR pour le cryptage
        /// </summary>
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
