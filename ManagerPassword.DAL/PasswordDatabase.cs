using LiteDB;
using ManagerPassword;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Core;
using Serilog.Events;

namespace ManagerPassword.DAL
{
    public class PasswordDatabase : IDisposable
    {
        // Переменные для хранения базы данных и коллекции паролей
        private readonly LiteDatabase db;
        private readonly LiteCollection<PasswordEntry> passwordEntries;

        // Конструктор класса PasswordDatabase
        public PasswordDatabase()
        {
            try
            {
                // Инициализация базы данных и коллекции паролей
                db = new LiteDatabase("PasswordManager.db");
                passwordEntries = (LiteCollection<PasswordEntry>)db.GetCollection<PasswordEntry>("passwordEntries");
            }
            catch (LiteException ex)
            {
                // Логирование ошибки при создании базы данных
                Log.Error($"Произошла ошибка при создании базы данных: {ex.Message}");
            }
        }

        // Метод для добавления пароля в базу данных
        public void AddPassword(PasswordEntry entry)
        {
            try
            {
                // Вставка записи о пароле в коллекцию
                passwordEntries.Insert(entry);

                // Логирование успешного добавления записи
                Log.Information("Запись добавлена.");
            }
            catch (LiteException ex)
            {
                // Логирование ошибки при добавлении записи
                Log.Error($"Произошла ошибка при добавлении записи: {ex.Message}");
            }
        }

        // Метод для получения всех паролей пользователя
        public List<PasswordEntry> GetAllPasswords(int userId)
        {
            try
            {
                // Поиск и возврат всех записей о паролях для указанного пользователя
                return passwordEntries.Find(x => x.UserId == userId).ToList();
            }
            catch (LiteException ex)
            {
                // Логирование ошибки при получении записей
                Log.Error($"Произошла ошибка при получении записей: {ex.Message}");

                // Возврат пустого списка в случае ошибки
                return new List<PasswordEntry>();
            }
        }

        // Метод для поиска паролей по запросу
        public IEnumerable<PasswordEntry> SearchPasswords(int userId, string query)
        {
            try
            {
                // Поиск записей о паролях по запросу для указанного пользователя
                var results = passwordEntries
                    .Find(x => x.UserId == userId &&
                               (x.Website.Equals(query, StringComparison.OrdinalIgnoreCase) ||
                                x.Username.Equals(query, StringComparison.OrdinalIgnoreCase)));

                // Проверка наличия результатов поиска
                if (results.Any())
                {
                    // Возврат результатов поиска
                    return results;
                }
                else
                {
                    // Логирование информации о том, что запись не найдена
                    Log.Information($"Запись с запросом '{query}' не найдена.");

                    // Возврат пустой последовательности в случае отсутствия результатов
                    return Enumerable.Empty<PasswordEntry>();
                }
            }
            catch (LiteException ex)
            {
                // Логирование ошибки при поиске записей
                Log.Error($"Произошла ошибка при поиске записей: {ex.Message}");

                // Возврат пустой последовательности в случае ошибки
                return Enumerable.Empty<PasswordEntry>();
            }
        }

        // Реализация интерфейса IDisposable для освобождения ресурсов
        public void Dispose()
        {
            db.Dispose();
        }
    }
}