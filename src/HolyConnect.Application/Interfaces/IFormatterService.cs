namespace HolyConnect.Application.Interfaces;

public interface IFormatterService
{
    string FormatJson(string json);
    string FormatXml(string xml);
    string FormatGraphQL(string graphql);
}
