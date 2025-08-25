namespace SharpSunSpec.Modbus.Functions;

/// <summary>
///     Functions to convert source endian to target endian.
/// </summary>
public static class Endian
{
	/// <summary>
	///     Convert big source endian to target endian.
	/// </summary>
	public static Func<byte[], byte[]> FromBigEndian = bytes =>
	{
		if (BitConverter.IsLittleEndian)
			return bytes.Reverse().ToArray();
		return bytes;
	};

	/// <summary>
	///     Convert little source endian to target endian.
	/// </summary>
	public static Func<byte[], byte[]> FromLittleEndian = bytes =>
	{
		if (!BitConverter.IsLittleEndian)
			return bytes.Reverse().ToArray();
		return bytes;
	};
}