using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;

namespace LionFire.AspNetCore.Mvc.Controllers
{
    public class TypedControllerFeatureProvider<TController> : ControllerFeatureProvider 
    {
        protected override bool IsController(TypeInfo typeInfo)
        {
            if (!typeof(TController).GetTypeInfo().IsAssignableFrom(typeInfo)) return false;
            return base.IsController(typeInfo);
        }
    }
}
