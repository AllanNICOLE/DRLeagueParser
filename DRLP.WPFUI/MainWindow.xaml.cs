using DRLP.Data;
using DRLP.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DRLP.WPFUI
{
    public partial class MainWindow : Window
    {
        private Rally rallyData = new Rally();
        private RacenetApiParser racenetApiParser = new RacenetApiParser(Properties.Settings.Default.ApiURL);

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            label_statusMessage.Content = "";
        }

        // enum for the type of output to print
        public enum PrintType
        {
            Stages,
            Overall,
            Chart
        }

        // holds the user selected print type
        public PrintType SelectedPrintType { get; set; }
        public Dictionary<PrintType, string> PrintTypeDict { get; set; } = new Dictionary<PrintType, string>
        {
            { PrintType.Stages, "Stages" },
            { PrintType.Overall, "Overall" },
            { PrintType.Chart, "Chart" }
        };

        #region Clicks btn
        private void button_clearAllData_Click(object sender, RoutedEventArgs e)
        {
            rallyData = new Rally();
            label_statusMessage.Content = "Data Cleared";
            label_statusMessage.Foreground = Brushes.Green;
            textBox_resultsInput.Clear();
        }



        private void button_print_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPrintType == PrintType.Stages)
                printStageTimes();
            else if (SelectedPrintType == PrintType.Overall)
                printOverallTimes();
            else if (SelectedPrintType == PrintType.Chart)
                printChartOutput();
        }

        private void button_copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(textBox_resultsInput.Text);
            label_statusMessage.Content = "Data Copied to Clipboard";
            label_statusMessage.Foreground = Brushes.Green;
        }

        private async void button_parseRacenetApi_Click(object sender, RoutedEventArgs e)
        {
            // check and clean inputs
            var eventId = textBox_eventId.Text;
            if (String.IsNullOrWhiteSpace(eventId))
            {
                label_statusMessage.Content = "No event ID";
                label_statusMessage.Foreground = Brushes.Red;
                return;
            }

            int eventIdInt;
            if (!Int32.TryParse(eventId, out eventIdInt))
            {
                label_statusMessage.Content = "Event ID must be an integer";
                label_statusMessage.Foreground = Brushes.Red;
                return;
            }

            eventId = eventId.Trim();

            // clear all data
            rallyData = new Rally();
            label_statusMessage.Content = "Data Cleared";
            label_statusMessage.Foreground = Brushes.Green;
            textBox_resultsInput.Clear();

            // progess reporting from task
            var progress = new Progress<int>(stagesProcessed =>
            {
                label_statusMessage.Content = "Fetching data from Racenet... " + stagesProcessed + " stages retrieved ";
                label_statusMessage.Foreground = Brushes.Green;
            });

            // run task to get data
            var getDataTask = Task<Rally>.Factory.StartNew(() => racenetApiParser.GetRallyData(eventId, progress));

            label_statusMessage.Content = "Fetching data from Racenet (be patient, Racenet is slow)...";
            label_statusMessage.Foreground = Brushes.Green;

            await getDataTask;

            rallyData = getDataTask.Result;

            // crunch numbers
            if (rallyData == null)
            {
                label_statusMessage.Content = "Failed to get data from Racenet";
                label_statusMessage.Foreground = Brushes.Red;
            } else if (rallyData.StageCount == 0)
            {
                label_statusMessage.Content = "No event found. Please check the EventID";
                label_statusMessage.Foreground = Brushes.Orange;
            }
            else
            {
                rallyData.CalculateTimes();
                label_statusMessage.Content = rallyData.StageCount + " stages retrieved from Racenet, numbers crunched sucessfully";
                label_statusMessage.Foreground = Brushes.Green;
                printStageTimes(); 
                comboBox_printType.SelectedValue = PrintType.Stages;
            }
        }

        #endregion

        #region Prints btn
        private void printOverallTimes()
        {
            var outputSB = new StringBuilder();

            int stageCount = 1;
            foreach (Stage stage in rallyData)
            {
                outputSB.AppendLine("SS" + stageCount);
                outputSB.AppendLine("Overall");
                outputSB.AppendLine("Pos, Pos Chng, PlayerID, Name, Vehicle, Time, Diff 1st, Diff Prev");

                List<KeyValuePair<string, DriverTime>> sortedStageData = stage.DriverTimes.ToList();
                sortedStageData.Sort((x, y) =>
                {
                    if (x.Value != null && y.Value == null)
                        return -1;
                    else if (x.Value == null && y.Value != null)
                        return 1;
                    else if (x.Value == null && y.Value == null)
                        return 0;
                    else
                        return x.Value.CalculatedOverallTime.CompareTo(y.Value.CalculatedOverallTime);
                });

                foreach (KeyValuePair<string, DriverTime> driverTimeKvp in sortedStageData)
                {
                    var driverName = driverTimeKvp.Key;
                    var driverTime = driverTimeKvp.Value;

                    if (driverTime != null)
                    {
                        var formatString = @"hh\:mm\:ss\.fff";
                        var line = driverTime.OverallPosition + "," +
                                   driverTime.CalculatedPositionChange + "," +
                                   driverTime.PlayerID + "," +
                                   driverTime.DriverName + "," +
                                   driverTime.Vehicle + "," +
                                   driverTime.CalculatedOverallTime.ToString(formatString) + "," +
                                   driverTime.CalculatedOverallDiffFirst.ToString(formatString) + "," +
                                   driverTime.CalculatedOverallDiffPrevious.ToString(formatString);

                        outputSB.AppendLine(line);
                    }
                    else
                    {
                        outputSB.AppendLine(",,," + driverName + ",,DNF");
                    }
                }

                outputSB.AppendLine("");
                stageCount++;
            }

            label_statusMessage.Content = "Displaying Overall Times";
            label_statusMessage.Foreground = Brushes.Green;
            textBox_resultsInput.Text = outputSB.ToString();
        }

        private void printStageTimes()
        {
            var outputSB = new StringBuilder();

            int stageCount = 1;

            foreach (Stage stage in rallyData)
            {
                outputSB.AppendLine("SS" + stageCount);
                outputSB.AppendLine("Stage");
                outputSB.AppendLine("Pos, PlayerID, Name, Vehicle, Time, Diff 1st, Diff Prev");

                List<KeyValuePair<string, DriverTime>> sortedStageData = stage.DriverTimes.ToList();
                sortedStageData.Sort((x, y) =>
                {
                    if (x.Value != null && y.Value == null)
                        return -1;
                    else if (x.Value == null && y.Value != null)
                        return 1;
                    else if (x.Value == null && y.Value == null)
                        return 0;
                    else
                        return x.Value.CalculatedStageTime.CompareTo(y.Value.CalculatedStageTime);
                });

                foreach (KeyValuePair<string, DriverTime> driverTimeKvp in sortedStageData)
                {
                    var driverName = driverTimeKvp.Key;
                    var driverTime = driverTimeKvp.Value;

                    if (driverTime != null)
                    {
                        var formatString = @"mm\:ss\.fff";
                        var line = driverTime.CaclulatedStagePosition + "," +
                                   driverTime.PlayerID + "," +
                                   driverTime.DriverName + "," +
                                   driverTime.Vehicle + "," +
                                   driverTime.CalculatedStageTime.ToString(formatString) + "," +
                                   driverTime.CalculatedStageDiffFirst.ToString(formatString) + "," +
                                   driverTime.CalculatedStageDiffPrevious.ToString(formatString);

                        outputSB.AppendLine(line);
                    }
                    else
                    {
                        outputSB.AppendLine(",," + driverName + ",,DNF");
                    }
                }

                outputSB.AppendLine("");
                stageCount++;
            }
            label_statusMessage.Content = "Displaying Stage Times";
            label_statusMessage.Foreground = Brushes.Green;
            textBox_resultsInput.Text = outputSB.ToString();
        }

        private void printChartOutput()
        {
            var outputSB = new StringBuilder();
            var positionDict = new Dictionary<string, List<int>>();
            List<KeyValuePair<string, DriverTime>> sortedStageData = null;

            foreach (Stage stage in rallyData)
            {
                sortedStageData = stage.DriverTimes.ToList();
                sortedStageData.Sort((x, y) =>
                {
                    if (x.Value != null && y.Value == null)
                        return -1;
                    else if (x.Value == null && y.Value != null)
                        return 1;
                    else if (x.Value == null && y.Value == null)
                        return 0;
                    else
                        return x.Value.CalculatedOverallTime.CompareTo(y.Value.CalculatedOverallTime);
                });

                foreach (KeyValuePair<string, DriverTime> driverTimeKvp in sortedStageData)
                {
                    if (driverTimeKvp.Value == null)
                        continue;

                    var driverKey = driverTimeKvp.Key;
                    var position = driverTimeKvp.Value.OverallPosition;

                    if (!positionDict.ContainsKey(driverKey))
                        positionDict.Add(driverKey, new List<int>());

                    positionDict[driverKey].Add(position);
                }
            }

            label_statusMessage.Content = "Displaying Chart Output";
            label_statusMessage.Foreground = Brushes.Green;


            // sortedStageData should contain the last stage, sorted by overall time
            if (sortedStageData == null)
                return;

            foreach (KeyValuePair<string, DriverTime> driverTimeKvp in sortedStageData)
            {
                var driverKey = driverTimeKvp.Key;
                var positionList = positionDict[driverKey];

                string line = driverKey + "," + String.Join(",", positionList);

                outputSB.AppendLine(line);
            }

            textBox_resultsInput.Text = outputSB.ToString();
        }
        #endregion
    }
}
