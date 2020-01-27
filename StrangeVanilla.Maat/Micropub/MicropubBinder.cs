using System;
using System.IO;

using Nancy;
using Nancy.ModelBinding;

namespace StrangeVanilla.Maat.Micropub
{
    public class MicropubBinder : IModelBinder
    {
        public object Bind(NancyContext context, Type modelType, object instance, BindingConfig configuration, params string[] blackList)
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
                    Categories = AsArray(context.Request.Form["category"]),
                    Title = context.Request.Form["name"],
                    BookmarkOf = context.Request.Form["bookmark-of"],
                    PostStatus = context.Request.Form["post-status"]
                };
            }

            return null;
        }

        public string[] AsArray(dynamic field)
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
            return modelType == typeof(MicropubPost);
        }
    }
}
