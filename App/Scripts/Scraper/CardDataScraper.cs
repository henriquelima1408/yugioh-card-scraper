using yugioh_card_scraper.Model;
using yugioh_card_scraper.Utils;
using Newtonsoft.Json;
using static yugioh_card_scraper.Model.CardData;
using yugioh_card_scraper.Scripts.Scraper;

namespace yugioh_card_scraper.Scraper
{
    internal class CardDataScraper : CardScraper
    {
        class RequestInfo
        {

            readonly string cardID;
            readonly string languageID;

            public RequestInfo(string cardID, string languageID)
            {
                this.cardID = cardID;
                this.languageID = languageID;
            }

            public string CardID => cardID;

            public string LanguageID => languageID;
        }

        readonly string imageFormat;
        readonly HashSet<string> cardIDs = new HashSet<string>();
        readonly Dictionary<string, string> wikiPageUriCache = new Dictionary<string, string>();
        readonly string wikiRootUri = "";
        readonly string cookie = "";

        public CardDataScraper(string wikiRootUri, string uriFormat, int[] delayRange, DirectoryInfo cacheDirectory, HashSet<string> cardIDs, string cookie, string imageFormat = "png") : base(uriFormat, delayRange, cacheDirectory)
        {
            this.cardIDs = cardIDs;
            this.imageFormat = imageFormat;
            this.wikiRootUri = wikiRootUri;
            this.cookie = cookie;
        }

        public override T LoadLocal<T>()
        {
            var collection = new List<CardData>();
            var directories = cacheDirectory.GetDirectories();
            foreach (var directory in directories)
            {
                var files = directory.GetFiles();
                foreach (var file in files)
                {
                    if (file.Extension == ".json")
                    {
                        using (var streamReader = new StreamReader(file.FullName))
                        {
                            var content = streamReader.ReadToEnd();
                            var cardData = JsonConvert.DeserializeObject<CardData>(content);
                            collection.Add(cardData);
                        }
                    }
                }
            }

            return (T)(object)collection;
        }

        protected override HttpRequestMessage CreateNewRequest<T>(T data)
        {
            var requestInfo = data as RequestInfo;

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(string.Format(uriFormat, requestInfo.CardID, requestInfo.LanguageID)),
                Headers =
                    {
                        { "cookie", cookie },
                        { "authority", "www.db.yugioh-card.com" },
                        { "accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7" },
                        { "accept-language", "en-US,en;q=0.9,pt-BR;q=0.8,pt;q=0.7" },
                        { "cache-control", "max-age=0" },
                        { "referer", string.Format(uriFormat, requestInfo.CardID, requestInfo.LanguageID) },
                        { "sec-ch-ua-mobile", "?0" },
                        { "sec-fetch-dest", "document" },
                        { "sec-fetch-mode", "navigate" },
                        { "sec-fetch-site", "same-origin" },
                        { "sec-fetch-user", "?1" },
                        { "upgrade-insecure-requests", "1" },
                        { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36" },
                    },
            };
            return request;
        }

        protected override async Task<IEnumerable<T>> Scrap<T>(string cardID)
        {
            var collection = new List<CardData>();
            if (!wikiPageUriCache.ContainsKey(cardID))
            {
                var cardNames = new Dictionary<string, string>();
                var cardDescription = new Dictionary<string, string>();
                var cardSets = new Dictionary<string, IEnumerable<CardSet>>();
                var cardType = "";
                var cardAttribute = "";
                var species = new List<string>();
                var cardLevel = "";
                var cardAttack = "";
                var cardDefense = "";
                var cardRank = "";
                var cardPendulumScale = "";
                var cardLinkType = "";
                IEnumerable<string> cardLinkArrows = null;

                foreach (var language in Languages)
                {
                    using (var response = await client.SendAsync(CreateNewRequest(new RequestInfo(cardID, language))))
                    {
                        response.EnsureSuccessStatusCode();
                        var body = await response.Content.ReadAsStringAsync();
                        var htmDocument = ToHtmlDocument(body);

                        try
                        {
                            var info = CardInfoExtractor.ExtractInfo(htmDocument);

                            var cardInfo = info.Item1;
                            var sets = info.Item2;

                            cardNames.Add(language, cardInfo.CardName);
                            cardDescription.Add(language, cardInfo.CardDescription);
                            cardSets.Add(language, sets);

                            if (language == "en")
                            {
                                cardType = cardInfo.CardType;
                                cardAttribute = cardInfo.Attribute;
                                species.AddRange(cardInfo.Species);
                                cardLevel = cardInfo.Level;
                                cardAttack = cardInfo.Attack;
                                cardDefense = cardInfo.Defense;
                                cardRank = cardInfo.Rank;
                                cardPendulumScale = cardInfo.PendulumScale;
                                cardLinkType = cardInfo.LinkType;
                                cardLinkArrows = cardInfo.CardLinkArrows;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Could not extract information from card {cardID} on language {language} with error:\n{e}\n");
                        }
                    }
                }

                if (string.IsNullOrEmpty(cardType))
                {
                    Console.WriteLine($"Could not extract information properly from {cardType}");
                }

                collection.Add(new CardData(cardID, cardNames, cardType, cardDescription, cardSets, "None", cardAttribute, species, cardLevel, cardAttack, cardDefense, cardRank, cardPendulumScale, cardLinkType, cardLinkArrows));

                return (IEnumerable<T>)collection;
            }

            return (IEnumerable<T>)collection;
        }

        internal override async Task ScrapAll(string saveFormat)
        {
            var min = delayRange[0];
            var max = delayRange[1];
            var delta = max - min;

            var cacheCardDatas = LoadLocal<IEnumerable<CardData>>();
            if (cacheCardDatas != null)
            {
                foreach (var cacheCardData in cacheCardDatas)
                {
                    cardIDs.Remove(cacheCardData.CardID);
                }
            }

            foreach (var cardID in cardIDs)
            {
                var cardInfo = await LinearBackoff.DoRequest(async () =>
                {
                    var cardInfo = await Scrap<CardData>(cardID);
                    return cardInfo;

                }, 5000);

                if (cardInfo == null)
                {
                    Console.WriteLine($"\n Error with Card {cardID}. Ignoring...");
                    continue;
                }

                var directory = Directory.CreateDirectory(Path.Combine(cacheDirectory.FullName, cardID));
                var imageFileName = string.Format("{0}.{1}", cardID, imageFormat);

                WriteInfoAsJson<CardData>(directory.FullName, string.Format(saveFormat, cardID), cardInfo.First());

                var r = new Random().NextDouble();
                var v = delta * r + min;

                await Task.Delay((int)v);
            }

        }
    }
}
