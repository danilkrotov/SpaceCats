using Discord.WebSocket;
using Discord;
using SpaceCatServer.Database;
using SpaceCatServer.Class.Enums;
using SpaceCatServer.Class.MiniClass;
using static SpaceCatServer.Class.Enums.Localization;
using System.Xml.Linq;

namespace SpaceCatServer.Class
{
    internal class Discord
    {
        private const ulong ChannelID = 658568022923149322;
        private const ulong BotID = 767602458725580800;
        public DiscordSocketClient Client { get; private set; }
        internal Discord(DiscordSocketClient discordSocketClient, string token)
        {
            Client = discordSocketClient;
            Client.Log += Client_Log;
            Client.MessageReceived += MessageReceived;
            Connect(token);
        }

        private async void Connect(string token) 
        {
            try
            {
                //await Client.LoginAsync(TokenType.Bot, "NzY3NjAyNDU4NzI1NTgwODAw.GfwG7Y.0qg8HASs-LoJrCFBHIkUiXTvb9h8HIQUukzI5g");
                await Client.LoginAsync(TokenType.Bot, token);
                await Client.StartAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Исключение при подключении бота к серверу: " + ex.Message);
                return;
            }
        }

        private Task Client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        private bool CheckAccess(Account account, SecurityLevel securityLevel) 
        {
            //если доступ превышает команду, выдать разрешение
            if (account.SecurityLevel >= securityLevel)
            {
                return true;
            }
            else 
            {
                return false;
            }
        }

        private async Task MessageReceived(SocketMessage message)
        {
            string[] args = message.Content.Split(' ');

            #region Устанавливаем ID чата куда должен отвечать бот
            if (message.Channel.Id != ChannelID)
            {
                //Console.WriteLine("Бот игнорирует канал");
                return;
            }
            #endregion

            #region Игнорируем сообщения от самих себя
            if (message.Author.Id == BotID)
            {
                //Console.WriteLine("Бот игнорирует сообщение сам от себя");
                return;
            }
            #endregion

            #region Идентифицируем пользователя или автоматически создаём аккаунт
            Account? account = Account.Get(message.Author.Id.ToString());

            if (account == null)
            {
                //Создаём аккаунт
                new Account(message.Author.Id.ToString(), new AccountOptions());
                Log logs = new Log(message.Content, message.Author.Id.ToString());
                logs.AddLog("Аккаунт не был распознан, автоматически создаю аккаунт для: " + message.Author.Id.ToString(), false);
                logs.Save();
                //Возвращаем в user созданный аккаунт
                account = Account.Get(message.Author.Id.ToString());
            }

            if (account == null)
            {
                Log logs = new Log(message.Content, message.Author.Id.ToString());
                logs.AddLog("Ожидатеся что аккаунт уже был создан, но система не смогла получить к нему доступ по ID");
                logs.Save();
                return;
            }
            #endregion

            #region Устанавливаем язык ответа
            Localization.Locale = account.Localization;
            #endregion

            #region Получаем активного персонажа (Если такой есть)
            Character? charct = Character.GetActiveCharacter(account.Did);
            #endregion

            #region Загружаем логирование
            Log log = new Log(message.Content, account.Did);
            #endregion

            #region Лист для ответа
            List<string> lstAnswer = new List<string>();
            #endregion

            switch (args[0].ToLower())
            {                
                //изменить на добавление слота
                #region !createaccount <did> <essence> <securityLevel> <characterSlot>
                case "!createaccount":
                    #region Длинна сообщения
                    if (args.Length != 5)
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        AccountOptions accopt = new AccountOptions();

                        dic.Add("iii", new Account("123213123", accopt));

                        log.AddLog("Длинна команды не соответствует шаблону");
                        await message.Channel.SendMessageAsync(Localization.Name("ErrNumArg"));
                        break;
                    }
                    #endregion
                    #region Права администратора на команду
                    if (!CheckAccess(account, SecurityLevel.Admin))
                    {
                        log.AddLog("Недостаточно прав для создания аккаунта");
                        await message.Channel.SendMessageAsync(Localization.Name("ErrPermission"));
                        break;
                    }
                    #endregion
                    #region Проверка args на число
                    if (!int.TryParse(args[2], out var x) || !int.TryParse(args[3], out var y) || !int.TryParse(args[4], out var z))
                    {
                        log.AddLog("Одно из значений [" + args[2] + "] [" + args[3] + "] [" + args[4] + "] не может быть преобразовано в число, операция прервана");
                        await message.Channel.SendMessageAsync("Invalid number of arguments in command, try this: !CreateAccount <DiscordID> <Essence Num> <Security Level> <Character Slots>");
                        break;
                    }
                    #endregion
                    #region Проверка существует ли аккаунт который пытаешься создать
                    Account? findUser = null;
                    using (var db = new DataBaseContext())
                    {
                        findUser = db.Accounts.FirstOrDefault(acc => acc.Did == args[1]);
                    }
                    if (findUser != null)
                    {
                        log.AddLog("Такой аккаунт уже существует");
                        await message.Channel.SendMessageAsync(Localization.Name("ErrAccount"));
                        break;
                    }
                    #endregion

                    AccountOptions options = new AccountOptions();
                    options.StartEssence = int.Parse(args[2]);
                    options.SecurityLevel = (SecurityLevel)int.Parse(args[3]);
                    new Account(args[1], options);
                    log.AddLog("Аккаунт [" + args[1] + "] успешно зарегестрирован", false);
                    await message.Channel.SendMessageAsync(message.Author.Mention + "\n" + Localization.Name("SucAccount"));
                    break;
                #endregion

                #region !createchar <name>
                case "!createchar":
                    #region Длинна сообщения
                    if (args.Length != 2)
                    {
                        log.AddLog("Длинна команды не соответствует шаблону");
                        await message.Channel.SendMessageAsync(Localization.Name("ErrNumArg"));
                        break;
                    }
                    #endregion
                    #region Проверить количество персонажей и доступных слотов
                    List<Character> listCharacter = Character.GetAllCharactersInAccount(account.Did);
                    if (listCharacter.Count >= account.CharacterSlot)
                    {
                        log.AddLog("У вас нет доступных слотов");
                        await message.Channel.SendMessageAsync(Localization.Name("ErrCharactCount"));
                        break;
                    }
                    #endregion

                    Character newCharct = new Character(account, args[1], new CharacterOptions());
                    new MapFog(account, Map.GetNeutralCapital(), true);
                    log.AddLog("Персонаж [" + args[1] + "] успешно создан", false);
                    //Делаем его активным при создании
                    newCharct.Activate(args[1]);
                    lstAnswer.Add(string.Format(Localization.Name("SucCharact"), args[1]));
                    await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                    break;
                #endregion

                #region !creategalaxy
                case "!creategalaxy":
                    #region Права администратора на команду
                    if (!CheckAccess(account, SecurityLevel.Admin))
                    {
                        log.AddLog("Недостаточно прав для создания аккаунта");
                        await message.Channel.SendMessageAsync(Localization.Name("ErrPermission"));
                        break;
                    }
                    #endregion
                    Galaxy galax = Map.CreateGalaxy(11, 11, 1);
                    log.AddLog("Создана галактика " + galax.Name, false);
                    lstAnswer.Add(string.Format(Localization.Name("SucCreateGalaxy"), galax.Name));
                    await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                    break;
                #endregion

                #region !choose <CharName>
                case "!choose":
                    #region Длинна сообщения
                    if (args.Length != 2 && args.Length != 1)
                    {
                        log.AddLog("Длинна команды не соответствует шаблону");
                        await message.Channel.SendMessageAsync(Localization.Name("ErrNumArg"));
                        break;
                    }
                    #endregion
                    
                    //Без параметров выведит список персонажей
                    if (args.Length == 1)
                    {
                        List<Character> lstChar = Character.GetAllCharactersInAccount(account.Did);

                        for (int i = 0; i < lstChar.Count; i++)
                        {
                            if (lstChar[i].IsActive == true)
                            {
                                lstAnswer.Add(lstChar[i].Name + " in Sector: [" + lstChar[i].Map.Sector.GetGalaxys().Name + "] " + lstChar[i].Map.Sector.Name + " **Active**");
                            }
                            else 
                            {
                                lstAnswer.Add(lstChar[i].Name + " in Sector: [" + lstChar[i].Map.Sector.GetGalaxys().Name + "] " + lstChar[i].Map.Sector.Name);
                            }
                        }

                        await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Your cats:", lstAnswer).Build());
                    }
                    //Делает активным персонажа
                    if (args.Length == 2) 
                    {
                        List<Character> lstChar = Character.GetAllCharactersInAccount(account.Did);
                        bool successActive = false;
                        for (int i = 0; i < lstChar.Count; i++)
                        {
                            if (lstChar[i].Name.ToLower() == args[1].ToLower()) 
                            {
                                lstChar[i].Activate(lstChar[i].Name);
                                successActive = true;
                            }
                        }

                        if (successActive)
                        {
                            log.AddLog("Успешная активация персонажа " + args[1], false);
                            lstAnswer.Add(args[1] + " goes on a travel");
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                        }
                        else 
                        {
                            log.AddLog("Персонаж с ником " + args[1] + " не найден");
                            lstAnswer.Add("No character found named " + args[1]);
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                        }
                    }
                    break;
                #endregion

                #region !move <SectorName>
                case "!move":
                    #region Длинна сообщения
                    if (args.Length != 2)
                    {
                        log.AddLog("Длинна команды не соответствует шаблону");
                        await message.Channel.SendMessageAsync(Localization.Name("ErrNumArg"));
                        break;
                    }
                    #endregion
                    #region Проверка на существование персонажа
                    if (charct == null)
                    {
                        await message.Channel.SendMessageAsync(Localization.Name("ErrCharactFound"));
                        break;
                    }
                    #endregion

                    //Пытаемся переместить персонажа
                    MoveError moveErr = charct.Move(args[1]);

                    if (moveErr == MoveError.Success) 
                    {
                        //Получаем тип тайла
                        if (charct.Map.Sector.Tile.Types == Tile.Type.Monster)
                        {
                            //Если в тайле монстр то бой неизбежен
                            FightAnswer fa = Monster.Fight(charct);
                            //Записываем ответ, для отправки одним сообщением
                            lstAnswer.Add("You find " + Enum.GetName(Tile.Type.Monster) + " in this sector.\nEnter fight with " + fa.MonsterInfo.Name + "\n" + Enum.GetName(fa.FightError));
                            if (fa.ItemList.Count > 0)
                            {
                                lstAnswer.Add("You find items:");
                                for (int i = 0; i < fa.ItemList.Count; i++)
                                {
                                    lstAnswer.Add(fa.ItemList[i].Item.Name);
                                }
                                //добавляем предметы в инвентарь
                                Inventory.AddItemList(charct, fa.ItemList);
                            }
                            //Добавляем информацию по поражению
                            if (fa.FightError == FightError.Defeat) 
                            {
                                lstAnswer.Add("You retreated to home sector " + charct.Map.Sector.Name);
                            }
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                        }
                        else 
                        {
                            log.AddLog("Персонаж успешно переместился в сектор " + args[1], false);
                            lstAnswer.Add("Welcome to sector: " + args[1] + "\nThis sector type: " + Enum.GetName(charct.Map.Sector.Tile.Types));
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                        }
                    }

                    if (moveErr == MoveError.NotFound) 
                    {
                        log.AddLog("Система с именем " + args[1] + " не найдена");
                        lstAnswer.Add("Sector " + args[1] + " not found aruond you");
                        await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                    }

                    if (moveErr == MoveError.SelfSector) 
                    {
                        log.AddLog("Персонаж пытается переместится в сектор в котором он уже находится: " + args[1]);
                        lstAnswer.Add("You are already here");
                        await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                    }
                    
                    break;
                #endregion

                #region !warp <GalaxyName>
                case "!warp":
                    //Перемещает персонажа в выбранную систему
                    #region Длинна сообщения
                    if (args.Length != 2)
                    {
                        log.AddLog("Длинна команды не соответствует шаблону");
                        await message.Channel.SendMessageAsync(Localization.Name("ErrNumArg"));
                        break;
                    }
                    #endregion
                    #region Проверка на существование персонажа
                    if (charct == null)
                        {
                            await message.Channel.SendMessageAsync(Localization.Name("ErrCharactFound"));
                            break;
                        }
                        #endregion

                    MoveError warpErr = charct.Warp(args[1]);

                    if (warpErr == MoveError.Success)
                    {
                        log.AddLog("Персонаж успешно переместился в галактику " + args[1], false);
                        lstAnswer.Add("Welcome to galaxy: " + args[1]);
                        await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                    }

                    if (warpErr == MoveError.NotFound)
                    {
                        log.AddLog("Галактика с именем " + args[1] + " не найдена");
                        lstAnswer.Add("Sector " + args[1] + " not found aruond you");
                        await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                    }

                    if (warpErr == MoveError.GateNotFound)
                    {
                        log.AddLog("Персонаж попытался прыгнуть в галактику " + args[1] + " находясь в секторе без ворот", false);
                        lstAnswer.Add("You dont warp in " + args[1] + " sector, find sector with gate");
                        await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                    }
                    break;
                #endregion

                #region !map <radius>
                case "!map":
                    #region Длинна сообщения
                    if (args.Length != 2 && args.Length != 1)
                    {
                        log.AddLog("Длинна команды не соответствует шаблону");
                        await message.Channel.SendMessageAsync(Localization.Name("ErrNumArg"));
                        break;
                    }
                    #endregion
                    #region Проверка на существование персонажа
                    if (charct == null)
                    {
                        await message.Channel.SendMessageAsync(Localization.Name("ErrCharactFound"));
                        break;
                    }
                    #endregion
                    if (args.Length == 1)
                    {
                        //Возвращает ссылку
                        await message.Channel.SendFileAsync(Map.GetPatchPngMapAroundSector(charct.Map.Sector.Id, 1, account), message.Author.Mention);
                    }

                    if (args.Length == 2)
                    {
                        int radius = int.Parse(args[1]);
                        if (radius > 5) { radius = 5; }
                        //Возвращает ссылку
                        await message.Channel.SendFileAsync(Map.GetPatchPngMapAroundSector(charct.Map.Sector.Id, radius, account), message.Author.Mention);
                    }
                    break;
                #endregion

                #region !scan
                case "!scan":
                    #region Длинна сообщения
                    if (args.Length != 1)
                    {
                        log.AddLog("Длинна команды не соответствует шаблону");
                        await message.Channel.SendMessageAsync(Localization.Name("ErrNumArg"));
                        break;
                    }
                    #endregion

                    //Получаем тип тайла
                    Tile.Type tileType = charct.Map.Sector.Tile.Types;                    
                    log.AddLog("Успешное сканирование сектора, тип: " + charct.Map.Sector.Tile.Types, false);
                    //Показываем подсказку в зависимости от типа тайла
                    switch (charct.Map.Sector.Tile.Types)
                    {
                        case Tile.Type.Quest:
                            QuestInfo qinfo = Quest.GetDescription(charct.Map.Sector.Tile.ModelId);
                            string qdescription = Quest.QuestInfo(qinfo);
                            lstAnswer.Add("\nYou find " + Enum.GetName(Tile.Type.Quest) + " in this sector.\n" + qdescription + "\nNOTE: Use command \"!quest\" to see your quest or use command \"!quest accept\" to give quest or use command \"!quest complete\" to complete quest. Attention old quest will be deleted!");
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                            break;
                        /*
                        // Если в тайле монстр то бой неизбежен, смотри !move
                        case Tile.Type.Monster:
                            break;
                        */
                        default:
                            lstAnswer.Add("There is nothing in this sector");
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                            break;
                    }
                    break;
                #endregion

                #region !quest <accept>
                case "!quest":
                    #region Длинна сообщения
                    if (args.Length != 2 && args.Length != 1)
                    {
                        log.AddLog("Длинна команды не соответствует шаблону");
                        await message.Channel.SendMessageAsync(Localization.Name("ErrNumArg"));
                        break;
                    }
                    #endregion
                    #region Проверка на существование персонажа
                    if (charct == null)
                    {
                        await message.Channel.SendMessageAsync(Localization.Name("ErrCharactFound"));
                        break;
                    }
                    #endregion
                    //Без параметров показываем текущий активный квест
                    if (args.Length == 1)
                    {
                        //Проверяем взят ли квест
                        if (charct.ActiveQuest == null)
                        {
                            log.AddLog("У вас нет активных квестов", false);
                            lstAnswer.Add("You not have active quest");
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                        }
                        else 
                        {
                            log.AddLog("Отображаем квест id: " + charct.ActiveQuest.Id, false);
                            QuestInfo qinfo = Quest.GetDescriptionByQuest(charct.ActiveQuest);
                            lstAnswer.Add(Quest.QuestInfo(qinfo));
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                        }
                    }
                    //Заменяем текущий квест на новый
                    if (args.Length == 2 && args[1] == "accept")
                    {
                        //Персонажу даем ссылку на квест. Квест ищем по ModelID.
                        charct.GetQuest(Quest.Get(charct.Map.Sector.Tile.ModelId));
                        //Опустошаем текущий сектор
                        charct.Map.Sector.EmptyThisTile();
                        //
                        log.AddLog("Успешно получено задание id: " + charct.ActiveQuest.Name, false);
                        lstAnswer.Add("You succesfull accept quest: **" + charct.ActiveQuest.Name + "**");
                        await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                    }
                    //Завершаем текущий квест
                    if (args.Length == 2 && args[1] == "complete")
                    {
                        if (charct.ActiveQuest == null)
                        {
                            log.AddLog("У " + charct.Name + " нет активных квестов", false);
                            lstAnswer.Add(charct.Name + " not have active quest");
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                            return;
                        }

                        SellError sellErr = charct.QuestComplete();

                        if (sellErr == SellError.NotFound)
                        {
                            lstAnswer.Add("Some items not found in " + charct.Name + " inventory");
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                        }

                        if (sellErr == SellError.NotEnough)
                        {
                            lstAnswer.Add("Some items are not so many in " + charct.Name + " inventory");
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                        }

                        if (sellErr == SellError.Success)
                        {
                            lstAnswer.Add("You successfull complete quest");
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                        }
                    }
                    break;
                #endregion

                #region !mapadmin <radius>
                case "!mapadmin":
                    #region Проверка на существование персонажа
                    if (charct == null)
                    {
                        await message.Channel.SendMessageAsync(Localization.Name("ErrCharactFound"));
                        break;
                    }
                    #endregion
                    #region Права администратора на команду
                    if (!CheckAccess(account, SecurityLevel.Admin))
                    {
                        log.AddLog("Недостаточно прав для создания аккаунта");
                        await message.Channel.SendMessageAsync(Localization.Name("ErrPermission"));
                        break;
                    }
                    #endregion
                    if (args.Length == 2)
                    {
                        //Возвращает ссылку
                        await message.Channel.SendFileAsync(Map.GetPatchAdminPngMapAroundSector(charct.Map.Sector.Id, int.Parse(args[1])));
                    }
                    break;
                #endregion

                #region !imgtest
                case "!imgtest":
                    if (args.Length == 1)
                    {
                        await message.Channel.SendFileAsync("G:\\img\\test.png", "Caption goes here");
                    }
                    break;
                #endregion

                #region !emb
                case "!emb":
                    if (args.Length == 1)
                    {
                        var embed = new EmbedBuilder
                        {
                            // Embed property can be set within object initializer
                            Title = "Hello world!",
                            Description = "I am a description set by initializer."
                        };
                        // Or with methods
                        embed.AddField("Field title","Field value. I also support [hyperlink markdown](https://example.com)!")
                            .WithAuthor(Client.CurrentUser)
                            //.WithAuthor(message.Author)
                            .WithFooter(footer => footer.Text = "I am a footer.")
                            .WithColor(Color.Blue) //цвет полоски ответа слева от сообщения
                            .WithTitle("I overwrote \"Hello world!\"")
                            .WithDescription("I am a description.")
                            .WithUrl("https://example.com")
                            .WithCurrentTimestamp();
                        embed.AddField("And new field title", "next field");

                        //Your embed needs to be built before it is able to be sent
                        await message.Channel.SendMessageAsync(embed: embed.Build());
                    }
                    break;
                #endregion

                #region !myid
                case "!myid":
                    if (args.Length == 1)
                    {
                        //Your embed needs to be built before it is able to be sent
                        await message.Channel.SendMessageAsync("You id: " + account.Did);
                    }
                    break;
                #endregion

                #region !user <@User>
                case "!user":
                    if (args.Length == 2)
                    {
                        List<SocketUser> userstst = message.MentionedUsers.ToList();
                        //Your embed needs to be built before it is able to be sent
                        await message.Channel.SendMessageAsync("You message user: " + userstst[0].Id + " " + userstst[0].Username + " " + message.Author.Mention);
                    }
                    break;
                #endregion

                #region !sharemap <galaxy name> <@User>
                case "!sharemap":
                    #region Проверка на длинну
                    if (args.Length != 3)
                    {
                        log.AddLog("Длинна команды не соответствует шаблону");
                        await message.Channel.SendMessageAsync(message.Author.Mention + "\n" + "Invalid number of arguments in command, try this: !sharemap <galaxy name> <@User>");
                        break;
                    }
                    #endregion

                    Galaxy? gal = Galaxy.GetGalaxyByName(args[1]);
                    if (gal == null) 
                    {
                        log.AddLog("Галактика с именем " + args[1] + " не найдена");
                        lstAnswer.Add("Sector " + args[1] + " not found aruond you");
                        await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                        break;
                    }

                    List<SocketUser> users = message.MentionedUsers.ToList();

                    Account? acc = Account.Get(users[0].Id.ToString());
                    if (acc == null) 
                    {
                        log.AddLog("Пользователь с ID " + users[0].Username + " не найден");
                        lstAnswer.Add("Account id " + users[0].Username + " not found");
                        await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                        break;
                    }

                    //Получаем список карт первого игрока + второго игрока уникальные значения
                    List<Map> map = MapFog.DistinctTwoFogs(account.Did, acc.Did, gal);
                    //Удаляем все открытые сектора у аккаунта в галактике которую расшариваем
                    MapFog.DeleteInGalaxy(acc, gal);
                    //Открываем все карты аккаунту из списка
                    for (int i = 0; i < map.Count; i++)
                    {
                        new MapFog(acc, map[i], map[i].IsInvulnerable);
                    }
                    log.AddLog("Галактика " + gal.Name + " успешна передана от игрока " + message.Author.Username + " игроку " + users[0].Username);
                    lstAnswer.Add("You successfull share map to " + users[0].Mention);
                    await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                    break;
                #endregion

                #region !inventory <item page>
                case "!inventory":
                    //без параметров первые Х позиций
                    if (args.Length == 1)
                    {
                        List<Inventory> inventory = Inventory.GetItem(charct, 0);
                        if (inventory.Count == 0)
                        {
                            lstAnswer.Add(charct.Name + " inventory is empty");
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Inventory item:", lstAnswer).Build());
                        }
                        else
                        {
                            for (int i = 0; i < inventory.Count; i++)
                            {
                                lstAnswer.Add(inventory[i].Item.Name + " " + inventory[i].Count);
                            }
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Inventory item:", lstAnswer).Build());
                        }
                    }
                    // С параметрами
                    if (args.Length == 2)
                    {
                        //Проверить args на число
                        List<Inventory> inventory = Inventory.GetItem(charct, int.Parse(args[1]));
                        if (inventory.Count == 0)
                        {
                            lstAnswer.Add(charct.Name + " inventory pages " + args[1] + " empty");
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Inventory item:", lstAnswer).Build());
                        }
                        else 
                        {
                            for (int i = 0; i < inventory.Count; i++)
                            {
                                lstAnswer.Add(inventory[i].Item.Name + " " + inventory[i].Count);
                            }
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Inventory item pages " + args[1] + ":", lstAnswer).Build());
                        }
                    }
                    break;
                #endregion

                #region !sell <item count> <item name>
                case "!sell":
                    // С параметрами (все после args[2] необходимо собирать в слово)
                    if (args.Length >= 3)
                    {
                        #region Должно быть числом
                        bool result = int.TryParse(args[1], out int itemCount);
                        if (result == false) 
                        {
                            lstAnswer.Add("Second parameter is not equal to number");
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                            break;
                        }
                        #endregion

                        #region Не может быть отрицательным или нулевым
                        if (itemCount <= 0) 
                        {
                            lstAnswer.Add("Second parameter cannot be zero or negative");
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                            break;
                        }
                        #endregion

                        //собираем название предмета в одну переменную (i == 2, пропускаем первые два параметра)
                        string itemname = "";
                        for (int i = 2; i < args.Length; i++)
                        {
                            itemname = itemname + args[i] + " ";
                        }
                        //удаляем пробел у последнего слова (upd и опускаем все буквы)
                        itemname = itemname.Substring(0, itemname.Length - 1).ToLower();

                        SellError sellErr = Inventory.SellItem(charct, itemname, int.Parse(args[1]));
                        
                        if (sellErr == SellError.NotFound) 
                        {
                            lstAnswer.Add("Item not found in " + charct.Name + " inventory");
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                        }

                        if (sellErr == SellError.NotEnough) 
                        {
                            lstAnswer.Add("You don't have " + args[1] + " " + itemname + " in " + charct.Name + " inventory");
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                        }

                        if (sellErr == SellError.Success)
                        {
                            lstAnswer.Add("You successfull sell " + args[1] + " " + itemname + " in " + charct.Name + " inventory");
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                        }
                    }
                    break;
                #endregion

                #region !craft <recept name>
                case "!craft":
                    // С параметрами (все после args[2] необходимо собирать в слово)
                    if (args.Length >= 2)
                    {
                        //собираем название предмета в одну переменную (i == 1, пропускаем первый параметр)
                        string craftname = "";
                        for (int i = 1; i < args.Length; i++)
                        {
                            craftname = craftname + args[i] + " ";
                        }
                        //удаляем пробел у последнего слова (опускаем буквы до низких)
                        craftname = craftname.Substring(0, craftname.Length - 1).ToLower();

                        SellError sellErr = Inventory.CraftItem(charct, craftname);

                        if (sellErr == SellError.NotFound)
                        {
                            lstAnswer.Add("Some items not found in " + charct.Name + " inventory");
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                        }

                        if (sellErr == SellError.NotEnough)
                        {
                            lstAnswer.Add("Some items are not so many in " + charct.Name + " inventory");
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                        }

                        if (sellErr == SellError.Success)
                        {
                            lstAnswer.Add("You successfull craft for " + craftname + " recept");
                            await message.Channel.SendMessageAsync(message.Author.Mention, embed: EmbBuilder("Message:", lstAnswer).Build());
                        }
                    }
                    break;
                #endregion

                #region ADMIN !fraction <char name> <fraction>
                case "!fraction":
                    if (args.Length == 3)
                    {
                        #region Права администратора на команду
                        if (!CheckAccess(account, SecurityLevel.Admin))
                        {
                            log.AddLog("Недостаточно прав для создания аккаунта");
                            await message.Channel.SendMessageAsync(Localization.Name("ErrPermission"));
                            break;
                        }
                        #endregion

                        if (args[2] == Enum.GetName(Fractions.Neutral)) 
                        {
                            charct.ChangeFraction(Fractions.Neutral);
                            await message.Channel.SendMessageAsync(message.Author.Mention + "\n" + charct.Name + " successfull join to " + Enum.GetName(Fractions.Neutral) + " fraction");
                        }

                        if (args[2] == Enum.GetName(Fractions.Good))
                        {
                            charct.ChangeFraction(Fractions.Good);
                            await message.Channel.SendMessageAsync(message.Author.Mention + "\n" + charct.Name + " successfull join to " + Enum.GetName(Fractions.Good) + " fraction");
                        }

                        if (args[2] == Enum.GetName(Fractions.Evyl))
                        {
                            charct.ChangeFraction(Fractions.Evyl);
                            await message.Channel.SendMessageAsync(message.Author.Mention + "\n" + charct.Name + " successfull join to " + Enum.GetName(Fractions.Evyl) + " fraction");
                        }
                    }
                    break;
                #endregion

                default:
                    log.AddLog("Команда не распознана: " + message);
                    break;
            }

            log.Save();
        }

        /// <summary>
        /// Возвращает отформатированный ответ с синей полоской сбоку
        /// </summary>
        private EmbedBuilder EmbBuilder(string header, List<string> strings) 
        {
            EmbedBuilder emb = new EmbedBuilder();
            string answer = "";
            for (int i = 0; i < strings.Count; i++)
            {
                answer += strings[i] + "\n";
            }
            emb.AddField(header, answer).WithColor(Color.Blue); //цвет полоски ответа слева от сообщения

            return emb;
        }

    }
}
