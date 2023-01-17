using System;
using UnityEngine;

namespace AMP_Configurable.PinConfig
{
  [Serializable]
  public class PinType
  {
    public int type;
    public string label;
    public string icon;
    public Sprite sprite;
    public int size;
    public string[] object_ids;
  }

  [Serializable]
  public class PinConfig
  {
    public PinType[] pins;
  }
}
