namespace yugioh_card_scraper.Model
{


    [Serializable]
    internal class CardData
    {
        public static string[] Languages
        {
            get
            {
                return new string[] {
                "English",
                "English—Worldwide",
                "English—North America",
                "French",
                "German",
                "Italian",
                "Portuguese",
                "Spanish",
                "Japanese",
                "Asian-English",
                "Korean",
                };
            }

        }

        [Serializable]
        internal class CardSet
        {
            readonly string releaseData;
            readonly string cardNumber;
            readonly string packName;
            readonly string rarity;

            public CardSet(string releaseData, string cardNumber, string packName, string rarity)
            {
                this.releaseData = releaseData;
                this.cardNumber = cardNumber;
                this.packName = packName;
                this.rarity = string.IsNullOrEmpty(rarity) ? "Normal" : rarity;
            }

            public string ReleaseData => releaseData;

            public string CardNumber => cardNumber;

            public string PackName => packName;

            public string Rarity => rarity;
        }


        readonly string cardID;
        readonly Dictionary<string, string> cardNames;
        readonly string cardType;
        readonly string attribute;
        readonly Dictionary<string, string> cardDescriptions;
        readonly string cardPendulumScale;
        readonly IEnumerable<string> species;
        readonly string cardLevel;
        readonly string cardRank;
        readonly string cardAtk;
        readonly string cardDefense;
        readonly string cardLinkType;
        readonly IEnumerable<string> cardLinkArrows;
        readonly Dictionary<string, IEnumerable<CardSet>> cardSets;
        readonly string statuses;

        public CardData(string cardID, Dictionary<string, string> cardNames, string cardType, Dictionary<string, string> cardDescriptions, Dictionary<string, IEnumerable<CardSet>> cardSets, string statuses, string attribute = "",
            IEnumerable<string> species = null, string cardLevel = "", string cardAtk = "", string cardDefense = "", string cardRank = "",
             string cardPendulumScale = "", string cardLinkType = "", IEnumerable<string> cardLinkArrows = null)
        {
            this.cardID = cardID;
            this.cardNames = cardNames;
            this.cardType = cardType;
            this.attribute = attribute;
            this.cardDescriptions = cardDescriptions;
            this.cardPendulumScale = cardPendulumScale;
            this.species = species;
            this.cardLevel = cardLevel;
            this.cardRank = cardRank;
            this.cardAtk = cardAtk;
            this.cardDefense = cardDefense;
            this.cardLinkType = cardLinkType;
            this.cardSets = cardSets;
            this.statuses = statuses;
            this.cardLinkArrows = cardLinkArrows;
        }

        public override string ToString()
        {
            return $"cardID: {cardID} cardName: {cardNames} cardType: {cardType} attribute: {attribute} cardLevel {cardLevel} cardRank: {cardRank} cardPendulumScale: {cardPendulumScale} cardAtk {cardAtk} cardDefense {cardDefense}";
        }

        public string CardID => cardID;
        public Dictionary<string, string> CardNames => cardNames;
        public Dictionary<string, string> CardDescriptions => cardDescriptions;
        public string CardType => cardType;
        public IEnumerable<string> Species => species;
        public string Attribute => attribute;
        public string CardLevel => cardLevel;
        public string CardRank => cardRank;
        public string CardAtk => cardAtk;
        public string CardDefense => cardDefense;
        public string CardPendulumScale => cardPendulumScale;
        public string CardLinkType => cardLinkType;
        public IEnumerable<string> CardLinkArrows => cardLinkArrows;
        public Dictionary<string, IEnumerable<CardSet>> CardSets => cardSets;
        public string Statuses => statuses;
    }
}
