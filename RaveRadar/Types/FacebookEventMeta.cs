using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RaveRadar.Types
{
  public class FacebookEventMeta : IEquatable<FacebookEventMeta>
  {
    public Int64 EID { get; set; }
    public Int64 Creator { get; set; }
    public DateTime UpdateTime { get; set; }

    public bool Equals(FacebookEventMeta other)
    {

      // Check whether the compared object is null.
      if (Object.ReferenceEquals(other, null)) return false;

      // Check whether the compared object references the same data.
      if (Object.ReferenceEquals(this, other)) return true;

      // Check whether the objects’ properties are equal.
      return EID.Equals(other.EID);
    }

    public override int GetHashCode()
    {
      return EID.GetHashCode();
    }
  }
}