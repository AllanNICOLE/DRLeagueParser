using System;
using System.Globalization;

namespace DRLP.Data
{
    /// <summary>
    /// Holds the data for a single driver on a single stage
    /// </summary>
    public class DriverTime
    {
        // raw data supplied by parser
        public int OverallPosition { get; private set; }
        public string Tags { get; private set; }
        public int PlayerID { get; private set; }
        public string DriverName { get; private set; }
        public string Vehicle { get; private set; }
        public string OverallTime { get; private set; }
        public string OverallDiffFirst { get; private set; }

        // calculated overall data
        public TimeSpan CalculatedOverallTime { get; internal set; }
        public TimeSpan CalculatedOverallDiffPrevious { get; internal set; }
        public TimeSpan CalculatedOverallDiffFirst { get; internal set; }
        public int CalculatedPositionChange { get; internal set; }

        // calculated stage data
        public int CaclulatedStagePosition { get; internal set; }
        public TimeSpan CalculatedStageTime { get; internal set; }
        public TimeSpan CalculatedStageDiffPrevious { get; internal set; }
        public TimeSpan CalculatedStageDiffFirst { get; internal set; }

        /// <summary>
        /// Creates a new DriverData object that represents a single driver's time on a single stage.
        /// Data is the 'raw' string data taken from the results
        /// Tags are optional (not all drivers will have tags)
        /// </summary>
        public DriverTime(int overallPosition, int playerID, string driverName, string vehicle, string overallTime, string overallDiffFirst, string tags = null)
        {
            OverallPosition = overallPosition;
            Tags = tags;
            PlayerID = playerID;
            DriverName = driverName;
            Vehicle = vehicle;
            OverallTime = overallTime;
            OverallDiffFirst = overallDiffFirst;

            // TODO: this parsing code should not be here (sparation of concerns)
            TimeSpan parsedOverallTime;

            if (TimeSpan.TryParseExact(OverallTime, @"mm\:ss\.fff", CultureInfo.InvariantCulture, out parsedOverallTime))
                CalculatedOverallTime = parsedOverallTime;
            else if (TimeSpan.TryParseExact(OverallTime, @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture, out parsedOverallTime))
                CalculatedOverallTime = parsedOverallTime;
            else
                throw new ArgumentException("Could not parse overall time");
        }
    }
}
