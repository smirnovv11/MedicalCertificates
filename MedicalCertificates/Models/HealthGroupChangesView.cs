using System;
using System.Collections.Generic;

namespace MedicalCertificates.Models
{
    public partial class HealthGroupChangesView
    {
        public string SecondName { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string? ThirdName { get; set; }
        public string GroupName { get; set; } = null!;
        public string CurrHealth { get; set; } = null!;
        public string CurrPe { get; set; } = null!;
        public string PrevHealth { get; set; } = null!;
        public string PrevPe { get; set; } = null!;
        public DateTime UpdateDate { get; set; }
    }
}
