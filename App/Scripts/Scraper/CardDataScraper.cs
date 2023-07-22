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

        public CardDataScraper(string wikiRootUri, string uriFormat, int[] delayRange, DirectoryInfo cacheDirectory, HashSet<string> cardIDs, string imageFormat = "png") : base(uriFormat, delayRange, cacheDirectory)
        {
            this.cardIDs = cardIDs;
            this.imageFormat = imageFormat;
            this.wikiRootUri = wikiRootUri;
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
                    { "cookie", "_au_1d=AU1D-0100-001687671475-UMWJ3ASJ-2S63; optimizelyEndUserId=oeu1687671475405r0.12619845264274376; tracking-opt-in-status=accepted; tracking-opt-in-version=5; _b2=2hcZtnQDpy.1687671473735; wikia_beacon_id=DK_6pYMlXI; wikia_session_id=pjSXZ9moNi; addtl_consent=1~2822.3116; euconsent-v2=CPt6yMAPt6yMACNAFAENDLCsAP_AAH_AACiQI8tR7D7NbSFD8e59YPs0OQ1Hx1DIYiQgAASBAmABQAKQIIwCgkE5BETABAgCAAAAIAJBAAAECABQCUAAQAAAIAFAIAAABQAKIAAAgAIRIgAICAAAAAEAEAAIgABAEgAB0AgIQIIACAwAhAAAAAAAAAAAAAABAgAAAAAAQAAIAAAAAAgAAAgAAAAAAGAAABAAAgcEACIKkxAAUJQ4E0gYRQoARBGEABAIAAAAIECAAAAABAgjAIQQRAAQIAACAAAAAAgAgEAAAgACEAAQAFAAAAAAAAAAAAAAAgAAAAAAAEgAAAAAAAAgAAAAAAAAEAAAAGBAEAAAAAIAAAAAAAgAAAAAAEAA.YAAAAAAAAAAA; _au_last_seen_iab_tcf=1687671478889; _cc_id=8d567957bc1ab385b825838d6966030; __qca=P0-670121236-1687671474300; fandom_global_id=d145d0d1-b5ff-4bbd-96c7-1f0868f0588e; _pbjs_userid_consent_data=5543119159564679; _sharedid=734bd090-203b-402f-8da4-5120ef8e931b; _lr_env_src_ats=false; fan_visited_wikis=249580,95269,410,9637; FCNEC=^%^5B^%^5B^%^22AKsRol93cywHwJlqIr0-WivhhEnGYgV9S7ep2IIvdcnnat-h3cL6-RVFbhyTUgTPk3GgSKHZdV3nHyfgmKLmlxTqowskNWRuJHS9d5CNFlT7i4zlGRNV65LuXFhSfLGcTs1ps5Cn71ckEHt8UniZvyMYeCmob7WlkA^%^3D^%^3D^%^22^%^5D^%^2Cnull^%^2C^%^5B^%^5D^%^5D; ss_galactus_enabled=true; AMP_MKTG_264a9266b5=JTdCJTIycmVmZXJyZXIlMjIlM0ElMjJodHRwcyUzQSUyRiUyRnd3dy5nb29nbGUuY29tJTJGJTIyJTJDJTIycmVmZXJyaW5nX2RvbWFpbiUyMiUzQSUyMnd3dy5nb29nbGUuY29tJTIyJTdE; AMP_264a9266b5=JTdCJTIyZGV2aWNlSWQlMjIlM0ElMjJkYjgyYWIyOC1lZDBlLTQzOTYtOWVmYy1hNTQ1MTExYmY2YjglMjIlMkMlMjJzZXNzaW9uSWQlMjIlM0ExNjg5MjA2MDYyNTkwJTJDJTIyb3B0T3V0JTIyJTNBZmFsc2UlMkMlMjJsYXN0RXZlbnRUaW1lJTIyJTNBMTY4OTIwNjA2MjYyMiUyQyUyMmxhc3RFdmVudElkJTIyJTNBNTAlN0Q=; _gid=GA1.2.2130182665.1689885416; Geo=^{^%^22region^%^22:^%^22PE^%^22^%^2C^%^22city^%^22:^%^22recife^%^22^%^2C^%^22country_name^%^22:^%^22brazil^%^22^%^2C^%^22country^%^22:^%^22BR^%^22^%^2C^%^22continent^%^22:^%^22SA^%^22^}; AMP_MKTG_6765a55f49=JTdCJTdE; cebs=1; _ce.s=v~9945ec34f8db6aca334edeb931c94fb9222e514a~lcw~1689885417306~vpv~9~lcw~1689955620505; _ce.clock_event=1; _ce.clock_data=1291^%^2C45.173.101.251^%^2C1^%^2C14d58a1ba286f087d9736249ec785314; _au_last_seen_pixels=eyJhcG4iOjE2ODk5NTU2MjAsInR0ZCI6MTY4OTk1NTYyMCwicHViIjoxNjg5OTU1NjIwLCJhZHgiOjE2ODk5NTU2MjAsInJ1YiI6MTY4OTk1NTYyMCwidGFwYWQiOjE2ODk5NTU2MjAsImdvbyI6MTY4OTk1NTYyMCwibWVkaWFtYXRoIjoxNjg4OTI3NTg4LCJhZG8iOjE2ODk5NTU2NjIsInNvbiI6MTY4OTk1NTY2MiwidW5ydWx5IjoxNjg5OTU1NjIwLCJjb2xvc3N1cyI6MTY4OTk1NTYyMCwib3BlbngiOjE2ODk5NTU2NjIsIl9mYW5kb20tY29tIjoxNjg5OTU1NjYyfQ^%^3D^%^3D; active_cms_notification=258; panoramaId_expiry=1690042083669; panoramaId=5d5207eab4b3b6194ec56640434aa9fb927abd9ff99e6a2a3c01b7f5c06ba047; panoramaIdType=panoDevice; featuredVideoSeenInSession=pjSXZ9moNi; __gads=ID=fe7a58f1a05dfba1:T=1687671479:RT=1689955684:S=ALNI_MZu1FPWN3MLDSzCC4GGnWMge5n6fQ; __gpi=UID=00000c67c24f9185:T=1687671479:RT=1689955684:S=ALNI_MZphaKy7hMWt_rqVy1v8ih02uEY1Q; _lr_sampling_rate=100; playerImpressionsInWiki=2; _ga_LVKNCJXRLW=GS1.1.1689955620.16.1.1689955873.0.0.0; AMP_6765a55f49=JTdCJTIyZGV2aWNlSWQlMjIlM0ElMjIzMjcwN2VkMi02NGZjLTQzMzQtOWJkZC0yZGFlYTlhMmU5OTQlMjIlMkMlMjJzZXNzaW9uSWQlMjIlM0ExNjg5OTU1NjE5NzY3JTJDJTIyb3B0T3V0JTIyJTNBZmFsc2UlMkMlMjJsYXN0RXZlbnRUaW1lJTIyJTNBMTY4OTk1NTg3MzUxOSUyQyUyMmxhc3RFdmVudElkJTIyJTNBMjMlN0Q=; _ga=GA1.2.223041512.1687671474; cebsp_=8; nol_fpid=qtxsxqlypamohpzcjehgkobe5luni1687671475^|1687671475801^|1689955874039^|1689955874176; cto_bundle=jnWtz184Y2I3OXVyUFdDT24lMkZGQmNMcW9JaWs4SWlOeHBTUmglMkJhNzklMkJpdjI3SzZWZ20zRFUzWnpDMExFJTJGVHlwR2Zhazh1TXkxQlpYWiUyQm9YdjFKbXpMZER3Y2hWQzg0eTA3MVMlMkY4UWYySXQlMkYxWU1SMDNEJTJGU3FsMnNUQ2U5NW54VlU5VVRRWHZ5V3JyUmhrajlmV0VHSnFaTnVRJTNEJTNE; cto_bundle=jnWtz184Y2I3OXVyUFdDT24lMkZGQmNMcW9JaWs4SWlOeHBTUmglMkJhNzklMkJpdjI3SzZWZ20zRFUzWnpDMExFJTJGVHlwR2Zhazh1TXkxQlpYWiUyQm9YdjFKbXpMZER3Y2hWQzg0eTA3MVMlMkY4UWYySXQlMkYxWU1SMDNEJTJGU3FsMnNUQ2U5NW54VlU5VVRRWHZ5V3JyUmhrajlmV0VHSnFaTnVRJTNEJTNE; cto_bundle=AhqdTl84Y2I3OXVyUFdDT24lMkZGQmNMcW9JaWlEUjhiRU9OZiUyRjd6UmtkTlFHaGlFSkZ5WXFHSndDSVJNWFp5NWxnNGJyR2J2SFl2TVhtZkFTSGc2Q0I2VjdrNVJGYTJnOEQlMkJTVG1tNVd6d1E5ZDZmVE83cmh6V1ZnTHBoMXhGamlpNjI3WjVsJTJGN0pLNzQ1NHN5eCUyQm1HSkMwaThnJTNEJTNE; cto_bidid=eHuqXl9sYnR5dUNOd1hOajV2N2N6bnlGZk55MzRFdDR0Z0RFbSUyRktTeXFaeGVaZjRpd0VHTnhyMSUyRlVJJTJGejlzMTUlMkI0cGZNYlNudU4xMk9wN0N4OVJwcjNLbDFRaEdSUVFCSXdqSlZGa01oVTJVQnJvJTNE; _ga_QJ8ZYXGZTQ=GS1.1.1689955619.11.1.1689955875.0.0.0; _ga_H6LPQNDD8W=GS1.1.1689955620.11.1.1689955875.7.0.0" },
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


                var languages = (Languages[])Enum.GetValues(typeof(Languages));

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
                                            Console.WriteLine(dt);
                                            if (dt != DateTime.MinValue)
                                            {
                                                cardSets.Add(new CardData.CardSet(spans[0].InnerText, spans[1].InnerText, spans[2].InnerText, spans[3].InnerText));
                                            }
                                        }
                                    }
                                }

                                if (!string.IsNullOrEmpty(language))
                                {
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
