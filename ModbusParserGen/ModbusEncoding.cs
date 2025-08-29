namespace ModbusParserGen;

/// <summary>
///     Specifies a certain encoding of datapoint.
/// </summary>
public enum ModbusEncoding
{
	IntAndScaleFactor,

	Int,

	// ReSharper disable once InconsistentNaming
	IEEE754,

	// ReSharper disable once InconsistentNaming
	UTF8,

	// ReSharper disable once InconsistentNaming
	UTF16
}