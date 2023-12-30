using System;
using System.Collections.Generic;

namespace MedicalCertificates.Models
{
    public partial class CoursesTable
    {
        public CoursesTable()
        {
            GroupsTables = new HashSet<GroupsTable>();
        }

        public int CourseId { get; set; }
        public int DepartmentId { get; set; }
        public int Number { get; set; }
        public int? Year { get; set; }

        public virtual DepartmentsTable Department { get; set; } = null!;
        public virtual ICollection<GroupsTable> GroupsTables { get; set; }
    }
}
