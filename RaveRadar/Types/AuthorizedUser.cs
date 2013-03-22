using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaveRadar.Types
{
    [Table("AuthorizedUsers")]
    public class AuthorizedUser
    {
        #region Public Members
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Int64 UserID { get; set; }

        [MaxLength(512)]
        public string AccessToken { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }

        public DateTime UpdateTime { get; set; }
        #endregion

        #region Constructors
        public AuthorizedUser(Int64 id, string accessToken, string name)
        {
            UserID = id;
            AccessToken = accessToken;
            Name = name;
        }
        public AuthorizedUser(Int64 id, string accessToken, string name, DateTime updateTime)
        {
            UserID = id;
            AccessToken = accessToken;
            Name = name;
            UpdateTime = updateTime;
        }
        #endregion
    }
}
