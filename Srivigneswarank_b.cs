using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;

// --- Data model ---
public class EmployeeWorkData
{
    public string EmployeeName { get; set; } = "";
    public double HoursWorked { get; set; }
}

class Program
{
    static void Main()
    {
        try
        {
            // --- 1. Fetch data from API ---
            string apiUrl = "https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=YOUR_API_KEY_HERE";

            List<EmployeeWorkData> employees;

            using (HttpClient client = new HttpClient())
            {
                var response = client.GetAsync(apiUrl).Result;
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API call failed: {response.StatusCode}");
                    Console.WriteLine("Using sample data instead.");
                    employees = GetSampleData();
                }
                else
                {
                    string json = response.Content.ReadAsStringAsync().Result;
                    employees = JsonConvert.DeserializeObject<List<EmployeeWorkData>>(json) ?? GetSampleData();
                }
            }

            // --- 2. Generate Pie Chart ---
            string outputPath = "EmployeePieChart.png";
            GeneratePieChart(employees, outputPath);
            Console.WriteLine($"Pie Chart generated: {outputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    // --- Sample data if API fails ---
    static List<EmployeeWorkData> GetSampleData()
    {
        return new List<EmployeeWorkData>
        {
            new EmployeeWorkData { EmployeeName = "Alice", HoursWorked = 120 },
            new EmployeeWorkData { EmployeeName = "Bob", HoursWorked = 80 },
            new EmployeeWorkData { EmployeeName = "Charlie", HoursWorked = 150 },
            new EmployeeWorkData { EmployeeName = "David", HoursWorked = 90 },
            new EmployeeWorkData { EmployeeName = "Eva", HoursWorked = 110 }
        };
    }

    // --- Pie Chart Generation ---
    static void GeneratePieChart(List<EmployeeWorkData> employees, string outputPath)
    {
        using var bitmap = new Bitmap(800, 600);
        using var graphics = Graphics.FromImage(bitmap);

        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(Color.White);

        double totalHours = employees.Sum(e => e.HoursWorked);
        Color[] colors = { Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Purple, Color.Teal };

        var pieRect = new RectangleF(50, 50, 400, 400);
        float startAngle = 0;

        // Draw pie slices
        for (int i = 0; i < employees.Count; i++)
        {
            float sweepAngle = (float)(employees[i].HoursWorked / totalHours * 360);
            using var brush = new SolidBrush(colors[i % colors.Length]);
            graphics.FillPie(brush, pieRect.X, pieRect.Y, pieRect.Width, pieRect.Height, startAngle, sweepAngle);
            graphics.DrawPie(Pens.Black, pieRect.X, pieRect.Y, pieRect.Width, pieRect.Height, startAngle, sweepAngle);
            startAngle += sweepAngle;
        }

        // Draw legend
        float legendY = 50;
        using var font = new Font("Arial", 10);
        for (int i = 0; i < employees.Count; i++)
        {
            double percentage = (employees[i].HoursWorked / totalHours) * 100;
            using var brush = new SolidBrush(colors[i % colors.Length]);
            graphics.FillRectangle(brush, 500, legendY, 20, 20);
            graphics.DrawString($"{employees[i].EmployeeName}: {percentage:F1}%", font, Brushes.Black, 530, legendY);
            legendY += 25;
        }

        // Draw title
        using var titleFont = new Font("Arial", 16, FontStyle.Bold);
        graphics.DrawString("Employee Work Time Distribution", titleFont, Brushes.Black, 200, 10);

        bitmap.Save(outputPath, ImageFormat.Png);
    }
}
