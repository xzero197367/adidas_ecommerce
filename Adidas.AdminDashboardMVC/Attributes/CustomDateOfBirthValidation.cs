using System;
using System.ComponentModel.DataAnnotations;

public class CustomDateOfBirthValidation : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null) return ValidationResult.Success; // Optional field

        DateTime dob = (DateTime)value;
        int age = DateTime.Today.Year - dob.Year;
        if (dob > DateTime.Today.AddYears(-age)) age--; // Adjust if birthday hasn't passed

        if (age < 18 || age > 100)
        {
            return new ValidationResult(ErrorMessage ?? "Invalid age.");
        }

        return ValidationResult.Success;
    }
}
