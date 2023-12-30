using System;
using System.Collections.Generic;

namespace MedicalCertificates.Models
{
    public partial class StudentsGroupArchiveTable
    {
        public int NoteId { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public int OldGroupId { get; set; }
        public int NewGroupId { get; set; }
        public int Year { get; set; }
        public DateTime AlterDate { get; set; }
    }
}
