namespace ModbusParserGen.Functions;

/// <summary>
///     Functions to convert source endian to target endian.
/// </summary>
public static class Endian
{
	/// <summary>
	///     Convert big source endian to target endian.
	/// </summary>
	public static Func<byte[], byte[]> BigEndian = bytes =>
	{
		if (BitConverter.IsLittleEndian)
			return bytes.Reverse().ToArray();
		return bytes;
	};

	/// <summary>
	///     Convert little source endian to target endian.
	/// </summary>
	public static Func<byte[], byte[]> LittleEndian = bytes =>
	{
		if (!BitConverter.IsLittleEndian)
			return bytes.Reverse().ToArray();
		return bytes;
	};
}