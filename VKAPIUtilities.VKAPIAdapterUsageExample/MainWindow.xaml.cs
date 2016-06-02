
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VKAPIUtilities.VKAPIAdapter.Authorization;
using VKAPIUtilities.VKAPIAdapter.Requests;

namespace VKAPIUtilities.VKAPIAdapterUsageExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const String _apiVersion = "5.52";
        private const String _language = "ru";

        private VKAPIAuthorizationResult _authorizationResult;

        public void DisableButtons()
        {
            buttonGetBoomburumFriends.IsEnabled = false;
            buttonGetHabrSubscribers.IsEnabled = false;
            buttonGetHabrSubscribersCount.IsEnabled = false;
            buttonGetOnlineUsersMoscow.IsEnabled = false;
        }

        public void EnableButtons()
        {
            buttonGetBoomburumFriends.IsEnabled = true;
            buttonGetHabrSubscribers.IsEnabled = true;
            buttonGetHabrSubscribersCount.IsEnabled = true;
            buttonGetOnlineUsersMoscow.IsEnabled = true;
        }

        // > Для того, чтобы из фонового потока можно было "достать" до интерфейса
        private void InvokeUIThread(Action uiDependentAction)
        {
            Dispatcher.Invoke(() =>
            {
                uiDependentAction();
            });
        }

        // > Метод для сохранения CSV - файлов
        public void SaveTextToFile(String text)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Файл CSV (*.csv)|*.csv|Все файлы (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, text);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // > Инициализация точки авторизации
            var authorizationPoint = new VKAPIAuthorizationPoint(new VKAPIAuthorizationSettings
            {
                APIVersion = _apiVersion,
                ApplicationIdentity = "5488902",
                ApplicationPermissions = new VKAPIAuthorizationPermissions
                {
                    friends = true
                },
                RevocationEnabled = true
            });

            _authorizationResult = authorizationPoint.Authorize();

            Activate(); //Предотвращение потери фокуса от закрытия диалога

            if (_authorizationResult == null)
            {
                MessageBox.Show("Вы отменили авторизацию, продолжение невозможно");
                Close();
            }
        }

        private void buttonGetHabrSubscribers_Click(object sender, RoutedEventArgs e)
        {
            var requestRescriptors = new List<VKAPIGetRequestDescriptor>();
            for (int i = 0; i < 30; ++i)
            {
                requestRescriptors.Add(new VKAPIGetRequestDescriptor
                {
                    MethodName = "groups.getMembers",
                    Parameters = new Dictionary<string, string>
                    {
                        { "group_id", "habr" },
                        {  "fields", "first_name, last_name" },
                        { "v", _apiVersion },
                        { "lang", _language },
                        { "offset", (i * 1000).ToString() }
                    }
                });
            }

            DisableButtons();

            Task.Run(() =>
            {
                VKAPIRequestsPoint.PerformMultipleAuthorizedVKAPIGetRequestsAsync(
                        requestRescriptors,
                        _authorizationResult,
                        (Double progressPercentage) =>
                        {
                            InvokeUIThread(() =>
                            {
                                progressBar.Value = progressPercentage;
                            });
                        },
                        (String[] requestResult) =>
                        {
                            var usersList = new List<String>();
                            for(int i = 0; i < requestResult.Length; i++)
                            {
                                var json = JObject.Parse(requestResult[i]);
                                foreach(var user in json["response"]["items"])
                                {
                                    usersList.Add(
                                        String.Format(
                                            "{0} {1} (vk.com/id{2})",
                                            user["first_name"],
                                            user["last_name"],
                                            user["id"]
                                        )
                                    );
                                }
                            }

                            InvokeUIThread(() =>
                            {
                                EnableButtons();
                                progressBar.Value = 0;
                                
                            });

                            SaveTextToFile(String.Join(",", usersList));
                        }
                    );
            });
        }

        private void buttonGetBoomburumFriends_Click(object sender, RoutedEventArgs e)
        {
            DisableButtons();

            Task.Run(() =>
            {
                VKAPIRequestsPoint.PerformSingleAuthorizedVKAPIGetRequestAsync(
                        new VKAPIGetRequestDescriptor
                        {
                            MethodName = "friends.get",
                            Parameters = new Dictionary<string, string>
                            {
                                { "user_id", "146891" },
                                {  "fields", "first_name, last_name" },
                                { "v", _apiVersion },
                                { "lang", _language }
                            }
                        },
                        _authorizationResult,
                        (Double progressPercentage) =>
                        {
                            InvokeUIThread(() =>
                            {
                                progressBar.Value = progressPercentage;
                            });
                        },
                        (String requestResult) =>
                        {
                            var usersList = new List<String>();
                            var json = JObject.Parse(requestResult);
                            foreach (var user in json["response"]["items"])
                            {
                                usersList.Add(
                                    String.Format(
                                        "{0} {1} (vk.com/id{2})",
                                        user["first_name"],
                                        user["last_name"],
                                        user["id"]
                                    )
                                );
                            }

                            InvokeUIThread(() =>
                            {
                                EnableButtons();
                                progressBar.Value = 0;
                            });

                            SaveTextToFile(String.Join(",", usersList));
                        }
                    );
            });
        }

        private void buttonGetOnlineUsersMoscow_Click(object sender, RoutedEventArgs e)
        {
            DisableButtons();

            Task.Run(() =>
            {
                VKAPIRequestsPoint.PerformSingleAuthorizedVKAPIGetRequestAsync(
                        new VKAPIGetRequestDescriptor
                        { 
                            MethodName = "users.search",
                            Parameters = new Dictionary<string, string>
                            {
                                {  "city", "1" },
                                {  "online", "1" },
                                {  "fields", "first_name, last_name" },
                                {  "count", "1000" },
                                { "v", _apiVersion },
                                { "lang", _language }
                            }
                        },
                        _authorizationResult,
                        (Double progressPercentage) =>
                        {
                            InvokeUIThread(() =>
                            {
                                progressBar.Value = progressPercentage;
                            });
                        },
                        (String requestResult) =>
                        {
                            var usersList = new List<String>();
                            var json = JObject.Parse(requestResult);
                            foreach (var user in json["response"]["items"])
                            {
                                usersList.Add(
                                    String.Format(
                                        "{0} {1} (vk.com/id{2})",
                                        user["first_name"],
                                        user["last_name"],
                                        user["id"]
                                    )
                                );
                            }

                            InvokeUIThread(() =>
                            {
                                EnableButtons();
                                progressBar.Value = 0;
                            });

                            SaveTextToFile(String.Join(",", usersList));
                        }
                    );
            });
        }

        private void buttonGetHabrSubscribersCount_Click(object sender, RoutedEventArgs e)
        {
            DisableButtons();

            Task.Run(() =>
            {
                VKAPIRequestsPoint.PerformSingleAuthorizedVKAPIGetRequestAsync(
                         new VKAPIGetRequestDescriptor
                         {
                            MethodName = "groups.getMembers",
                            Parameters = new Dictionary<string, string>
                            {
                                { "group_id", "habr" },
                                { "count", "0" },
                                { "v", _apiVersion },
                            }
                         },
                        _authorizationResult,
                        (Double progressPercentage) =>
                        {
                            
                        },
                        (String requestResult) =>
                        {
                            var json = JObject.Parse(requestResult);
                            MessageBox.Show(json["response"]["count"].ToObject<String>());
                            
                            InvokeUIThread(() =>
                            {
                                EnableButtons();
                                progressBar.Value = 0;
                            });
                        }
                    );
            });
        }
    }
}
