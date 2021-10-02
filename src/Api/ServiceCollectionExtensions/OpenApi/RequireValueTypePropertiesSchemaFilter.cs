﻿using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace SharedKernel.Api.ServiceCollectionExtensions.OpenApi
{
    /// <summary>
    /// Sets required attribute to not nullable properties
    /// </summary>
    public class RequireValueTypePropertiesSchemaFilter : ISchemaFilter
    {
        private readonly HashSet<OpenApiSchema> _valueTypes = new HashSet<OpenApiSchema>();

        /// <summary>
        /// Apply filter
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="context"></param>
        public virtual void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsValueType)
                _valueTypes.Add(schema);

            if (schema.Properties == null)
                return;

            foreach (var prop in schema.Properties)
            {
                if (_valueTypes.Contains(prop.Value) || IsEnumRequired(prop))
                    schema.Required.Add(prop.Key);
            }
        }

        /// <summary>
        /// To know if is a required enum
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public virtual bool IsEnumRequired(KeyValuePair<string, OpenApiSchema> prop)
        {
            return prop.Value.Reference != default;
        }
    }
}
