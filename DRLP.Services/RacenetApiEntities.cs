using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DRLP.Services
{
    // JSON classes for deserialization
    [DataContract]
    public class Restriction
    {
        [DataMember]
        public List<string> VehicleClass { get; set; }
    }

    [DataContract]
    public class Entry
    {
        [DataMember]
        public int Position { get; set; }
        [DataMember]
        public string NationalityImage { get; set; }
        [DataMember]
        public bool IsFounder { get; set; }
        [DataMember]
        public bool IsVIP { get; set; }
        [DataMember]
        public bool HasGhost { get; set; }
        [DataMember]
        public int PlayerId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string VehicleName { get; set; }
        [DataMember]
        public string Time { get; set; }
        [DataMember]
        public string DiffFirst { get; set; }
        [DataMember]
        public int PlayerDiff { get; set; }
        [DataMember]
        public int TierID { get; set; }
        [DataMember]
        public string ProfileUrl { get; set; }
    }

    [DataContract]
    public class RacenetRallyData
    {
        [DataMember]
        public string EventName { get; set; }
        [DataMember]
        public int TotalStages { get; set; }
        [DataMember]
        public bool ShowStageInfo { get; set; }
        [DataMember]
        public object LocationName { get; set; }
        [DataMember]
        public object LocationImage { get; set; }
        [DataMember]
        public object StageName { get; set; }
        [DataMember]
        public object StageImage { get; set; }
        [DataMember]
        public object TimeOfDay { get; set; }
        [DataMember]
        public object WeatherImageUrl { get; set; }
        [DataMember]
        public object WeatherImageAltUrl { get; set; }
        [DataMember]
        public object WeatherText { get; set; }
        [DataMember]
        public Restriction Restriction { get; set; }
        [DataMember]
        public bool EventRestart { get; set; }
        [DataMember]
        public bool StageRetry { get; set; }
        [DataMember]
        public bool HasServiceArea { get; set; }
        [DataMember]
        public bool AllowCareerEngineers { get; set; }
        [DataMember]
        public bool OnlyOwnedVehicles { get; set; }
        [DataMember]
        public bool AllowVehicleTuning { get; set; }
        [DataMember]
        public bool IsCheckpoint { get; set; }
        [DataMember]
        public int Page { get; set; }
        [DataMember]
        public int Pages { get; set; }
        [DataMember]
        public int LeaderboardTotal { get; set; }
        [DataMember]
        public object PlayerEntry { get; set; }
        [DataMember]
        public List<Entry> Entries { get; set; }
        [DataMember]
        public bool FiltersEnabled { get; set; }
        [DataMember]
        public bool IsWagerEvent { get; set; }
    }
}
