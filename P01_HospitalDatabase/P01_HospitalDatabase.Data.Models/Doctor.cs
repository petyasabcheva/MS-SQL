﻿using System;
using System.Collections.Generic;
using System.Text;

namespace P01_HospitalDatabase.Data.Models
{
    public class Doctor
    {
        public Doctor()
        {
            this.Visitations=new HashSet<Visitation>();
        }
        public string DoctorId { get; set; }
        public string Name { get; set; }
        public string Specialty { get; set; }
        public virtual ICollection<Visitation> Visitations { get; set; }

    }
}
