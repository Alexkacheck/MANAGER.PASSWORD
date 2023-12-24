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
        private bool isMasterPasswordSet = false;
        private int userId;

        public PasswordManager()
        {
            passwordDatabase = new PasswordDatabase();
        }

        public string GetMasterPassword()
        {
            if (!isMasterPasswordSet)
            {
                Console.WriteLine("Введите мастер-пароль:");
                masterPassword = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(masterPassword))
                {
                    Console.WriteLine("Предупреждение: Пустой пароль может быть небезопасным.");
                }

                Console.WriteLine("Мастер-пароль установлен.");
                isMasterPasswordSet = true;
            }

            return masterPassword;
        }

        public bool IsMasterPasswordSet => !string.IsNullOrWhiteSpace(masterPassword);
        public int UserId => userId;

        public void SetMasterPassword()
        {
            GetMasterPassword();
        }

        public void AddPasswordEntry()
        {
            try
            {
                GetMasterPassword(); // Проверка наличия введенного мастер-пароля

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
                    EncryptedPassword = encryptedPassword,
                    UserId = UserId // Добавим UserId
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
                var entries = passwordDatabase.GetAllPasswords(UserId);

                foreach (var entry in entries)
                {
                    Console.Write($"Веб-сайт: {entry.Website,-20} Логин: {entry.Username}");

                    // Расшифровка и отображение пароля, если мастер-пароль установлен
                    if (IsMasterPasswordSet)
                    {
                        string decryptedPassword = DecryptPassword(entry.EncryptedPassword);
                        Console.Write($" Пароль: {decryptedPassword}");
                    }

                    Console.WriteLine(); // Переход на новую строку после каждой записи
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при просмотре записей: {ex.Message}");
            }
        }

        private string DecryptPassword(string encryptedPassword)
        {
            try
            {
                using (AesManaged aesAlg = new AesManaged())
                {
                    Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(masterPassword, Encoding.UTF8.GetBytes(PasswordKeySalt));
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                    {
                        using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedPassword)))
                        {
                            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                            {
                                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                                {
                                    return srDecrypt.ReadToEnd();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при расшифровке пароля: {ex.Message}");
                return null;
            }
        }

        public void SearchPasswordEntries(string query)
        {
            try
            {
                GetMasterPassword(); // Проверка наличия введенного мастер-пароля

                var searchResults = passwordDatabase.SearchPasswords(UserId, query);

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
                    Console.WriteLine($"Запись с запросом '{query}' не найдена.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при поиске записей: {ex.Message}");
            }
        }
    }
}