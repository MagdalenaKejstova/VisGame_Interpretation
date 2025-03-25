namespace Helpers.Data
{
    public class ChartData
    {
        public ChartData(XAxisData xAxisData, YAxisData yAxisData)
        {
            XAxisData = xAxisData;
            YAxisData = yAxisData;
        }

        public XAxisData XAxisData { get; set; }
        public YAxisData YAxisData { get; set; }
    }
}