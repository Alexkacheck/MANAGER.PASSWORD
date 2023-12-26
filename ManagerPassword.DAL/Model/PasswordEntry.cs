using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace ManagerPassword.DAL
{
    // Класс, представляющий запись о пароле
    public class PasswordEntry
    {
        // Идентификатор пользователя, к которому относится запись
        public int UserId { get; set; }

        // Название веб-сайта, для которого сохранен пароль
        public string Website { get; set; }

        // Имя пользователя (логин) для веб-сайта
        public string Username { get; set; }

        // Зашифрованный пароль
        public string EncryptedPassword { get; set; }
    }
}