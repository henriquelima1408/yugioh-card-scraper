using yugioh_card_scraper.Model;
using yugioh_card_scraper.Scraper;


const string yuGiOhDataRootUri = "https://yugioh.fandom.com";
const string yuGiOhMetadataUriFormat = "https://www.db.yugioh-card.com/yugiohdb/card_search.action?ope=1&sess=3&page={0}&stype=1&link_m=2&othercon=2&sort=1&rp={1}";
const string yiGiOhDataUriFormat = "https://yugioh.fandom.com/wiki/Special:SearchByProperty?property=Database%20ID&value={0}";

var directoryPath = Directory.GetCurrentDirectory();
var yugiohDataDirectory = Directory.GetParent(directoryPath)?.Parent?.Parent?.GetDirectories().Where(d => d.Name.Equals("App")).First().GetDirectories().Where(d => d.Name.Equals("Data")).First();
var yugiohMetadataDirectory = yugiohDataDirectory.GetDirectories().Where(d => d.Name.Equals("CardMetada")).First();
var yugiohCardDataDirectory = yugiohDataDirectory.GetDirectories().Where(d => d.Name.Equals("CardData")).First();

int[]? metadataDelay = null;

try
{
    var metadaDelayArgs = args.Where(a => a.Contains("-metadataRequestDelay")).First();

    if (metadaDelayArgs != null)
    {
        var argIndex = args.ToList().IndexOf(metadaDelayArgs);
        var min = int.Parse(args[argIndex + 1]);
        var max = int.Parse(args[argIndex + 2]);
        metadataDelay = new int[] { min, max };
        Console.WriteLine($"Using metadataRequestDelay with {min} - {max}");
    }
}
catch (Exception _)
{
    Console.WriteLine("Could not convert args. Using default metadataDelay");
}
finally
{
    if (metadataDelay == null)
    {
        metadataDelay = new int[] { 100, 200 };
    }
}


var cardMetadataScraper = new CardMetadaScraper(yuGiOhMetadataUriFormat, new int[] { 100, 200 }, yugiohMetadataDirectory);
await cardMetadataScraper.ScrapAll("cardsMetadata-page-{0}.json");

var cardData = new CardDataScraper(yuGiOhDataRootUri, yiGiOhDataUriFormat, new int[] { 100, 200 }, yugiohCardDataDirectory, cardMetadataScraper.LoadLocal<IEnumerable<CardMetadata>>().Select(c => c.CardID).ToHashSet());
await cardData.ScrapAll("{0}.json");

Console.WriteLine("Finished!");
