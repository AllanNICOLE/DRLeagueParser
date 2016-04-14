﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DRTimeCruncher
{
    /// <summary>
    /// Holds all stage times
    /// </summary>
    public class Rally : IEnumerable
    {
		public int StageCount { get { return stages.Count; } }

        private List<Stage> stages = new List<Stage>();

        /// <summary>
        /// Adds a stage to the rally's stage collection
		/// Enumerating returns Stages
        /// </summary>
        /// <returns>true</returns>
        public bool AddStage(Stage stage)
        {
            stages.Add(stage);
            return true;
        }

        public bool CalculateTimes()
        {
			// create lookup tables for comparing times
            var previousStage = new Dictionary<string, DriverTime>();
            var currentStage = new Dictionary<string, DriverTime>();

			previousStage = stages[0].DriverTimes;

			CalculateDeltas(stages[0], true);

			// for each stage after SS1
			for (int i = 1; i < stages.Count; i++)
			{
				currentStage = stages[i].DriverTimes;

				// for each driver on the previous stage
				foreach (KeyValuePair<string,DriverTime> previousDriverTimeKvp in previousStage)
				{
					// get the current time and compute the stage time and position change
					DriverTime currentDriverTime;
					if (true == currentStage.TryGetValue(previousDriverTimeKvp.Key, out currentDriverTime))
					{
						if (currentDriverTime != null)
						{
							currentDriverTime.CalculatedStageTime = currentDriverTime.CalculatedOverallTime - previousDriverTimeKvp.Value.CalculatedOverallTime;
							currentDriverTime.CalculatedPositionChange = previousDriverTimeKvp.Value.OverallPosition - currentDriverTime.OverallPosition;
						}
					}
					else
					{
						currentStage.Add(previousDriverTimeKvp.Key, null); // track DNFs
					}
				}

				previousStage = currentStage;

				CalculateDeltas(stages[i], false);
			}

            return true;
        }

		private void CalculateDeltas(Stage currentStage, bool isFirstStage)
		{
			// order by overall time and calculate differences
			TimeSpan fastestOverallTime;
			TimeSpan previousOverallTime;
			TimeSpan fastestStageTime;
			TimeSpan previousStageTime;
			foreach (DriverTime driverTime in currentStage.DriverTimes.Values.Where(x => x != null).OrderBy(x => x.CalculatedOverallTime))
			{
				if (driverTime == null)
					continue;

				if (driverTime.OverallPosition == 1)
				{
					fastestOverallTime = driverTime.CalculatedOverallTime;
					previousOverallTime = driverTime.CalculatedOverallTime;

					if (isFirstStage == true)
					{
						driverTime.CaclulatedStagePosition = driverTime.OverallPosition;
						driverTime.CalculatedStageTime = driverTime.CalculatedOverallTime;
					}

					continue;
				}

				driverTime.CalculatedOverallDiffFirst = driverTime.CalculatedOverallTime - fastestOverallTime;
				driverTime.CalculatedOverallDiffPrevious = driverTime.CalculatedOverallTime - previousOverallTime;

				if (isFirstStage == true)
				{
					driverTime.CaclulatedStagePosition = driverTime.OverallPosition;
					driverTime.CalculatedStageTime = driverTime.CalculatedOverallTime;
					driverTime.CalculatedStageDiffFirst = driverTime.CalculatedOverallDiffFirst;
					driverTime.CalculatedStageDiffPrevious = driverTime.CalculatedOverallDiffPrevious;
				}

				previousOverallTime = driverTime.CalculatedOverallTime;
			}

			// order by stage time and calculate differences
			if (isFirstStage == false)
			{
				int stagePosition = 1;
				foreach (DriverTime driverTime in currentStage.DriverTimes.Values.Where(x => x != null).OrderBy(x => x.CalculatedStageTime))
				{
					if (driverTime == null)
					{
						stagePosition++;
						continue;
					}
					if (stagePosition == 1)
					{
						fastestStageTime = driverTime.CalculatedStageTime;
						previousStageTime = driverTime.CalculatedStageTime;
						driverTime.CaclulatedStagePosition = stagePosition;
						stagePosition++;
						continue;
					}

					driverTime.CalculatedStageDiffFirst = driverTime.CalculatedStageTime - fastestStageTime;
					driverTime.CalculatedStageDiffPrevious = driverTime.CalculatedStageTime - previousStageTime;
					driverTime.CaclulatedStagePosition = stagePosition;
					previousStageTime = driverTime.CalculatedStageTime;
					stagePosition++;
				}
			}
		}

		public IEnumerator GetEnumerator()
		{
			return ((IEnumerable)stages).GetEnumerator();
		}
	}

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
			DriverTimes.Add(driverTime.Driver, driverTime);
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
    }

    /// <summary>
    /// Holds the data for a single driver on a single stage
    /// </summary>
    public class DriverTime
    {
        // raw data supplied by parser
        public int OverallPosition { get; private set; }
        public string Tags { get; private set; }
        public string Driver { get; private set; }
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
        public DriverTime(int overallPosition, string driver, string vehicle, string overallTime, string overallDiffFirst, string tags = null)
        {
            OverallPosition = overallPosition;
            Tags = tags;
            Driver = driver;
            Vehicle = vehicle;
            OverallTime = overallTime;
            OverallDiffFirst = overallDiffFirst;

			CalculatedOverallTime = TimeSpan.ParseExact(OverallTime, @"mm\:ss\.fff", CultureInfo.InvariantCulture); // TODO: this parsing code should not be here (sparation of concerns)
        }
    }
}