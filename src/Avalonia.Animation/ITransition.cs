// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using Avalonia.Collections;
using System;

namespace Avalonia.Animation.Transitions
{
    /// <summary>
    /// Interface for transition objects.
    /// </summary>
    public interface ITransition
    {
        /// <summary>
        /// Applies the transition to the specified <see cref="Animatable"/>.
        /// </summary>
        IDisposable Apply(Animatable control, object oldValue, object newValue);

        /// <summary>
        /// Gets the property to be animated.
        /// </summary>
        AvaloniaProperty Property { get; set; }

    }

    public class Transitions : AvaloniaList<ITransition>
    {
        public Transitions()
        {
            ResetBehavior = ResetBehavior.Remove;
        }
    }
}