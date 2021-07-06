    public sealed class ApiResult : ApiResult<object>
    {

    }

    public class ApiResult<T>:IActionResult
    {
        [JsonIgnore]
        public int? StatusCode { get; set; }

        public bool Error => StatusCode != null && StatusCode != 200;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public T Data { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ModelError> Errors { get; set; }

        public ApiResult() { }

        public ApiResult(ModelStateDictionary modelState) : this()
        {
            if (modelState.Any(m => m.Value.Errors.Count > 0))
            {
                StatusCode = 400;
                Message = "The model submitted was invalid. Please correct the specified errors and try again.";
                Errors = modelState
                    .SelectMany(m => m.Value.Errors.Select(me => new ModelError
                    {
                        FieldName = m.Key,
                        ErrorMessage = me.ErrorMessage
                    }));
            }
        }
        public ApiResult(IEnumerable<ValidationFailure> errors) : this()
        {
            if (errors.Any())
            {
                StatusCode = 400;
                Message = "The model submitted was invalid. Please correct the specified errors and try again.";
                Errors = errors.Select(e => new ModelError { ErrorMessage = e.ErrorMessage, FieldName = e.PropertyName });
            }
        }

        public  Task ExecuteResultAsync(ActionContext context)
        {
            if (StatusCode.HasValue)
            {
                context.HttpContext.Response.StatusCode = StatusCode.Value;
            }

            return new ObjectResult(this).ExecuteResultAsync(context);
        }


        public class ModelError
        {
            public string FieldName { get; set; }

            public string ErrorMessage { get; set; }
        }


    }
