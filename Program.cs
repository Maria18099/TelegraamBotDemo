using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.ReplyMarkups;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TelegraamBotDemo
{
    enum Currencies { EUR, USD, GBP }
    class Program
    {
        static ITelegramBotClient bot = new TelegramBotClient("7163670002:AAFDXXL5V5O-xOCMKOm6gmQh3Vm1VYfBbXw");
        static double sum;
        static async Task Main(string[] args)
        {
            Console.WriteLine("Run bot " + bot.GetMeAsync().Result.FirstName);
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
        }
        public static async Task<List<Currency>> readNBUCurrencies()
        {
            using (HttpClient client = new HttpClient())
            {
                string url = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json";
                HttpResponseMessage responseMessage = await client.GetAsync(url);
                responseMessage.EnsureSuccessStatusCode();
                string responceContent = await responseMessage.Content.ReadAsStringAsync();//масив json об'єктів
                List<Currency> currencies = JsonConvert.DeserializeObject<List<Currency>>(responceContent);
                return currencies;
            }
        }
        public static async Task<double> getRateCurrency(Currencies currencyName)
        {
            List<Currency> currencies = await readNBUCurrencies();
            foreach (Currency currency in currencies)
            {
                if (currency.Name.Equals(currencyName.ToString()))
                {
                    return currency.Rate;
                }
            }
            return 0;
        } 
        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }


        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text.ToLower() == "/start")
                {
                    InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Check currency rate", "Check_currency_button_click"),
                            InlineKeyboardButton.WithCallbackData("Convert", "Convert_button_click")
                        }
                    });
                    await botClient.SendTextMessageAsync(update.Message.Chat, "Welcome to our bot! Choose operation:", replyMarkup: inlineKeyboard);
                    return;

                }
                await botClient.SendTextMessageAsync(message.Chat, "Hi! How can I help you?");
            }
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            {
                var callbackQuery = update.CallbackQuery;
                if(callbackQuery.Data == "Check_currency_button_click")
                {
                    InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("EUR", "EUR_button_click"),
                            InlineKeyboardButton.WithCallbackData("USD", "USD_button_click"),
                            InlineKeyboardButton.WithCallbackData("GBP", "GBP_button_click")
                        }
                    });
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Choose currency:", replyMarkup: inlineKeyboard);
                    return;
                }
                if (callbackQuery.Data == "Convert_button_click")
                {
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Choose currency:");
                    return;
                }
                if (callbackQuery.Data == "Convert_button_click")
                {
                    InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("EUR", "EUR_Convert_button_click"),
                            InlineKeyboardButton.WithCallbackData("USD", "USD_Convert_button_click"),
                            InlineKeyboardButton.WithCallbackData("GBP", "GBP_Convert_button_click")
                        }
                    });
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Choose currency:", replyMarkup: inlineKeyboard);
                    return;
                }
                if (callbackQuery.Data == "EUR_button_click")
                {
                    double rate = await getRateCurrency(Currencies.EUR);
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, rate.ToString());
                }
                if (callbackQuery.Data == "USD_button_click")
                {
                    double rate = await getRateCurrency(Currencies.USD);
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, rate.ToString());
                }
                if (callbackQuery.Data == "GBP_button_click")
                {
                    double rate = await getRateCurrency(Currencies.GBP);
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, rate.ToString());
                }
                return;
            }
        }
    }
    class Currency
    {
        [JsonProperty("cc")]
        public string Name { get; set; }
        [JsonProperty("rate")]
        public double Rate { get; set; } 
    }
}





