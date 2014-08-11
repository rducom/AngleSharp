﻿using AngleSharp;
using AngleSharp.DOM;
using AngleSharp.DOM.Html;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace UnitTests.Library
{
    [TestClass]
    public class TreeWalkerTests
    {
        [TestMethod]
        public void TreeWalkerJavaScriptKitDivision()
        {
            var source = @"<div id=contentarea>
<p>Some <span>text</span></p>
<b>Bold text</b>
</div>";
            var doc = DocumentBuilder.Html(source);
            Assert.IsNotNull(doc);

            var rootnode = doc.GetElementById("contentarea");
            Assert.IsNotNull(rootnode);

            var walker = doc.CreateTreeWalker(rootnode, FilterSettings.Element);
            Assert.IsNotNull(walker);
            Assert.AreEqual(rootnode, walker.Current);

            var results = new List<INode>();

            while (walker.ToNext() != null)
                results.Add(walker.Current);

            Assert.AreEqual(3, results.Count);
            Assert.IsInstanceOfType(results[0], typeof(HTMLParagraphElement));
            Assert.IsInstanceOfType(results[1], typeof(HTMLSpanElement));
            Assert.IsInstanceOfType(results[2], typeof(HTMLBoldElement));

            walker.Current = rootnode;
            Assert.IsInstanceOfType(walker.ToFirst(), typeof(HTMLParagraphElement));
        }

        [TestMethod]
        public void TreeWalkerJavaScriptKitParagraph()
        {
            var source = @"<p id=essay>George<span> loves </span><b>JavaScript!</b></p>";
            var doc = DocumentBuilder.Html(source);
            Assert.IsNotNull(doc);

            var rootnode = doc.GetElementById("essay");
            Assert.IsNotNull(rootnode);

            var walker = doc.CreateTreeWalker(rootnode, FilterSettings.Text);
            Assert.IsNotNull(walker);
            Assert.AreEqual(rootnode, walker.Current);

            Assert.AreEqual("George", walker.ToFirst().TextContent);

            var paratext = walker.Current.TextContent;

            while (walker.ToNextSibling() != null)
                paratext += walker.Current.TextContent;

            Assert.AreEqual("George loves JavaScript!", paratext);
        }

        [TestMethod]
        public void TreeWalkerJavaScriptKitList()
        {
            var source = @"<ul id=mylist>
<li class='item'>List 1</li>
<li class='item'>List 2</li>
<li>List 3</li>
</ul>";
            var doc = DocumentBuilder.Html(source);
            Assert.IsNotNull(doc);

            var rootnode = doc.GetElementById("mylist");
            Assert.IsNotNull(rootnode);

            var walker = doc.CreateTreeWalker(rootnode, FilterSettings.Element, node =>
            {
                var element = node as IHtmlListItemElement;

                if (element != null && element.ClassList.Contains("item"))
                    return FilterResult.Accept;

                return FilterResult.Reject;
            });

            Assert.IsNotNull(walker);
            Assert.AreEqual(rootnode, walker.Current);

            var results = new List<INode>();

            while (walker.ToNext() != null)
                results.Add(walker.Current);

            Assert.AreEqual(7, rootnode.ChildNodes.Length);
            Assert.AreEqual(3, rootnode.Children.Length);
            Assert.AreEqual(2, results.Count);

            var item1 = results[0] as IHtmlListItemElement;
            var item2 = results[1] as IHtmlListItemElement;

            Assert.IsNotNull(item1);
            Assert.IsNotNull(item2);

            Assert.AreEqual("item", item1.ClassName);
            Assert.AreEqual("item", item2.ClassName);
        }

        [TestMethod]
        public void TreeWalkerDotteroSpans()
        {
            var source = @"<div id=""content"">
        <span>
            <b>1. Section</b><br />
            <span>
                <b>1.1. Subsection</b><br />
            </span>
        </span>
        <span>
            <b>2.Section</b><br />
        </span>
    </div>";
            var doc = DocumentBuilder.Html(source);
            Assert.IsNotNull(doc);

            var rootnode = doc.GetElementById("content");
            Assert.IsNotNull(rootnode);

            var walker = doc.CreateTreeWalker(rootnode, FilterSettings.Element, 
                m => m.NodeName == "span" ? FilterResult.Accept : FilterResult.Skip);
            Assert.IsNotNull(walker);
            Assert.AreEqual(rootnode, walker.Current);

            var node = walker.ToFirst();
            var sections = 0;
            Assert.IsNotNull(node);

            while (node != null)
            {
                Assert.AreEqual("span", node.NodeName);
                sections++;
                node = walker.ToNextSibling();
            }

            Assert.AreEqual(2, sections);
        }
    }
}