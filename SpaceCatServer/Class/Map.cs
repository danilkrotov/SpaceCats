using Discord;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using SpaceCatServer.Class.Enums;
using SpaceCatServer.Class.MiniClass;
using SpaceCatServer.Database;
using System.Drawing;
using System.Linq.Expressions;
using static System.Net.Mime.MediaTypeNames;

namespace SpaceCatServer.Class
{
    internal class Map
    {
        public int Id { get; private set; }
        public Galaxy Galaxy { get; private set; }
        public Sector Sector { get; private set; }
        public int Row { get; private set; }
        public int Column { get; private set; }
        /// <summary>
        /// Неуязвимая карта? не будет очищена из бд таблицы секторов при перезапуске мира
        /// </summary>
        public bool IsInvulnerable { get; private set; }
        /// <summary>
        /// Пустой конструктор для создания полей в БД
        /// </summary>
        private Map() { }
        internal Map(Galaxy galaxy, Sector sector, int row, int column, bool isInvulnerable = false)
        {
            Galaxy = galaxy;
            Sector = sector;
            Row = row;
            Column = column;
            IsInvulnerable = isInvulnerable;
            Save();
        }

        private void Save()
        {
            using (var db = new DataBaseContext())
            {
                this.Galaxy = db.Galaxys.FirstOrDefault(p => p.Id == this.Galaxy.Id); //пробрасываем ссылку на Galaxy
                this.Sector = db.Sectors.FirstOrDefault(p => p.Id == this.Sector.Id); //пробрасываем ссылку на Sector
                db.Maps.Add(this);
                db.SaveChanges();
            }
        }
        /// <summary>
        /// Создает новую галактику размером row * column, с технологическим уровнем galaxyLevel
        /// </summary>
        public static Galaxy CreateGalaxy(int row, int column, int galaxyLevel) 
        {
            //Создаем центральный сектор с вратами, создаём галактику (потому что она должна быть создана вместе с центральным сектором)
            Sector sect = Sector.CreateGateSector(); // Создаем сектор с вратами
            Galaxy gal = new Galaxy(sect); //Создаем галактику и назначем центральным сектором только что созданный
            new Map(gal, sect, row / 2, column / 2); // Создаем новую карту (где X = row / 2, Y = column / 2 это врата в центре)

            for (int x = 0; x < row; x++)
            {
                for (int y = 0; y < column; y++)
                {
                    if (x == row / 2 && y == column / 2)
                    {
                        //ничего не делаем т.к. выше уже создали сектор с вратами
                    }
                    else 
                    {
                        Sector sectr = Sector.CreateRandomSector(galaxyLevel); // создаем случайный сектор (и сохраняем в бд)
                        new Map(gal, sectr, x, y); // Присваеваем этот сектор к карте галактики
                    }
                }
            }
            return gal;
        }

        /// <summary>
        /// Возвращает карту по координатам, в рамках галактики
        /// </summary>
        public static Map? GetMapByCoordinate(int row, int column, int groupSectorId)
        {
            using (var db = new DataBaseContext())
            {
                Map? map = db.Maps
                    .Include(z => z.Sector)
                    .Include(z => z.Galaxy)
                    .Include(z => z.Sector.Tile)
                    .FirstOrDefault(s => s.Row == row && s.Column == column && s.Galaxy.Id == groupSectorId);
                return map;                
            }
        }
        public static List<Map> AllSectorAround(int sectorId)
        {
            using (var db = new DataBaseContext())
            {
                List<Map> maps = new List<Map>();
                Map? map = db.Maps.Include(z => z.Sector).Include(z => z.Galaxy).FirstOrDefault(s => s.Sector.Id == sectorId); //получаем сектор вокруг которого нужно вернуть все сектора
                if (map == null)
                {
                    throw new Exception("Исключение: Ожидается что Map с ID сектора: " + sectorId + " уже существует, но он не найден");
                }
                else
                {
                    for (int row = map.Row - 1; row < map.Row + 2; row++)
                    {
                        for (int column = map.Column - 1; column < map.Column + 2; column++)
                        {
                            Map? findMap = GetMapByCoordinate(row, column, map.Galaxy.Id);
                            if (findMap != null)
                            {
                                /* Вернуть с игнором сектора где мы находимся, пока идея на паузе
                                if (findMap.Row == map.Row && findMap.Column == map.Column)
                                {
                                    //это сектор в котором мы находимся сейчас, игнорируем его
                                }
                                else 
                                {
                                    maps.Add(findMap);
                                }
                                */
                                //Возвращаем все сектора и там где мы находимся тоже
                                maps.Add(findMap);
                            }
                        }
                    }
                }
                return maps;
            }
        }

        /// <summary>
        /// Сохраняет в png карту на жёстком диске всех секторов вокруг sectorId
        /// </summary>
        public static string GetPatchPngMapAroundSector(int sectorId, int radius, Account account)
        {
            using (var db = new DataBaseContext())
            {
                List<SectorInfo> secInfo = new List<SectorInfo>();
                //
                List<Map> maps = new List<Map>();
                Map? map = db.Maps.Include(z => z.Sector).Include(z => z.Galaxy).FirstOrDefault(s => s.Sector.Id == sectorId); //получаем сектор вокруг которого нужно вернуть все сектора
                if (map == null) 
                {
                    throw new Exception("Исключение: Ожидается что Map с ID сектора: " + sectorId + " уже существует, но он не найден");
                }
                else
                {
                    for (int row = map.Row - radius; row <= map.Row + radius; row++)
                    {
                        for (int column = map.Column - radius; column <= map.Column + radius; column++)
                        {
                            Map? findMap = GetMapByCoordinate(row, column, map.Galaxy.Id);
                            if (findMap != null) 
                            {
                                /* Вернуть с игнором сектора где мы находимся, пока идея на паузе
                                if (findMap.Row == map.Row && findMap.Column == map.Column)
                                {
                                    //это сектор в котором мы находимся сейчас, игнорируем его
                                }
                                else 
                                {
                                    maps.Add(findMap);
                                }
                                */
                                //Возвращаем все сектора и там где мы находимся тоже
                                maps.Add(findMap);
                            }
                        }
                    }

                    //Заводим Dictonary в котором "r1c1" это текстовая координата r - row, c - column
                    Dictionary<string, Map> dMap = new Dictionary<string, Map>();
                    //Заполняем его
                    for (int i = 0; i < maps.Count; i++)
                    {
                        dMap.Add("r" + maps[i].Row + "c" + maps[i].Column, maps[i]);
                    }

                    //Заполняем лист типом для отображения нужной картинки или 3 wall типом т.к. это стена\открытый космос
                    for (int row = map.Row - radius; row <= map.Row + radius; row++)
                    {
                        for (int column = map.Column - radius; column <= map.Column + radius; column++)
                        {
                            if (dMap.ContainsKey("r" + row + "c" + column))
                            {
                                //Если такой сектор уже был посещён, то не изменяем ему тип. Если такой сектор не был посещён изменяем здесь тип на Tile.Hidden
                                MapFog? mapFog = MapFog.GetMapFog(account.Did, dMap["r" + row + "c" + column].Id);
                                //сектор не был посещён, подменяем на Tile.Hidden
                                if (mapFog == null)
                                {
                                    secInfo.Add(new SectorInfo { Tile = new Tile { ModelId = Tile.Model.Hidden, Types = Tile.Type.Empty }, Name = dMap["r" + row + "c" + column].Sector.Name });
                                }
                                else 
                                {
                                    //Если такая локация была посещена ранее, показывает тайл
                                    //Если такая локация есть Заполняем в заголовок имя сектора
                                    secInfo.Add(new SectorInfo { Tile = dMap["r" + row + "c" + column].Sector.Tile, Name = dMap["r" + row + "c" + column].Sector.Name });
                                }
                            }
                            else
                            {
                                //Если такой локации нет пишем ХХХ и Пустой космос
                                secInfo.Add(new SectorInfo { Tile = new Tile { ModelId = Tile.Model.Wall, Types = Tile.Type.Empty }, Name = "" });
                            }
                        }
                    }                    
                }
                
                return PaintMap(secInfo, 1 + 2 * radius);
            }
        }

        /// <summary>
        /// Сохраняет в png карту на жёстком диске всех секторов вокруг sectorId
        /// </summary>
        public static string GetPatchAdminPngMapAroundSector(int sectorId, int radius)
        {
            using (var db = new DataBaseContext())
            {
                List<SectorInfo> secInfo = new List<SectorInfo>();
                //
                List<Map> maps = new List<Map>();
                Map? map = db.Maps.Include(z => z.Sector).Include(z => z.Galaxy).FirstOrDefault(s => s.Sector.Id == sectorId); //получаем сектор вокруг которого нужно вернуть все сектора
                if (map == null)
                {
                    throw new Exception("Исключение: Ожидается что Map с ID сектора: " + sectorId + " уже существует, но он не найден");
                }
                else
                {
                    for (int row = map.Row - radius; row <= map.Row + radius; row++)
                    {
                        for (int column = map.Column - radius; column <= map.Column + radius; column++)
                        {
                            Map? findMap = GetMapByCoordinate(row, column, map.Galaxy.Id);
                            if (findMap != null)
                            {
                                /* Вернуть с игнором сектора где мы находимся, пока идея на паузе
                                if (findMap.Row == map.Row && findMap.Column == map.Column)
                                {
                                    //это сектор в котором мы находимся сейчас, игнорируем его
                                }
                                else 
                                {
                                    maps.Add(findMap);
                                }
                                */
                                //Возвращаем все сектора и там где мы находимся тоже
                                maps.Add(findMap);
                            }
                        }
                    }

                    //Заводим Dictonary в котором "r1c1" это текстовая координата r - row, c - column
                    Dictionary<string, Map> dMap = new Dictionary<string, Map>();
                    //Заполняем его
                    for (int i = 0; i < maps.Count; i++)
                    {
                        dMap.Add("r" + maps[i].Row + "c" + maps[i].Column, maps[i]);
                    }

                    //Заполняем лист типом для отображения нужной картинки или 3 wall типом т.к. это стена\открытый космос
                    for (int row = map.Row - radius; row <= map.Row + radius; row++)
                    {
                        for (int column = map.Column - radius; column <= map.Column + radius; column++)
                        {
                            if (dMap.ContainsKey("r" + row + "c" + column))
                            {
                                //Если такая локация есть Заполняем в заголовок имя сектора
                                secInfo.Add(new SectorInfo { Tile = dMap["r" + row + "c" + column].Sector.Tile, Name = dMap["r" + row + "c" + column].Sector.Name });
                            }
                            else
                            {
                                //Если такой локации нет пишем ХХХ и Пустой космос
                                secInfo.Add(new SectorInfo { Tile = new Tile { ModelId = Tile.Model.Wall, Types = Tile.Type.Empty }, Name = "" });
                            }
                        }
                    }
                }

                return PaintMap(secInfo, 1 + 2 * radius);
            }
        }

        /// <summary>
        /// Возвращаем карту Map по id Сектора
        /// </summary>
        public static Map GetMap(int sectorId)
        {
            using (var db = new DataBaseContext())
            {
                Map? map = db.Maps.Include(z => z.Sector).Include(z => z.Galaxy).FirstOrDefault(s => s.Sector.Id == sectorId); //получаем сектор
                if (map == null)
                {
                    throw new Exception("Исключение: Ожидается что Map с ID сектора: " + sectorId + " уже существует, но он не найден #2");
                }
                else
                {
                    return map;
                }
            }
        }

        /// <summary>
        /// Тестовое описание - рисуем карту
        /// </summary>p
        public static string PaintMap(List<SectorInfo> secInfo, int matrixSize) 
        {
            //combine them into one image
            SKImage stitchedImage = Combine(secInfo, matrixSize);

            //save the new image
            using (SKData encoded = stitchedImage.Encode(SKEncodedImageFormat.Png, 100))
            using (Stream outFile = File.OpenWrite("C:\\img\\test.png"))
            {
                encoded.SaveTo(outFile);
            }

            return "C:\\img\\test.png";
        }

        private static SKImage Combine(List<SectorInfo> secInfo, int matrixSize, int imageWidth = 100, int imageHeight = 100)
        {
            //read all images into memory
            List<SKBitmap> images = new List<SKBitmap>();
            SKImage finalImage = null;

            try
            {
                int width = 0;
                int height = 0;

                for (int i = 0; i < secInfo.Count; i++)
                {
                    //create a bitmap from the file and add it to the list
                    SKBitmap bitmap = SKBitmap.Decode("C:\\tst\\" + (int)secInfo[i].Tile.ModelId + ".png");

                    //update the size of the final bitmap
                    width += bitmap.Width;
                    height += bitmap.Height;

                    images.Add(bitmap);
                }

                //get a surface so we can draw an image
                using (var tempSurface = SKSurface.Create(new SKImageInfo(imageWidth * matrixSize, imageHeight * matrixSize)))
                {
                    //get the drawing canvas of the surface
                    var canvas = tempSurface.Canvas;

                    //set background color
                    canvas.Clear(SKColors.Transparent);

                    //матрица X на X
                    for (int y = 0, count = 0; y < imageHeight * matrixSize; y += 100)
                    {
                        for (int x = 0; x < imageWidth * matrixSize; x += 100)
                        {
                            canvas.DrawBitmap(images[count], SKRect.Create(x, y, imageWidth, imageHeight));
                            DrawText(canvas, secInfo[count].Name, x + 50f, y + 93f); // вручную подогнан текст
                            count++;
                        }
                    }

                    // return the surface as a manageable image
                    finalImage = tempSurface.Snapshot();
                }

                //return the image that was just drawn
                return finalImage;
            }
            finally
            {
                //clean up memory
                foreach (SKBitmap image in images)
                {
                    image.Dispose();
                }
            }
        }

        private static SKPaint DrawText(SKCanvas canvas, string text, float x, float y)
        {
            using (var paint = new SKPaint())
            {
                paint.TextSize = 18.0f;
                paint.IsAntialias = true;
                paint.Color = new SKColor(0xFF, 0xFF, 0xFF);
                paint.IsStroke = false;
                paint.StrokeWidth = 3;
                paint.TextAlign = SKTextAlign.Center;

                canvas.DrawText(text, x, y, paint);
                return paint;
            }
        }

        /// <summary>
        /// Возвращает сектор столицы нейтральных
        /// </summary>
        public static Map GetNeutralCapital()
        {
            using (var db = new DataBaseContext())
            {
                Map? map = db.Maps.Include(z => z.Sector).Include(z => z.Galaxy).FirstOrDefault(s => s.Sector.Tile.ModelId == Tile.Model.NeutralCapital);
                if (map == null)
                {
                    throw new Exception("Исключение: Не найден сектор столицы нейтральной фракции, ожидается что сектор нейтральной столицы существует при создании базы данных");
                }
                else
                {
                    return map;
                }
            }
        }
    }
}
