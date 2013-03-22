using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaveRadar.Types
{
    [Table("Raves")]
    public class Rave
    {
        #region Public Members

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public Int64 RaveID { get; set; }

        public Int64 OwnerID { get; set; }

        public Int64? VenueID { get; set; }

        [MaxLength(256)]
        public string Name { get; set; }

        [MaxLength(256)]
        public string PicURL { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public Boolean IsDateOnly { get; set; }

        [MaxLength(50)]
        public string Location { get; set; }

        public Boolean IsApproved { get; set; }

        public Int64? SubmitterID { get; set; }

        public DateTime UpdateTime { get; set; }
        #endregion

        public void SetPicUrlToDefaultIcon()
        {
            PicURL = ConfigurationManager.AppSettings["RaveRadarDomain"] + ConfigurationManager.AppSettings["DefaultRaveIcon"];
        }
    }
}