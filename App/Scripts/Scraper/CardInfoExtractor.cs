using HtmlAgilityPack;
using yugioh_card_scraper.Utils;
using static yugioh_card_scraper.Model.CardData;

namespace yugioh_card_scraper.Scripts.Scraper
{
    internal class CardInfo
    {
        readonly string cardName;
        readonly string cardType;
        readonly string cardDescription;
        readonly string attribute;
        readonly string level;
        readonly string attack;
        readonly string defense;
        readonly List<string> species;
        readonly string rank;
        readonly string pendulumScale;
        readonly string pendulumDescription;
        readonly string linkType;
        readonly IEnumerable<string> cardLinkArrows;

        public CardInfo(string cardName, string cardType, string cardDescription, string attribute, string level, string attack, string defense, List<string> species, string rank, string pendulumScale, string pendulumDescription, string linkType, byte[] imageBytes, IEnumerable<string> cardLinkArrows)
        {
            this.cardName = cardName;
            this.cardType = cardType;
            this.cardDescription = cardDescription;
            this.attribute = attribute;
            this.level = level;
            this.attack = attack;
            this.defense = defense;
            this.species = species;
            this.rank = rank;
            this.pendulumScale = pendulumScale;
            this.pendulumDescription = pendulumDescription;
            this.linkType = linkType;
            this.cardLinkArrows = cardLinkArrows;
        }

        public string CardName => cardName;

        public string CardType => cardType;

        public string CardDescription => cardDescription;

        public string Attribute => attribute;

        public string Level => level;

        public string Attack => attack;

        public string Defense => defense;

        public List<string> Species => species;

        public string Rank => rank;

        public string PendulumScale => pendulumScale;

        public string PendulumDescription => pendulumDescription;

        public string LinkType => linkType;

        public IEnumerable<string> CardLinkArrows => cardLinkArrows;
    }

    static class CardInfoExtractor
    {
        const string levelIcon = "external/image/parts/icon_level.png";
        const string rankIcon = "external/image/parts/icon_rank.png";
        const string pendulumIcon = "external/image/parts/icon_pendulum.png";

        public static Tuple<CardInfo, IEnumerable<CardSet>> ExtractInfo(HtmlDocument htmDocument)
        {
            var cardDataNode = htmDocument.DocumentNode.FindNode("id", "CardSet").First();

            var cardSetInformation = ExtractCardSetInformation(htmDocument);
            var cardInfo = ExtractCardInformation(cardDataNode);

            return new Tuple<CardInfo, IEnumerable<CardSet>>(cardInfo, cardSetInformation);
        }

        static CardInfo ExtractCardInformation(HtmlNode cardDataNode)
        {
            var cardTextNode = cardDataNode.FindChildrenByClass("top").FindChildrensByAttribute("id").Where(n => n.FindChildrenByClass("CardText") != null).First();

            var cardName = cardDataNode.FindChildrenByAttribute("id").InnerText.TrimStart().TrimEnd();
            var cardTexts = cardTextNode.FindChildrensByClass("CardText").ToArray();

            var frames = cardTexts[0].FindChildrensByClass("frame").ToArray();
            var attackDefenseNode = GetAttackAndDefenseNode(frames);
            var isMonster = attackDefenseNode != null;

            var cardType = "";
            var attribute = "";
            var level = "";
            var pendulumScale = "";
            var rank = "";
            var attack = "";
            var defense = "";
            var species = new List<string>();
            var linkType = "";
            var pendulumDescription = "";

            if (isMonster && attackDefenseNode != null)
            {
                var levelNode = GetLevelNode(frames);
                var rankNode = GetRankNode(frames);
                var linkNode = GetLinkNode(frames);
                var pendulumNode = GetPendulumNode(cardTexts);
                var attackDefense = attackDefenseNode.FindChildrensByClass("item_box").ToArray();

                if (levelNode != null)
                {
                    level = levelNode.InnerText.TrimStart().TrimEnd();
                }

                if (rankNode != null)
                {
                    rank = rankNode.InnerText.TrimStart().TrimEnd();
                }

                if (linkNode != null)
                {
                    linkType = linkNode.GetAttributeValue("class", "").TrimStart().TrimEnd().Replace("icon_img_set", "");
                }

                if (pendulumNode != null)
                {
                    pendulumDescription = pendulumNode.FindChildrenByClass("pen_effect").FindChildrenByClass("item_box_text").InnerText.TrimStart().TrimEnd();
                    pendulumScale = pendulumNode.FindChildrenByClass("frame").FindChildrenByClass("t_center").FindChildrenByClass("item_box_value").FindChildrenByClass("icon_img").NextSibling.InnerText.TrimStart().TrimEnd();
                }

                cardType = "Monster";
                attribute = GetAttributeNode(frames).GetAttributeValue("title", "").TrimStart().TrimEnd();
                attack = attackDefense[0].FindChildrenByClass("item_box_value").InnerText.TrimStart().TrimEnd();
                defense = attackDefense[1].FindChildrenByClass("item_box_value").InnerText.TrimStart().TrimEnd();

                var split = frames[2].InnerText.Split('/');
                foreach (var s in split)
                {
                    species.Add(s.TrimStart().TrimEnd());
                }
            }
            else
            {
                cardType = cardTexts[0].FindChildrenByClass("frame").FindChildrenByClass("item_box").FindChildrenByClass("item_box_value").InnerText.TrimStart().TrimEnd();
            }

            var cardDescription = cardTexts.Last().FindChildrenByClass("item_box_text").GetDirectInnerText().TrimStart().TrimEnd();

            return new CardInfo(cardName, cardType, cardDescription, attribute, level, attack, defense, species, rank, pendulumScale, pendulumDescription, linkType, null, ConvertLinkArrows(linkType));
        }

        static IEnumerable<string> ConvertLinkArrows(string linkType) { 
            var result = new List<string>();

            foreach (var c in linkType.ToCharArray())
            {
                if (c == '1')
                {
                    result.Add("Down-Left");
                }
                else if (c == '2')
                {
                    result.Add("Down");
                }
                else if (c == '3')
                {
                    result.Add("Down-Right");
                }
                else if (c == '4')
                {
                    result.Add("Left");
                }
                else if (c == '6')
                {
                    result.Add("Right");
                }
                else if (c == '7')
                {
                    result.Add("Top-Left");
                }
                else if (c == '8')
                {
                    result.Add("Top");
                }
                else if (c == '9')
                {
                    result.Add("Top");
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            return result;
        }

        static IEnumerable<Model.CardData.CardSet> ExtractCardSetInformation(HtmlDocument htmDocument)
        {
            var cardSetRowNodes = htmDocument.DocumentNode.FindNode("id", "update_list").First().FindChildrenByClass("t_body").FindChildrensByClass("t_row");
            var cardSets = new List<Model.CardData.CardSet>();

            if (cardSetRowNodes != null)
            {

                foreach (var rowNode in cardSetRowNodes)
                {
                    var insideNode = rowNode.FindChildrenByClass("inside");
                    var flexNode = insideNode.FindChildrenByClass("flex_1");

                    var releaseDate = insideNode.FindChildrenByClass("time").InnerText.TrimStart().TrimEnd();
                    var cardNumber = flexNode.FindChildrenByClass("card_number").InnerText.TrimStart().TrimEnd();
                    var packName = flexNode.FindChildrenByClass("pack_name").InnerText.TrimStart().TrimEnd();
                    var rarity = insideNode.FindChildrenByClass("icon").FindChildrenByClass("lr_icon").FindChildrenNodesByName("span").First().InnerText.TrimStart().TrimEnd();

                    cardSets.Add(new CardSet(releaseDate, cardNumber, packName, rarity));

                }
            }
            return cardSets;
        }
        static HtmlNode GetAttributeNode(IEnumerable<HtmlNode> frames)
        {
            foreach (var frame in frames)
            {
                var itemBoxes = frame.FindChildrensByClass("item_box");

                foreach (var itemBox in itemBoxes)
                {
                    var itemBoxValue = itemBox.FindChildrenByClass("item_box_value");

                    if (itemBoxValue == null)
                        continue;


                    var iconImgNode = itemBoxValue.FindChildrenByClass("icon_img");
                    if (iconImgNode != null && iconImgNode.GetAttributeValue("src", "") != levelIcon)
                    {
                        return iconImgNode;
                    }
                }
            }
            return null;
        }
        static HtmlNode GetLevelNode(IEnumerable<HtmlNode> frames)
        {
            foreach (var frame in frames)
            {
                var itemBoxes = frame.FindChildrensByClass("item_box");

                foreach (var itemBox in itemBoxes)
                {
                    var itemBoxValue = itemBox.FindChildrenByClass("item_box_value");

                    if (itemBoxValue == null)
                        continue;


                    var iconImgNode = itemBoxValue.FindChildrenByClass("icon_img");
                    if (iconImgNode != null && iconImgNode.GetAttributeValue("src", "") == levelIcon)
                    {
                        return iconImgNode.NextSibling;
                    }
                }
            }
            return null;
        }
        static HtmlNode GetRankNode(IEnumerable<HtmlNode> frames)
        {
            foreach (var frame in frames)
            {
                var itemBoxes = frame.FindChildrensByClass("item_box");

                foreach (var itemBox in itemBoxes)
                {
                    var itemBoxValue = itemBox.FindChildrenByClass("item_box_value");

                    if (itemBoxValue == null)
                        continue;


                    var iconImgNode = itemBoxValue.FindChildrenByClass("icon_img");
                    if (iconImgNode != null && iconImgNode.GetAttributeValue("src", "") == rankIcon)
                    {
                        return iconImgNode.NextSibling;
                    }
                }
            }
            return null;
        }
        static HtmlNode GetLinkNode(IEnumerable<HtmlNode> frames)
        {
            foreach (var frame in frames)
            {
                var itemBoxes = frame.FindChildrensByClass("item_box");

                foreach (var itemBox in itemBoxes)
                {
                    var itemBoxValue = itemBox.FindChildrenByClass("item_box_value");

                    if (itemBoxValue == null)
                        continue;

                    var iconImgNode = itemBoxValue.FindChildrenByClass("icon_img_set");
                    if (iconImgNode != null && iconImgNode.GetAttributeValue("alt", "") == "Link")
                    {
                        return iconImgNode;
                    }
                }
            }
            return null;
        }
        static HtmlNode GetPendulumNode(IEnumerable<HtmlNode> cardtexts)
        {
            foreach (var cardText in cardtexts)
            {
                var frame = cardText.FindChildrensByClass("frame");

                if (frame == null)
                    continue;

                foreach (var f in frame)
                {
                    var tCenter = f.FindChildrenByClass("t_center");

                    if (tCenter == null)
                        continue;

                    var itemBoxValue = tCenter.FindChildrenByClass("item_box_value");
                    if (itemBoxValue == null)
                        continue;

                    var iconImg = itemBoxValue.FindChildrenByClass("icon_img");
                    if (iconImg == null)
                        continue;

                    if (iconImg.GetAttributeValue("src", "") == pendulumIcon)
                    {
                        return cardText;
                    }

                }
            }
            return null;
        }
        static HtmlNode GetAttackAndDefenseNode(IEnumerable<HtmlNode> frames)
        {
            return frames.Count() > 1 ? frames.ToArray()[1] : null;
        }
    }
}
