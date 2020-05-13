#region License
/*
The MIT License

Copyright (c) 2008 Sky Morey

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace System
{
    /// <summary>
    /// Class Nameable.
    /// </summary>
    public static class Nameable
    {
        /// <summary>
        /// Compares the specified n1.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="n1">The n1.</param>
        /// <param name="n2">The n2.</param>
        /// <returns>System.Int32.</returns>
        public static int Compare<T>(Nameable<T> n1, Nameable<T> n2) => Comparer<T>.Default.Compare(n1.Value, n2.Value);

        /// <summary>
        /// Equalses the specified n1.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="n1">The n1.</param>
        /// <param name="n2">The n2.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool Equals<T>(Nameable<T> n1, Nameable<T> n2) => EqualityComparer<T>.Default.Equals(n1.Value, n2.Value);

        /// <summary>
        /// Gets the type of the underlying.
        /// </summary>
        /// <param name="nameableType">Type of the nameable.</param>
        /// <returns>Type.</returns>
        /// <exception cref="System.ArgumentNullException">nameableType</exception>
        public static Type GetUnderlyingType(Type nameableType) => (nameableType ?? throw new ArgumentNullException(nameof(nameableType))).IsGenericType && !nameableType.IsGenericTypeDefinition && ReferenceEquals(nameableType.GetGenericTypeDefinition(), typeof(Nameable<>)) ? nameableType.GetGenericArguments()[0] : null;
    }

    /// <summary>
    /// Struct Nameable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [TypeConverter(typeof(NameableConverter)), JsonObject(MemberSerialization.OptIn)]
    public struct Nameable<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Nameable{T}"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public Nameable(T value)
        {
            Value = value;
            Name = null;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Nameable{T}"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        public Nameable(T value, string name)
        {
            Value = value;
            Name = name;
        }

        /// <summary>
        /// The value
        /// </summary>
        [JsonProperty("v")] public T Value { get; set; }

        /// <summary>
        /// The name
        /// </summary>
        [JsonProperty("n")] public string Name { get; set; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj) => Value != null ? obj != null && Value.Equals(obj) : obj == null;

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString() => Value?.ToString();

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode() => Value?.GetHashCode() ?? 0;

        /// <summary>
        /// Performs an implicit conversion from <see cref="T"/> to <see cref="Nameable{T}"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Nameable<T>(T value) => new Nameable<T>(value);

        /// <summary>
        /// Performs an explicit conversion from <see cref="Nameable{T}"/> to <see cref="T"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator T(Nameable<T> value) => value.Value;

        /// <summary>
        /// Implements the == operator.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Nameable<T> a, Nameable<T> b) => EqualityComparer<T>.Default.Equals(a.Value, b.Value);

        /// <summary>
        /// Implements the != operator.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(Nameable<T> a, Nameable<T> b) => !EqualityComparer<T>.Default.Equals(a.Value, b.Value);

        /// <summary>
        /// Ases the name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Nameable&lt;T&gt;.</returns>
        public Nameable<T> AsName(string name) => new Nameable<T>(Value, name);
    }
}
