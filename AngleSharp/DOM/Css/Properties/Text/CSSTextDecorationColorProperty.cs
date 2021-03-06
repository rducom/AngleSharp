﻿namespace AngleSharp.Dom.Css
{
    using AngleSharp.Css;
    using System;

    /// <summary>
    /// More Information:
    /// https://developer.mozilla.org/en-US/docs/Web/CSS/text-decoration-color
    /// </summary>
    sealed class CssTextDecorationColorProperty : CssProperty, ICssTextDecorationColorProperty
    {
        #region Fields

        internal static readonly Color Default = Color.Black;
        internal static readonly IValueConverter<Color> Converter = Converters.ColorConverter;
        Color _color;

        #endregion

        #region ctor

        internal CssTextDecorationColorProperty(CssStyleDeclaration rule)
            : base(PropertyNames.TextDecorationColor, rule, PropertyFlags.Animatable)
        {
            Reset();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the selected text-decoration color.
        /// </summary>
        public Color Color
        {
            get { return _color; }
        }

        #endregion

        #region Methods

        public void SetColor(Color color)
        {
            _color = color;
        }

        internal override void Reset()
        {
            _color = Default;
        }

        /// <summary>
        /// Determines if the given value represents a valid state of this property.
        /// </summary>
        /// <param name="value">The state that should be used.</param>
        /// <returns>True if the state is valid, otherwise false.</returns>
        protected override Boolean IsValid(ICssValue value)
        {
            return Converter.TryConvert(value, SetColor);
        }

        #endregion
    }
}
