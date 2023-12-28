using System;
using System.Collections.Generic;

namespace MedicalCertificates.Models
{
    public partial class CertificatesTable
    {
        public int CertificateId { get; set; }
        public int StudentId { get; set; }
        public int HealthGroupId { get; set; }
        public int PegroupId { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ValidDate { get; set; }
        public string? Note { get; set; }

        public virtual HealthGroupTable HealthGroup { get; set; } = null!;
        public virtual PegroupTable Pegroup { get; set; } = null!;
        public virtual StudentsTable Student { get; set; } = null!;
    }
}
