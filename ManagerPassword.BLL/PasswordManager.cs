using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ManagerPassword.DAL;

namespace ManagerPassword.BLL
{
    public class PasswordManager
    {
        private readonly PasswordDatabase passwordDatabase;
        private string masterPassword;
        private const string PasswordKeySalt = "SaltForPasswordKey";
      


        public PasswordManager()
        {
            passwordDatabase = new PasswordDatabase();
        }

        public void SetMasterPassword()
        {
            try
            {
                Console.WriteLine("Введите мастер-пароль:");
                masterPassword = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(masterPassword))
                {
                    Console.WriteLine("Предупреждение: Пустой пароль может быть небезопасным.");
                }

                Console.WriteLine("Мастер-пароль установлен.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при установке мастер-пароля: {ex.Message}");
            }
        }

        public void AddPasswordEntry()
        {
            try
            {
                Console.WriteLine("Введите веб-сайт:");
                string website = Console.ReadLine();

                Console.WriteLine("Введите логин:");
                string username = Console.ReadLine();

                Console.WriteLine("Введите пароль:");
                string password = Console.ReadLine();

                // Проверки на пустые строки или недопустимые символы
                if (string.IsNullOrWhiteSpace(website) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    throw new ArgumentException("Некорректный ввод. Пожалуйста, убедитесь, что введены все данные.");
                }

                string encryptedPassword = EncryptPassword(password);

                PasswordEntry entry = new PasswordEntry
                {
                    Website = website,
                    Username = username,
                    EncryptedPassword = encryptedPassword
                };

                passwordDatabase.AddPassword(entry);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при добавлении записи: {ex.Message}");
            }
        }

        private string EncryptPassword(string password)
        {
            try
            {
                using (AesManaged aesAlg = new AesManaged())
                {
                    Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(masterPassword, Encoding.UTF8.GetBytes(PasswordKeySalt));
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

                    using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                    {
                        using (MemoryStream msEncrypt = new MemoryStream())
                        {
                            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                            {
                                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                                {
                                    swEncrypt.Write(password);
                                }
                            }

                            return Convert.ToBase64String(msEncrypt.ToArray());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при шифровании пароля: {ex.Message}");
                return null;
            }
        }
        public void ViewPasswordEntries()
        {
            try
            {
                Console.WriteLine("Список сохраненных записей:");
                var entries = passwordDatabase.GetAllPasswords();
                foreach (var entry in entries)
                {
                    Console.WriteLine($"Веб-сайт: {entry.Website,-20} Логин: {entry.Username}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при просмотре записей: {ex.Message}");
            }
        }

        public void SearchPasswordEntries(string query)
        {
            try
            {
                var searchResults = passwordDatabase.SearchPasswords(query);

                if (searchResults.Any())
                {
                    Console.WriteLine("Результаты поиска:");
                    foreach (var entry in searchResults)
                    {
                        Console.WriteLine($"Веб-сайт: {entry.Website,-20} Логин: {entry.Username}");
                    }
                }
                else
                {
                    Console.WriteLine("Записей не найдено.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при поиске записей: {ex.Message}");
            }
        }
    }
}