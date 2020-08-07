using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using TeisterMask.Data.Models;
using TeisterMask.Data.Models.Enums;
using TeisterMask.DataProcessor.ImportDto;

namespace TeisterMask.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    using Data;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedProject
            = "Successfully imported project - {0} with {1} tasks.";

        private const string SuccessfullyImportedEmployee
            = "Successfully imported employee - {0} with {1} tasks.";

        public static string ImportProjects(TeisterMaskContext context, string xmlString)
        {
            var sb = new StringBuilder();
            var serializer = new XmlSerializer(typeof(ImportProjectDto[]), new XmlRootAttribute("Projects"));

            using (var stringReader = new StringReader(xmlString))
            {
                var projectDtos = (ImportProjectDto[])serializer.Deserialize(stringReader);
                var validProjects = new List<Project>();
                foreach (var projectDto in projectDtos)
                {
                    if (!IsValid(projectDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    DateTime openDate;
                    bool isOpenDateValid = DateTime.TryParseExact(projectDto.OpenDate, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out openDate);

                    if (!isOpenDateValid)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }
                    DateTime? dueDate;
                    if (!String.IsNullOrEmpty(projectDto.DueDate))
                    {
                        DateTime projectDueDateValue;
                        bool isDueDateValid = DateTime.TryParseExact(projectDto.DueDate, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out projectDueDateValue);
                        if (!isDueDateValid)
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        dueDate = projectDueDateValue;
                    }
                    else
                    {
                        dueDate = null;
                    }

                    var project = new Project
                    {
                        Name = projectDto.Name,
                        OpenDate = openDate,
                        DueDate = dueDate
                    };
                    foreach (var taskDto in projectDto.Tasks)
                    {
                        DateTime openDateTask;
                        DateTime dueDateTask;
                        bool isOpenDateTaskValid = DateTime.TryParseExact(taskDto.OpenDate, "dd/MM/yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out openDateTask);
                        bool isDueDateTaskValid = DateTime.TryParseExact(taskDto.DueDate, "dd/MM/yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out dueDateTask);
                        if (!IsValid(taskDto))
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        if (!isOpenDateTaskValid || !isDueDateTaskValid)
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        if (dueDateTask < openDateTask || openDateTask < openDate)
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        if (dueDate.HasValue)
                        {
                            if (dueDateTask > dueDate)
                            {
                                sb.AppendLine(ErrorMessage);
                                continue;
                            }
                        }

                        project.Tasks.Add(new Task
                        {
                            Name = taskDto.Name,
                            OpenDate = openDateTask,
                            DueDate = dueDateTask,
                            ExecutionType = (ExecutionType)taskDto.ExecutionType,
                            LabelType = (LabelType)taskDto.LabelType
                        });
                    }
                    validProjects.Add(project);
                    sb.AppendLine(string.Format(SuccessfullyImportedProject, project.Name, project.Tasks.Count));

                }
                context.Projects.AddRange(validProjects);
                context.SaveChanges();
                return sb.ToString().TrimEnd();
            }


        }

        public static string ImportEmployees(TeisterMaskContext context, string jsonString)
        {
            var sb = new StringBuilder();
            var employeesDtos = JsonConvert.DeserializeObject<ImportEmployeeDto[]>(jsonString);
            var validEmployees = new List<Employee>();
            foreach (var employeeDto in employeesDtos)
            {
                if (!IsValid(employeeDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                var employee = new Employee
                {
                    Username = employeeDto.Username,
                    Email = employeeDto.Email,
                    Phone = employeeDto.Phone
                };
                foreach (var taskId in employeeDto.Tasks.Distinct())
                {
                    var foundTask = context.Tasks.FirstOrDefault(t => t.Id == taskId);
                    if (foundTask == null)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }
                    employee.EmployeesTasks.Add(new EmployeeTask
                    {
                        Employee = employee,
                        Task = foundTask
                    });
                }
                validEmployees.Add(employee);
                sb.AppendLine(string.Format(SuccessfullyImportedEmployee, employee.Username,
                    employee.EmployeesTasks.Count));
            }
            context.Employees.AddRange(validEmployees);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}