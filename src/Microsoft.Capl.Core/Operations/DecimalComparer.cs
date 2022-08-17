using System.Collections;

namespace Capl.Operations
{
    /// <summary>
    ///     Comparers two decimal types.
    /// </summary>
    public class DecimalComparer : IComparer
    {
        #region IComparer Members

        /// <summary>
        ///     Compares two DateTime types by UTC.
        /// </summary>
        /// <param name="x">LHS datetime parameter to test.</param>
        /// <param name="y">RHS datatime parameter to test.</param>
        /// <returns>0 for equality; 1 for x greater than y; -1 for x less than y.</returns>
        public int Compare(object? x, object? y)
        {
            _ = x ?? throw new ArgumentNullException(nameof(x));
            _ = y ?? throw new ArgumentNullException(nameof(y));

            decimal lhs = Convert.ToDecimal(x);
            decimal rhs = Convert.ToDecimal(y);

            return lhs == rhs ? 0 : lhs < rhs ? -1 : 1;

        }

        #endregion IComparer Members
    }
}
