﻿namespace AngleSharp.Dom.Css
{
    /// <summary>
    /// Represents the CSS min-width property.
    /// </summary>
    public interface ICssMinWidthProperty : ICssProperty
    {
        /// <summary>
        /// Gets the minimum height of the element.
        /// </summary>
        Length? Limit { get; }
    }
}
