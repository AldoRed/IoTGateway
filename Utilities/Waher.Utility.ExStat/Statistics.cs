﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Utility.ExStat
{
    public class Statistics
    {
        public Histogram<string> PerType = new Histogram<string>();
        public Histogram<string> PerMessage = new Histogram<string>();
        public Histogram<string> PerSource = new Histogram<string>();
        public Histogram<DateTime> PerHour = new Histogram<DateTime>();
        public Histogram<DateTime> PerDay = new Histogram<DateTime>();
        public Histogram<DateTime> PerMonth = new Histogram<DateTime>();
    }
}
