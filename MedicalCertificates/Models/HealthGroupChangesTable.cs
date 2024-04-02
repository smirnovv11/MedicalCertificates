using System;
using System.Collections.Generic;

namespace MedicalCertificates.Models
{
    public partial class HealthGroupChangesTable
    {
        public int StudentId { get; set; }
        public string CurrHealth { get; set; } = null!;
        public string CurrPe { get; set; } = null!;
        public string PrevHealth { get; set; } = null!;
        public string PrevPe { get; set; } = null!;
        public DateTime Date { get; set; }

        public virtual StudentsTable Student { get; set; } = null!;
    }
}
