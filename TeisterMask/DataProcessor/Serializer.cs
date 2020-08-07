using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using TeisterMask.Data.Models.Enums;
using TeisterMask.DataProcessor.ExportDto;

namespace TeisterMask.DataProcessor
{
    using System;
    using Data;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportProjectWithTheirTasks(TeisterMaskContext context)
        {
            var sb=new StringBuilder();
            var projects = context.Projects.Where(p => p.Tasks.Count > 0).ToArray()
                .OrderByDescending(p => p.Tasks.Count)
                .ThenBy(p => p.Name)
                .Select(p => new ExportProjectDto
                {
                    TasksCount = p.Tasks.Count,
                    ProjectName = p.Name,
                    HasEndDate = (p.DueDate.HasValue) ? "Yes" : "No",
                    Tasks = p.Tasks.Select(t => new ExportTaskDto
                    {
                        Name = t.Name,
                        Label = t.LabelType.ToString()

                    }).ToArray().OrderBy(t => t.Name).ToArray()

                }).OrderByDescending(p => p.Tasks.Length)
                .ThenBy(p => p.ProjectName).ToArray();

            var xmlSerializer=new XmlSerializer(typeof(ExportProjectDto[]),new XmlRootAttribute("Projects"));
            var namespaces=new XmlSerializerNamespaces();
            namespaces.Add(string.Empty,string.Empty);
            using (StringWriter stringWriter=new StringWriter(sb))
            {
                xmlSerializer.Serialize(stringWriter,projects,namespaces);
            }

            return sb.ToString().TrimEnd();
        }

        public static string ExportMostBusiestEmployees(TeisterMaskContext context, DateTime date)
        {
            var busiestEmployees = context.Employees.ToArray().Where(e => e.EmployeesTasks.FirstOrDefault(et => et.Task.OpenDate >= date)!=null)
                .Select(e => new
                {
                    Username = e.Username,
                    Tasks = e.EmployeesTasks.ToArray().Where(et => et.Task.OpenDate >= date).OrderByDescending(t=>t.Task.DueDate).ThenBy(t=>t.Task.Name).Select(et => new
                        {
                            TaskName = et.Task.Name,
                            OpenDate = et.Task.OpenDate.ToString("d",CultureInfo.InvariantCulture),
                            DueDate = et.Task.DueDate.ToString("d",CultureInfo.InvariantCulture),
                            LabelType = et.Task.LabelType.ToString(),
                            ExecutionType = et.Task.ExecutionType.ToString()
                        })
                        .ToArray()
                })
                .OrderByDescending(e => e.Tasks.Length)
                .ThenBy(e => e.Username).Take(10);
            var json = JsonConvert.SerializeObject(busiestEmployees, Formatting.Indented);
            return json;
        }
    }
}