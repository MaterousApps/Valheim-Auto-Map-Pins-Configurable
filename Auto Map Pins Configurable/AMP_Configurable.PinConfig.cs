using System;
using UnityEngine;

namespace AMP_Configurable.PinConfig
{
  [Serializable]
  public class PinType
  {
    public int type = 0;
    public string label;
    public string icon;
    public Sprite sprite = null;
    public int size = 20;
    public int minimapSize = 0;
    public string[] object_ids;
    public Minimap.PinData minimapPin = null;
    public bool isPinned = false;
    public string pinCat = "";
  }

  [Serializable]
  public class PinConfig
  {
    public PinType[] pins;
  }
}
