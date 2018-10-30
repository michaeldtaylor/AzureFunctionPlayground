using System;

namespace AddressFulfilment.Shared.Storage.Table
{
    public class MissingKeyAttributeException : Exception
    {
        public MissingKeyAttributeException(string attributeName, string entityName) : base($"Missing {attributeName} from {entityName}")
        {
        }
    }
}