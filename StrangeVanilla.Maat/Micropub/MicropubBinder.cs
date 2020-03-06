using System;
using System.Collections.Generic;
using System.IO;
using Nancy;
using Nancy.ModelBinding;

namespace StrangeVanilla.Maat.Micropub
{
    public class MicropubBinder : IModelBinder
    {
        const string micropubPost = "MicropubPost";
        const string micropubPayload = "MicropubPayload";

        public object Bind(NancyContext context, Type modelType, object instance, BindingConfig configuration, params string[] blackList)
        {
            switch (modelType.Name)
            {
                case micropubPost:
                    return GetMicropubPost(context);
                case micropubPayload:
                    return GetMicropubPayload(context);
                default:
                    return null;
            }
        }

        public MicropubPost GetMicropubPost(NancyContext context)
        {
            if (context.Request.Headers.ContentType == "application/json")
            {
                using (var reader = new StreamReader(context.Request.Body))
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<MicropubPost>(reader.ReadToEnd());
                }
            }
            else if (context.Request.Headers.ContentType == "application/x-www-form-urlencoded" ||
                context.Request.Headers.ContentType.ToString().StartsWith("multipart/form-data", StringComparison.CurrentCultureIgnoreCase))
            {
                return new MicropubPost
                {
                    Type = context.Request.Form["h"],
                    Content = context.Request.Form["content"],
                    Categories = AsArray(context.Request.Form["category[]"]),
                    Title = context.Request.Form["name"],
                    BookmarkOf = context.Request.Form["bookmark-of"],
                    PostStatus = context.Request.Form["post-status"]
                };
            }

            return null;
        }

        public MicropubPayload GetMicropubPayload(NancyContext context)
        {
            if (context.Request.Headers.ContentType == "application/json")
            {
                using (var reader = new StreamReader(context.Request.Body))
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<MicropubPayload>(reader.ReadToEnd());
                }
            }
            else if (context.Request.Headers.ContentType == "application/x-www-form-urlencoded" ||
                context.Request.Headers.ContentType.ToString().StartsWith("multipart/form-data", StringComparison.CurrentCultureIgnoreCase))
            {
                var payload = new MicropubPayload();
                payload.Type = new [] { $"h-{context.Request.Form["h"]}" };
                payload.Properties = new Dictionary<string, object[]>();

                payload.Properties["content"] = new string[] { context.Request.Form["content"] };

                payload.Properties["category"] = AsArray(context.Request.Form["category[]"]);
                payload.Properties["name"] = new string[] { context.Request.Form["name"] };
                payload.Properties["bookmark-of"] = new string[] { context.Request.Form["bookmark-of"] };
                payload.Properties["post-status"] = new string[] { context.Request.Form["post-status"] };
                return payload;
            }

            return null;
        }

        public static string[] AsArray(dynamic field)
        {
            if (field == null)
            {
                return new string[] { };
            }
            if (field is Array)
            {
                return (string[])field;
            }
            return field.ToString().Split(new[] { ' ', ',' });
        }

        public bool CanBind(Type modelType)
        {
            return modelType == typeof(MicropubPost) || modelType == typeof(MicropubPayload);
        }
    }
}
