using HtmlAgilityPack;
using Newtonsoft.Json;

namespace yugioh_card_scraper.Scraper
{
    internal abstract class CardScraper
    {
        protected readonly HttpClientHandler clientHandler;
        protected readonly HttpClient client;
        protected readonly int[] delayRange;
        protected readonly DirectoryInfo cacheDirectory;

        protected string uriFormat;

        public CardScraper(string uriFormat, int[] delayRange, DirectoryInfo cacheDirectory)
        {
            clientHandler = new HttpClientHandler
            {
                UseCookies = false,
            };

            client = new HttpClient(clientHandler);
            this.uriFormat = uriFormat;
            this.delayRange = delayRange;
            this.cacheDirectory = cacheDirectory;
        }

        protected abstract HttpRequestMessage CreateNewRequest<T>(T data);
        protected abstract Task<IEnumerable<T>> Scrap<T>(string data);
        internal abstract Task ScrapAll(string savePath);
        public abstract T LoadLocal<T>();


        protected virtual string getBetween(string strSource, string strStart, string strEnd)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                int Start, End;
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }

            return "";
        }

        protected virtual HtmlDocument ToHtmlDocument(string htmlBody)
        {
            HtmlDocument htlmDocument = new HtmlDocument();
            htlmDocument.LoadHtml(htmlBody);
            return htlmDocument;
        }

        protected virtual void WriteInfoAsJson<T>(string directoryPath, string savePath, IEnumerable<T> cardInfoElements)
        {
            var json = JsonConvert.SerializeObject(cardInfoElements, Formatting.Indented);

            using (var streamWrite = new StreamWriter(Path.Combine(directoryPath, savePath)))
            {
                Console.WriteLine($"Saving file {savePath} in {directoryPath}");
                streamWrite.Write(json);
            }
        }

    }
}
