namespace SettingsProfileDemo.BusinessLayer.Interfaces
{
    public interface ICommandParsingService
    {
        string[] ParseCommandString(string command);
    }
}