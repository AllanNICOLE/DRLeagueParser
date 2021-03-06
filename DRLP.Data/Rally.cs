﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DRLP.Data
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
            if (stages.Count < 1)
                return true;

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
                foreach (KeyValuePair<string, DriverTime> previousDriverTimeKvp in previousStage)
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
            // order by overall time and calculate overall time deltas
            TimeSpan fastestOverallTime = new TimeSpan();
            TimeSpan previousOverallTime = new TimeSpan();
            TimeSpan fastestStageTime = new TimeSpan();
            TimeSpan previousStageTime = new TimeSpan();
            bool firstDriverProcessed = false;

            foreach (DriverTime driverTime in currentStage.DriverTimes.Values.Where(x => x != null).OrderBy(x => x.CalculatedOverallTime))
            {
                if (driverTime == null)
                    continue;

                if (driverTime.OverallPosition == 1)
                {
                    fastestOverallTime = driverTime.CalculatedOverallTime;
                    previousOverallTime = driverTime.CalculatedOverallTime;
                    firstDriverProcessed = true;

                    if (isFirstStage == true)
                    {
                        driverTime.CaclulatedStagePosition = driverTime.OverallPosition;
                        driverTime.CalculatedStageTime = driverTime.CalculatedOverallTime;
                    }

                    continue;
                }

                // error - if the first driver has not been processed at this point,
                // we can't calculate deltas for the other drivers.
                if (firstDriverProcessed == false)
                    return;

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

            // reset error flag for reuse
            firstDriverProcessed = false;

            // order by stage time and calculate stage time deltas
            if (isFirstStage == false)  // skip for the first stage, there is no stage delta to calculate
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
                        firstDriverProcessed = true;
                        continue;
                    }

                    // error - if the first driver has not been processed at this point,
                    // we can't calculate deltas for the other drivers.
                    if (firstDriverProcessed == false)
                        return;

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
}
