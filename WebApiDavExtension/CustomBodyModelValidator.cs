using System;
using System.Web.Http.Validation;
using DDay.iCal;

namespace WebApiDavExtension
{
    public class CustomBodyModelValidator : DefaultBodyModelValidator
    {
        public override bool ShouldValidateType(Type type)
        {
            return type != typeof(iCalendar) && base.ShouldValidateType(type);
        }
    }
}
