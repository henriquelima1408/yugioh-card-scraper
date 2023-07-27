using Newtonsoft.Json;
using yugioh_card_scraper.Model;
using yugioh_card_scraper.Utils;


namespace yugioh_card_scraper.Scraper
{
    internal class CardImageScraper : CardScraper
    {
        public class YGOPROResponse
        {
            readonly CardInfo[] data;

            public YGOPROResponse(CardInfo[] data)
            {
                this.data = data;
            }

            internal CardInfo[] Data => data;

            public class CardInfo
            {
                readonly string id;
                readonly CardSet[] card_sets;
                readonly CardImages[] card_images;

                public CardInfo(string id, CardSet[] card_sets, CardImages[] card_images)
                {
                    this.id = id;
                    this.card_sets = card_sets;
                    this.card_images = card_images;
                }

                public string Id => id;

                public CardSet[] Card_sets => card_sets;

                public CardImages[] Card_images => card_images;
            }
            public class CardSet
            {
                readonly string set_name;
                readonly string set_code;

                public CardSet(string set_name, string set_code)
                {
                    this.set_name = set_name;
                    this.set_code = set_code;
                }

                public string Set_name => set_name;

                public string Set_code => set_code;
            }
            public class CardImages
            {
                readonly string id;
                readonly string image_url;

                public CardImages(string id, string image_url)
                {
                    this.id = id;
                    this.image_url = image_url;
                }

                public string Id => id;

                public string Image_url => image_url;
            }
        }

        readonly Dictionary<string, IEnumerable<YGOPROResponse.CardImages>> cardImageDict = new Dictionary<string, IEnumerable<YGOPROResponse.CardImages>>();
        readonly IEnumerable<CardData> cardDatas;

        public CardImageScraper(string uriFormat, int[] delayRange, DirectoryInfo cacheDirectory, IEnumerable<CardData> cardDatas) : base(uriFormat, delayRange, cacheDirectory)
        {
            this.cardDatas = cardDatas;
        }

        protected override HttpRequestMessage CreateNewRequest<T>(T uri)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri as string),
            };

            return request;
        }

        protected override async Task<IEnumerable<T>> Scrap<T>(string data)
        {
            IEnumerable<byte> result = null;
            using (var response = await client.SendAsync(CreateNewRequest(data)))
            {
                response.EnsureSuccessStatusCode();
                var byteArray = await response.Content.ReadAsByteArrayAsync();
                result = byteArray;
            }

            return (IEnumerable<T>)result;
        }

        internal override async Task ScrapAll(string savePath)
        {
            var min = delayRange[0];
            var max = delayRange[1];
            var delta = max - min;

            var ygoProResponse = await LinearBackoff.DoRequest(async () =>
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(uriFormat),
                };

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<YGOPROResponse>(body);
                }
            }, 5000);

            foreach (var data in ygoProResponse.Data)
            {
                var sets = data.Card_sets;
                var images = data.Card_images;

                if (sets == null)
                    continue;

                foreach (var set in sets)
                {
                    if (!cardImageDict.ContainsKey(set.Set_code))
                        cardImageDict.Add(set.Set_code, images);
                }
            }

            var cacheImages = LoadLocal<HashSet<string>>();

            foreach (var cardData in cardDatas)
            {
                var sets = cardData.CardSets;
                foreach (var set in sets.Values.First())
                {
                    if (cardImageDict.ContainsKey(set.CardNumber))
                    {
                        //Do request to download images
                        foreach (var cardImage in cardImageDict[set.CardNumber])
                        {
                            if (cacheImages.Contains(cardImage.Id))
                                continue;

                            var imageBytes = await LinearBackoff.DoRequest(async () =>
                            {
                                var imageBytes = await Scrap<byte>(cardImage.Image_url);
                                return imageBytes;
                            }, 5000);

                            var fileName = string.Format(savePath, cardImage.Id);
                            var folderPath = Path.Combine(cacheDirectory.FullName, cardData.CardID.ToString());
                            File.WriteAllBytes(Path.Combine(folderPath, fileName), imageBytes.ToArray());
                        }

                        var r = new Random().NextDouble();
                        var v = delta * r + min;

                        await Task.Delay((int)v);
                        break;
                    }
                }
            }
        }

        public override T LoadLocal<T>()
        {
            var cardImageIDs = new HashSet<string>();

            foreach (var cardData in cardDatas)
            {
                var files = Directory.GetFiles(Path.Combine(cacheDirectory.FullName, cardData.CardID.ToString()));
                foreach (var file in files)
                {
                    if (Path.GetExtension(file) == ".jpg")
                    {
                        cardImageIDs.Add(Path.GetFileNameWithoutExtension(file));
                    }                
                }
            }

            return (T)(object)cardImageIDs;
        }
    }
}
