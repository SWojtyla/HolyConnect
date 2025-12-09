using Bogus;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Infrastructure.Services;

/// <summary>
/// Service for generating dynamic/fake test data using the Bogus library.
/// </summary>
public class DataGeneratorService : IDataGeneratorService
{
    private readonly Faker _faker;

    public DataGeneratorService()
    {
        _faker = new Faker();
    }

    public string GenerateValue(DynamicVariable dynamicVariable)
    {
        try
        {
            var value = dynamicVariable.GeneratorType switch
            {
                // Person data
                DataGeneratorType.FirstName => _faker.Name.FirstName(),
                DataGeneratorType.LastName => _faker.Name.LastName(),
                DataGeneratorType.FullName => _faker.Name.FullName(),
                DataGeneratorType.Email => _faker.Internet.Email(),
                DataGeneratorType.PhoneNumber => _faker.Phone.PhoneNumber(),
                DataGeneratorType.Username => _faker.Internet.UserName(),

                // Numbers
                DataGeneratorType.Integer => GenerateInteger(dynamicVariable.Constraints),
                DataGeneratorType.Decimal => GenerateDecimal(dynamicVariable.Constraints),

                // Dates
                DataGeneratorType.Date => GenerateDate(dynamicVariable.Constraints),
                DataGeneratorType.DatePast => _faker.Date.Past().ToString("yyyy-MM-dd"),
                DataGeneratorType.DateFuture => _faker.Date.Future().ToString("yyyy-MM-dd"),
                DataGeneratorType.DateTime => _faker.Date.Recent().ToString("yyyy-MM-ddTHH:mm:ss"),

                // Text
                DataGeneratorType.Word => _faker.Lorem.Word(),
                DataGeneratorType.Sentence => _faker.Lorem.Sentence(),
                DataGeneratorType.Paragraph => _faker.Lorem.Paragraph(),

                // Internet
                DataGeneratorType.Url => _faker.Internet.Url(),
                DataGeneratorType.IpAddress => _faker.Internet.Ip(),
                DataGeneratorType.MacAddress => _faker.Internet.Mac(),

                // Identifiers
                DataGeneratorType.Guid => System.Guid.NewGuid().ToString(),
                DataGeneratorType.Uuid => System.Guid.NewGuid().ToString(),

                // Finance
                DataGeneratorType.CreditCardNumber => _faker.Finance.CreditCardNumber(),
                DataGeneratorType.CurrencyCode => _faker.Finance.Currency().Code,
                DataGeneratorType.Amount => _faker.Finance.Amount().ToString("F2"),

                // Address
                DataGeneratorType.StreetAddress => _faker.Address.StreetAddress(),
                DataGeneratorType.City => _faker.Address.City(),
                DataGeneratorType.Country => _faker.Address.Country(),
                DataGeneratorType.ZipCode => _faker.Address.ZipCode(),

                // Boolean
                DataGeneratorType.Boolean => _faker.Random.Bool().ToString().ToLower(),

                // Custom (placeholder for future extensibility)
                DataGeneratorType.Custom => _faker.Lorem.Word(),

                _ => _faker.Lorem.Word()
            };

            return value;
        }
        catch (Exception ex)
        {
            // If generation fails, return a fallback value
            return $"[Error: {ex.Message}]";
        }
    }

    public bool ValidateConfiguration(DynamicVariable dynamicVariable)
    {
        if (string.IsNullOrWhiteSpace(dynamicVariable.Name))
        {
            return false;
        }

        // Validate constraints based on generator type
        foreach (var constraint in dynamicVariable.Constraints)
        {
            switch (constraint.Type)
            {
                case ConstraintType.Minimum:
                case ConstraintType.Maximum:
                    if (!int.TryParse(constraint.Value, out _) && !decimal.TryParse(constraint.Value, out _))
                    {
                        return false;
                    }
                    break;

                case ConstraintType.MinimumAge:
                case ConstraintType.MaximumAge:
                    if (!int.TryParse(constraint.Value, out _))
                    {
                        return false;
                    }
                    break;

                case ConstraintType.MinimumDate:
                case ConstraintType.MaximumDate:
                    if (!DateTime.TryParse(constraint.Value, out _))
                    {
                        return false;
                    }
                    break;

                case ConstraintType.MinLength:
                case ConstraintType.MaxLength:
                    if (!int.TryParse(constraint.Value, out _))
                    {
                        return false;
                    }
                    break;

                // Pattern and Format are strings, so they're always valid
                case ConstraintType.Pattern:
                case ConstraintType.Format:
                    break;
            }
        }

        return true;
    }

    private string GenerateInteger(List<ConstraintRule> constraints)
    {
        int min = int.MinValue;
        int max = int.MaxValue;

        foreach (var constraint in constraints)
        {
            if (constraint.Type == ConstraintType.Minimum && int.TryParse(constraint.Value, out var minValue))
            {
                min = minValue;
            }
            else if (constraint.Type == ConstraintType.Maximum && int.TryParse(constraint.Value, out var maxValue))
            {
                max = maxValue;
            }
        }

        // Ensure min is less than max
        if (min >= max)
        {
            max = min + 1000;
        }

        // Bogus doesn't support full int range, so we need to handle large ranges
        if (min == int.MinValue || max == int.MaxValue)
        {
            return _faker.Random.Int(0, 1000000).ToString();
        }

        return _faker.Random.Int(min, max).ToString();
    }

    private string GenerateDecimal(List<ConstraintRule> constraints)
    {
        decimal min = 0;
        decimal max = 1000000;

        foreach (var constraint in constraints)
        {
            if (constraint.Type == ConstraintType.Minimum && decimal.TryParse(constraint.Value, out var minValue))
            {
                min = minValue;
            }
            else if (constraint.Type == ConstraintType.Maximum && decimal.TryParse(constraint.Value, out var maxValue))
            {
                max = maxValue;
            }
        }

        // Ensure min is less than max
        if (min >= max)
        {
            max = min + 1000;
        }

        return _faker.Random.Decimal(min, max).ToString("F2");
    }

    private string GenerateDate(List<ConstraintRule> constraints)
    {
        DateTime? minDate = null;
        DateTime? maxDate = null;
        int? minAge = null;
        int? maxAge = null;

        foreach (var constraint in constraints)
        {
            switch (constraint.Type)
            {
                case ConstraintType.MinimumDate when DateTime.TryParse(constraint.Value, out var min):
                    minDate = min;
                    break;
                case ConstraintType.MaximumDate when DateTime.TryParse(constraint.Value, out var max):
                    maxDate = max;
                    break;
                case ConstraintType.MinimumAge when int.TryParse(constraint.Value, out var minAgeValue):
                    minAge = minAgeValue;
                    break;
                case ConstraintType.MaximumAge when int.TryParse(constraint.Value, out var maxAgeValue):
                    maxAge = maxAgeValue;
                    break;
            }
        }

        // If age constraints are specified, convert them to date constraints
        if (minAge.HasValue || maxAge.HasValue)
        {
            var today = DateTime.Today;
            if (maxAge.HasValue)
            {
                // Person with maxAge should have birthdate no earlier than maxAge years ago
                minDate = today.AddYears(-maxAge.Value);
            }
            if (minAge.HasValue)
            {
                // Person with minAge should have birthdate no later than minAge years ago (minus 1 day to ensure they've reached that age)
                maxDate = today.AddYears(-minAge.Value).AddDays(-1);
            }
        }

        // Generate date within constraints
        if (minDate.HasValue && maxDate.HasValue)
        {
            return _faker.Date.Between(minDate.Value, maxDate.Value).ToString("yyyy-MM-dd");
        }
        else if (minDate.HasValue)
        {
            return _faker.Date.Between(minDate.Value, DateTime.Today).ToString("yyyy-MM-dd");
        }
        else if (maxDate.HasValue)
        {
            return _faker.Date.Between(DateTime.Today.AddYears(-100), maxDate.Value).ToString("yyyy-MM-dd");
        }

        // Default: random date in the past 30 years
        return _faker.Date.Past(30).ToString("yyyy-MM-dd");
    }
}
