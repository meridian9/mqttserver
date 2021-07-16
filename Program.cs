using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Newtonsoft.Json;

namespace com.koerber
{
    public class Program
    {
        private static ILogger Logger;

        public static void Main()
        {
            var loggerFactory = LoggerFactory.Create(builder => {  builder.AddConsole(); });
            
            Logger = loggerFactory.CreateLogger<Program>();
            Logger.LogInformation("Starting up");
        
            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var config = ReadConfiguration(currentPath);

            var optionsBuilder = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(config.Port)
                .WithConnectionValidator(c => {
                    var currentUser = config.Users.FirstOrDefault(u => u.Username == c.Username);

                    if (currentUser == null || (c.Username != currentUser.Username || c.Password != currentUser.Password))
                    {
                        c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                        LogMessage(c);
                        return;
                    }

                    c.ReasonCode = MqttConnectReasonCode.Success;
                    LogMessage(c);
                })
                .WithSubscriptionInterceptor(c => { c.AcceptSubscription = true; LogMessage(c, true); })
                .WithApplicationMessageInterceptor(c => { c.AcceptPublish = true; LogMessage(c); });

            var mqttServer = new MqttFactory().CreateMqttServer();
            mqttServer.StartAsync(optionsBuilder.Build());
            
            Console.ReadLine();
        }

        private static Config ReadConfiguration(string currentPath)
        {
            var filePath = $"{currentPath}\\config.json";

            Config config = null;
            if (File.Exists(filePath))
            {
                using var r = new StreamReader(filePath);
                var json = r.ReadToEnd();
                config = JsonConvert.DeserializeObject<Config>(json);
            }

            return config;
        }

        private static void LogMessage(MqttSubscriptionInterceptorContext context, bool successful)
        {
            if (context == null) return;
            Logger.LogInformation($"{(successful ? "New subscription" : "Subscription failed for clientId") }: ClientId = {context.ClientId}, TopicFilter = {context.TopicFilter}");
        }

        private static void LogMessage(MqttApplicationMessageInterceptorContext context)
        {
            if (context == null) return;
            var payload = context.ApplicationMessage?.Payload == null ? null : Encoding.UTF8.GetString(context.ApplicationMessage?.Payload);
            Logger.LogInformation($"Message: ClientId = {context.ClientId}, Topic = {context.ApplicationMessage?.Topic}, Payload = {payload}, QoS = {context.ApplicationMessage?.QualityOfServiceLevel}, Retain-Flag = {context.ApplicationMessage?.Retain}");
        }

        private static void LogMessage(MqttConnectionValidatorContext context)
        {
            if (context == null) return;
            Logger.LogInformation($"New connection: ClientId = {context.ClientId}, Endpoint = {context.Endpoint}, Username = {context.Username}, Password = {context.Password}, CleanSession = {context.CleanSession}");
        }
    }
}