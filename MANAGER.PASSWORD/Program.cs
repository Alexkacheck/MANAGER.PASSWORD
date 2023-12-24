using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagerPassword.BLL;
namespace MANAGER.PASSWORD
{
    class Program
    {
        static void Main(string[] args)
        {
            PasswordManagerApp();
        }

        static void PasswordManagerApp()
        {
            PasswordManager passwordManager = new PasswordManager();

            while (true)
            {
                Console.WriteLine("\nВыберите действие:");
                Console.WriteLine("1. Добавить запись о пароле");
                Console.WriteLine("2. Просмотреть записи");
                Console.WriteLine("3. Поиск записей");
                Console.WriteLine("4. Выйти");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        // Добавление записи о пароле
                        passwordManager.AddPasswordEntry();
                        break;
                    case "2":
                        // Запрос мастер-пароля перед просмотром записей
                        passwordManager.SetMasterPassword();

                        // Проверка наличия введенного мастер-пароля
                        if (passwordManager.IsMasterPasswordSet)
                        {
                            // Просмотр записей с расшифрованными паролями
                            passwordManager.ViewPasswordEntries();
                        }
                        else
                        {
                            Console.WriteLine("Введите мастер-пароль для просмотра записей.");
                        }
                        break;
                    case "3":
                        // Проверка наличия введенного мастер-пароля
                        if (passwordManager.IsMasterPasswordSet)
                        {
                            Console.WriteLine("Введите запрос для поиска:");
                            string query = Console.ReadLine();
                            passwordManager.SearchPasswordEntries(query);
                        }
                        else
                        {
                            Console.WriteLine("Введите мастер-пароль для поиска записей.");
                        }
                        break;
                    case "4":
                        Console.WriteLine("До свидания!");
                        return;
                    default:
                        Console.WriteLine("Неверный выбор. Попробуйте снова.");
                        break;
                }
            }
        }
    }
}