using LiteDB;
using ManagerPassword;
using System;
using System.Collections.Generic;
using System.Linq;
namespace ManagerPassword.DAL
{
    public class PasswordDatabase
    {
        private readonly LiteDatabase db;
        private readonly LiteCollection<PasswordEntry> passwordEntries;

        public PasswordDatabase()
        {
            db = new LiteDatabase("PasswordManager.db");
            passwordEntries = (LiteCollection<PasswordEntry>)db.GetCollection<PasswordEntry>("passwordEntries");
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

        public List<PasswordEntry> GetAllPasswords()
        {
            try
            {
                return passwordEntries.FindAll().ToList();
            }
            catch (LiteException ex)
            {
                Console.WriteLine($"Произошла ошибка при получении записей: {ex.Message}");
                return new List<PasswordEntry>();
            }
        }

        public List<PasswordEntry> SearchPasswords(string query)
        {
            try
            {
                return passwordEntries
    .Find(x => x.Website.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1 ||
               x.Username.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1)
    .ToList();
            }
            catch (LiteException ex)
            {
                Console.WriteLine($"Произошла ошибка при поиске записей: {ex.Message}");
                return new List<PasswordEntry>();
            }
        }

        // Другие методы для работы с базой данных, если необходимо

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
