﻿namespace AngleSharp.DOM.Mathml
{
    using AngleSharp.DOM.Factories;
    using System;

    sealed class MathElementFactory : ElementFactory<MathElement>
    {
        static readonly MathElementFactory Instance = new MathElementFactory();

        MathElementFactory()
        {
            creators.Add(Tags.Mn, () => new MathNumberElement());
            creators.Add(Tags.Mo, () => new MathOperatorElement());
            creators.Add(Tags.Mi, () => new MathIdentifierElement());
            creators.Add(Tags.Ms, () => new MathStringElement());
            creators.Add(Tags.Mtext, () => new MathTextElement());
            creators.Add(Tags.AnnotationXml, () => new MathAnnotationXmlElement());
        }

        protected override MathElement CreateDefault(String name, Document document)
        {
            return new MathElement { NodeName = name, Owner = document };
        }

        /// <summary>
        /// Returns a specialized MathMLElement instance for the given tag name.
        /// </summary>
        /// <param name="tagName">The given tag name.</param>
        /// <param name="document">The document that owns the element.</param>
        /// <returns>The specialized MathMLElement instance.</returns>
        public static MathElement Create(String tagName, Document document)
        {
            return Instance.CreateSpecific(tagName, document);

        }
    }
}