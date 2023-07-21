namespace yugioh_card_scraper.Model
{
    internal class CardMetadata
    {
        readonly string cardName;
        readonly string cardID;

        public CardMetadata(string cardName, string cardID)
        {
            this.cardName = cardName;
            this.cardID = cardID;
        }

        public string CardName => cardName;

        public string CardID => cardID;
    }
}
