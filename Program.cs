﻿using Microsoft.Extensions.Configuration;
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
using var bot = new WTelegram.Bot(botToken, apiId, apiHash, connection);
//          use new WTelegramBotClient(...) instead, if you want the power of WTelegram with Telegram.Bot compatibility for existing code
//          use new TelegramBotClient(...)  instead, if you just want Telegram.Bot classic code
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
        var chats = await bot.Client.Messages_GetAllChats();
        var chat = await bot.Client.Messages_GetChats(originChannel.Chat.Id);
        var test = await bot.Client.Messages_GetDiscussionMessage(chat.chats.First().Value, msg.TLMessage.ID);
    }
    catch (Exception)
    {
    }

}

//using var client = new WTelegram.Client(Config);
//var myself = await client.LoginUserIfNeeded();
//Console.WriteLine($"We are logged-in as {myself} (id {myself.id})");


var chats = await bot.Client.Messages_GetAllChats();
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

