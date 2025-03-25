using System.Collections.Generic;

namespace Helpers.Data
{
    public class YAxisData
    {
        public YAxisData(List<float> data)
        {
            Data = data;
        }

        
        public List<float> Data { get; set; }
    }
}