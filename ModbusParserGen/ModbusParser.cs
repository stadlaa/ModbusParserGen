using System.Buffers.Binary;
using SharpSunSpec.Modbus.Functions;

namespace SharpSunSpec.Modbus;

/// <summary>
///     Class to parse Modbus register data into various types.
/// </summary>
/// <param name="endianConverter">
///     Endian converter suitable for endian of data source. (Recommended to use function of:
///     <see cref="WordEndian" />.)
/// </param>
public class ModbusParser(Func<ushort[], byte[]> endianConverter)
{
	/// <summary>
	///     Deserializes a modbus register array into the specified type <typeparamref name="T" /> using the specified
	///     encoding.
	/// </summary>
	/// <typeparam name="T">Requested output type.</typeparam>
	/// <param name="registers">Input data in 16bit words.</param>
	/// <param name="valueEncoding">What encoding should be used to interpret the value?</param>
	/// <param name="signed">Should the value be decoded as signed?</param>
	/// <param name="scaleFactor">Scale factor to be applied.</param>
	/// <returns>Decoded value.</returns>
	/// <exception cref="NotSupportedException">An input param combination or the output type is not supported.</exception>
	public T Deserialize<T>(ushort[] registers, Encoding valueEncoding, bool signed, double? scaleFactor = null)
	{
		//Bring to target endian.
		byte[] bytes = endianConverter(registers);

		// Decode based on valueEncoding and convert to the requested target type, if it is supported for the given valueEncoding.
		// If the target type is not specified for the given valueEncoding, a NotSupportedException is thrown after the switch statement.
		switch (valueEncoding)
		{
			case Encoding.UTF8:
				if (typeof(T) == typeof(string))
					return (T)(object)System.Text.Encoding.UTF8.GetString(bytes).TrimEnd('\0', ' ');
				break;
			case Encoding.UTF16:
				if (typeof(T) == typeof(string))
					return (T)(object)System.Text.Encoding.Unicode.GetString(bytes).TrimEnd('\0', ' ');
				break;
			case Encoding.IEEE754:
			{
				object value = bytes.Length switch
				{
					4 => BinaryPrimitives.ReadSingleBigEndian(bytes),
					8 => BinaryPrimitives.ReadDoubleBigEndian(bytes),
					_ => throw new NotSupportedException($"Length of {bytes.Length} bytes is not supported for {valueEncoding} valueEncoding.")
				};
				if (typeof(T) == typeof(double))
					return (T)(object)Convert.ToDouble(value);
				if (typeof(T) == typeof(float))
					return (T)(object)Convert.ToSingle(value);
				break;
			}
			case Encoding.IntAndScaleFactor:
			{
				object value = bytes.Length switch
				{
					2 when signed => BinaryPrimitives.ReadInt16BigEndian(bytes) * (scaleFactor ?? 1),
					2 when !signed => BinaryPrimitives.ReadUInt16BigEndian(bytes) * (scaleFactor ?? 1),
					4 when signed => BinaryPrimitives.ReadInt32BigEndian(bytes) * (scaleFactor ?? 1),
					4 when !signed => BinaryPrimitives.ReadUInt32BigEndian(bytes) * (scaleFactor ?? 1),
					8 when signed => BinaryPrimitives.ReadInt64BigEndian(bytes) * (scaleFactor ?? 1),
					8 when !signed => BinaryPrimitives.ReadUInt64BigEndian(bytes) * (scaleFactor ?? 1),
					// ReSharper disable once CompareOfFloatsByEqualityOperator
					16 when scaleFactor != null => throw new NotSupportedException("Scale factor for 128 bit integers is not supported."),
					16 when signed => BinaryPrimitives.ReadInt128BigEndian(bytes),
					16 when !signed => BinaryPrimitives.ReadUInt128BigEndian(bytes),
					_ => throw new NotSupportedException($"Length of {bytes.Length} bytes is not supported for {valueEncoding} valueEncoding.")
				};
				if (typeof(T) == typeof(bool))
					return (T)(object)Convert.ToBoolean(Math.Round((double)value, 0));
				if (typeof(T) == typeof(byte))
					return (T)(object)Convert.ToByte(Math.Round((double)value, 0));
				if (typeof(T) == typeof(sbyte))
					return (T)(object)Convert.ToSByte(Math.Round((double)value, 0));
				if (typeof(T) == typeof(double))
					return (T)(object)Convert.ToDouble(value);
				if (typeof(T) == typeof(float))
					return (T)(object)Convert.ToSingle(value);
				if (typeof(T) == typeof(short))
					return (T)(object)Convert.ToInt16(Math.Round((double)value, 0));
				if (typeof(T) == typeof(ushort))
					return (T)(object)Convert.ToUInt16(Math.Round((double)value, 0));
				if (typeof(T) == typeof(int))
					return (T)(object)Convert.ToInt32(Math.Round((double)value, 0));
				if (typeof(T) == typeof(uint))
					return (T)(object)Convert.ToUInt32(Math.Round((double)value, 0));
				if (typeof(T) == typeof(long))
					return (T)(object)Convert.ToInt64(Math.Round((double)value, 0));
				if (typeof(T) == typeof(ulong))
					return (T)(object)Convert.ToUInt64(Math.Round((double)value, 0));
				break;
			}
			default:
				throw new NotSupportedException($"Encoding {valueEncoding} is not supported.");
		}

		// If we reach this point, the type was not supported for the given valueEncoding
		throw new NotSupportedException($"Type {typeof(T)} is not supported for valueEncoding {valueEncoding}.");
	}
}

/// <summary>
///     Specifies a certain encoding of datapoint.
/// </summary>
public enum Encoding
{
	IntAndScaleFactor,

	// ReSharper disable once InconsistentNaming
	IEEE754,

	// ReSharper disable once InconsistentNaming
	UTF8,

	// ReSharper disable once InconsistentNaming
	UTF16
}