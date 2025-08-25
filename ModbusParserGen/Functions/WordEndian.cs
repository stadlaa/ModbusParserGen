namespace SharpSunSpec.Modbus.Functions;

/// <summary>
///     Functions to convert source endian of words to target endian.
/// </summary>
public static class WordEndian
{
	/// <summary>
	///     Converts from big endian words to target endian byte array.
	/// </summary>
	public static Func<ushort[], byte[]> FromBigEndian = words =>
	{
		byte[] bytes = new byte[2 * words.Length];

		// Always bring words to big endian byte order
		for (int i = 0; i < words.Length; i++)
			BitConverter.GetBytes(words[i]).CopyTo(bytes, i * 2);

		//convert to target endian
		return Endian.FromBigEndian(bytes);
	};

	/// <summary>
	///     Converts from little endian words to target endian byte array.
	/// </summary>
	public static Func<ushort[], byte[]> FromLittleEndian = words =>
	{
		byte[] bytes = new byte[2 * words.Length];

		// Always bring words to big endian byte order
		for (int i = 0; i < words.Length; i++)
			BitConverter.GetBytes(words[i]).Reverse().ToArray().CopyTo(bytes, i * 2);

		//convert to target endian
		return Endian.FromBigEndian(bytes);
	};

	/// <summary>
	///     Converts from mid-big endian words to target endian byte array.
	/// </summary>
	public static Func<ushort[], byte[]> FromMidBigEndian = words => FromBigEndian(words.Reverse().ToArray());

	/// <summary>
	///     Converts from mid-little endian words to target endian byte array.
	/// </summary>
	public static Func<ushort[], byte[]> FromMidLittleEndian = words => FromLittleEndian(words.Reverse().ToArray());
}