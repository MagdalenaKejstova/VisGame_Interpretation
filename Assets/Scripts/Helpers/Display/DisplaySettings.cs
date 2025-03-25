namespace Helpers
{
    public class DisplaySettings
    {
        public DisplaySettings(bool drawAxisLabels = true, bool drawDataPoints = true, bool drawGridlines = true,
            int xAxisLabelsCount = 8, int yAxisLabelsCount = 8)
        {
            DrawAxisLabels = drawAxisLabels;
            DrawDataPoints = drawDataPoints;
            DrawGridlines = drawGridlines;
            XAxisLabelsCount = xAxisLabelsCount;
            YAxisLabelsCount = yAxisLabelsCount;
        }

        public bool DrawAxisLabels { get; set; }
        public bool DrawDataPoints { get; set; }
        public bool DrawGridlines { get; set; }
        public int XAxisLabelsCount { get; set; }
        public int YAxisLabelsCount { get; set; }
    }
}