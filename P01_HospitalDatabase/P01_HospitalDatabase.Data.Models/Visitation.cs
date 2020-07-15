using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace P01_HospitalDatabase.Data.Models
{
    public class Visitation
    {
        public int VisitationId { get; set; }
        public DateTime Date { get; set; }
        public string Comments { get; set; }
        public int PatientId { get; set; }
        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
        public int DoctorId { get; set; }
    }
}
