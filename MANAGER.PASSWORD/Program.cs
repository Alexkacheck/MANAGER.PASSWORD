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

            passwordManager.SetMasterPassword();

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
                        passwordManager.AddPasswordEntry();
                        break;
                    case "2":
                        passwordManager.ViewPasswordEntries();
                        break;
                    case "3":
                        Console.WriteLine("Введите запрос для поиска:");
                        string query = Console.ReadLine();
                        passwordManager.SearchPasswordEntries(query);
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
