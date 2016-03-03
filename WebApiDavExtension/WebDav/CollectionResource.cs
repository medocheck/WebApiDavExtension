using System;
using System.Collections.Generic;
using System.IO;

namespace WebApiDavExtension.WebDav
{
    public abstract class CollectionResource : Resource
    {
        public List<Resource> Resources { get; } = new List<Resource>();

        public override MemoryStream GetOutputData()
        {
            throw new NotImplementedException("Collections have no output data");
        }
    }
}
