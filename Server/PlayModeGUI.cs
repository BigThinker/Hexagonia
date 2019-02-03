using UnityEngine;
using UnityEngine.UI;

public class PlayModeGUI : MonoBehaviour
{
    public Text FrameRate;
    public Text Mocap;
    public Text MocapDeltaCompression;
    public Text DeflateSystemIOCompression;
    public Text DeflateUnityIOCompression;
    public Text VoIP;
    public Text Tiles;
    public Text TilesLowerRate;
    public Text Total;
    public Text TotalOptimized;

    public void SetFrameRateText(int value)
    {
        FrameRate.text = "Frame rate: " + value;
    }

    public void SetMocapText(int dataPerFrame, int dataPerSecond)
    {
        Mocap.text = "Mocap: " + dataPerFrame + " bytes (" + dataPerSecond + " bytes/s)";
    }

    public void SetMocapDeltaCompressionText(int dataPerFrame, int dataPerSecond)
    {
        MocapDeltaCompression.text = "- Delta compression: " + dataPerFrame + " bytes (" + dataPerSecond + " bytes/s)";
    }

    public void SetDeflateSystemCompressionText(int dataPerFrame, int dataPerSecond)
    {
        DeflateSystemIOCompression.text = "- Deflate (System.IO.Compression): " + dataPerFrame + " bytes (" + dataPerSecond + " bytes/s)";
    }

    public void SetDeflateUnityCompressionText(int dataPerFrame, int dataPerSecond)
    {
        DeflateUnityIOCompression.text = "- Deflate (Unity.IO.Compression): " + dataPerFrame + " bytes (" + dataPerSecond + " bytes/s)";
    }

    public void SetVoIPText(string value)
    {
        VoIP.text = "VoIP: " + value;
    }

    public void SetTilesText(int dataPerSecond, int numTiles, int numPlayers)
    {
        Tiles.text = "Tiles: " + dataPerSecond + " bytes/s (" + numTiles + " tiles to " + numPlayers + " players)";
    }

    public void SetTilesLowerRateText(int dataPerSecond)
    {
        TilesLowerRate.text = "Tiles (lower rate): " + dataPerSecond + " bytes/s";
    }

    public void SetTotalText(int dataPerSecond)
    {
        Total.text = "Total: " + dataPerSecond + " bytes/s";
    }

    public void SetTotalOptimizedText(int dataPerSecond)
    {
        TotalOptimized.text = "Total (optimized): " + dataPerSecond + " bytes/s";
    }
}
