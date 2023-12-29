using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string Year { get; set; } = null!;

        public virtual DepartmentsTable Department { get; set; } = null!;
        public virtual ICollection<GroupsTable> GroupsTables { get; set; }
    }
}
