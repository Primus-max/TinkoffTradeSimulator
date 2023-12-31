﻿using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using System;
using System.Threading.Tasks;
using Tinkoff.InvestApi;

namespace TinkoffTradeSimulator.ApiServices
{
    /// <summary>
    /// Создане клиента TinkoffInvestAPI
    /// </summary>
    public class TinkoffClient
    {
        // Создание клиента Тинькофф 
        public static Task<InvestApiClient> CreateAsync()
        {
            // Токен временно храню в строке
            string token = "t.MdgtB4uUizON-6sWQMirH61UzLxM-pLE6I8OqCp2F25au0VUR6Ee9ou-DLA682CUjpO1WQd0vzz1pp5QerFIKQ";

            var appName = "tinkoff.invest-api-csharp-sdk";
            var callCredentials = CallCredentials.FromInterceptor((_, metadata) =>
            {
                metadata.Add("Authorization", "Bearer " + token);
                metadata.Add("x-app-name", appName);
                return Task.CompletedTask;
            });

            var methodConfig = new MethodConfig
            {
                Names =
                {
                    MethodName.Default,
                },
                RetryPolicy = new RetryPolicy
                {
                    MaxAttempts = 5,
                    InitialBackoff = TimeSpan.FromSeconds(1.0),
                    MaxBackoff = TimeSpan.FromSeconds(5.0),
                    BackoffMultiplier = 1.5,
                    RetryableStatusCodes =
            {
                StatusCode.Unavailable,
            },
                },
            };

            var channel = GrpcChannel.ForAddress("https://invest-public-api.tinkoff.ru:443", new GrpcChannelOptions()
            {
                Credentials = ChannelCredentials.Create(new SslCredentials(), callCredentials),
                ServiceConfig = new ServiceConfig
                {
                    MethodConfigs =
            {
                methodConfig,
            },
                },
            });

            var invoker = channel.CreateCallInvoker();
            var client = new InvestApiClient(invoker);

            return Task.FromResult(client);
        }

        // Загружаю токен
        //private static SettingsModel GetSettings()
        //{
        //    string filePath = "settings.json";
        //    SettingsModel settingsModel = new SettingsModel();

        //    try
        //    {
        //        if (File.Exists(filePath))
        //        {
        //            // Если файл существует, загрузите его содержимое
        //            string jsonData = File.ReadAllText(filePath);
        //            JObject data = JObject.Parse(jsonData);

        //            // Пример загрузки данных из JSON в модель представления
        //            settingsModel.TinkoffToken = data["TinkoffToken"]?.ToString();
        //            settingsModel.ChromeLocation = data["ChromeLocation"]?.ToString();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
        //    }

        //    return settingsModel;
        //}
    }
}
