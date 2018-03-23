﻿// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using Avalonia.Animation.Easings;
using Avalonia.Collections;
using System;
using System.Reactive.Linq;

namespace Avalonia.Animation.Transitions
{
    /// <summary>
    /// Defines how a property should be animated using a transition.
    /// </summary>
    public abstract class Transition<T> : ITransition
    {
        private AvaloniaProperty _prop;
        private IEasing _easing;


        /// <summary>
        /// Gets the duration of the animation.
        /// </summary> 
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets the easing class to be used.
        /// </summary>
        public IEasing Easing
        {
            get
            {
                if (_easing == null) _easing = new LinearEasing();
                return _easing;
            }
            set
            {
                _easing = value;
            }
        }


        /// <inheritdocs/>
        public AvaloniaProperty Property
        {
            get
            {
                return _prop;
            }
            set
            {
                if (!(typeof(T) == value.PropertyType))
                    throw new InvalidCastException
                        ($"Invalid property type \"{typeof(T).Name}\" for this {GetType().Name} transition.");

                _prop = value;
            }
        }

        /// <summary>
        /// Apply interpolation to the property.
        /// </summary>
        public abstract IObservable<T> DoTransition(IObservable<double> progress, T oldValue, T newValue);

        /// <inheritdocs/>
        public IDisposable Apply(Animatable control, object oldValue, object newValue)
        {
            var transition = DoTransition(Timing.GetTimer(Duration), (T)oldValue, (T)newValue).Select(p => (object)p);
            return control.Bind(Property, transition, Data.BindingPriority.Animation);
        }

    }




}
