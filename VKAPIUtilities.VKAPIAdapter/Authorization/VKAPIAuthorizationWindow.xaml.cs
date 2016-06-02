using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VKAPIUtilities.VKAPIAdapter;

namespace VKAPIUtilities.VKAPIAdapter.Authorization
{
    public partial class VKAPIAuthorizationWindow : Window
    {
        // > Установки авторизации (принимаются при создании экземпляра)
        private VKAPIAuthorizationSettings _authorizationSettings;

        // > Переменная с результатом авторизации (возвращается диалогом)
        private VKAPIAuthorizationResult _authorizationResult;

        public VKAPIAuthorizationWindow(VKAPIAuthorizationSettings authorizationSettings)
        {
            _authorizationSettings = authorizationSettings;
        }

        private void Window_Loaded(object sender, RoutedEventArgs arguments)
        {
            // > WebBrowser базируется на движке IE и "не понимает" последних стандартов JavaScript 
            // > Действием в строке ниже мы выключаем сообщения о каждом несоответствии в скриптах
            webBrowser.ScriptErrorsSuppressed = true;

            // > Вызывается, когда браузер начинает переходить по определенной ссылке
            webBrowser.Navigated += WebBrowser_Navigated;

            // > Вызывается, когда в окне браузера страница загрузилась целиком
            webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;

            // > Интерфейс готов, обработчики привязаны - можно начинать авторизацию
            webBrowser.Navigate(_authorizationSettings.GetAuthorizationUri());
        }

private void WebBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs arguments)
{
    // > Пока страница не загрузилась, свернем окно
    WindowState = WindowState.Minimized; 
    // > Достанем из URI часть, которая идёт после адреса
    var uriQueryFragment = arguments.Url.Fragment;
    // > При авторизации VK возвращает ссылку типа https://oauth.vk.com/blank.html#access_token=...
    // > Это особенная ситуация, так как отделение параметров знаком # не является стандартом
    if (uriQueryFragment.StartsWith("#"))
    {
        // >> Для того, чтобы парсер смог обработать фрагмент запроса, требуется убрать этот символ
        uriQueryFragment = uriQueryFragment.Replace("#", String.Empty);
    }
    // > Соответственно, теперь можно её распарсить
    var queryParameters = HttpUtility.ParseQueryString(uriQueryFragment);
    // > Состояние интерфейса авторизации нужно отслеживать по параметрам в строке навигации
    // > В певую очередь проверим, не содержит ли строка параметра, который означает отмену
    var isCancelledByUser = !String.IsNullOrEmpty(queryParameters["error"]);
    if (isCancelledByUser)
    {
        // >> Если таковой присутствует, завершим диалог
        DialogResult = false;
    }
    else
    {
        // >> Если пользователь не отменял процесс, возможно, он как раз авторизовался
        var isAccessTokenObtained = !String.IsNullOrEmpty(queryParameters["access_token"]);
        var isUserIdentityObtained = !String.IsNullOrEmpty(queryParameters["user_id"]);
        if (isAccessTokenObtained && isUserIdentityObtained)
        {
            // >>> В таком случае запишем полученные параметры в переменную
            _authorizationResult = new VKAPIAuthorizationResult
            {
                AccessToken = queryParameters["access_token"],
                UserIdentity = queryParameters["user_id"]
            };
            // >>> Теперь с завершением диалога можно вернуть данные
            DialogResult = true;
        }
        else
        {
            // >> Пока пользователь ничего не отменял и еще не авторизовался
        }
    }
}

        // + Визуальное изменение окна при загрузке страниц
        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs arguments)
        {
            // ++ Изменение размеров соответственно содержимому страницы
            webBrowser.Width = webBrowser.Document.Body.ScrollRectangle.Width;
            webBrowser.Height = webBrowser.Document.Body.ScrollRectangle.Height;
            // -- Изменение размеров соответственно содержимому страницы
            // ++ Показ обновлённого вывода
            WindowState = WindowState.Normal; //Когда страница загрузилась, возобновляем окно
            Activate();
            UpdateLayout();
            // -- Показ обновлённого вывода
        }
        // - Визуальное изменение окна при загрузке страниц

        public new VKAPIAuthorizationResult ShowDialog()
        {
            InitializeComponent();
            // > Программа останавливается на этом месте, пока не присвоен DialogResult
            base.ShowDialog();
            // > Когда DialogResult будет присвоен в коде (либо посредством закрытия окна)
            // > Базовый метод ShowDialog завершится и этот метод вернёт данные
            // > Если процесс отменен и до заполнения переменной не дошло, она будет null
            return _authorizationResult;
        }

       
    }
}
