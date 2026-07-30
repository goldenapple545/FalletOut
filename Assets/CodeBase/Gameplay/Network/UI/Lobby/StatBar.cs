using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.Gameplay.Network.UI
{
    /// <summary>
    /// Displays a stat as a segmented fill bar.
    /// Value range: 0–100, divided into 10 cells (10 units each).
    /// </summary>
    public sealed class StatBar : MonoBehaviour
    {
        [SerializeField] private Image[] cells;
        [SerializeField] private Color filledColor = Color.white;
        [SerializeField] private Color emptyColor = new Color(1f, 1f, 1f, 0.2f);

        /// <summary>
        /// Sets the fill level. Value 0–100 maps to 0–10 filled cells.
        /// </summary>
        public void SetValue(float value)
        {
            int filledCount = Mathf.Clamp(Mathf.RoundToInt(value / 10f), 0, 10);

            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] == null) continue;

                cells[i].color = i < filledCount ? filledColor : emptyColor;
            }
        }
    }
}
