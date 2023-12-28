using System;
using System.Collections.Generic;

namespace MedicalCertificates.Models
{
    public partial class HealthGroupTable
    {
        public HealthGroupTable()
        {
            CertificatesTables = new HashSet<CertificatesTable>();
        }

        public int HealthGroupId { get; set; }
        public string HealthGroup { get; set; } = null!;

        public virtual ICollection<CertificatesTable> CertificatesTables { get; set; }
    }
}
