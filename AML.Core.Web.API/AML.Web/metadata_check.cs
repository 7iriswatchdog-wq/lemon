using System;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;

namespace MetadataInspector
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Inspecting IExcelDataValidationCollection...");
            var validationCollectionType = typeof(ExcelDataValidationCollection);
            foreach (var method in validationCollectionType.GetMethods().Where(m => m.Name.StartsWith("Add")))
            {
                Console.WriteLine($"Method: {method.Name}");
            }

            Console.WriteLine("\nInspecting IExcelDataValidationList Formula...");
            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("Test");
                var listValidation = sheet.DataValidations.AddListValidation("A1");
                var formulaType = listValidation.Formula.GetType();
                Console.WriteLine($"Formula Type: {formulaType.FullName}");
                foreach (var prop in formulaType.GetProperties())
                {
                    Console.WriteLine($"Property: {prop.Name} ({prop.PropertyType.Name})");
                }
            }
        }
    }
}
