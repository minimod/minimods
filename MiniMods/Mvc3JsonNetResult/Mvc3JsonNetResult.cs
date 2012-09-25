using System.Net;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Minimod.Mvc3JsonNetResult
{
    /// <summary>
    /// <h1>Minimod.Mvc3JsonNetResult, Version 0.9.2, Copyright © Lars Corneliussen 2011</h1>
    /// <para>Comes with a DataActionResult that serializes passed data using JSON.NET from Newtonsoft.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    internal static class ControllerExtensions
    {
        public static ActionResult Data(this Controller controller, object data)
        {
            return new DataActionResult(data);
        }

        public static ActionResult Data(this Controller controller, HttpStatusCode statusCode, object data)
        {
            return new DataActionResult(statusCode, data);
        }
    }

    internal class DataActionResult : ActionResult
    {
        public object Data { get; private set; }
        public HttpStatusCode StatusCode { get; set; }

        public DataActionResult(object data)
        {
            this.Data = data;
            StatusCode = HttpStatusCode.OK;
        }

        public DataActionResult(HttpStatusCode statusCode, object data)
        {
            Data = data;
            StatusCode = statusCode;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            HttpResponseBase response = context.HttpContext.Response;

            JsonTextWriter writer = new JsonTextWriter(response.Output);
            writer.Formatting = Formatting.Indented;
            JsonSerializer serializer = JsonSerializer.Create(
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });

            serializer.Converters.Add(new IsoDateTimeConverter());

            response.StatusCode = (int) StatusCode;
            response.ContentType = "application/json";

            serializer.Serialize(writer, Data);
            writer.Flush();
        }
    }
}