using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EmployeeTableVisualizer
{
    public class WorkLog
    {
        public string name { get; set; }
        public double hours { get; set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Fetching employee data...");

                
                string jsonResponse = "[{\"name\":\"RamVignes\",\"hours\":120},{\"name\":\"Sethupathi\",\"hours\":80},{\"name\":\"GuruCharran\",\"hours\":150}]";
                var workLogs = JsonSerializer.Deserialize<List<WorkLog>>(jsonResponse);

                if (workLogs == null || !workLogs.Any())
                {
                    Console.WriteLine("No data found. Bailing out.");
                    return;
                }

                var employeeTotals = workLogs
                    .GroupBy(log => log.name.Trim())
                    .Select(group => new
                    {
                        Name = group.Key,
                        TotalHours = group.Sum(log => log.hours)
                    })
                    .OrderByDescending(emp => emp.TotalHours)
                    .ToList();

                Console.WriteLine($"Processed {employeeTotals.Count} unique employees.");

                var htmlBuilder = new System.Text.StringBuilder();
                htmlBuilder.AppendLine("<!DOCTYPE html>");
                htmlBuilder.AppendLine("<html lang='en'>");
                htmlBuilder.AppendLine("<head>");
                htmlBuilder.AppendLine("    <title>Employee Work Hours Report</title>");
                htmlBuilder.AppendLine("    <style>");
                htmlBuilder.AppendLine("        table { border-collapse: collapse; width: 50%; margin: 20px 0; }");
                htmlBuilder.AppendLine("        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
                htmlBuilder.AppendLine("        th { background-color: #f2f2f2; }");
                htmlBuilder.AppendLine("        .low-hours { background-color: #FF0000; }");
                htmlBuilder.AppendLine("    </style>");
                htmlBuilder.AppendLine("</head>");
                htmlBuilder.AppendLine("<body>");
                htmlBuilder.AppendLine("    <h1>Employees Sorted by Total Hours Worked (Descending)</h1>");
                htmlBuilder.AppendLine("    <p>Generated on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "</p>");
                htmlBuilder.AppendLine("    <table>");
                htmlBuilder.AppendLine("        <thead>");
                htmlBuilder.AppendLine("            <tr><th>Employee Name</th><th>Total Hours</th></tr>");
                htmlBuilder.AppendLine("        </thead>");
                htmlBuilder.AppendLine("        <tbody>");

                foreach (var emp in employeeTotals)
                {
                    bool isLowHours = emp.TotalHours < 100;
                    string rowClass = isLowHours ? " class='low-hours'" : "";
                    
                    htmlBuilder.AppendLine($"<tr{rowClass}>");
                    htmlBuilder.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(emp.Name)}</td>");
                    htmlBuilder.AppendLine($"<td>{emp.TotalHours:F1}</td>");
                    htmlBuilder.AppendLine("</tr>");
                    
                    if (isLowHours)
                    {
                        Console.WriteLine($"- {emp.Name} has low hours: {emp.TotalHours:F1}");
                    }
                }

                htmlBuilder.AppendLine("        </tbody>");
                htmlBuilder.AppendLine("    </table>");
                htmlBuilder.AppendLine("    <footer><small>Data from local JSON. Rows in yellow indicate &lt; 100 hours.</small></footer>");
                htmlBuilder.AppendLine("</body>");
                htmlBuilder.AppendLine("</html>");

                string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "employee_work_report.html");
                await File.WriteAllTextAsync(outputPath, htmlBuilder.ToString());

                Console.WriteLine($"Done! Check out the report at: {outputPath}");
                Console.WriteLine("Pop it open in your browser to see the table.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something unexpected happened: {ex.Message}");
            }
        }
    }
}