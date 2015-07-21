using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScheduleParser
{
    class SessionInfo
    {
        public string MovieName { get; set; }
        public bool Is3D { get; set; }
        public string PlaceName { get; set; }
        public DateTime Time { get; set; }

        internal SessionInfo Clone()
        {
            return new SessionInfo()
            {
                MovieName = this.MovieName,
                Is3D = this.Is3D,
                PlaceName = this.PlaceName,
                Time = this.Time
            };
        }
    }
}
