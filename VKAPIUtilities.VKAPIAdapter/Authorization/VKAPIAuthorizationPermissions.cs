using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VKAPIUtilities.VKAPIAdapter.Authorization
{
    
    public class VKAPIAuthorizationPermissions
    {
        // > Названия полей в точности копируют таковые в спецификации API
        public bool notify;
        public bool friends;
        public bool photos;
        public bool audio;
        public bool video;
        public bool docs;
        public bool notes;
        public bool pages;
        public bool status;
        public bool wall;
        public bool groups;
        public bool messages;
        public bool email;
        public bool notifications;
        public bool stats;
        public bool ads;
        public bool market;
        public bool offline = true; // > Для получения неистекаемого ключа (важно)
        public bool nohttps;

        // > Метод, преобразующий объект разрешений в строковый фрагмент для URI
        public override String ToString()
        {
            // >> Эта операция - одно из проявлений использования "рефлексии"
            // >> Внутри класса происходит чтение имён его же полей
            var fieldsInformation = 
                typeof(VKAPIAuthorizationPermissions)
                .GetFields(BindingFlags.Public | BindingFlags.Instance);

            var includedPermissions = new List<String>();

            foreach (var fieldInfo in fieldsInformation)
            {
                // >>> Если сканируемое поле имеет значение true
                if ((bool)fieldInfo.GetValue(this))
                {
                    // >>>> Добавляем название этого поля в список строк
                    includedPermissions.Add(fieldInfo.Name);
                }
            }
            // >> По завершению операции возвращаем названия полей, разделённые запятой
            return String.Join(",", includedPermissions);
        }
    }
}
