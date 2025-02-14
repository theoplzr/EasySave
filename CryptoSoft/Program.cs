using System;
using System.IO;
using System.Text;

class CryptoSoft
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: cryptosoft <fichier> <clé>");
            Environment.Exit(-1);
        }

        string filePath = args[0];
        string key = args[1];

        if (!File.Exists(filePath))
        {
            Console.WriteLine("❌ Fichier introuvable.");
            Environment.Exit(-1);
        }

        if (key.Length < 8)
        {
            Console.WriteLine("❌ La clé doit contenir au moins 8 caractères.");
            Environment.Exit(-1);
        }

        try
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] encryptedBytes = XorEncrypt(fileBytes, keyBytes);
            
            File.WriteAllBytes(filePath, encryptedBytes);
            Console.WriteLine($"✅ Fichier {filePath} crypté avec succès !");
            Environment.Exit(fileBytes.Length);  // Retourne la taille du fichier
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur : {ex.Message}");
            Environment.Exit(-1);
        }
    }

    static byte[] XorEncrypt(byte[] data, byte[] key)
    {
        byte[] output = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            output[i] = (byte)(data[i] ^ key[i % key.Length]);
        }
        return output;
    }
}
