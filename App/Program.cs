using Newtonsoft.Json;
using yugioh_card_scraper.Model;
using yugioh_card_scraper.Scraper;

internal class Program
{
    class ArgInfo
    {
        readonly int[] requestDelay;
        readonly string metadataCookie;
        readonly string dataCookies;

        public ArgInfo(int[] requestDelay, string metadataCookie, string dataCookies)
        {
            this.requestDelay = requestDelay;
            this.metadataCookie = metadataCookie;
            this.dataCookies = dataCookies;
        }

        public int[] RequestDelay => requestDelay;

        public string MetadataCookie => metadataCookie;

        public string DataCookies => dataCookies;
    }

    const string yuGiOhMetadataUriFormat = "https://www.db.yugioh-card.com/yugiohdb/card_search.action?ope=1&sess=3&page={0}&stype=1&link_m=2&othercon=2&sort=1&rp={1}";
    const string yiGiOhDataUriFormat = "https://www.db.yugioh-card.com/yugiohdb/card_search.action?ope=2&cid={0}&request_locale={1}";
    const string yuGiOhImageUriFormat = "https://db.ygoprodeck.com/api/v7/cardinfo.php";

    private static async Task Main(string[] args)
    {
        var directoryPath = Directory.GetCurrentDirectory();
        var argJson = Directory.GetParent(directoryPath)?.Parent?.Parent?.GetDirectories().Where(d => d.Name.Equals("App")).First().GetFiles().Where(f => f.Name == "Args.json").First();
        var yugiohDataDirectory = Directory.GetParent(directoryPath)?.Parent?.Parent?.GetDirectories().Where(d => d.Name.Equals("App")).First().GetDirectories().Where(d => d.Name.Equals("Data")).First();
        var yugiohMetadataDirectory = yugiohDataDirectory.GetDirectories().Where(d => d.Name.Equals("CardMetada")).First();
        var yugiohCardDataDirectory = yugiohDataDirectory.GetDirectories().Where(d => d.Name.Equals("CardData")).First();

        ArgInfo argsInfo;
        using (var streamReade = new StreamReader(argJson.FullName))
        {
            argsInfo = JsonConvert.DeserializeObject<ArgInfo>(streamReade.ReadToEnd());
        }

        var cardMetadataScraper = new CardMetadaScraper(yuGiOhMetadataUriFormat, argsInfo.RequestDelay, yugiohMetadataDirectory, argsInfo.MetadataCookie);
        await cardMetadataScraper.ScrapAll("cardsMetadata-page-{0}.json");

        var cardData = new CardDataScraper(yiGiOhDataUriFormat, argsInfo.RequestDelay, yugiohCardDataDirectory, cardMetadataScraper.LoadLocal<IEnumerable<CardMetadata>>().Select(c => c.CardID).ToHashSet(), argsInfo.DataCookies);
        await cardData.ScrapAll("{0}.json");

        var cardImageScraper = new CardImageScraper(yuGiOhImageUriFormat, argsInfo.RequestDelay, yugiohCardDataDirectory, cardData.LoadLocal<IEnumerable<CardData>>());
        await cardImageScraper.ScrapAll("{0}.jpg");
        Console.WriteLine("Finished!");
    }
}