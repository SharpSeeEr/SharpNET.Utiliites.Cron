using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpNET.Utilities.Cron
{
    interface ICronField
    {
        /// <summary>
        /// The raw string value of the field
        /// </summary>
        string Value { get; }

        /// <summary>
        /// Gets the first valid value that satisfies the field
        /// </summary>
        /// <returns>The first valid value</returns>
        int GetFirst();

        /// <summary>
        /// Gets the last valid value that satisfies the field
        /// </summary>
        int GetLast();

        /// <summary>
        /// Retrieves the next valid value for the field that is greater
        /// than the value passed in.
        /// </summary>
        /// <param name="start">Value to start checking at</param>
        /// <returns>Next valid value, or -1 if none found</returns>
        int GetNext(int after);

        /// <summary>
        /// Retrieves the previous valid value for the field that is less
        /// than or equal to the value passed in.
        /// </summary>
        /// <param name="before">Start value</param>
        /// <returns>Previous valid value, or -1 if none found.</returns>
        int GetPrev(int before);

        bool IsValid(int value_);
    }
}
