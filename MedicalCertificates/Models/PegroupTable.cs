using System;
using System.Collections.Generic;

namespace MedicalCertificates.Models
{
    public partial class PegroupTable
    {
        public PegroupTable()
        {
            CertificatesTables = new HashSet<CertificatesTable>();
        }

        public int PegroupId { get; set; }
        public string Pegroup { get; set; } = null!;

        public virtual ICollection<CertificatesTable> CertificatesTables { get; set; }
    }
}
