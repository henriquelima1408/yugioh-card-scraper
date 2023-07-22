using yugioh_card_scraper.Model;
using yugioh_card_scraper.Utils;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace yugioh_card_scraper.Scraper
{
    internal class CardDataScraper : CardScraper
    {
        class CardInfo
        {

            readonly CardData cardData;
            readonly byte[] imageBytes;

            public CardInfo(CardData cardData, byte[] imageBytes)
            {
                this.cardData = cardData;
                this.imageBytes = imageBytes;
            }

            public byte[] ImageBytes => imageBytes;

            internal CardData CardData => cardData;
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
            var cardID = data as string;

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(string.Format(uriFormat, cardID)),
                Headers =
                {
                    { "authority", "yugioh.fandom.com" },
                    { "accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7" },
                    { "accept-language", "en-US,en;q=0.9,pt-BR;q=0.8,pt;q=0.7" },
                    { "cache-control", "max-age=0" },
                    { "cookie", cookie },
                    { "referer", string.Format("https://yugioh.fandom.com/wiki/Special:SearchByProperty?limit=500&offset=0&property=Database+ID&value={0}", cardID )},
                    { "sec-ch-ua-mobile", "?0" },
                    { "sec-fetch-dest", "document" },
                    { "sec-fetch-mode", "navigate" },
                    { "sec-fetch-site", "same-origin" },
                    { "sec-fetch-user", "?1" },
                    { "upgrade-insecure-requests", "1" },
                    { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36" },
                },
            };
            return request;
        }

        protected override async Task<IEnumerable<T>> Scrap<T>(string cardID)
        {
            var collection = new List<CardInfo>();
            if (!wikiPageUriCache.ContainsKey(cardID))
            {
                using (var response = await client.SendAsync(CreateNewRequest(cardID)))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    var htmDocument = ToHtmlDocument(body);
                    var bodyContentNode = htmDocument.DocumentNode.FindNode("class", "mw-body-content").First();
                    var ulNodes = bodyContentNode.FindChildrenNodesByName("ul");


                    foreach (var ulNode in ulNodes)
                    {
                        var childrenNodes = ulNode.ChildNodes;
                        foreach (var childrenNode in childrenNodes)
                        {

                            var strong = childrenNode.FindChildrenNodesByName("strong");
                            HtmlNode wikiCardIDNode;
                            if (strong != null && strong.Count() > 0)
                            {
                                wikiCardIDNode = strong.First().FindChildrenNodesByName("em").First();
                            }
                            else
                            {
                                wikiCardIDNode = childrenNode.FindChildrenNodesByName("em").First();
                            }

                            var wikiCardID = wikiCardIDNode.InnerText.RemoveSpecialCharacters();
                            var uri = (strong != null && strong.Count() > 0) ?
                                strong.First().FindChildrenNodesByName("a").First().GetAttributeValue("href", "") :
                                childrenNode.FindChildrenNodesByName("a").First().GetAttributeValue("href", "");

                            if (!wikiPageUriCache.ContainsKey(wikiCardID))
                            {
                                wikiPageUriCache.Add(wikiCardID, uri);
                            }
                        }
                    }
                }
            }

            var wikiUri = wikiRootUri + wikiPageUriCache[cardID];


            CardData cardData = null;
            var cardNames = new Dictionary<string, string>();
            var cardDescriptions = new Dictionary<string, string>();
            var cardType = "";
            var cardAttribute = "";
            var cardTypes = new List<string>();
            var cardLevel = "";
            var attack = "";
            var defense = "";
            var rank = "";
            var pendulumScale = "";
            var statuses = "";
            var linkRating = "";
            var linkArrows = new List<string>();
            var materials = "";
            var sets = new Dictionary<string, IEnumerable<CardData.CardSet>>();
            var cardImageUri = "";

            var cardDataRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(wikiUri)
            };

            HttpRequestMessage cardImageRequest;

            using (var response = await client.SendAsync(cardDataRequest))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var htmDocument = ToHtmlDocument(body);
                var bodyContentNode = htmDocument.DocumentNode.FindNode("class", "mw-parser-output").First();
                var cardTable = bodyContentNode.FindChildrenByClass("cardtable");
                var childNodes = cardTable.FindChildrenNodesByName("tbody").First().ChildNodes;


                var languages = CardData.Languages;

                foreach (var childNode in childNodes)
                {
                    var cardRowHeader = childNode.FindChildrenByClass("cardtablerowheader");
                    var cardRowData = childNode.FindChildrenByClass("cardtablerowdata");
                    var cardTableAnRow = childNode.FindChildrenByClass("cardtablespanrow");
                    var cardImage = childNode.FindChildrenByClass("cardtable-cardimage");

                    if (cardImage != null)
                    {
                        cardImageUri = cardImage.FindChildrenNodesByName("a").First().GetAttributeValue("href", "");
                    }

                    if (cardRowHeader != null && cardRowData != null)
                    {
                        var header = cardRowHeader.InnerText;
                        var data = cardRowData.InnerText.TrimStart().TrimEnd();
                        if (languages.Any(l => l.ToString() == cardRowHeader.InnerText))
                        {
                            cardNames[header] = data;
                        }
                        else if (header == "Card type")
                        {
                            cardType = data;
                        }
                        else if (header == "Property")
                        {
                            cardTypes.Add(data);
                        }
                        else if (header == "Attribute")
                        {
                            cardAttribute = data;
                        }
                        else if (header == "Types")
                        {
                            var types = data.Split('/');
                            for (int i = 0; i < types.Length; i++)
                            {
                                types[i] = types[i].TrimStart().TrimEnd();
                            }
                            cardTypes.AddRange(types);
                        }
                        else if (header == "Level")
                        {
                            cardLevel = data;
                        }
                        else if (header == "ATK / DEF")
                        {
                            var numbers = data.FindAllNumbers();
                            attack = numbers.First().ToString().TrimStart().TrimEnd();
                            defense = numbers.Last().ToString().TrimStart().TrimEnd();
                        }
                        else if (header == "ATK / LINK")
                        {
                            var numbers = data.FindAllNumbers();
                            attack = numbers.First().ToString().TrimStart().TrimEnd();
                            linkRating = numbers.Last().ToString().TrimStart().TrimEnd();
                        }
                        else if (header == "Rank")
                        {
                            rank = data;
                        }
                        else if (header == "Pendulum Scale")
                        {
                            pendulumScale = data;
                        }
                        else if (header == "Link Arrows")
                        {
                            var arrows = data.Split(",");
                            for (int i = 0; i < arrows.Length; i++)
                            {
                                arrows[i] = arrows[i].TrimStart().TrimEnd();
                            }
                            linkArrows.AddRange(arrows);
                        }
                        else if (header == "Statuses")
                        {
                            statuses = data;
                        }
                        else if (header == "Materials")
                        {
                            materials = data;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (cardTableAnRow != null)
                        {
                            var navBoxes = cardTableAnRow.FindChildrensByClass("navbox");

                            foreach (var navBox in navBoxes)
                            {
                                var tBody = navBox.FindChildrenNodesByName("tbody").First();
                                var tr = tBody.FindChildrenNodesByName("tr").First();
                                var td = tr.FindChildrenNodesByName("td").First();
                                var table = td.FindChildrenNodesByName("table").First();
                                var innetTBody = table.FindChildrenNodesByName("tbody").First();

                                var language = "";
                                var description = "";
                                List<CardData.CardSet> cardSets = new List<CardData.CardSet>();

                                foreach (var innerTBodyChildNode in innetTBody.ChildNodes)
                                {
                                    var nodeContent = innerTBodyChildNode.InnerText.TrimStart().TrimEnd();

                                    if (!string.IsNullOrEmpty(nodeContent))
                                    {
                                        if (languages.Any(l => l.ToString() == nodeContent))
                                        {
                                            language = nodeContent;
                                        }
                                        else
                                        {
                                            description = nodeContent;
                                        }
                                    }

                                }

                                var trs = innetTBody.FindChildrenNodesByName("tr").Where(t => t.FindChildrenByClass("navbox-list") != null);
                                if (trs != null && trs.Count() > 0)
                                {
                                    var navBoxList = trs.First().FindChildrenByClass("navbox-list");
                                    var cardSetNode = navBoxList.FindChildrenByClass("cardSet");
                                    if (cardSetNode != null)
                                    {
                                        var divs = cardSetNode.FindChildrenNodesByName("div");

                                        foreach (var div in divs)
                                        {
                                            var spans = div.FindChildrenNodesByName("span").ToArray();

                                            var date = spans.First().InnerText;
                                            DateTime.TryParse(date.ToString(), out DateTime dt);
                                            if (dt != DateTime.MinValue)
                                            {
                                                cardSets.Add(new CardData.CardSet(spans[0].InnerText, spans[1].InnerText, spans[2].InnerText, spans[3].InnerText));
                                            }
                                            else
                                            {
                                                if (string.IsNullOrEmpty(date))
                                                    cardSets.Add(new CardData.CardSet("None", spans[1].InnerText, spans[2].InnerText, spans[3].InnerText));
                                            }


                                        }
                                    }
                                    else
                                    {
                                        var t = navBoxList.FindChildrenNodesByName("table");

                                        if (t.Count() > 0)
                                        {
                                            var caption = t.First().FindChildrenNodesByName("caption");
                                            if (caption != null && caption.Count() > 0)
                                            {
                                                var setBody = t.First().FindChildrenNodesByName("tbody");
                                                foreach (var element in setBody)
                                                {
                                                    var setInfos = element.FindChildrenNodesByName("tr");
                                                    foreach (var setInfo in setInfos)
                                                    {
                                                        var tds = setInfo.FindChildrenNodesByName("td").ToArray();
                                                        if (tds.Length > 0)
                                                        {
                                                            cardSets.Add(new CardData.CardSet(tds[0].InnerText, tds[1].InnerText, tds[2].InnerText, tds[3].InnerText));
                                                        }
                                                    }
                                                }
                                            }

                                        }





                                    }
                                }

                                if (!string.IsNullOrEmpty(language))
                                {
                                    if (!languages.Contains(language))
                                        throw new Exception($"Language {language} not found");

                                    if (cardSets.Count > 0)
                                    {
                                        sets.Add(language, cardSets);
                                    }
                                    else
                                    {
                                        cardDescriptions.Add(language, description);
                                    }
                                }
                            }
                        }
                    }
                }

                cardData = new CardData(cardID, cardNames, cardType, cardDescriptions, sets, statuses, cardAttribute, cardTypes, cardLevel, attack, defense, rank, pendulumScale, linkRating, linkArrows);

                cardImageRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(cardImageUri)
                };

            }

            if (cardImageRequest != null && cardData != null)
            {
                using (var response = await client.SendAsync(cardImageRequest))
                {
                    response.EnsureSuccessStatusCode();
                    var byteArray = await response.Content.ReadAsByteArrayAsync();
                    collection.Add(new CardInfo(cardData, byteArray));

                }
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
                    var cardInfo = await Scrap<CardInfo>(cardID);
                    return cardInfo;

                }, 5000);

                if (cardInfo == null)
                {
                    Console.WriteLine($"\n Error with Card {cardID}. Ignoring...");
                    continue;
                }

                var directory = Directory.CreateDirectory(Path.Combine(cacheDirectory.FullName, cardID));
                var imageFileName = string.Format("{0}.{1}", cardID, imageFormat);
                File.WriteAllBytes(Path.Combine(directory.FullName, imageFileName), cardInfo.First().ImageBytes);
                WriteInfoAsJson<CardData>(directory.FullName, string.Format(saveFormat, cardID), cardInfo.First().CardData);

                var r = new Random().NextDouble();
                var v = delta * r + min;

                await Task.Delay((int)v);
            }

        }
    }
}
