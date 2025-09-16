using System.Net;
using System.Runtime.InteropServices;

namespace ModbusParserGen;

/// <summary>
///     Class to parse Modbus register data into various types.
/// </summary>
/// <param name="wordSwap">Apply word swap on incoming data before endian conversion.</param>
/// <param name="endian">Specify the endian of the incoming (word-swapped) data by injecting a function from <see cref="Endian"/>. The endian will be reversed if it differs from the target endian.</param>
public class ModbusParser(bool wordSwap, Func<byte[], byte[]> endian)
{
	/// <summary>
	///		Apply word swap on incoming data before endian conversion.
	/// </summary>
	public bool WordSwap { get; } = wordSwap;


	/// <summary>
	///		Specify the endian of the incoming (word-swapped) data by injecting a function from <see cref="Endian"/>. The endian will be reversed if it differs from the target endian.
	/// </summary>
	public Func<byte[], byte[]> Endian { get; } = endian;

	/// <summary>
	///     Deserializes a modbus register array into the specified type <typeparamref name="T" /> using the specified
	///     encoding.
	/// </summary>
	/// <typeparam name="T">Requested output type.</typeparam>
	/// <param name="registers">Input data in 16bit words.</param>
	/// <param name="modbusEncoding">What encoding should be used to interpret the value?</param>
	/// <param name="signed">Should the value be decoded as signed?</param>
	/// <param name="scaleFactor">Scale factor to be applied.</param>
	/// <returns>Decoded value.</returns>
	/// <exception cref="NotSupportedException">An input param combination or the output type is not supported.</exception>
	/// <exception cref="ArgumentNullException"></exception>
	public T Deserialize<T>(ushort[] registers, ModbusEncoding modbusEncoding, bool signed, decimal? scaleFactor = null)
	{
		var bytes = FromSource(registers);

		// Decode based on modbusEncoding and convert to the requested target type, if it is supported for the given modbusEncoding.
		// If the target type is not specified for the given modbusEncoding, a NotSupportedException is thrown after the switch statement.
		switch (modbusEncoding)
		{
			case ModbusEncoding.UTF8:
				{
					if (scaleFactor != null)
						throw new NotSupportedException($"No scale factor supported for encoding {modbusEncoding}");

					if (typeof(T) == typeof(string))
						return (T)(object)System.Text.Encoding.UTF8.GetString(bytes).TrimEnd('\0');
					break;
				}
			case ModbusEncoding.UTF16:
				{
					if (scaleFactor != null)
						throw new NotSupportedException($"No scale factor supported for encoding {modbusEncoding}");

					if (typeof(T) == typeof(string))
						return (T)(object)System.Text.Encoding.Unicode.GetString(bytes).TrimEnd('\0');
					break;
				}
			case ModbusEncoding.IEEE754:
				{
					object value = bytes.Length switch
					{
						4 => MemoryMarshal.Read<float>(bytes),
						8 => MemoryMarshal.Read<double>(bytes),
						_ => throw new NotSupportedException($"Length of {registers.Length} words is not supported for {modbusEncoding} encoding.")
					};
					if (typeof(T) == typeof(double))
						return (T)(object)Convert.ToDouble(value);
					if (typeof(T) == typeof(float))
						return (T)(object)Convert.ToSingle(value);
					break;
				}
			case ModbusEncoding.IntAndScaleFactor:
				{
					if (scaleFactor is null)
						throw new ArgumentNullException($"No scale factor provided for encoding {modbusEncoding}");

					object value = bytes.Length switch
					{
						2 when signed => MemoryMarshal.Read<short>(bytes),
						2 when !signed => MemoryMarshal.Read<ushort>(bytes),
						4 when signed => MemoryMarshal.Read<int>(bytes),
						4 when !signed => MemoryMarshal.Read<uint>(bytes),
						8 when signed => MemoryMarshal.Read<long>(bytes),
						8 when !signed => MemoryMarshal.Read<ulong>(bytes),
						_ => throw new NotSupportedException($"Length of {registers.Length} words is not supported for {modbusEncoding} encoding.")
					};
					if (typeof(T) == typeof(decimal))
						return (T)(object)Convert.ToDecimal(Convert.ToDecimal(value) * (decimal)scaleFactor);
					if (typeof(T) == typeof(double))
						return (T)(object)Convert.ToDouble(Convert.ToDouble(value) * (double)scaleFactor);
					if (typeof(T) == typeof(float))
						return (T)(object)Convert.ToSingle(Convert.ToSingle(value) * (float)scaleFactor);
					break;
				}
			case ModbusEncoding.Int:
				{
					if (scaleFactor != null)
						throw new NotSupportedException($"No scale factor supported for encoding {modbusEncoding}");

					object value = bytes.Length switch
					{
						2 when signed => MemoryMarshal.Read<short>(bytes),
						2 when !signed => MemoryMarshal.Read<ushort>(bytes),
						4 when signed => MemoryMarshal.Read<int>(bytes),
						4 when !signed => MemoryMarshal.Read<uint>(bytes),
						8 when signed => MemoryMarshal.Read<long>(bytes),
						8 when !signed => MemoryMarshal.Read<ulong>(bytes),
						16 when signed => MemoryMarshal.Read<Int128>(bytes),
						16 when !signed => MemoryMarshal.Read<UInt128>(bytes),
						_ => throw new NotSupportedException($"Length of {registers.Length} words is not supported for {modbusEncoding} encoding.")
					};
					if (typeof(T) == typeof(bool))
						return (T)(object)Convert.ToBoolean(value);
					if (typeof(T) == typeof(byte))
						return (T)(object)Convert.ToByte(value);
					if (typeof(T) == typeof(sbyte))
						return (T)(object)Convert.ToSByte(value);
					if (typeof(T) == typeof(double))
						return (T)(object)Convert.ToDouble(value);
					if (typeof(T) == typeof(decimal))
						return (T)(object)Convert.ToDecimal(value);
					if (typeof(T) == typeof(float))
						return (T)(object)Convert.ToSingle(value);
					if (typeof(T) == typeof(short))
						return (T)(object)Convert.ToInt16(value);
					if (typeof(T) == typeof(ushort))
						return (T)(object)Convert.ToUInt16(value);
					if (typeof(T) == typeof(int))
						return (T)(object)Convert.ToInt32(value);
					if (typeof(T) == typeof(uint))
						return (T)(object)Convert.ToUInt32(value);
					if (typeof(T) == typeof(long))
						return (T)(object)Convert.ToInt64(value);
					if (typeof(T) == typeof(ulong))
						return (T)(object)Convert.ToUInt64(value);
					if (typeof(T) == typeof(Int128))
						return (T)value;
					if (typeof(T) == typeof(UInt128))
						return (T)value;
					break;
				}
			case ModbusEncoding.IPAddress:
				{
					if (scaleFactor != null)
						throw new NotSupportedException($"No scale factor supported for encoding {modbusEncoding}");

					if (typeof(T) == typeof(IPAddress))
						return bytes.Length switch
						{
							4 or 16 => (T)(object)new IPAddress(Functions.Endian.BigEndian(bytes)),
							_ => throw new NotSupportedException($"Length of {registers.Length} words is not supported for {modbusEncoding} encoding.")
						};

					break;
				}
			case ModbusEncoding.RawBytes:
				{
					if (scaleFactor != null)
						throw new NotSupportedException($"No scale factor supported for encoding {modbusEncoding}");

					if (typeof(T) == typeof(byte[]))
						return (T)(object)bytes;

					break;
				}
			default:
				throw new NotSupportedException($"Encoding {modbusEncoding} is not supported.");
		}
		// If we reach this point, the type was not supported for the given modbusEncoding
		throw new NotSupportedException($"Type {typeof(T)} is not supported for encoding {modbusEncoding}.");
	}

	/// <summary>
	///     Serializes a specified type <typeparamref name="T" /> into a modbus register array using the specified
	///     encoding.
	/// </summary>
	/// <typeparam name="T">Given input type.</typeparam>
	/// <param name="value">Given input value</param>
	/// <param name="targetLength">Length of data in target registers</param>
	/// <param name="modbusEncoding">What encoding should be used to serialize the value?</param>
	/// <param name="signed">Should the value be interpreted as signed?</param>
	/// <param name="scaleFactor">Scale factor to be applied.</param>
	/// <returns>Serialized registers.</returns>
	/// <exception cref="NotSupportedException">An input param combination is not supported.</exception>
	public ushort[] Serialize<T>(T value, int targetLength, ModbusEncoding modbusEncoding, bool signed, decimal? scaleFactor = null)
	{
		if (value == null)
			throw new ArgumentNullException(nameof(value));

		byte[] bytes = new byte[targetLength * 2];

		// Encode based on modbusEncoding, to register targetLength if it is supported for the given modbusEncoding.
		switch (modbusEncoding)
		{
			case ModbusEncoding.UTF8:
				{
					if (scaleFactor != null)
						throw new NotSupportedException($"No scale factor supported for encoding {modbusEncoding}");

					if (!(typeof(T) == typeof(string)))
						throw new NotSupportedException($"Type {typeof(T)} is not supported for encoding {modbusEncoding}.");

					byte[] strBytes = System.Text.Encoding.UTF8.GetBytes((string)(object)value);
					strBytes.AsSpan(0, Math.Min(bytes.Length, strBytes.Length)).CopyTo(bytes);
					break;
				}
			case ModbusEncoding.UTF16:
				{
					if (scaleFactor != null)
						throw new NotSupportedException($"No scale factor supported for encoding {modbusEncoding}");

					if (!(typeof(T) == typeof(string)))
						throw new NotSupportedException($"Type {typeof(T)} is not supported for encoding {modbusEncoding}.");

					byte[] strBytes = System.Text.Encoding.Unicode.GetBytes((string)(object)value);
					strBytes.AsSpan(0, Math.Min(bytes.Length, strBytes.Length)).CopyTo(bytes);
					break;
				}
			case ModbusEncoding.IEEE754:
				{
					if (!(typeof(T) == typeof(double) || typeof(T) == typeof(float)))
						throw new NotSupportedException($"Type {typeof(T)} is not supported for encoding {modbusEncoding}.");

					switch (bytes.Length)
					{
						case 4:
							MemoryMarshal.Write(bytes, Convert.ToSingle(value));
							break;
						case 8:
							MemoryMarshal.Write(bytes, Convert.ToDouble(value));
							break;
						default:
							throw new NotSupportedException($"Length of {targetLength} words is not supported for modbusEncoding {modbusEncoding}.");
					}
					break;
				}
			case ModbusEncoding.IntAndScaleFactor:
				{
					if (scaleFactor is null)
						throw new ArgumentNullException($"No scale factor provided for encoding {modbusEncoding}");

					if (!(typeof(T) == typeof(float) || typeof(T) == typeof(double) || typeof(T) == typeof(decimal)))
						throw new NotSupportedException($"Type {typeof(T)} is not supported for encoding {modbusEncoding}.");

					switch (bytes.Length)
					{
						case 2 when signed:
							{
								var result = typeof(T) switch
								{
									{ } t when t == typeof(float) => Convert.ToInt16(Convert.ToSingle(value) / (float)scaleFactor),
									{ } t when t == typeof(double) => Convert.ToInt16(Convert.ToDouble(value) / (double)scaleFactor),
									{ } t when t == typeof(decimal) => Convert.ToInt16(Convert.ToDecimal(value) / (decimal)scaleFactor),
									_ => throw new InvalidOperationException($"Programmer error: Type {typeof(T)} is not supported for encoding {modbusEncoding}.")
								};
								MemoryMarshal.Write(bytes, result);
								break;
							}
						case 2 when !signed:
							{
								var result = typeof(T) switch
								{
									{ } t when t == typeof(float) => Convert.ToUInt16(Convert.ToSingle(value) / (float)scaleFactor),
									{ } t when t == typeof(double) => Convert.ToUInt16(Convert.ToDouble(value) / (double)scaleFactor),
									{ } t when t == typeof(decimal) => Convert.ToUInt16(Convert.ToDecimal(value) / (decimal)scaleFactor),
									_ => throw new InvalidOperationException($"Programmer error: Type {typeof(T)} is not supported for encoding {modbusEncoding}.")
								};
								MemoryMarshal.Write(bytes, result);
								break;
							}
						case 4 when signed:
							{
								var result = typeof(T) switch
								{
									{ } t when t == typeof(float) => Convert.ToInt32(Convert.ToSingle(value) / (float)scaleFactor),
									{ } t when t == typeof(double) => Convert.ToInt32(Convert.ToDouble(value) / (double)scaleFactor),
									{ } t when t == typeof(decimal) => Convert.ToInt32(Convert.ToDecimal(value) / (decimal)scaleFactor),
									_ => throw new InvalidOperationException($"Programmer error: Type {typeof(T)} is not supported for encoding {modbusEncoding}.")
								};
								MemoryMarshal.Write(bytes, result);
								break;
							}
						case 4 when !signed:
							{
								var result = typeof(T) switch
								{
									{ } t when t == typeof(float) => Convert.ToUInt32(Convert.ToSingle(value) / (float)scaleFactor),
									{ } t when t == typeof(double) => Convert.ToUInt32(Convert.ToDouble(value) / (double)scaleFactor),
									{ } t when t == typeof(decimal) => Convert.ToUInt32(Convert.ToDecimal(value) / (decimal)scaleFactor),
									_ => throw new InvalidOperationException($"Programmer error: Type {typeof(T)} is not supported for encoding {modbusEncoding}.")
								};
								MemoryMarshal.Write(bytes, result);
								break;
							}
						case 8 when signed:
							{
								var result = typeof(T) switch
								{
									{ } t when t == typeof(float) => Convert.ToInt64(Convert.ToSingle(value) / (float)scaleFactor),
									{ } t when t == typeof(double) => Convert.ToInt64(Convert.ToDouble(value) / (double)scaleFactor),
									{ } t when t == typeof(decimal) => Convert.ToInt64(Convert.ToDecimal(value) / (decimal)scaleFactor),
									_ => throw new InvalidOperationException($"Programmer error: Type {typeof(T)} is not supported for encoding {modbusEncoding}.")
								};
								MemoryMarshal.Write(bytes, result);
								break;
							}
						case 8 when !signed:
							{
								var result = typeof(T) switch
								{
									{ } t when t == typeof(float) => Convert.ToUInt64(Convert.ToSingle(value) / (float)scaleFactor),
									{ } t when t == typeof(double) => Convert.ToUInt64(Convert.ToDouble(value) / (double)scaleFactor),
									{ } t when t == typeof(decimal) => Convert.ToUInt64(Convert.ToDecimal(value) / (decimal)scaleFactor),
									_ => throw new InvalidOperationException($"Programmer error: Type {typeof(T)} is not supported for encoding {modbusEncoding}.")
								};
								MemoryMarshal.Write(bytes, result);
								break;
							}
						default:
							throw new NotSupportedException($"Length of {targetLength} words is not supported for encoding {modbusEncoding}.");
					}

					break;
				}
			case ModbusEncoding.Int:
				{
					if (scaleFactor != null)
						throw new NotSupportedException($"No scale factor supported for encoding {modbusEncoding}");

					if (!(typeof(T) == typeof(bool) || typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte) || typeof(T) == typeof(decimal) || typeof(T) == typeof(double) || typeof(T) == typeof(float) || typeof(T) == typeof(short) | typeof(T) == typeof(ushort) || typeof(T) == typeof(int) || typeof(T) == typeof(uint) || typeof(T) == typeof(long) || typeof(T) == typeof(ulong) || typeof(T) == typeof(Int128) || typeof(T) == typeof(UInt128)))
						throw new NotSupportedException($"Type {typeof(T)} is not supported for encoding {modbusEncoding}.");

					switch (bytes.Length)
					{
						case 2 when signed:
							MemoryMarshal.Write(bytes, Convert.ToInt16(value));
							break;
						case 2 when !signed:
							MemoryMarshal.Write(bytes, Convert.ToUInt16(value));
							break;
						case 4 when signed:
							MemoryMarshal.Write(bytes, Convert.ToInt32(value));
							break;
						case 4 when !signed:
							MemoryMarshal.Write(bytes, Convert.ToUInt32(value));
							break;
						case 8 when signed:
							MemoryMarshal.Write(bytes, Convert.ToInt64(value));
							break;
						case 8 when !signed:
							MemoryMarshal.Write(bytes, Convert.ToUInt64(value));
							break;
						case 16 when signed:
							MemoryMarshal.Write(bytes, (Int128)(object)value);
							break;
						case 16 when !signed:
							MemoryMarshal.Write(bytes, (UInt128)(object)value);
							break;
						default:
							throw new NotSupportedException($"Length of {targetLength} words is not supported for encoding {modbusEncoding}.");
					}
					break;
				}
			case ModbusEncoding.IPAddress:
				{
					if (scaleFactor != null)
						throw new NotSupportedException($"No scale factor supported for encoding {modbusEncoding}");

					if (!(typeof(T) == typeof(IPAddress)))
						throw new NotSupportedException($"Type {typeof(T)} is not supported for encoding {modbusEncoding}.");

					var ipBytes = Functions.Endian.BigEndian(((IPAddress)(object)value).GetAddressBytes());

					bytes = ipBytes.Length == bytes.Length ? ipBytes : throw new ArgumentOutOfRangeException($"Length of {targetLength} words is not compatible with length of specified ip-address: {ipBytes.Length} bytes.");
					break;
				}
			case ModbusEncoding.RawBytes:
				{
					if (scaleFactor != null)
						throw new NotSupportedException($"No scale factor supported for encoding {modbusEncoding}");

					if (!(typeof(T) == typeof(byte[])))
						throw new NotSupportedException($"Type {typeof(T)} is not supported for encoding {modbusEncoding}.");

					byte[] rawBytes = (byte[])(object)value;

					bytes = rawBytes.Length == bytes.Length ? rawBytes : throw new ArgumentOutOfRangeException($"Length of {targetLength} words is not compatible with length of specified byte array: {rawBytes.Length} bytes.");
					break;
				}
			default:
				throw new NotSupportedException($"Encoding {modbusEncoding} is not supported.");
		}

		return ToSource(bytes);
	}

	/// <summary>
	/// From source endian modbus register array to host endian byte array.
	/// </summary>
	/// <param name="registers"></param>
	/// <returns></returns>
	public byte[] FromSource(ushort[] registers)
	{
		// Apply word swap
		if (WordSwap)
			Array.Reverse(registers);

		//Copy to byte array
		var bytes = MemoryMarshal.Cast<ushort, byte>(registers).ToArray();

		//Convert from source endian.
		return Endian(bytes);
	}

	/// <summary>
	/// From host endian byte array to source modbus register array.
	/// </summary>
	/// <param name="bytes"></param>
	/// <returns></returns>
	public ushort[] ToSource(byte[] bytes)
	{
		//Convert to source endian.
		bytes = Endian(bytes);

		//Copy to register array
		var registers = MemoryMarshal.Cast<byte, ushort>(bytes).ToArray();

		// Apply word swap
		if (WordSwap)
			Array.Reverse(registers);

		return registers;
	}
}