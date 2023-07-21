using Newtonsoft.Json;
using yugioh_card_scraper.Model;
using yugioh_card_scraper.Utils;

namespace yugioh_card_scraper.Scraper
{
    internal class CardMetadaScraper : CardScraper
    {
        int pagesCount = 1;
        const int elementsPerPage = 100;

        internal CardMetadaScraper(string uriFormat, int[] delayRange, DirectoryInfo cacheDirectory) : base(uriFormat, delayRange, cacheDirectory) { }

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
                        { "cookie", "Analytics=2; CountryCd=--; Edgescape=1; JSESSIONID=4E9152D23BC21988D74308B913F4FF51; visid_incap_1363207=vSuCi8qYQMKErCH5xusggQGjmGQAAAAAQUIPAAAAAADIsgEfl126BB8wVE8sB7Wp; AG=2; OptanonAlertBoxClosed=2023-06-28T04:34:35.245Z; AO=2; _ga_DSVT4C66K4=deleted; _ga_BKZXYG3T1Z=GS1.2.1688412799.8.1.1688412870.50.0.0; nlbi_1363207=3WUtL00hSm8nh5lXxbLhpQAAAADRpy+lEU9KoJqQ68nVtsJf; _gid=GA1.2.49161900.1689882647; incap_ses_1527_1363207=+Ihqa3zqwwL1No/22v4wFe+auWQAAAAA5a/c6HKvmavw21DvQbnUNg==; incap_ses_1474_1363207=CABmWq/kmEaB/wGAtbN0FJT2uWQAAAAAbJf7cc7P9WbGexlfko9u+A==; _gat_UA-97638476-6=1; _ga_DSVT4C66K4=GS1.1.1689908889.23.1.1689909130.0.0.0; _ga=GA1.1.1737813437.1687926875; OptanonConsent=isGpcEnabled=0&datestamp=Fri+Jul+21+2023+00^%^3A12^%^3A10+GMT-0300+(Hora+padr^%^C3^%^A3o+de+Bras^%^C3^%^ADlia)&version=202211.2.0&isIABGlobal=false&hosts=&consentId=ebc3d197-6c76-450e-a5a4-723ecb6d37d4&interactionCount=2&landingPath=NotLandingPage&groups=C0001^%^3A1^%^2CC0003^%^3A1^%^2CC0002^%^3A1^%^2CC0004^%^3A1&AwaitingReconsent=false&geolocation=BR^%^3BPE; _ga_QJ8ZYXGZTQ=GS1.1.1689908891.13.1.1689909130.0.0.0; _ga_H6LPQNDD8W=GS1.1.1689908891.13.1.1689909130.59.0.0; AWSALB=t0UxXIGp3qaRccyEpiQ+VMsrtp24LgEq4/icIdnRcIpZf5n2JImg2XadUkYgXeWAT6NnKA+4tIpONCG9VLKArnV//JC97rDSH60uxseMNEBcxugTO1+CwYnP7Yzr; AWSALBCORS=t0UxXIGp3qaRccyEpiQ+VMsrtp24LgEq4/icIdnRcIpZf5n2JImg2XadUkYgXeWAT6NnKA+4tIpONCG9VLKArnV//JC97rDSH60uxseMNEBcxugTO1+CwYnP7Yzr" },
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
