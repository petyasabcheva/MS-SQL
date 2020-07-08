using System;
using System.Linq;
using System.Text;
using SoftUni.Data;
using SoftUni.Models;

namespace SoftUni
{
    public class StartUp
    {
        static void Main(string[] args)
        {
            var context = new SoftUniContext();
            string result = GetEmployee147(context);
            Console.WriteLine(result);
        }
        //Problem 3
        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            var sb = new StringBuilder();
            var employees = context.Employees.OrderBy(e => e.EmployeeId).ToList();
            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} {employee.MiddleName} {employee.JobTitle} {employee.Salary:f2}");

            }
            return sb.ToString().TrimEnd();

        }
        //Problem 4
        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            var sb = new StringBuilder();
            var employees = context.Employees.
                Where(e => e.Salary > 50000).
                Select(e => new
                {
                    e.FirstName,
                    e.Salary
                }).
                OrderBy(e => e.FirstName).
                ToList();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} - {employee.Salary:f2}");

            }
            return sb.ToString().TrimEnd();
        }
        //Problem 5
        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            var sb = new StringBuilder();
            var employees = context.Employees.
                Where(e => e.Department.Name == "Research and Development").
                Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.Department.Name,
                    e.Salary
                }).
                OrderBy(e => e.Salary).
                ThenByDescending(e => e.FirstName).
                ToList();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} from Research and Development - ${employee.Salary:f2}");

            }
            return sb.ToString().TrimEnd();
        }
        //Problem 6
        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            var sb = new StringBuilder();
            var address = new Address()
            {
                AddressText = "Vitoshka 15",
                TownId = 4
            };
            var employeeNakov = context.Employees.First(e => e.LastName == "Nakov");
            employeeNakov.Address = address;
            context.SaveChanges();
            var employees = context.Employees.
                OrderByDescending(e => e.AddressId).
                Take(10).
                Select(e => new
                {
                    e.Address.AddressText
                }).
                ToList();
            foreach (var employee in employees)
            {
                sb.AppendLine(employee.AddressText);

            }
            return sb.ToString().TrimEnd();
        }
        //Problem 7
        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            var sb = new StringBuilder();
            var employees = context.Employees.
                Where(e => e.EmployeesProjects.FirstOrDefault(p => p.Project.StartDate.Year >= 2001 && p.Project.StartDate.Year <= 2003) != null).
                Take(10).
                Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    managerFirstName = e.Manager.FirstName,
                    managerLastName = e.Manager.LastName,
                    projects = e.EmployeesProjects.Select(p => new
                    {
                        p.Project.Name,
                        p.Project.StartDate,
                        p.Project.EndDate

                    }).ToList()
                }).ToList();
            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} - Manager: {e.managerFirstName} {e.managerLastName}");
                foreach (var p in e.projects)
                {
                    var endDate = (p.EndDate == null ? "not finished" : p.EndDate.ToString());
                    sb.AppendLine($"--{p.Name} - {p.StartDate} - {endDate}");
                }

            }
            return sb.ToString().TrimEnd();
        }
        //Problem 8 
        public static string GetAddressesByTown(SoftUniContext context)
        {
            var sb = new StringBuilder();
            var addresses = context.Addresses.
                OrderByDescending(a=>a.Employees.Count).ThenBy(a=>a.Town.Name).
                ThenBy(a=>a.AddressText).Take(10).Select(a => new
                {
                    a.AddressText,
                    a.Town.Name,
                    a.Employees.Count
                }).ToList();
            foreach (var a in addresses)
            {
                sb.AppendLine($"{a.AddressText}, {a.Name} - {a.Count} employees");
            }
            return sb.ToString().TrimEnd();
        }
        //Problem 9
        public static string GetEmployee147(SoftUniContext context)
        {
            var sb= new StringBuilder();
            var employee147 = context.Employees.FirstOrDefault(e => e.EmployeeId == 147);
            var projects = context.EmployeesProjects.Where(ep => ep.EmployeeId == 147).Select(p=>new
            {
                p.Project.Name
            }).OrderBy(p=>p.Name).ToList();
            if (employee147 != null)
            {
                sb.AppendLine($"{employee147.FirstName} {employee147.LastName} - {employee147.JobTitle}");
                foreach (var project in projects)
                {
                    sb.AppendLine($"{project.Name}");
                }
            }
            return sb.ToString().TrimEnd();
        }
    }
}
