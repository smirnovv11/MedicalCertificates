using System;
using System.Collections.Generic;

namespace MedicalCertificates.Models
{
    public partial class GroupsTable
    {
        public GroupsTable()
        {
            StudentsTables = new HashSet<StudentsTable>();
        }

        public int GroupId { get; set; }
        public int CourseId { get; set; }
        public string Name { get; set; } = null!;

        public virtual CoursesTable Course { get; set; } = null!;
        public virtual ICollection<StudentsTable> StudentsTables { get; set; }
    }
}
