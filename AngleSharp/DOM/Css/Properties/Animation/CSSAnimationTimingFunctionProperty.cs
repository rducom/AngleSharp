﻿namespace AngleSharp.Dom.Css
{
    using AngleSharp.Css;
    using AngleSharp.Css.Values;
    using AngleSharp.Extensions;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// More information available at:
    /// https://developer.mozilla.org/en-US/docs/CSS/animation-timing-function
    /// </summary>
    sealed class CssAnimationTimingFunctionProperty : CssProperty, ICssAnimationTimingFunctionProperty
    {
        #region Fields

        internal static readonly IValueConverter<ITimingFunction> SingleConverter = Converters.TransitionConverter;
        internal static readonly IValueConverter<ITimingFunction[]> Converter = SingleConverter.FromList();
        internal static readonly ITimingFunction Default = Map.TimingFunctions[Keywords.Ease];
        readonly List<ITimingFunction> _functions;

        #endregion

        #region ctor

        internal CssAnimationTimingFunctionProperty(CssStyleDeclaration rule)
            : base(PropertyNames.AnimationTimingFunction, rule)
        {
            _functions = new List<ITimingFunction>();
            Reset();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the enumeration over all timing functions.
        /// </summary>
        public IEnumerable<ITimingFunction> TimingFunctions
        {
            get { return _functions; }
        }

        #endregion

        #region Methods

        public void SetTimingFunctions(IEnumerable<ITimingFunction> functions)
        {
            _functions.Clear();
            _functions.AddRange(functions);
        }

        internal override void Reset()
        {
            _functions.Clear();
            _functions.Add(Default);
        }

        /// <summary>
        /// Determines if the given value represents a valid state of this property.
        /// </summary>
        /// <param name="value">The state that should be used.</param>
        /// <returns>True if the state is valid, otherwise false.</returns>
        protected override Boolean IsValid(ICssValue value)
        {
            return Converter.TryConvert(value, SetTimingFunctions);
        }

        #endregion
    }
}
