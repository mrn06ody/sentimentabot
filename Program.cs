using Microsoft.Extensions.Configuration;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TL;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("config.json", optional: false)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();


var apiId = config.GetValue<int>("ApiId");
var apiHash = config.GetValue<string>("ApiHash")!;
var botToken = config.GetValue<string>("BotToken")!;

StreamWriter WTelegramLogs = new("WTelegramBot.log", true, Encoding.UTF8) { AutoFlush = true };
WTelegram.Helpers.Log = (lvl, str) => WTelegramLogs.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{"TDIWE!"[lvl]}] {str}");

using var connection = new Microsoft.Data.Sqlite.SqliteConnection(@"Data Source=WTelegramBot.sqlite");
using var bot = new WTelegram.Bot(Config, connection);
var my = await bot.GetMe();
Console.WriteLine($"I am @{my.Username}");

bot.WantUnknownTLUpdates = true;
bot.OnError += (e, s) => Console.Error.WriteLineAsync(e.ToString());
bot.OnMessage += OnMessage;
while (Console.ReadKey(true).Key != ConsoleKey.Escape) { }


async Task OnMessage(WTelegram.Types.Message msg, UpdateType type)
{
    // commands accepted:
    if (msg.Text == "/start")
    {
        //---> It's easy to reply to a message by giving its id to replyParameters: (was broken in Telegram.Bot v20.0.0)
        await bot.SendMessage(msg.Chat, $"Hello, {msg.From}!\nTry commands /pic /react /lastseen /getchat /setphoto", replyParameters: msg);
        return;
    }


    if (msg.ForwardOrigin is not MessageOriginChannel originChannel)
    {
        return;
    }

    try
    {
        await bot.Client.LoginUserIfNeeded();

        var chatId = long.Parse(originChannel.Chat.Id.ToString().Replace("-100", string.Empty));
        var chat = await bot.Client.Messages_GetChats(chatId);
        var test = await bot.Client.Messages_GetDiscussionMessage(chat.chats[chatId], msg.TLMessage.ID);
    }
    catch (Exception)
    {
    }
}

using var client = new WTelegram.Client(Config);
var myself = await client.LoginUserIfNeeded();
Console.WriteLine($"We are logged-in as {myself} (id {myself.id})");


var chats = await client.Messages_GetAllChats();
InputPeer peer = chats.chats[1482890345]; // the chat (or User) we want
                                          //for (int offset_id = 0; ;)
                                          //{
                                          //    var messages = await client.Messages_GetHistory(peer, offset_id);
                                          //    if (messages.Messages.Length == 0) break;
                                          //    foreach (var msgBase in messages.Messages)
                                          //    {
                                          //        var test = await client.Messages_GetDiscussionMessage(peer, msgBase.ID);
                                          //        var test2 = (test.messages[0] as Message);
                                          //        var test3 = await client.Messages_GetReplies(peer, msgBase.ID);

//        var from = messages.UserOrChat(msgBase.From ?? msgBase.Peer); // from can be User/Chat/Channel
//        if (msgBase is Message msg)
//            Console.WriteLine($"{from}> {msg.message} {msg.media}");
//        else if (msgBase is MessageService ms)
//            Console.WriteLine($"{from} [{ms.action.GetType().Name[13..]}]");
//    }
//    offset_id = messages.Messages[^1].ID;
//}

string? Config(string what)
{
    switch (what)
    {
        case "api_id": return config.GetValue<string>("ApiId");
        case "api_hash": return config.GetValue<string>("ApiHash");
        case "bot_token": return config.GetValue<string>("BotToken");
        case "phone_number": return config.GetValue<string>("PhoneNumber");
        case "verification_code": Console.Write("Code: "); return Console.ReadLine();
        case "first_name": return "John";      // if sign-up is required
        case "last_name": return "Doe";        // if sign-up is required
        case "password": return config.GetValue<string>("Password");     // if user has enabled 2FA
        default: return null;                  // let WTelegramClient decide the default config
    }
}