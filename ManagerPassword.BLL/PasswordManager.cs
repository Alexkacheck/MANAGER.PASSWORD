// Подключение необходимых пространств имен
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

// Объявление пространства имен и класса PasswordManager в нем
namespace ManagerPassword.BLL
{
    // Объявление класса PasswordManager
    public class PasswordManager
    {
        // Закрытое поле, содержащее экземпляр класса PasswordDatabase
        private readonly PasswordDatabase passwordDatabase;
        // Закрытое поле, содержащее мастер-пароль
        private string masterPassword;
        // Константа для соли, используемой при генерации ключа для шифрования пароля
        private const string PasswordKeySalt = "SaltForPasswordKey";
        // Флаг, указывающий, установлен ли мастер-пароль
        private bool isMasterPasswordSet = false;
        // Переменная для хранения идентификатора пользователя
        private int userId;

        // Создание статического логгера с использованием библиотеки Serilog
        private static readonly ILogger Log = new LoggerConfiguration()
            .WriteTo.Console() // Указываем, что логи будут выводиться в консоль
            .CreateLogger();

        // Конструктор класса PasswordManager
        public PasswordManager()
        {
            // Инициализация экземпляра класса PasswordDatabase
            passwordDatabase = new PasswordDatabase();
        }

        // Метод для получения мастер-пароля
        public string GetMasterPassword()
        {
            // Проверка, установлен ли уже мастер-пароль
            if (!isMasterPasswordSet)
            {
                // Вывод запроса на ввод мастер-пароля
                Console.Write("Введите мастер-пароль:");
                // Считывание введенного мастер-пароля
                masterPassword = Console.ReadLine();

                // Проверка наличия введенного мастер-пароля
                if (string.IsNullOrWhiteSpace(masterPassword))
                {
                    // Вывод предупреждения о небезопасности пустого пароля
                    Log.Warning("Предупреждение: Пустой пароль может быть небезопасным.");
                }

                // Логирование информации об установке мастер-пароля
                Log.Information("Мастер-пароль установлен.");
                // Установка флага, указывающего на установку мастер-пароля
                isMasterPasswordSet = true;
            }

            // Возврат установленного мастер-пароля
            return masterPassword;
        }

        // Свойство, возвращающее true, если мастер-пароль установлен
        public bool IsMasterPasswordSet => !string.IsNullOrWhiteSpace(masterPassword);

        // Свойство, возвращающее идентификатор пользователя
        public int UserId => userId;

        // Метод для установки мастер-пароля
        public void SetMasterPassword()
        {
            // Вызов метода для получения мастер-пароля
            GetMasterPassword();
        }

        // Метод для добавления новой записи о пароле
        public void AddPasswordEntry()
        {
            try
            {
                // Вызов метода для получения мастер-пароля и проверки его наличия
                GetMasterPassword();

                // Вывод информации о вводе веб-сайта
                Log.Information("Введите веб-сайт:");
                // Считывание введенного веб-сайта
                string website = Console.ReadLine();

                // Вывод информации о вводе логина
                Log.Information("Введите логин:");
                // Считывание введенного логина
                string username = Console.ReadLine();

                // Вывод информации о вводе пароля
                Log.Information("Введите пароль:");
                // Считывание введенного пароля
                string password = Console.ReadLine();

                // Проверки на пустые строки или недопустимые символы
                if (string.IsNullOrWhiteSpace(website) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    // Генерация исключения с сообщением о некорректном вводе
                    throw new ArgumentException("Некорректный ввод. Пожалуйста, убедитесь, что введены все данные.");
                }

                // Шифрование введенного пароля
                string encryptedPassword = EncryptPassword(password);

                // Создание экземпляра класса PasswordEntry с введенными данными
                PasswordEntry entry = new PasswordEntry
                {
                    Website = website,
                    Username = username,
                    EncryptedPassword = encryptedPassword,
                    UserId = UserId // Добавление идентификатора пользователя
                };

                // Добавление записи о пароле в базу данных
                passwordDatabase.AddPassword(entry);
            }
            // Обработка исключения ArgumentException
            catch (ArgumentException ex)
            {
                // Логирование ошибки с сообщением исключения
                Log.Error($"Ошибка: {ex.Message}");
            }
            // Обработка других исключений
            catch (Exception ex)
            {
                // Логирование ошибки с сообщением исключения
                Log.Error($"Произошла ошибка при добавлении записи: {ex.Message}");
            }
        }

        // Метод для шифрования пароля
        private string EncryptPassword(string password)
        {
            try
            {
                // Использование using для автоматического освобождения ресурсов
                using (AesManaged aesAlg = new AesManaged())
                {
                    // Генерация ключа для шифрования на основе мастер-пароля и соли
                    Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(masterPassword, Encoding.UTF8.GetBytes(PasswordKeySalt));
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

                    // Использование using для автоматического освобождения ресурсов
                    using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                    {
                        // Использование using для автоматического освобождения ресурсов
                        using (MemoryStream msEncrypt = new MemoryStream())
                        {
                            // Использование using для автоматического освобождения ресурсов
                            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                            {
                                // Использование using для автоматического освобождения ресурсов
                                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                                {
                                    // Запись зашифрованного пароля в поток
                                    swEncrypt.Write(password);
                                }
                            }

                            // Возврат зашифрованного пароля в виде строки
                            return Convert.ToBase64String(msEncrypt.ToArray());
                        }
                    }
                }
            }
            // Обработка исключений
            catch (Exception ex)
            {
                // Логирование ошибки с сообщением исключения
                Log.Error($"Произошла ошибка при шифровании пароля: {ex.Message}");
                // Возврат null в случае ошибки
                return null;
            }
        }

        // Метод для просмотра записей о паролях
        public void ViewPasswordEntries()
        {
            try
            {
                // Вывод информации о просмотре сохраненных записей
                Log.Information("Список сохраненных записей:");
                // Получение всех записей о паролях из базы данных для текущего пользователя
                var entries = passwordDatabase.GetAllPasswords(UserId);

                // Перебор и вывод каждой записи
                foreach (var entry in entries)
                {
                    // Вывод информации о веб-сайте и логине
                    Log.Information($"Веб-сайт: {entry.Website,-20} Логин: {entry.Username}");

                    // Расшифровка и вывод пароля, если мастер-пароль установлен
                    if (IsMasterPasswordSet)
                    {
                        string decryptedPassword = DecryptPassword(entry.EncryptedPassword);
                        Log.Information($" Пароль: {decryptedPassword}");
                    }

                    // Вывод информации о добавлении новой записи в базу данных
                    Log.Information("Добавлена новая запись в базу данных"); // Переход на новую строку после каждой записи
                }
            }
            // Обработка исключений
            catch (Exception ex)
            {
                // Логирование ошибки с сообщением исключения
                Log.Error($"Произошла ошибка при просмотре записей: {ex.Message}");
            }
        }

        // Метод для расшифровки пароля
        private string DecryptPassword(string encryptedPassword)
        {
            try
            {
                // Использование using для автоматического освобождения ресурсов
                using (AesManaged aesAlg = new AesManaged())
                {
                    // Генерация ключа для расшифровки на основе мастер-пароля и соли
                    Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(masterPassword, Encoding.UTF8.GetBytes(PasswordKeySalt));
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

                    // Использование using для автоматического освобождения ресурсов
                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                    {
                        // Использование using для автоматического освобождения ресурсов
                        using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedPassword)))
                        {
                            // Использование using для автоматического освобождения ресурсов
                            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                            {
                                // Использование using для автоматического освобождения ресурсов
                                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                                {
                                    // Возврат расшифрованного пароля в виде строки
                                    return srDecrypt.ReadToEnd();
                                }
                            }
                        }
                    }
                }
            }
            // Обработка исключений
            catch (Exception ex)
            {
                // Логирование ошибки с сообщением исключения
                Log.Error($"Произошла ошибка при расшифровке пароля: {ex.Message}");
                // Возврат null в случае ошибки
                return null;
            }
        }

        // Метод для поиска записей о паролях по запросу
        public void SearchPasswordEntries(string query)
        {
            try
            {
                // Вызов метода для получения мастер-пароля и проверки его наличия
                GetMasterPassword();
                // Поиск записей о паролях по запросу
                var searchResults = passwordDatabase.SearchPasswords(UserId, query);

                // Проверка наличия результатов поиска
                if (searchResults.Any())
                {
                    // Вывод информации о результатах поиска
                    Log.Information("Результаты поиска:");
                    foreach (var entry in searchResults)
                    {
                        // Вывод информации о веб-сайте и логине каждой найденной записи
                        Log.Information($"Веб-сайт: {entry.Website,-20} Логин: {entry.Username}");
                    }
                }
                else
                {
                    // Вывод информации о том, что запись с запросом не найдена
                    Log.Information($"Запись с запросом '{query}' не найдена.");
                }
            }
            // Обработка исключений
            catch (Exception ex)
            {
                // Логирование ошибки с сообщением исключения
                Log.Error($"Произошла ошибка при поиске записей: {ex.Message}");
            }
        }
    }
}