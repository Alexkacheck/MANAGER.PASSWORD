using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ManagerPassword.DAL;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace ManagerPassword.BLL
{
    public class PasswordManager
    {
        private readonly PasswordDatabase passwordDatabase;
        private string masterPassword;
        private const string PasswordKeySalt = "SaltForPasswordKey";
        private bool isMasterPasswordSet = false;
        private int userId;

        // Создаем логгер
        private static readonly ILogger Log = new LoggerConfiguration()
            .WriteTo.Console() // Указываем, что логи будут выводиться в консоль
            .CreateLogger();

        public PasswordManager()
        {
            passwordDatabase = new PasswordDatabase();
        }

        // Получение мастер-пароля
        public string GetMasterPassword()
        {
            if (!isMasterPasswordSet)
            {
                Console.Write("Введите мастер-пароль:");
                masterPassword = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(masterPassword))
                {
                    Log.Warning("Предупреждение: Пустой пароль может быть небезопасным.");
                }

                Log.Information("Мастер-пароль установлен.");
                isMasterPasswordSet = true;
            }

            return masterPassword;
        }

        // Проверка, установлен ли мастер-пароль
        public bool IsMasterPasswordSet => !string.IsNullOrWhiteSpace(masterPassword);

        // Получение идентификатора пользователя
        public int UserId => userId;

        // Установка мастер-пароля
        public void SetMasterPassword()
        {
            GetMasterPassword();
        }

        // Добавление новой записи о пароле
        public void AddPasswordEntry()
        {
            try
            {
                GetMasterPassword(); // Проверка наличия введенного мастер-пароля

                Log.Information("Введите веб-сайт:");
                string website = Console.ReadLine();

                Log.Information("Введите логин:");
                string username = Console.ReadLine();

                Log.Information("Введите пароль:");
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
                Log.Error($"Ошибка: {ex.Message}");
            }
            catch (Exception ex)
            {
                Log.Error($"Произошла ошибка при добавлении записи: {ex.Message}");
            }
        }

        // Шифрование пароля
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
                Log.Error($"Произошла ошибка при шифровании пароля: {ex.Message}");
                return null;
            }
        }

        // Просмотр записей о паролях
        public void ViewPasswordEntries()
        {
            try
            {
                Log.Information("Список сохраненных записей:");
                var entries = passwordDatabase.GetAllPasswords(UserId);

                foreach (var entry in entries)
                {
                    Log.Information($"Веб-сайт: {entry.Website,-20} Логин: {entry.Username}");

                    // Расшифровка и отображение пароля, если мастер-пароль установлен
                    if (IsMasterPasswordSet)
                    {
                        string decryptedPassword = DecryptPassword(entry.EncryptedPassword);
                        Log.Information($" Пароль: {decryptedPassword}");
                    }

                    Log.Information("Добавлена новая запись в базу данных"); // Переход на новую строку после каждой записи
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Произошла ошибка при просмотре записей: {ex.Message}");
            }
        }

        // Расшифровка пароля
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
                Log.Error($"Произошла ошибка при расшифровке пароля: {ex.Message}");
                return null;
            }
        }

        // Поиск записей о паролях по запросу
        public void SearchPasswordEntries(string query)
        {
            try
            {
                GetMasterPassword(); // Проверка наличия введенного мастер-пароля

                var searchResults = passwordDatabase.SearchPasswords(UserId, query);

                if (searchResults.Any())
                {
                    Log.Information("Результаты поиска:");
                    foreach (var entry in searchResults)
                    {
                        Log.Information($"Веб-сайт: {entry.Website,-20} Логин: {entry.Username}");
                    }
                }
                else
                {
                    Log.Information($"Запись с запросом '{query}' не найдена.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Произошла ошибка при поиске записей: {ex.Message}");
            }
        }
    }
}