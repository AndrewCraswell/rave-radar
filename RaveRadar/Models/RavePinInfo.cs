using System;

namespace RaveRadar.Models
{
    public class RavePinInfo
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public Int64? RaveID { get; set; }

        public Int64? OwnerID { get; set; }

        public Int64? VenueID { get; set; }

        public string Location { get; set; }

        public string OwnerName { get; set; }

        public string PicURL { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public bool IsDateOnly { get; set; }

        public string DateText { get; set; }

        public string TimeText { get; set; }
    }
}