using System.Collections;
using System.Collections.Generic;

namespace DRLP.Data
{
    /// <summary>
    /// Holds all driver times for a single stage
	/// Enumeration returns DriverTimes
    /// </summary>
    public class Stage : IEnumerable
    {
        public Dictionary<string, DriverTime> DriverTimes { get; private set; }

        /// <summary>
        /// Adds a driver's stage results to the stage's collection
        /// </summary>
        /// <returns>true</returns>
        public bool AddDriver(DriverTime driverTime)
        {
            DriverTimes.Add(driverTime.DriverName, driverTime);
            return true;
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)DriverTimes).GetEnumerator();
        }

        public Stage()
        {
            DriverTimes = new Dictionary<string, DriverTime>();
        }

        public int Count
        {
            get { return DriverTimes.Count; }
        }
    }
}
