using Newtonsoft.Json;
using yugioh_card_scraper.Model;
using yugioh_card_scraper.Utils;

namespace yugioh_card_scraper.Scraper
{
    internal class CardMetadaScraper : CardScraper
    {
        int pagesCount = 1;
        const int elementsPerPage = 100;
        readonly string cookie = "";

        internal CardMetadaScraper(string uriFormat, int[] delayRange, DirectoryInfo cacheDirectory, string cookie) : base(uriFormat, delayRange, cacheDirectory)
        {
            this.cookie = cookie;
        }

        protected override HttpRequestMessage CreateNewRequest<T>(T data)
        {
            var page = 0;

            if (data is string s)
                page = int.Parse(s);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(string.Format(uriFormat, page.ToString(), elementsPerPage)),
                Headers =
                    {
                        { "cookie", cookie },
                        { "authority", "www.db.yugioh-card.com" },
                        { "accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7" },
                        { "accept-language", "en-US,en;q=0.9,pt-BR;q=0.8,pt;q=0.7" },
                        { "cache-control", "max-age=0" },
                        { "referer", string.Format("https://www.db.yugioh-card.com/yugiohdb/card_search.action?ope=1&sess=1&rp={0}&mode=&sort=1&keyword=&stype=1&ctype=&othercon=2&starfr=&starto=&pscalefr=&pscaleto=&linkmarkerfr=&linkmarkerto=&link_m=2&atkfr=&atkto=&deffr=&defto=", elementsPerPage)  },
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

        protected override async Task<IEnumerable<T>> Scrap<T>(string data)
        {
            var cardIDs = new List<CardMetadata>();

            using (var response = await client.SendAsync(CreateNewRequest(data)))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var htmDocument = ToHtmlDocument(body);

                if (pagesCount == 1)
                {
                    var sortSetNode = htmDocument.DocumentNode.FindNode("class", "sort_set").First();
                    var searchResultNode = sortSetNode.FindChildrenByClass("text");

                    var searchResultValueCollection = searchResultNode.InnerText.TrimStart().TrimEnd().RemoveSpecialCharacters().FindAllNumbers().ToList();
                    searchResultValueCollection.Sort((i1, i2) => i1.CompareTo(i2));
                    pagesCount = (int)Math.Round((double)searchResultValueCollection.Last() / elementsPerPage);
                }

                var cardList = htmDocument.DocumentNode.FindNode("id", "card_list").First();
                var cardRows = cardList.FindChildrensByClass("t_row");


                foreach (var cardRow in cardRows)
                {
                    var flexNode = cardRow.FindChildrenByClass("flex_1");
                    var boxCardName = flexNode.FindChildrenByClass("box_card_name");
                    var cardName = boxCardName.FindChildrenByClass("card_name").InnerText;

                    var removeBtn = flexNode.FindChildrenByClass("remove_btn");
                    var btn = removeBtn.FindChildrenByClass("btn");
                    var cid = btn.FindChildrenByClass("cid").GetAttributeValue("value", -1).ToString();

                    cardIDs.Add(new CardMetadata(cardName, cid));
                }


                return (IEnumerable<T>)cardIDs;
            }
        }

        internal override async Task ScrapAll(string savePathFormat)
        {
            var min = delayRange[0];
            var max = delayRange[1];
            var delta = max - min;

            var cacheCount = LoadLocal<IEnumerable<CardMetadata>>().Count();
            if (cacheCount > 0)
            {
                cacheCount = cacheCount / 100;
            }

            // First request to store page count
            await LinearBackoff.DoRequest(async () =>
            {
                var cardMetadata = await Scrap<CardMetadata>(1.ToString());
                return cardMetadata;

            }, 5000);


            for (int i = cacheCount + 1; i < pagesCount + 1; i++)
            {
                var metadaCollection = await LinearBackoff.DoRequest(async () =>
                {
                    var cardMetadata = await Scrap<CardMetadata>(i.ToString());
                    return cardMetadata;

                }, 5000);

                if (metadaCollection != null)
                    WriteInfoAsJson(cacheDirectory.FullName, string.Format(savePathFormat, i), metadaCollection);

                var r = new Random().NextDouble();
                var v = delta * r + min;

                await Task.Delay((int)v);
            }
        }

        public override T LoadLocal<T>()
        {
            var cardMetadatas = new List<CardMetadata>();

            var files = cacheDirectory.GetFiles();

            if (files != null)
            {
                foreach (var file in files)
                {
                    using (var streamReader = new StreamReader(file.FullName))
                    {
                        var content = streamReader.ReadToEnd();
                        cardMetadatas.AddRange(JsonConvert.DeserializeObject<CardMetadata[]>(content));
                    }
                }
            }
            return (T)(object)cardMetadatas;
        }
    }
}
