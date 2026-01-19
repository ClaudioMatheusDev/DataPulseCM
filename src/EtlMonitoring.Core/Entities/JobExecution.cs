using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtlMonitoring.Core.Entities
{
    public class JobExecution
    {
        public long ExecutionID { get; set; }
        public string JobName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string ErrorMessage { get; set; }

        public double? DurationInSeconds
        {
            get
            {
                return (EndDate - StartDate).TotalSeconds;
            }
        }

    }
}
