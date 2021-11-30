using ExtraSpace.API.Models;
using ExtraSpace.API.Models.OrdersModels;
using ExtraSpace.API.Models.TelegramModels;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExtraSpace.API.Repositories
{
    public partial class TelegramRepository : ITelegramRepository
    {
        private readonly IConfiguration _configuration;
        private AutoDataManager.AutoDataManager _manager;
        private readonly string _connectionString;
        private Telegram.Bot.TelegramBotClient _bot;
        private List<TelegramCommandEntity> _actions;
        private const long _adminChatId = 748807755;

        public TelegramRepository(IConfiguration configuration)
        {
            this._configuration = configuration;
            this._connectionString = configuration.GetConnectionString("MainConnectionString");
            this._manager = new AutoDataManager.AutoDataManager(_connectionString);
            _bot = new Telegram.Bot.TelegramBotClient(configuration["Telegram:Token"]);

            _actions = new List<TelegramCommandEntity>();
            _actions.Add(new TelegramCommandEntity("/start", "/start", "Список доступных команд", AllAvailableActions));
            _actions.Add(new TelegramCommandEntity("/allClients", "/allClients", "Список всех клиентов", AllClients));
            _actions.Add(new TelegramCommandEntity("/approve", "/approve_clientId", "Подтвердить регистрацию клиента", ApproveClient));
            _actions.Add(new TelegramCommandEntity("/newOrders", "/newOrders", "Не обработанные заказы", NotCompletedOrders));
            _actions.Add(new TelegramCommandEntity("/completeOrder", "/completeOrder_orderId", "Завершить заказ", CompleteOrder));
        }
        public ApiResponse<bool> ProcessMessage(UpdateModel update) =>
            ApiResponse<bool>.DoMethod(resp =>
            {
                if (update == null)
                    throw new Exception("empty_data");

                _bot.SendChatActionAsync(update.message.chat.id, Telegram.Bot.Types.Enums.ChatAction.Typing);

                WriteLog(update);

                bool isClientRegistred = IsClientRegistred(update).Data;
                if(!isClientRegistred)
                {
                    _bot.SendTextMessageAsync(update.message.chat.id, "Для продолжения администрация бота должна вас зарегистрировать").Wait();
                    return;
                }

                resp.Data = Action(update).Data;
            });

        public ApiResponse<List<TelegramClientModel>> GetAllClients() =>
            ApiResponse<List<TelegramClientModel>>.DoMethod(resp => resp.Data = _manager.GetListAuto<TelegramClientModel>());

        public ApiResponse<TelegramClientModel> ApproveClient(long chatId) =>
            ApiResponse<TelegramClientModel>.DoMethod(resp =>
            {
                string sql = "SELECT * FROM dbo.TelegramClients tc (NOLOCK) WHERE tc.Id = @chatId";
                TelegramClientModel client = _manager.Get<TelegramClientModel>(sql, false, new { chatId });
                if (client == null)
                    resp.Throw(1, "Клиент не найден");
                client.IsApproved = true;
                _manager.UpdateModel(client);
                resp.Data = client;
            });

        public ApiResponse<bool> NotifyAllMembers(string text) =>
            ApiResponse<bool>.DoMethod(resp =>
            {
                List<TelegramClientModel> clients = GetAllClients().GetResultIfNotError();
                clients = clients.Where(c => c.IsApproved).ToList();
                clients.ForEach(c => SendHtml(c.Id, text));
                resp.Data = true;
            });

        public ApiResponse<bool> NotifyAllMembersAboutNewOrder(OrderModel order) =>
            NotifyAllMembers("New order!\n" + order.ToString() + $"\nЗавершить заказ - /completeOrder_{order.Id}");
    }


    public partial class TelegramRepository
    {
        #region Внутренние методы
        private ApiResponse<bool> IsClientRegistred(UpdateModel update) =>
            Models.ApiResponse<bool>.DoMethod(resp =>
            {
                TelegramClientModel client = _manager.Get<TelegramClientModel>("SELECT * FROM dbo.TelegramClients (NOLOCK) WHERE Id = @id",
                    new { id = update.message.chat.id });

                if (client == null)
                {
                    client = new TelegramClientModel()
                    {
                        Id = update.message.chat.id,
                        FirstName = update.message.chat.first_name,
                        LastName = update.message.chat.last_name,
                        Username = update.message.chat.username,
                        IsApproved = false,
                        InsertDate = DateTime.Now
                    };

                    _manager.InsertModel(client);

                    SendHtml(_adminChatId, $"Была попытка входа нового пользователя. " +
                        $"{update.message.chat.id} | {update.message.chat.first_name} {update.message.chat.last_name} | " +
                        $"{update.message.chat.username}");
                }

                resp.Data = client.IsApproved;
            });

        private ApiResponse<bool> WriteLog(UpdateModel update) =>
            Models.ApiResponse<bool>.DoMethod(resp =>
            {
                TelegramLogModel log = new TelegramLogModel()
                {
                    JsonLog = Newtonsoft.Json.JsonConvert.SerializeObject(update),
                    ClientId = update.message.chat.id,
                    InsertDate = DateTime.Now
                };

                _manager.InsertModel(log);
                resp.Data = true;
            });

        private ApiResponse<bool> Action(UpdateModel update) =>
            ApiResponse<bool>.DoMethod(resp =>
            {
                string text = update.message.text;
                if (string.IsNullOrEmpty(text))
                {
                    SendHtml(update, "Не ясно, что вы хотели эти сказать");
                    return;
                }

                if (text.StartsWith("/"))
                {
                    string[] commands = text.Split('_');
                    TelegramCommandEntity entity = _actions.FirstOrDefault(a => a.Command == commands[0]);

                    if (entity != null)
                    {
                        ApiResponse<bool> actionResult = entity.Action(update);
                        resp.Data = actionResult.Data;
                        if(actionResult.Code != 0)
                        {
                            SendHtml(update, "Упс! Во время выполнения что-то сломалось");
                            return;
                        }
                    }
                    else
                        resp.Data = SendHtml(update, "Такая команда не найдена").Data;
                }
                else
                    resp.Data = AllAvailableActions(update).Data;
            });

        private ApiResponse<bool> SendHtml(long chatId, string html) =>
            ApiResponse<bool>.DoMethod(resp =>
            {
                _bot.SendTextMessageAsync(chatId, html, Telegram.Bot.Types.Enums.ParseMode.Html);
                resp.Data = true;
            });

        private ApiResponse<bool> SendHtml(UpdateModel update, string html) =>
            SendHtml(update.message.chat.id, html);
        #endregion

        #region Команды
        private ApiResponse<bool> AllAvailableActions(UpdateModel update) =>
            ApiResponse<bool>.DoMethod(resp =>
            {
                string html = "Список доступных команд:\n";
                foreach (var item in _actions)
                    html += $"{item.CommandTextToView} - {item.Description}\n";
                SendHtml(update, html);
                resp.Data = true;
            });

        private ApiResponse<bool> AllClients(UpdateModel update) =>
            ApiResponse<bool>.DoMethod(resp =>
            {
                List<TelegramClientModel> clients = GetAllClients().Data;
                if(clients == null || clients.Count <= 0)
                {
                    SendHtml(update, "Упс! Клиенты не найдены или возможно произошла ошибка");
                    return;
                }

                string html = $"Подтверждено клиентов <b>{clients.Where(c => c.IsApproved).Count()}/{clients.Count}</b>\n";
                foreach (var item in clients)
                {
                    html += item.Username + " - " + (item.IsApproved ? "[Подтвержден]" : "[Не подтвержден]");
                    if (!item.IsApproved)
                        html += " /approve_" + item.Id.ToString();
                    html += "\n";
                }

                resp.Data = SendHtml(update, html).Data;
            });

        private ApiResponse<bool> ApproveClient(UpdateModel update) =>
            ApiResponse<bool>.DoMethod(resp =>
            {
                string text = update.message.text;
                string[] textParts = text.Split('_');
                if(textParts.Count() != 2)
                {
                    SendHtml(update, "Не передан id клиента");
                    return;
                }

                long chatId = 0;
                bool parseResult = long.TryParse(textParts.Last(), out chatId);
                if (!parseResult)
                {
                    SendHtml(update, "Неверно передан id клиента");
                    return;
                }

                ApiResponse<TelegramClientModel> approveResult = ApproveClient(chatId);
                if(approveResult.Code < 0)
                {
                    SendHtml(update, "Произошла ошибка при подтверждении клиента");
                    return;
                }

                else if(approveResult.Code > 0)
                {
                    SendHtml(update, approveResult.Message);
                    return;
                }

                SendHtml(update, "Клиент успешно подтвержден и будет уведомлен об этом");
                SendHtml(approveResult.Data.Id, "Поздравляем! Вас успешно зарегистрировали в нашем боте. Для начала работы введите /start");
                resp.Data = true;
            });

        private ApiResponse<bool> NotCompletedOrders(UpdateModel update) =>
            ApiResponse<bool>.DoMethod(resp =>
            {
                string sql = "SELECT * FROM dbo.Orders o (NOLOCK) WHERE o.IsComplete = 0 AND IsDeleted = 0";
                List<OrderModel> orders = _manager.GetList<OrderModel>(sql);
                if(orders.Count() <= 0)
                {
                    SendHtml(update, "Все заказы обработаны!");
                    return;
                }

                string html = "Не обработанные заказы на сегодня: \n";
                foreach (var item in orders)
                    html += item.ToString() + "\n Завершить заказ - /completeOrder_" + item.Id + "\n\n";

                SendHtml(update, html);
                resp.Data = true;
            });

        private ApiResponse<bool> CompleteOrder(UpdateModel update) =>
            ApiResponse<bool>.DoMethod(resp =>
            {
                string text = update.message.text;
                string[] textParts = text.Split('_');
                if (textParts.Count() != 2)
                {
                    SendHtml(update, "Не передан номер заказа");
                    return;
                }

                int orderId = 0;
                bool parseResult = int.TryParse(textParts.Last(), out orderId);
                if (!parseResult)
                {
                    SendHtml(update, "Неверно передан номер заказа");
                    return;
                }

                OrderModel order = _manager.Get<OrderModel>("SELECT * FROM dbo.Orders o (NOLOCK) WHERE o.Id = @orderId AND o.IsDeleted = 0", false, new { orderId });
                if(order == null)
                {
                    SendHtml(update, "Заказ не найден");
                    return;
                }

                order.IsComplete = true;
                _manager.UpdateModel(order);

                SendHtml(update, $"Готово! Заказ №{order.Id} успешно завершен!");
                resp.Data = true;
            });

        #endregion
    }

    public interface ITelegramRepository
    {
        ApiResponse<bool> ProcessMessage(UpdateModel update);
        ApiResponse<bool> NotifyAllMembers(string text);
        ApiResponse<bool> NotifyAllMembersAboutNewOrder(OrderModel order);
        ApiResponse<List<TelegramClientModel>> GetAllClients();
        ApiResponse<TelegramClientModel> ApproveClient(long chatId);
    }
}
