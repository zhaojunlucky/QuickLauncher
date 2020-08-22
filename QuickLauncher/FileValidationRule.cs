using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace QuickLauncher.Validation
{
    public class FileValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string filePath = Convert.ToString(value);
            if (filePath == null || filePath.Length == 0)
            {
                return new ValidationResult(false, "Please choose a file!!!");
            } else if (!File.Exists(filePath))
            {
                return new ValidationResult(false, "File doen't exist");
            } else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
