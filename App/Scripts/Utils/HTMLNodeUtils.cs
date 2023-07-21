using HtmlAgilityPack;

namespace yugioh_card_scraper.Utils
{
    internal static class HTMLNodeUtils
    {
        public static HtmlNodeCollection FindNode(this HtmlNode htmlNodeCollection, string nodeType, string nodeName, string root = "div")
        {
            var q = string.Format("//{2}[@{0}='{1}']", nodeType, nodeName, root);

            return htmlNodeCollection.SelectNodes(q);
        }

        public static HtmlNode FindChildrenByClass(this HtmlNode htmlNode, string nodeClassType)
        {
            foreach (var node in htmlNode.ChildNodes)
            {
                if (node != null)
                {
                    var nodeClasses = node.GetClasses();

                    foreach (var nodeClass in nodeClasses)
                    {

                        if (nodeClass == nodeClassType)
                        {
                            return node;
                        }
                    }
                }
            }
            return null;
        }

        public static IEnumerable<HtmlNode> FindChildrensByClass(this HtmlNode htmlNode, string nodeClassType)
        {
            var outNodes = new HashSet<HtmlNode>();

            foreach (var node in htmlNode.ChildNodes)
            {
                if (node != null)
                {
                    var nodeClasses = node.GetClasses();

                    foreach (var nodeClass in nodeClasses)
                    {
                        if (nodeClass == nodeClassType)
                        {
                            outNodes.Add(node);
                        }
                    }

                }
            }
            return outNodes;
        }


        public static HtmlNode FindChildrenByAttribute(this HtmlNode htmlNode, string attributeName)
        {
            foreach (var node in htmlNode.ChildNodes)
            {
                if (node != null)
                {
                    var nodeAttributes = node.GetAttributes();

                    foreach (var attribute in nodeAttributes)
                    {
                        if (attribute.Name == attributeName)
                        {
                            return node;
                        }
                    }
                }
            }
            return null;
        }

        public static IEnumerable<HtmlNode> FindChildrensByAttribute(this HtmlNode htmlNode, string attributeName)
        {
            var outNodes = new HashSet<HtmlNode>();

            foreach (var node in htmlNode.ChildNodes)
            {
                if (node != null)
                {
                    var nodeAttributes = node.GetAttributes();

                    foreach (var attribute in nodeAttributes)
                    {
                        if (attribute.Name == attributeName)
                        {
                            outNodes.Add(node);
                        }
                    }
                }
            }
            return outNodes;
        }

    }
}
