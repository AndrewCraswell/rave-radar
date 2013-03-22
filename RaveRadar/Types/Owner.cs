using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaveRadar.Types
{

    [Table("Owners")]
    public class Owner
    {
        #region Public Members
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Int64 OwnerID { get; set; }

        [MaxLength(512)]
        public string AccessToken { get; set; }
        
        [MaxLength(50)]
        public string Name { get; set; }

        public Boolean IsCompany { get; set; }

        public Boolean IsTrusted { get; set; }
        #endregion

        #region Constructors
        public Owner()
        {
        }
        
        public Owner(Int64 id, string name, string accessToken, bool isCompany = false, bool isTrusted = false)
        {
            OwnerID = id;
            Name = name.Trim();
            AccessToken = accessToken;
            IsCompany = isCompany;
            IsTrusted = isTrusted;
        }
        #endregion

    }
}