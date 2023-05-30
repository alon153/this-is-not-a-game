using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

public class RingTrigger : MonoBehaviour
{
  #region Serialized Fields
  
  #endregion
  #region Non-Serialized Fields

  private readonly HashSet<RhythmRing> _rings = new();

  #endregion

  #region Properties

  #endregion

  #region Function Events

  #endregion

  #region Public Methods

  public void RegisterRing(RhythmRing ring)
  {
    if(!_rings.Contains(ring))
      _rings.Add(ring);
  }
  
  public void UnregisterRing(RhythmRing ring)
  {
    if(_rings.Contains(ring))
      _rings.Remove(ring);
  }

  public bool Beat()
  {
    if(_rings.Count == 0) 
      return false;
    
    var ring = _rings.OrderBy((rhythmRing => rhythmRing.transform.localScale.magnitude)).First();
    ring.ResetRing();
    return true;
  }

  #endregion

  #region Private Methods

  #endregion
}

