namespace SparePartsWebApi.Models
{
    public class WebApiResult
    {
        public bool Success { get; set; }

        string Message { get; set; }

        object Result { get; set; }
    }
}