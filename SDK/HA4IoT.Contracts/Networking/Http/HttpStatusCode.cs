namespace HA4IoT.Contracts.Networking.Http
{
    public enum HttpStatusCode
    {
        SwitchingProtocols = 101,

        OK = 200,
        NoContent = 204,
        
        NotModified = 304,

        BadRequest = 400,
        NotFound = 404,

        InternalServerError = 500,
        NotImplemented = 501,
        HttpVersionNotSupported = 505
    }
}
