﻿using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    ///     Set of extension methods for <see cref="ControlChangeEvent" /> event.
    /// </summary>
    public static class ControlUtilities
    {
        #region Methods

        /// <summary>
        ///     Gets name of the controller presented by an instance of <see cref="ControlChangeEvent" />.
        /// </summary>
        /// <param name="controlChangeEvent">Control Change event to get controller name of.</param>
        /// <returns>Controller name of the <paramref name="controlChangeEvent" /> event.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="controlChangeEvent" /> is null.</exception>
        public static ControlName GetControlName(this ControlChangeEvent controlChangeEvent)
        {
            ThrowIfArgument.IsNull(nameof(controlChangeEvent), controlChangeEvent);

            return GetControlName(controlChangeEvent.ControlNumber);
        }

        /// <summary>
        ///     Converts <see cref="ControlName" /> to the corresponding value of the <see cref="SevenBitNumber" /> type.
        /// </summary>
        /// <param name="controlName"><see cref="ControlName" /> to convert to <see cref="SevenBitNumber" />.</param>
        /// <returns><see cref="SevenBitNumber" /> representing the <paramref name="controlName" />.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="controlName" /> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this ControlName controlName)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(controlName), controlName);

            return (SevenBitNumber) (byte) controlName;
        }

        /// <summary>
        ///     Gets an instance of the <see cref="ControlChangeEvent" /> corresponding to the specified controller.
        /// </summary>
        /// <param name="controlName"><see cref="ControlName" /> to get an event for.</param>
        /// <param name="controlValue">Controller value to set to event.</param>
        /// <returns>An instance of the <see cref="ControlChangeEvent" /> corresponding to the <paramref name="controlName" />.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="controlName" /> specified an invalid value.</exception>
        public static ControlChangeEvent GetControlChangeEvent(this ControlName controlName,
            SevenBitNumber controlValue)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(controlName), controlName);

            return new ControlChangeEvent(controlName.AsSevenBitNumber(), controlValue);
        }

        /// <summary>
        ///     Gets name of the controller presented by control number.
        /// </summary>
        /// <param name="controlNumber">Control number to get controller name of.</param>
        /// <returns>Name of the controller presented by <paramref name="controlNumber" />.</returns>
        private static ControlName GetControlName(SevenBitNumber controlNumber)
        {
            return Enum.IsDefined(typeof(ControlName), controlNumber)
                ? (ControlName) (byte) controlNumber
                : ControlName.Undefined;
        }

        #endregion
    }
}