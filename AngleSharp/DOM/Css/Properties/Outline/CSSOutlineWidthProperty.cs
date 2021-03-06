﻿namespace AngleSharp.Dom.Css
{
    using AngleSharp.Css;
    using System;

    /// <summary>
    /// More information available:
    /// https://developer.mozilla.org/en-US/docs/Web/CSS/outline-width
    /// </summary>
    sealed class CssOutlineWidthProperty : CssProperty, ICssOutlineWidthProperty
    {
        #region Fields

        internal static readonly Length Default = Length.Medium;
        internal static readonly IValueConverter<Length> Converter = Converters.LineWidthConverter;
        Length _width;

        #endregion

        #region ctor

        internal CssOutlineWidthProperty(CssStyleDeclaration rule)
            : base(PropertyNames.OutlineWidth, rule, PropertyFlags.Animatable)
        {
            Reset();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the width of the outline of an element. An outline is a
        /// line that is drawn around elements, outside the border edge,
        /// to make the element stand out.
        /// </summary>
        public Length Width
        {
            get { return _width; }
        }

        #endregion

        #region Methods

        public void SetWidth(Length width)
        {
            _width = width;
        }

        internal override void Reset()
        {
            _width = Length.Medium;
        }

        /// <summary>
        /// Determines if the given value represents a valid state of this property.
        /// </summary>
        /// <param name="value">The state that should be used.</param>
        /// <returns>True if the state is valid, otherwise false.</returns>
        protected override Boolean IsValid(ICssValue value)
        {
            return Converter.TryConvert(value, SetWidth);
        }

        #endregion
    }
}
