using System;
using System.Collections.Generic;

namespace MedicalCertificates.Models
{
    public partial class DepartmentsTable
    {
        public DepartmentsTable()
        {
            CoursesTables = new HashSet<CoursesTable>();
        }

        public int DepartmentId { get; set; }
        public string Name { get; set; } = null!;
        public int MaxCourse { get; set; }

        public virtual ICollection<CoursesTable> CoursesTables { get; set; }
    }
}
