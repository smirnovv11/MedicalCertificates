using System;
using System.Collections.Generic;

namespace MedicalCertificates.Models
{
    public partial class StudentsTable
    {
        public StudentsTable()
        {
            CertificatesTables = new HashSet<CertificatesTable>();
        }

        public int StudentId { get; set; }
        public int GroupId { get; set; }
        public string FirstName { get; set; } = null!;
        public string SecondName { get; set; } = null!;
        public string ThirdName { get; set; } = null!;
        
        public DateTime BirthDate { get; set; }

        public virtual GroupsTable Group { get; set; } = null!;
        public virtual ICollection<CertificatesTable> CertificatesTables { get; set; }
    }
}
