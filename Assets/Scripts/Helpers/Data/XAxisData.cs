using System;
using System.Collections.Generic;

namespace Helpers.Data
{
    public class XAxisData
    {
        public XAxisData(List<DateTime> dates, string format = "d.M.yyyy")
        {
            Format = format;
            Dates = dates;
        }

        public string Format { get; set; }
        public List<DateTime> Dates { get; set; }
    }
}