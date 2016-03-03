using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;
using System.Xml.Linq;
using log4net;

namespace WebApiDavExtension
{
    /// <summary>
    ///    A cal dav model binder. 
    /// </summary>
    /// <typeparam name="T">
    ///     Request type to bind. 
    /// </typeparam>
	public abstract class CalDavModelBinder<T> : IModelBinder
        where T : Request
	{
	    protected static ILog Log { get; } =
	        LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	    protected RequestDepth RequestDepth { get; private set; } = RequestDepth.Zero;
		protected List<string> RequestedProperties { get; private set; } = new List<string>(); 
		protected ModelBindingContext BindingContext { get; set; }
        protected T Model { get; private set; }

        /// <summary>
        /// Binds the model to a value by using the specified controller context and binding context.
        /// </summary>
        ///
        /// <param name="actionContext">    The action context. </param>
        /// <param name="bindingContext">   The binding context. </param>
        ///
        /// <returns>   true if model binding is successful; otherwise, false. </returns>
        public virtual bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
		{
			BindingContext = bindingContext;
            RequestedProperties = new List<string>();
			ValueProviderResult val = bindingContext.ValueProvider.GetValue("content");

            if (val == null)
			{
				return false;
			}

			XDocument document = val.RawValue as XDocument;

			if (document == null)
			{
				bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Wrong value type");
				return false;
			}

            SetRequestDepth();
            SetRequestedProperties(document);

            return PrepareCreateModel(document);
		}

	    private bool PrepareCreateModel(XDocument document)
	    {
	        try
	        {
	            return TryPrepareCreateModel(document);
	        }
	        catch (Exception exception)
	        {
	            Log.Error($"Could not create Model of type: {typeof (T)}", exception);
                return false;
	        }
	    }

        private bool TryPrepareCreateModel(XDocument document)
        {
            if (BindingContext.ModelType != typeof(T))
            {
                return false;
            }

            Model = (T) Activator.CreateInstance(typeof (T));
            Model.RequestDepth = RequestDepth;

            Model.RequestedPropList.AddRange(RequestedProperties);
            CreateModel(document);

            BindingContext.Model = Model;
            return true;
        }

	    private void SetRequestDepth()
	    {
	        try
	        {
                TrySetRequestDepth();
	        }
	        catch (Exception exception)
	        {
                Log.Error($"Could not set request depth for type: {typeof(T)}", exception);
                RequestDepth = RequestDepth.Zero;
            }
	    }

        private void TrySetRequestDepth()
		{
			ValueProviderResult val = BindingContext.ValueProvider.GetValue("depth");
			string value = val.RawValue as string;

            if (value == "1")
			{
				RequestDepth = RequestDepth.One;
			}
			else if (value == "infinity")
			{
				RequestDepth = RequestDepth.Infinity;
			}
			else
			{
				RequestDepth = RequestDepth.Zero;
			}
		}

	    private void SetRequestedProperties(XDocument document)
	    {
	        try
	        {
	            TrySetRequestedProperties(document);

	        }
	        catch (Exception exception)
	        {
                Log.Error($"Could not set request depth for type: {typeof(T)}", exception);
            }
	    }

        private void TrySetRequestedProperties(XDocument document)
		{
			if (document.Elements().First().Elements(Namespaces.Dav + "prop").Any())
			{
				var propElement = document.Elements().First().Elements(Namespaces.Dav + "prop").FirstOrDefault();

				if (propElement == null)
				{
					return;
				}

				foreach (var element in propElement.Elements())
				{
					RequestedProperties.Add(element.Name.LocalName);
				}
			}
		}

        /// <summary>
        /// Create the model from the given XDocument
        /// </summary>
        /// <param name="document"></param>
        /// <returns>true if creation was succdessfull; otherwise false</returns>
		internal abstract bool CreateModel(XDocument document);
	}
}
