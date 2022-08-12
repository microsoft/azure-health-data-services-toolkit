using System.Collections;
using Newtonsoft.Json;

namespace Capl.Operations
{
    /// <summary>
    ///     Compares two DateTime types by UTC.
    /// </summary>
    public class DateTimeComparer : IComparer
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

            DateTime lhs = GetDateTime(x);
            DateTime rhs = GetDateTime(y);

            return lhs == rhs ? 0 : lhs < rhs ? -1 : 1;
        }

        private static DateTime GetDateTime(object dateTime)
        {
            try
            {
                return DateTime.Parse((string)dateTime);
            }
            catch
            {
                return JsonConvert.DeserializeObject<DateTime>((string)dateTime);
            }
        }

        #endregion IComparer Members
    }
}
