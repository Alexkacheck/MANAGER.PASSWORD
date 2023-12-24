using LiteDB;
using ManagerPassword;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManagerPassword.DAL
{
    public class PasswordDatabase : IDisposable
    {
        private readonly LiteDatabase db;
        private readonly LiteCollection<PasswordEntry> passwordEntries;

        public PasswordDatabase()
        {
            try
            {
                db = new LiteDatabase("PasswordManager.db");
                passwordEntries = (LiteCollection<PasswordEntry>)db.GetCollection<PasswordEntry>("passwordEntries");
            }
            catch (LiteException ex)
            {
                Console.WriteLine($"Произошла ошибка при создании базы данных: {ex.Message}");
            }
        }

        public void AddPassword(PasswordEntry entry)
        {
            try
            {
                passwordEntries.Insert(entry);
                Console.WriteLine("Запись добавлена.");
            }
            catch (LiteException ex)
            {
                Console.WriteLine($"Произошла ошибка при добавлении записи: {ex.Message}");
            }
        }

        public List<PasswordEntry> GetAllPasswords(int userId)
        {
            try
            {
                return passwordEntries.Find(x => x.UserId == userId).ToList();
            }
            catch (LiteException ex)
            {
                Console.WriteLine($"Произошла ошибка при получении записей: {ex.Message}");
                return new List<PasswordEntry>();
            }
        }

        public IEnumerable<PasswordEntry> SearchPasswords(int userId, string query)
        {
            try
            {
                var results = passwordEntries
                    .Find(x => x.UserId == userId &&
                               (x.Website.Equals(query, StringComparison.OrdinalIgnoreCase) ||
                                x.Username.Equals(query, StringComparison.OrdinalIgnoreCase)));

                if (results.Any())
                {
                    return results;
                }
                else
                {
                    Console.WriteLine($"Запись с запросом '{query}' не найдена.");
                    return Enumerable.Empty<PasswordEntry>();
                }
            }
            catch (LiteException ex)
            {
                Console.WriteLine($"Произошла ошибка при поиске записей: {ex.Message}");
                return Enumerable.Empty<PasswordEntry>();
            }
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}