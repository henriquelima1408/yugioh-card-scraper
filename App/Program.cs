using System.Collections.Generic;
using yugioh_card_scraper.Model;
using yugioh_card_scraper.Scraper;
using yugioh_card_scraper.Utils;

const string yuGiOhMetadataUriFormat = "https://www.db.yugioh-card.com/yugiohdb/card_search.action?ope=1&sess=3&page={0}&stype=1&link_m=2&othercon=2&sort=1&rp={1}";

var directoryPath = Directory.GetCurrentDirectory();
var yugiohDataDirectory = Directory.GetParent(directoryPath)?.Parent?.Parent?.GetDirectories().Where(d => d.Name.Equals("App")).First().GetDirectories().Where(d => d.Name.Equals("Data")).First();
var yugiohMetadataDirectory = yugiohDataDirectory.GetDirectories().Where(d => d.Name.Equals("Metada")).First();

int[]? metadataDelay = null;

try
{
    var metadaDelayArgs = args.Where(a => a.Contains("-metadataDelay")).First();
    if (metadaDelayArgs != null && metadaDelayArgs.Count() > 0)
    {
        var delay = metadaDelayArgs.FindAllNumbers().ToArray();
        metadataDelay = new int[] { delay[0], delay[1] };
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
Console.WriteLine("Finished!");
