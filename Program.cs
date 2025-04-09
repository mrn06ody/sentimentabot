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

//bot.Client.OnUpdates += (test) =>
//{
//    return Task.CompletedTask;
//};

bot.WantUnknownTLUpdates = true;
bot.OnError += (e, s) => Console.Error.WriteLineAsync(e.ToString());
bot.OnMessage += OnMessage;
while (Console.ReadKey(true).Key != ConsoleKey.Escape) { }

async Task OnMessage(WTelegram.Types.Message message, UpdateType type)
{
    if (message.Text == "/start")
    {
        await bot.SendMessage(message.Chat, $"Hello, {message.From}!\nTry commands /pic /react /lastseen /getchat /setphoto", replyParameters: message);
        return;
    }


    if (message.ForwardOrigin is not MessageOriginChannel originChannel)
    {
        return;
    }

    try
    {
        //var test = await bot.GetChat($"@{originChannel.Chat.Username}");
        //var tes2 = await bot.GetMessagesById($"@{originChannel.Chat.Username}", [originChannel.MessageId]);

        await bot.Client.LoginUserIfNeeded();
        var channel = await bot.Client.Contacts_ResolveUsername(originChannel.Chat.Username);

        //var channel = originChannel.Chat.TLInfo() as Channel;
        //var chats = await bot.Client.Messages_GetAllChats();
        //if (!chats.chats.ContainsKey(channel.ID))
        //{
        //    await bot.Client.Channels_JoinChannel(channel);
        //}

        //var peer = chats.chats[channel!.ID]; // the chat (or User) we want

        //var messages = await bot.Client.Messages_GetHistory(peer, 0);

        var minId = 0;
        var replies = new List<MessageBase>();
        while (true)
        {
            var test3 = await bot.Client.Messages_GetReplies(channel, originChannel.MessageId, add_offset: int.MaxValue);
            replies.AddRange(test3.Messages);

            if (test3.Count < 100)
            {
                break;
            }

            minId = test3.Messages[^1].ID;
        }

        //var chats = await bot.Client.Messages_GetAllChats();
        ////var chat = await bot.Client.Messages_GetChats(chatId);
        ////var test = await bot.Client.Messages_GetDiscussionMessage(chat.chats[chatId], msg.TLMessage.ID);
        //var test3 = await bot.Client.Messages_GetReplies(chats.chats[1], message.TLMessage.ID);
    }
    catch (Exception)
    {
    }
}

//using var client = new WTelegram.Client(Config);
//var myself = await client.LoginUserIfNeeded();
//Console.WriteLine($"We are logged-in as {myself} (id {myself.id})");


//var chats = await client.Messages_GetAllChats();
//InputPeer peer = chats.chats[1482890345]; // the chat (or User) we want
//for (int offset_id = 0; ;)
//{
//    var messages = await client.Messages_GetHistory(peer, offset_id);
//    if (messages.Messages.Length == 0) break;
//    foreach (var msgBase in messages.Messages)
//    {
//        var test = await client.Messages_GetDiscussionMessage(peer, msgBase.ID);
//        var test2 = (test.messages[0] as TL.Message);
//        var test3 = await client.Messages_GetReplies(peer, msgBase.ID);

//        var from = messages.UserOrChat(msgBase.From ?? msgBase.Peer); // from can be User/Chat/Channel
//        if (msgBase is TL.Message msg)
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
        case "first_name": return "Azhdar";
        case "last_name": return "Shirinzada";
        case "password": return config.GetValue<string>("Password");
        case "server_address": return "2>149.154.167.40:443"; // test DC
        default: return null; // let WTelegramClient decide the default config
    }
}

//static Task HandleMessage(MessageBase messageBase, bool edit = false)
//{
//    if (edit) Console.Write("(Edit): ");
//    switch (messageBase)
//    {
//        case TL.Message m: Console.WriteLine($"{Peer(m.from_id) ?? m.post_author} in {Peer(m.peer_id)}> {m.message}"); break;
//        case MessageService ms: Console.WriteLine($"{Peer(ms.from_id)} in {Peer(ms.peer_id)} [{ms.action.GetType().Name[13..]}]"); break;
//    }
//    return Task.CompletedTask;
//}