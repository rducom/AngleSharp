﻿namespace AngleSharp.Dom.Css
{
    using AngleSharp.Css;
    using AngleSharp.Extensions;
    using System;

    /// <summary>
    /// Information:
    /// https://developer.mozilla.org/en-US/docs/Web/CSS/font-style
    /// </summary>
    sealed class CssFontStyleProperty : CssProperty, ICssFontStyleProperty
    {
        #region Fields

        internal static readonly FontStyle Default = FontStyle.Normal;
        internal static readonly IValueConverter<FontStyle> Converter = Map.FontStyles.ToConverter();
        FontStyle _style;

        #endregion

        #region ctor

        internal CssFontStyleProperty(CssStyleDeclaration rule)
            : base(PropertyNames.FontStyle, rule, PropertyFlags.Inherited)
        {
            Reset();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the selected font style.
        /// </summary>
        public FontStyle Style
        {
            get { return _style; }
        }

        #endregion

        #region Methods

        void SetStyle(FontStyle style)
        {
            _style = style;
        }

        internal override void Reset()
        {
            _style = Default;
        }

        /// <summary>
        /// Determines if the given value represents a valid state of this property.
        /// </summary>
        /// <param name="value">The state that should be used.</param>
        /// <returns>True if the state is valid, otherwise false.</returns>
        protected override Boolean IsValid(ICssValue value)
        {
            return Converter.TryConvert(value, SetStyle);
        }

        #endregion
    }
}
