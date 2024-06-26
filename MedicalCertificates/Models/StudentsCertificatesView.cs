﻿using System;
using System.Collections.Generic;

namespace MedicalCertificates.Models
{
    public partial class StudentsCertificatesView
    {
        public int StudentId { get; set; }
        public string FirstName { get; set; } = null!;
        public string SecondName { get; set; } = null!;
        public string? ThirdName { get; set; }
        public DateTime BirthDate { get; set; }
        public string HealthGroup { get; set; } = null!;
        public string Pegroup { get; set; } = null!;
        public DateTime ValidDate { get; set; }
        public DateTime IssueDate { get; set; }
        public string? Note { get; set; }
    }
}
