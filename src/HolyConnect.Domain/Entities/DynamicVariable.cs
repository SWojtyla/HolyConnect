namespace HolyConnect.Domain.Entities;

/// <summary>
/// Represents a variable that generates dynamic/fake test data.
/// </summary>
public class DynamicVariable
{
    /// <summary>
    /// Name of the variable (used in {{ variableName }} syntax)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of data to generate
    /// </summary>
    public DataGeneratorType GeneratorType { get; set; }

    /// <summary>
    /// Optional constraints for the generated data
    /// </summary>
    public List<ConstraintRule> Constraints { get; set; } = new();

    /// <summary>
    /// Whether this variable is marked as secret (masked in UI)
    /// </summary>
    public bool IsSecret { get; set; }
}

/// <summary>
/// Types of data that can be dynamically generated
/// </summary>
public enum DataGeneratorType
{
    // Person data
    FirstName,
    LastName,
    FullName,
    Email,
    PhoneNumber,
    Username,
    
    // Numbers
    Integer,
    Decimal,
    
    // Dates
    Date,
    DatePast,
    DateFuture,
    DateTime,
    
    // Text
    Word,
    Sentence,
    Paragraph,
    
    // Internet
    Url,
    IpAddress,
    MacAddress,
    
    // Identifiers
    Guid,
    Uuid,
    
    // Finance
    CreditCardNumber,
    CurrencyCode,
    Amount,
    
    // Address
    StreetAddress,
    City,
    Country,
    ZipCode,
    
    // Boolean
    Boolean,
    
    // Custom (for future extensibility)
    Custom
}

/// <summary>
/// Represents a constraint/rule for generated data
/// </summary>
public class ConstraintRule
{
    /// <summary>
    /// Type of constraint
    /// </summary>
    public ConstraintType Type { get; set; }

    /// <summary>
    /// Value for the constraint (e.g., minimum value, maximum value, format pattern)
    /// </summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Types of constraints that can be applied to generated data
/// </summary>
public enum ConstraintType
{
    // Numeric constraints
    Minimum,
    Maximum,
    
    // Date constraints
    MinimumAge,
    MaximumAge,
    MinimumDate,
    MaximumDate,
    
    // Date/Time offset constraints (relative to now)
    DaysOffset,
    HoursOffset,
    MinutesOffset,
    SecondsOffset,
    
    // String constraints
    MinLength,
    MaxLength,
    Pattern,
    
    // Custom format
    Format
}
