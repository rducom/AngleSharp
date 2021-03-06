﻿namespace AngleSharp.Dom
{
    using AngleSharp.Dom.Collections;
    using AngleSharp.Dom.Events;
    using AngleSharp.Extensions;
    using AngleSharp.Html;
    using AngleSharp.Parser.Css;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Represents an element node.
    /// </summary>
    [DebuggerStepThrough]
    class Element : Node, IElement
    {
        #region Fields

        readonly List<IAttr> _attributes;
        readonly Dictionary<String, Action<String>> _attributeHandlers;

        HtmlElementCollection _elements;
        String _prefix;
        String _namespace;
        TokenList _classList;

        #endregion

        #region ctor

        /// <summary>
        /// Creates a new element node.
        /// </summary>
        public Element(Document owner, String name, NodeFlags flags = NodeFlags.None)
            : base(owner, name, NodeType.Element, flags)
        {
            _attributes = new List<IAttr>();
            _attributeHandlers = new Dictionary<String, Action<String>>();
            RegisterAttributeObserver(AttributeNames.Class, UpdateClassList);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the namespace prefix of the specified node, or null if no prefix is specified.
        /// </summary>
        public String Prefix
        {
            get { return _prefix; }
            internal set { _prefix = value; }
        }

        /// <summary>
        /// Gets the local part of the qualified name of this node.
        /// </summary>
        public String LocalName
        {
            get { return NodeName; }
        }

        /// <summary>
        /// Gets the namespace URI of this node.
        /// </summary>
        public String NamespaceUri
        {
            get { return _namespace; }
            internal set { _namespace = value; }
        }

        /// <summary>
        /// Gets or sets the text content of a node and its descendants.
        /// </summary>
        public sealed override String TextContent
        {
            get
            {
                var sb = Pool.NewStringBuilder();

                foreach (var child in this.GetDescendants().OfType<IText>())
                    sb.Append(child.Data);

                return sb.ToPool();
            }
            set
            {
                var node = !String.IsNullOrEmpty(value) ? new TextNode(Owner, value) : null;
                ReplaceAll(node, false);
            }
        }

        /// <summary>
        /// Gets the list of class names.
        /// </summary>
        public ITokenList ClassList
        {
            get 
            {
                if (_classList == null)
                {
                    _classList = new TokenList(GetAttribute(AttributeNames.Class));
                    _classList.Changed += (s, ev) => UpdateAttribute(AttributeNames.Class, _classList.ToString());
                }

                return _classList; 
            }
        }

        /// <summary>
        /// Gets or sets the value of the class attribute.
        /// </summary>
        public String ClassName
        {
            get { return GetAttribute(AttributeNames.Class); }
            set { SetAttribute(AttributeNames.Class, value); }
        }

        /// <summary>
        /// Gets or sets the value of the id attribute.
        /// </summary>
        public String Id
        {
            get { return GetAttribute(AttributeNames.Id); }
            set { SetAttribute(AttributeNames.Id, value); }
        }

        /// <summary>
        /// Gets the tagname of the element.
        /// </summary>
        public String TagName
        {
            get { return NodeName.ToUpperInvariant(); }
        }

        /// <summary>
        /// Gets the element immediately preceding in this node's parent's list of nodes, 
        /// null if the current element is the first element in that list.
        /// </summary>
        public IElement PreviousElementSibling
        {
            get
            {
                var parent = Parent;

                if (parent != null)
                {
                    var found = false;

                    for (int i = parent.ChildNodes.Length - 1; i >= 0; i--)
                    {
                        if (parent.ChildNodes[i] == this)
                            found = true;
                        else if (found && parent.ChildNodes[i] is IElement)
                            return (IElement)parent.ChildNodes[i];
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the element immediately following in this node's parent's list of nodes,
        /// or null if the current element is the last element in that list.
        /// </summary>
        public IElement NextElementSibling
        {
            get
            {
                var parent = Parent;

                if (parent != null)
                {
                    var n = parent.ChildNodes.Length;
                    var found = false;

                    for (int i = 0; i < n; i++)
                    {
                        if (parent.ChildNodes[i] == this)
                            found = true;
                        else if (found && parent.ChildNodes[i] is IElement)
                            return (IElement)parent.ChildNodes[i];
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the number of child elements.
        /// </summary>
        public Int32 ChildElementCount
        {
            get
            {
                var children = ChildNodes;
                var n = children.Length;
                var count = 0;

                for (int i = 0; i < n; i++)
                {
                    if (children[i].NodeType == NodeType.Element)
                        count++;
                }

                return count;
            }
        }

        /// <summary>
        /// Gets the child elements.
        /// </summary>
        public IHtmlCollection Children
        {
            get { return _elements ?? (_elements = new HtmlElementCollection(this, deep: false)); }
        }

        /// <summary>
        /// Gets the first child element of this element.
        /// </summary>
        public IElement FirstElementChild
        {
            get 
            {
                var children = ChildNodes;
                var n = children.Length;

                for (int i = 0; i < n; i++)
                {
                    var child = children[i] as IElement;

                    if (child != null)
                        return child;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the last child element of this element.
        /// </summary>
        public IElement LastElementChild
        {
            get
            {
                var children = ChildNodes;

                for (int i = children.Length - 1; i >= 0; i--)
                {
                    var child = children[i] as IElement;

                    if (child != null)
                        return child;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the HTML syntax describing the element's descendants.
        /// </summary>
        public String InnerHtml
        {
            get { return ChildNodes.ToHtml(); }
            set { ReplaceAll(new DocumentFragment(this, value), false); }
        }

        /// <summary>
        /// Gets or sets the HTML syntax describing the element including its descendants. 
        /// </summary>
        public String OuterHtml
        {
            get { return ToHtml(); }
            set
            {
                var parent = Parent;

                if (parent != null)
                {
                    if (Owner != null && Owner.DocumentElement == this)
                        throw new DomException(ErrorCode.NoModificationAllowed);

                    parent.InsertChild(parent.IndexOf(this), new DocumentFragment(this, value));
                    parent.RemoveChild(this);
                }
                else
                    throw new DomException(ErrorCode.NotSupported);
            }
        }
        
        /// <summary>
        /// Gets the sequence of associated attributes.
        /// </summary>
        IEnumerable<IAttr> IElement.Attributes
        {
            get { return _attributes; }
        }

        /// <summary>
        /// Gets the associated attribute container.
        /// </summary>
        internal List<IAttr> Attributes
        {
            get { return _attributes; }
        }

        /// <summary>
        /// Gets if the element is currently focused.
        /// </summary>
        public Boolean IsFocused
        {
            get
            {
                var owner = Owner;

                if (owner == null)
                    return false;

                return owner.FocusElement == this;
            }
            protected set
            {
                var owner = Owner;

                if (owner == null)
                    return;

                if (value)
                {
                    owner.SetFocus(this);
                    this.Fire<FocusEvent>(m => m.Init(EventNames.Focus, false, false));
                }
                else
                {
                    owner.SetFocus(null);
                    this.Fire<FocusEvent>(m => m.Init(EventNames.Blur, false, false));
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the first element within the document (using depth-first pre-order traversal
        /// of the document's nodes) that matches the specified group of selectors.
        /// </summary>
        /// <param name="selectors">A string containing one or more CSS selectors separated by commas.</param>
        /// <returns>An element object.</returns>
        public IElement QuerySelector(String selectors)
        {
            return ChildNodes.QuerySelector(selectors);
        }

        /// <summary>
        /// Returns a list of the elements within the document (using depth-first pre-order traversal
        /// of the document's nodes) that match the specified group of selectors.
        /// </summary>
        /// <param name="selectors">A string containing one or more CSS selectors separated by commas.</param>
        /// <returns>A collection of HTML elements.</returns>
        public IHtmlCollection QuerySelectorAll(String selectors)
        {
            return ChildNodes.QuerySelectorAll(selectors);
        }

        /// <summary>
        /// Returns a set of elements which have all the given class names.
        /// </summary>
        /// <param name="classNames">A string representing the list of class names to match; class names are separated by whitespace.</param>
        /// <returns>A collection of HTML elements.</returns>
        public IHtmlCollection GetElementsByClassName(String classNames)
        {
            return ChildNodes.GetElementsByClassName(classNames);
        }

        /// <summary>
        /// Returns a NodeList of elements with the given tag name. The complete document is searched, including the root node.
        /// </summary>
        /// <param name="tagName">A string representing the name of the elements. The special string "*" represents all elements.</param>
        /// <returns>A NodeList of found elements in the order they appear in the tree.</returns>
        public IHtmlCollection GetElementsByTagName(String tagName)
        {
            return ChildNodes.GetElementsByTagName(tagName);
        }

        /// <summary>
        /// Returns a list of elements with the given tag name belonging to the given namespace.
        /// The complete document is searched, including the root node.
        /// </summary>
        /// <param name="namespaceURI">The namespace URI of elements to look for.</param>
        /// <param name="tagName">Either the local name of elements to look for or the special value "*", which matches all elements.</param>
        /// <returns>A NodeList of found elements in the order they appear in the tree.</returns>
        public IHtmlCollection GetElementsByTagNameNS(String namespaceURI, String tagName)
        {
            return ChildNodes.GetElementsByTagName(namespaceURI, tagName);
        }

        /// <summary>
        /// Checks if the element is matched by the given selector.
        /// </summary>
        /// <param name="selectors">Represents the selector to test.</param>
        /// <returns>True if the element would be selected by the specified selector, otherwise false.</returns>
        public Boolean Matches(String selectors)
        {
            return CssParser.ParseSelector(selectors).Match(this);
        }

        /// <summary>
        /// Returns a duplicate of the node on which this method was called.
        /// </summary>
        /// <param name="deep">Optional value: true if the children of the node should also be cloned, or false to clone only the specified node.</param>
        /// <returns>The duplicate node.</returns>
        public override INode Clone(Boolean deep = true)
        {
            var node = new Element(Owner, NodeName, Flags);
            CopyProperties(this, node, deep);
            CopyAttributes(this, node);
            return node;
        }

        /// <summary>
        /// Creates a pseudo element for the current element.
        /// </summary>
        /// <param name="pseudoElement">The element to create (e.g. ::after).</param>
        /// <returns>The created element or null, if not possible.</returns>
        public IPseudoElement Pseudo(String pseudoElement)
        {
            return PseudoElement.Create(this, pseudoElement);
        }

        /// <summary>
        /// Returns a boolean value indicating whether the specified element has the specified attribute or not.
        /// </summary>
        /// <param name="name">The attributes name.</param>
        /// <returns>The return value of true or false.</returns>
        public Boolean HasAttribute(String name)
        {
            if (_namespace == Namespaces.HtmlUri)
                name = name.ToLowerInvariant();

            return _attributes.Has(name);
        }

        /// <summary>
        /// Returns a boolean value indicating whether the specified element has the specified attribute or not.
        /// </summary>
        /// <param name="namespaceUri">A string specifying the namespace of the attribute.</param>
        /// <param name="localName">The attributes name.</param>
        /// <returns>The return value of true or false.</returns>
        public Boolean HasAttribute(String namespaceUri, String localName)
        {
            if (String.IsNullOrEmpty(namespaceUri))
                namespaceUri = null;

            return _attributes.Has(namespaceUri, localName);
        }

        /// <summary>
        /// Returns the value of the named attribute on the specified element.
        /// </summary>
        /// <param name="name">The name of the attribute whose value you want to get.</param>
        /// <returns>If the named attribute does not exist, the value returned will be null, otherwise the attribute's value.</returns>
        public String GetAttribute(String name)
        {
            if (_namespace == Namespaces.HtmlUri)
                name = name.ToLower();

            var attr = _attributes.Get(name);
            return attr != null ? attr.Value : null;
        }

        /// <summary>
        /// Returns the value of the named attribute on the specified element.
        /// </summary>
        /// <param name="namespaceUri">A string specifying the namespace of the attribute.</param>
        /// <param name="localName">The name of the attribute whose value you want to get.</param>
        /// <returns>If the named attribute does not exist, the value returned will be null, otherwise the attribute's value.</returns>
        public String GetAttribute(String namespaceUri, String localName)
        {
            if (String.IsNullOrEmpty(namespaceUri))
                namespaceUri = null;

            var attr = _attributes.Get(namespaceUri, localName);
            return attr != null ? attr.Value : null;
        }

        /// <summary>
        /// Adds a new attribute or changes the value of an existing attribute on the specified element.
        /// </summary>
        /// <param name="name">The name of the attribute as a string.</param>
        /// <param name="value">The desired new value of the attribute.</param>
        public void SetAttribute(String name, String value)
        {
            if (value != null)
            {
                if (!name.IsXmlName())
                    throw new DomException(ErrorCode.InvalidCharacter);

                if (_namespace == Namespaces.HtmlUri)
                    name = name.ToLowerInvariant();

                for (int i = 0; i < _attributes.Count; i++)
                {
                    if (_attributes[i].Prefix == null && _attributes[i].LocalName == name)
                    {
                        _attributes[i].Value = value;
                        return;
                    }
                }

                _attributes.Add(new Attr(this, name, value));
                AttributeChanged(name, null, null);
            }
            else
                RemoveAttribute(name);
        }

        /// <summary>
        /// Adds a new attribute or changes the value of an existing attribute on the specified element.
        /// </summary>
        /// <param name="namespaceUri">A string specifying the namespace of the attribute.</param>
        /// <param name="name">The name of the attribute as a string.</param>
        /// <param name="value">The desired new value of the attribute.</param>
        public void SetAttribute(String namespaceUri, String name, String value)
        {            
            if (value != null)
            {
                if (String.IsNullOrEmpty(namespaceUri))
                    namespaceUri = null;

                if (!name.IsXmlName())
                    throw new DomException(ErrorCode.InvalidCharacter);
                else if (!name.IsQualifiedName())
                    throw new DomException(ErrorCode.Namespace);

                var index = name.IndexOf(Symbols.Colon);
                var prefix = index >= 0 ? name.Substring(0, index) : null;
                var localName = index >= 0 ? name.Substring(index + 1) : name;

                if (prefix != null && namespaceUri == null)
                    throw new DomException(ErrorCode.Namespace);

                if (prefix == Namespaces.XmlPrefix && namespaceUri != Namespaces.XmlUri)
                    throw new DomException(ErrorCode.Namespace);
                else if ((name == Namespaces.XmlNsPrefix || prefix == Namespaces.XmlNsPrefix) && namespaceUri != Namespaces.XmlNsUri)
                    throw new DomException(ErrorCode.Namespace);
                else if (namespaceUri == Namespaces.XmlNsUri && name != Namespaces.XmlNsPrefix && prefix != Namespaces.XmlNsPrefix)
                    throw new DomException(ErrorCode.Namespace);

                for (int i = 0; i < _attributes.Count; i++)
                {
                    if (_attributes[i].LocalName == localName && _attributes[i].NamespaceUri == namespaceUri)
                    {
                        _attributes[i].Value = value;
                        return;
                    }
                }

                _attributes.Add(new Attr(this, prefix, localName, value, namespaceUri));
                AttributeChanged(localName, namespaceUri, null);
            }
            else
                RemoveAttribute(namespaceUri, name);
        }

        /// <summary>
        /// Removes an attribute from the specified element.
        /// </summary>
        /// <param name="name">Is a string that names the attribute to be removed.</param>
        /// <returns>The current element.</returns>
        public void RemoveAttribute(String name)
        {
            if (_namespace == Namespaces.HtmlUri)
                name = name.ToLower();

            for (int i = 0; i < _attributes.Count; i++)
            {
                if (_attributes[i].Prefix == null && _attributes[i].LocalName == name)
                {
                    var attr = _attributes[i];
                    _attributes.RemoveAt(i);
                    AttributeChanged(attr.LocalName, attr.NamespaceUri, attr.Value);
                    return;
                }
            }
        }

        /// <summary>
        /// Removes an attribute from the specified element.
        /// </summary>
        /// <param name="namespaceUri">A string specifying the namespace of the attribute.</param>
        /// <param name="localName">Is a string that names the attribute to be removed.</param>
        /// <returns>The current element.</returns>
        public void RemoveAttribute(String namespaceUri, String localName)
        {
            if (String.IsNullOrEmpty(namespaceUri))
                namespaceUri = null;

            for (int i = 0; i < _attributes.Count; i++)
            {
                if (_attributes[i].LocalName == localName && _attributes[i].NamespaceUri == namespaceUri)
                {
                    var attr = _attributes[i];
                    _attributes.RemoveAt(i);
                    AttributeChanged(attr.LocalName, attr.NamespaceUri, attr.Value);
                    return;
                }
            }
        }

        /// <summary>
        /// Prepends nodes to the current node.
        /// </summary>
        /// <param name="nodes">The nodes to prepend.</param>
        public void Prepend(params INode[] nodes)
        {
            this.PrependNodes(nodes);
        }

        /// <summary>
        /// Appends nodes to current node.
        /// </summary>
        /// <param name="nodes">The nodes to append.</param>
        public void Append(params INode[] nodes)
        {
            this.AppendNodes(nodes);
        }

        public override Boolean Equals(INode otherNode)
        {
            var otherElement = otherNode as IElement;

            if (otherElement != null)
            {
                if (this.NamespaceUri != otherElement.NamespaceUri)
                    return false;

                if (_attributes.Count != otherElement.Attributes.Count())
                    return false;

                for (int i = 0; i < _attributes.Count; i++)
                {
                    if (!otherElement.Attributes.Any(m => m.Name == _attributes[i].Name && m.Value == _attributes[i].Value))
                        return false;
                }

                return base.Equals(otherNode);
            }

            return false;
        }

        /// <summary>
        /// Inserts nodes before the current node.
        /// </summary>
        /// <param name="nodes">The nodes to insert before.</param>
        /// <returns>The current element.</returns>
        public void Before(params INode[] nodes)
        {
            this.InsertBefore(nodes);
        }

        /// <summary>
        /// Inserts nodes after the current node.
        /// </summary>
        /// <param name="nodes">The nodes to insert after.</param>
        /// <returns>The current element.</returns>
        public void After(params INode[] nodes)
        {
            this.InsertAfter(nodes);
        }

        /// <summary>
        /// Replaces the current node with the nodes.
        /// </summary>
        /// <param name="nodes">The nodes to replace.</param>
        public void Replace(params INode[] nodes)
        {
            this.ReplaceWith(nodes);
        }

        /// <summary>
        /// Removes the current element from the parent.
        /// </summary>
        public void Remove()
        {
            this.RemoveFromParent();
        }

        /// <summary>
        /// Inserts new HTML elements specified by the given HTML string at
        /// a position relative to the current element specified by the position.
        /// </summary>
        /// <param name="position">The relation to the current element.</param>
        /// <param name="html">The HTML code to generate elements for.</param>
        public void Insert(AdjacentPosition position, String html)
        {
            var useThis = position == AdjacentPosition.BeforeBegin || position == AdjacentPosition.AfterEnd;
            var nodeParent = useThis ? this : Parent as Element;
            var nodes = new DocumentFragment(nodeParent, html);

            switch (position)
            {
                case AdjacentPosition.BeforeBegin:
                    Parent.InsertBefore(nodes, this);
                    break;

                case AdjacentPosition.AfterEnd:
                    Parent.InsertChild(Parent.IndexOf(this) + 1, nodes);
                    break;

                case AdjacentPosition.AfterBegin:
                    InsertChild(0, nodes);
                    break;

                case AdjacentPosition.BeforeEnd:
                    AppendChild(nodes);
                    break;
            }
        }

        #endregion

        #region String Representation

        /// <summary>
        /// Returns an HTML-code representation of the node.
        /// </summary>
        /// <returns>A string containing the HTML code.</returns>
        public override String ToHtml()
        {
            var sb = Pool.NewStringBuilder();
            var tagName = (Flags & (NodeFlags.HtmlMember | NodeFlags.SvgMember | NodeFlags.MathMember)) != NodeFlags.None ? LocalName : NodeName;

            sb.Append(Symbols.LessThan).Append(tagName);

            foreach (var attribute in _attributes)
                sb.Append(Symbols.Space).Append(attribute.ToString());

            sb.Append(Symbols.GreaterThan);

            if (!Flags.HasFlag(NodeFlags.SelfClosing))
            {
                if (Flags.HasFlag(NodeFlags.LineTolerance) && FirstChild is IText)
                {
                    var text = (IText)FirstChild;

                    if (text.Data.Length > 0 && text.Data[0] == Symbols.LineFeed)
                        sb.Append(Symbols.LineFeed);
                }

                foreach (var child in ChildNodes)
                    sb.Append(child.ToHtml());

                sb.Append(Symbols.LessThan).Append(Symbols.Solidus).Append(tagName);
                sb.Append(Symbols.GreaterThan);
            }

            return sb.ToPool();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Updates an attribute's value without notifying the observers.
        /// </summary>
        /// <param name="name">The name of the attribute to update.</param>
        /// <param name="value">The value of the attribute to set.</param>
        protected void UpdateAttribute(String name, String value)
        {
            Action<String> handler = null;

            if (_attributeHandlers.TryGetValue(name, out handler))
                _attributeHandlers.Remove(name);

            SetAttribute(name, value);

            if (handler != null)
                _attributeHandlers.Add(name, handler);
        }

        internal void AttributeChanged(String localName, String namespaceUri, String oldValue, Boolean suppressMutationObservers = false)
        {
            Action<String> handler = null;

            if (_attributeHandlers.TryGetValue(localName, out handler))
            {
                var attr = _attributes.Get(localName);
                handler(attr != null ? attr.Value : null);
            }

            if (!suppressMutationObservers)
            {
                Owner.QueueMutation(MutationRecord.Attributes(
                    target: this,
                    attributeName: localName,
                    attributeNamespace: namespaceUri,
                    previousValue: oldValue));
            }
        }

        /// <summary>
        /// Locates the namespace of the given prefix.
        /// </summary>
        /// <param name="prefix">The prefix of the namespace to find.</param>
        /// <returns>The url of the namespace or null, if the prefix could not be found.</returns>
        protected sealed override String LocateNamespace(String prefix)
        {
            return ElementExtensions.LocateNamespace(this, prefix);
        }

        /// <summary>
        /// Locates the prefix of the given namespace.
        /// </summary>
        /// <param name="namespaceUri">The url of the namespace.</param>
        /// <returns>The prefix or null, if the namespace could not be found.</returns>
        protected sealed override String LocatePrefix(String namespaceUri)
        {
            return ElementExtensions.LocatePrefix(this, namespaceUri);
        }

        /// <summary>
        /// Copies the attributes from the source element to the target element.
        /// Each attribute will be recreated on the target.
        /// </summary>
        /// <param name="source">The source of the attributes.</param>
        /// <param name="target">The target where to create the attributes.</param>
        protected static void CopyAttributes(Element source, Element target)
        {
            target._namespace = source._namespace;
            target._prefix = source._prefix;

            for (int i = 0; i < source._attributes.Count; i++)
                target._attributes.Add(new Attr(target, source._attributes[i].Name, source._attributes[i].Value));
        }

        /// <summary>
        /// Registers an observer for attribute events.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="callback">The callback to invoke.</param>
        protected void RegisterAttributeObserver(String name, Action<String> callback)
        {
            Action<String> handler = null;

            if (_attributeHandlers.TryGetValue(name, out handler))
                handler += callback;
            else
                handler = callback;

            _attributeHandlers[name] = handler;
        }

        void UpdateClassList(String value)
        {
            if (_classList != null)
                _classList.Update(value);
        }

        #endregion

        #region Updateable Location

        /// <summary>
        /// Represents an updateable location (url) holder.
        /// </summary>
        protected sealed class BoundLocation : ILocation
        {
            #region Fields

            readonly Element _parent;
            Location _location;
            Url _baseUrl;
            String _value;

            #endregion

            #region ctor

            public BoundLocation(Element parent)
            {
                _parent = parent;
            }

            #endregion

            #region Properties

            public Location Location
            {
                get
                {
                    var value = _parent.GetAttribute(AttributeNames.Href) ?? String.Empty;
                    var baseUrl = _parent.BaseUrl;

                    if (_location == null || !baseUrl.Equals(_baseUrl) || !value.Equals(_value, StringComparison.Ordinal))
                    {
                        var url = new Url(baseUrl, value);
                        _baseUrl = baseUrl;
                        _value = value;
                        _location = new Location(url);
                    }

                    return _location;
                }
            }

            public String Href
            {
                get { return Location.Href; }
                set { Location.Href = value; Assign(value); }
            }

            public String Protocol
            {
                get { return Location.Protocol; }
                set { Location.Protocol = value; Reload(); }
            }

            public String Host
            {
                get { return Location.Host; }
                set { Location.Host = value; Reload(); }
            }

            public String HostName
            {
                get { return Location.HostName; }
                set { Location.HostName = value; Reload(); }
            }

            public String Port
            {
                get { return Location.Port; }
                set { Location.Port = value; Reload(); }
            }

            public String PathName
            {
                get { return Location.PathName; }
                set { Location.PathName = value; Reload(); }
            }

            public String Search
            {
                get { return Location.Search; }
                set { Location.Search = value; Reload(); }
            }

            public String Hash
            {
                get { return Location.Hash; }
                set { Location.Hash = value; Reload(); }
            }

            public String UserName
            {
                get { return Location.UserName; }
                set { Location.UserName = value; Reload(); }
            }

            public String Password
            {
                get { return Location.Password; }
                set { Location.Password = value; Reload(); }
            }

            public String Origin
            {
                get { return Location.Origin; }
            }

            #endregion

            #region Methods

            public void Assign(String url)
            {
                _parent.SetAttribute(AttributeNames.Href, url);
                _value = url;
            }

            public void Replace(String url)
            {
                Href = url;
            }

            public void Reload()
            {
                Assign(Href);
            }

            #endregion
        }

        #endregion
    }
}
