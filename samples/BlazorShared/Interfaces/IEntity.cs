using System;

namespace BlazorShared.Interfaces
{
    /// <summary>
    /// Default entity with int key implementation
    /// </summary>
    public interface IEntity : IEntity<int>
    {
    }

    /// <summary>
    /// Generic entity with key implementation
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IEntity<TKey> where TKey : struct, IComparable<TKey>
    {
        /// <summary>
        /// Entity key
        /// </summary>
        public TKey Id { get; set; }
    }
}
