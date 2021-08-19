using System;
using System.Collections.Generic;
using System.Linq;

namespace iMessengerCoreAPI
{
#pragma warning disable CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена
    public static class Extensions
#pragma warning restore CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена
    {
        /// <summary>
        /// Checks if sequence contains all elements of <paramref name="subset"/> by using the default equality comparer.
        /// </summary>
        /// <typeparam name="T"> Type of elements of both sequences. </typeparam>
        /// <param name="superset"> Sequence which is expected to be superset for <paramref name="subset"/>. </param>
        /// <param name="subset"> Sequence which is expected to be subset of <paramref name="superset"/>. </param>
        /// <returns> 
        /// <see langword="true"/> if <paramref name="superset"/> contains all elements of <paramref name="subset"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"/>
        public static bool IsSupersetFor<T>(this IEnumerable<T> superset, IEnumerable<T> subset) =>
            (superset, subset) switch
            {
                (null, _) => throw new ArgumentNullException(nameof(superset)),
                (_, null) => throw new ArgumentNullException(nameof(subset)),
                (_, _) => subset.All(superset.Contains)
            };
    }
}
