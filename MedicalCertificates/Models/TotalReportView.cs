﻿using System;
using System.Collections.Generic;

namespace MedicalCertificates.Models
{
    public partial class TotalReportView
    {
        public long? RowNum { get; set; }
        public string FirstName { get; set; } = null!;
        public string SecondName { get; set; } = null!;
        public string? ThirdName { get; set; }
        public string GroupName { get; set; } = null!;
        public string Pegroup { get; set; } = null!;
        public string HealthGroup { get; set; } = null!;
        public DateTime IssueDate { get; set; }
        public DateTime ValidDate { get; set; }
        public int Course { get; set; }
        public string Department { get; set; } = null!;
    }
}
