
using System.ComponentModel.DataAnnotations;

namespace TeisterMask.Data.Models
{
    public class EmployeeTask
    {
        public int EmployeeId { get; set; }
        [Required]
        public Employee Employee { get; set; }
        public int TaskId { get; set; }
        [Required]
        public Task Task { get; set; }
    }
}
