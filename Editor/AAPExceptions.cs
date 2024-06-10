using System;
using DocumentFormat.OpenXml.Math;

namespace AAPathGenerator
{
	public class AAPFormatExceptions : Exception
	{
		public AAPFormatExceptions(string addressableName) : base($"Addressable Name must contain \"/\"\nAddressable Name: {addressableName}")
		{
		}
	}

	public class AAPDuplicateNameException : Exception
	{
		public AAPDuplicateNameException(string addressableName) : base($"There are DuplicateAddressableName : {addressableName}")
		{
		}
	}
}