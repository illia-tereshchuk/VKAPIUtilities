using VKAPIUtilities.VKAPIAdapter.Miscellaneous;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Specialized;
using VKAPIUtilities.VKAPIAdapter.Authorization;
using System.Windows;

namespace VKAPIUtilities.VKAPIAdapter.Requests
{
    public class VKAPIRequestsPoint
    {
        // > Ограничитель количества выполняемых запросов на единицу времени
        private static RateGate _apiRequestsRateGate = new RateGate(2, TimeSpan.FromMilliseconds(1000));

        // > Асинхронный метод выполнения запроса к API без авторизации
        public static void PerformSingleUnauthorizedVKAPIGetRequestAsync(
            VKAPIGetRequestDescriptor requestDescriptor, // >> Объект запроса
            Action<Double> onDownloadProgressChanged, // >> Делегат на передачу прогресса выполнения
            Action<String> onDownloadCompleted) // >> Делегат на передачу результата выполнения
        {
            // >> Вызов этого метода откладывает выполнение контекста, где он вызывался, в очередь 
            _apiRequestsRateGate.WaitToProceed();

            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;

                // >>> Привязка обработчика на изменение прогресса загрузки
                webClient.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs arguments) =>
                {
                    // >>>> Вызывается делегат, переданный в параметре извне
                    onDownloadProgressChanged(arguments.ProgressPercentage);
                };

                // >>> Привязка обработчика на завершение прогресса загрузки
                webClient.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs arguments) =>
                {
                    // >>>> Вызывается делегат, переданный в параметре извне
                    onDownloadCompleted(arguments.Result);
                };
                // >>> После привязки обработчиков запускается выполнение запроса
                webClient.DownloadStringAsync(requestDescriptor.GetUnauthorizedRequestUri());
            }
        }

        public static void PerformSingleAuthorizedVKAPIGetRequestAsync(
            VKAPIGetRequestDescriptor requestDescriptor,
            VKAPIAuthorizationResult authorizationResult,
            Action<Double> onDownloadProgressChanged,
            Action<String> onDownloadCompleted)
        { 
            _apiRequestsRateGate.WaitToProceed();

            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;

                webClient.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs arguments) =>
                {
                    onDownloadProgressChanged(arguments.ProgressPercentage);
                };

                webClient.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs arguments) =>
                {
                    onDownloadCompleted(arguments.Result);
                };
                webClient.DownloadStringAsync(requestDescriptor.GetAuthorizedRequestUri(authorizationResult));
            }
        }

       

        public static void PerformMultipleAuthorizedVKAPIGetRequestsAsync(
            List<VKAPIGetRequestDescriptor> requestsDescriptors,
            VKAPIAuthorizationResult authorizationResult,
            Action<Double> onDownloadProgressChanged,
            Action<String[]> onDownloadCompleted)
        {
            // > Количество переданных объектов запросов
            Int32 requestsCount = requestsDescriptors.Count();
            // > Массив, в котором хранятся значения прогресса для каждого запроса
            Int32[] progressPercentageSegments = new Int32[requestsCount];
            // > Переменная, которая хранит текущее количество выполненных запросов
            Int32 performedRequestsCount = 0;
            // > Объект для lock (для предотвращения конфликта потоков за переменную выше)
            Object performedRequestsSyncLock = new Object();
            // > Массив, в котором сохраняются фрагменты данных от каждого запроса
            String[] dataChunks = new String[requestsCount];
            // > Циклический обход списка переданных запросов
            foreach (var request in requestsDescriptors)
            {
                // >> Регулировка частоты выполнения
                _apiRequestsRateGate.WaitToProceed();

                using (var webClient = new WebClient())
                {
                    webClient.Encoding = Encoding.UTF8;

                    webClient.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs arguments) =>
                    {
                        // >>>> Так как методов было передано много, отслеживаем общий (!) процент прогресса
                        progressPercentageSegments[requestsDescriptors.IndexOf(request)] = arguments.ProgressPercentage;
                        // >>>> При его изменении передаём на интерфейс общее среднее арифметическое
                        onDownloadProgressChanged(Convert.ToDouble(progressPercentageSegments.Sum()) / requestsCount);
                    };
                    webClient.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs arguments) =>
                    {
                        // >>>> Сохранение фгагмента данных в массив строк
                        dataChunks[requestsDescriptors.IndexOf(request)] = arguments.Result;
                        // >>>> Поскольку методы выполняются асинхронно, они могут конфликтовать за одну переменную
                        lock (performedRequestsSyncLock)
                        {
                            // >>>>> Чтобы этого не произошло, в момент времени доступ будет иметь только один метод
                            performedRequestsCount++;
                            // >>>>> В случае, если счётчик выполненных методов равен их общему количеству
                            if (performedRequestsCount == requestsCount)
                            {
                                // >>>>>> Выполнение множества запросов можно считать завершённым
                                onDownloadCompleted(dataChunks);
                            }
                        }
                    };
                    // >>> Запуск одного из множества запросов
                    webClient.DownloadStringAsync(request.GetAuthorizedRequestUri(authorizationResult));
                }
            }
        }
    }
}
