using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePoint.CarManagement.Domain.Models.TrafficViolation
{
    public class ExportedFileResult
    {
        public byte[] FileContents { get; set; }
        public string ContentType { get; set; }
        public string FileDownloadName { get; set; }
    }
}
