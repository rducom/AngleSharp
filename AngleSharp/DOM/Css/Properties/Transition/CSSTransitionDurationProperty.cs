﻿namespace AngleSharp.Dom.Css
{
    using AngleSharp.Css;
    using AngleSharp.Extensions;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// More information available at:
    /// https://developer.mozilla.org/en-US/docs/CSS/transition-duration
    /// </summary>
    sealed class CssTransitionDurationProperty : CssProperty, ICssTransitionDurationProperty
    {
        #region Fields

        internal static readonly IValueConverter<Time> SingleConverter = Converters.TimeConverter;
        internal static readonly IValueConverter<Time[]> Converter = SingleConverter.FromList();
        internal static readonly Time Default = Time.Zero;
        readonly List<Time> _times;

        #endregion

        #region ctor

        internal CssTransitionDurationProperty(CssStyleDeclaration rule)
            : base(PropertyNames.TransitionDuration, rule)
        {
            _times = new List<Time>();
            Reset();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the durations for the transitions.
        /// </summary>
        public IEnumerable<Time> Durations
        {
            get { return _times; }
        }

        #endregion

        #region Methods

        public void SetDurations(IEnumerable<Time> times)
        {
            _times.Clear();
            _times.AddRange(times);
        }

        internal override void Reset()
        {
            _times.Clear();
            _times.Add(Default);
        }

        /// <summary>
        /// Determines if the given value represents a valid state of this property.
        /// </summary>
        /// <param name="value">The state that should be used.</param>
        /// <returns>True if the state is valid, otherwise false.</returns>
        protected override Boolean IsValid(ICssValue value)
        {
            return Converter.TryConvert(value, SetDurations);
        }

        #endregion
    }
}
