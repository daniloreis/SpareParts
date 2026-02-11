namespace SparePartsApp.Helpers
{
    public interface IConnectionProvider
    {
        bool CheckWifi();

        bool HasWebServerAvaiable(string checkUrl);
    }
}
