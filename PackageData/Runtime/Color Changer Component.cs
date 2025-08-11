using UnityEngine;
using ColorChanger.Models;

public class ColorChangerForUnity : MonoBehaviour
{
    public Texture2D targetTexture;

    public Color previousColor;
    public Color newColor;
    public BalanceModeConfiguration balanceModeConfiguration = new BalanceModeConfiguration();
    public AdvancedColorConfiguration advancedColorConfiguration = new AdvancedColorConfiguration();
}
