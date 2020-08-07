﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;
using TeisterMask.Data.Models;

namespace TeisterMask.DataProcessor.ImportDto
{
    [XmlType("Project")]
    public class ImportProjectDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(40)]
        public string Name { get; set; }
        [Required]
        public string OpenDate { get; set; }
        public string DueDate { get; set; }
        public ImportTaskDto[] Tasks { get; set; }
    }
}
