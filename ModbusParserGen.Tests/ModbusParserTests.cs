using System.Collections;
using ModbusParserGen.Functions;

namespace ModbusParserGen.Tests
{
	public class ModbusParserTests
	{
		[Theory]
		[ClassData(typeof(ModbusParserDeserializeTestDatasets))]
		public void RoundTrip<T>(RoundTripParameters param, T value, T expected) where T : notnull
		{
			ModbusParser parser = new ModbusParser(param.WordSwap,param.EndianFunction);
			var registers = parser.Serialize(value, param.TargetLength, param.ValueEncoding, param.Signed, param.ScaleFactor);
			var result = parser.Deserialize<T>(registers, param.ValueEncoding, param.Signed, param.ScaleFactor);

			if (typeof(T) == typeof(double))
			{
                if (Double.IsInfinity((double)(object)value))
                    Assert.True(Double.IsInfinity((double)(object)result), $"{result} != {expected}");
                else
                    Assert.True(Math.Abs((double)(object)result-(double)(object)expected) < 0.00001,$"{result} != {expected}");
			}

			if (typeof(T) == typeof(float))
			{
                if(Single.IsInfinity((float)(object)value))
                    Assert.True(Single.IsInfinity((float)(object)result), $"{result} != {expected}");
                else
                    Assert.True(Math.Abs((float)(object)result - (float)(object)expected) < 0.00001, $"{result} != {expected}");
			}

			Assert.Equal(expected, result);
		}
		
	}

	public class ModbusParserDeserializeTestDatasets : IEnumerable<object[]>
	{
		public IEnumerator<object[]> GetEnumerator()
		{ 
			foreach (var param in RoundTripParameters)
                yield return
                [ //Utf 8 string normal
                    new RoundTripParameters()
                    {
                        TargetLength = 6,
                        ValueEncoding = Encoding.UTF8,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (string)"Hello World!",
					(string)"Hello World!"
                ];

            foreach (var param in RoundTripParameters)
                yield return
                [ //Utf 8 string longer than target length
                    new RoundTripParameters()
                    {
                        TargetLength = 5,
                        ValueEncoding = Encoding.UTF8,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (string)"Hello World!",
                    (string)"Hello Worl"
                ];
            foreach (var param in RoundTripParameters)
                yield return
                [ //Utf 8 string shorter than target length
                    new RoundTripParameters()
                    {
                        TargetLength = 7,
                        ValueEncoding = Encoding.UTF8,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (string)"Hello World!!",
                    (string)"Hello World!!"
                ];
            foreach (var param in RoundTripParameters)
                yield return
                [ //Utf 8 string with multiple byte chars
                    new RoundTripParameters()
                    {
                        TargetLength = 8,
                        ValueEncoding = Encoding.UTF8,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (string)"Hello World!\u2502",
                    (string)"Hello World!\u2502"
                ];
            foreach (var param in RoundTripParameters)
                yield return
                [ //Utf 16 string normal
                    new RoundTripParameters()
                    {
                        TargetLength = 12,
                        ValueEncoding = Encoding.UTF16,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (string)"Hello World!",
                    (string)"Hello World!"
                ];
            foreach (var param in RoundTripParameters)
                yield return
                [ //Utf 16 string longer than target length
                    new RoundTripParameters()
                    {
                        TargetLength = 11,
                        ValueEncoding = Encoding.UTF16,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (string)"Hello World!",
                    (string)"Hello World"
                ];
            foreach (var param in RoundTripParameters)
                yield return
                [ //Utf 16 string shorter than target length
                    new RoundTripParameters()
                    {
                        TargetLength = 13,
                        ValueEncoding = Encoding.UTF16,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (string)"Hello World!",
                    (string)"Hello World!"
                ]; 
            foreach (var param in RoundTripParameters)
                yield return
                [ //IEEE754 32 bit negative value
                    new RoundTripParameters()
                    {
                        TargetLength = 2,
                        ValueEncoding = Encoding.IEEE754,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (float)-3.4E-38,
                    (float)-3.4E-38
                ];
            foreach (var param in RoundTripParameters)
                yield return
                [ //IEEE754 32 bit positive value
                    new RoundTripParameters()
                    {
                        TargetLength = 2,
                        ValueEncoding = Encoding.IEEE754,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (float)+3.4E38,
                    (float)+3.4E38
                ]; 
            foreach (var param in RoundTripParameters)
                yield return
                [ //IEEE754 32 bit infinity value
                    new RoundTripParameters()
                    {
                        TargetLength = 2,
                        ValueEncoding = Encoding.IEEE754,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (float)Single.PositiveInfinity,
                    (float)Single.PositiveInfinity
                ];
            foreach (var param in RoundTripParameters)
                yield return
                [ //IEEE754 64 bit negative smallest value
                    new RoundTripParameters()
                    {
                        TargetLength = 4,
                        ValueEncoding = Encoding.IEEE754,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (double)-1.7E-308,
                    (double)-1.7E-308
                ];
            foreach (var param in RoundTripParameters)
                yield return
                [ //IEEE754 64 bit positive biggest value
                    new RoundTripParameters()
                    {
                        TargetLength = 4,
                        ValueEncoding = Encoding.IEEE754,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (double)1.7E+308,
                    (double)1.7E+308
                ];
            foreach (var param in RoundTripParameters)
                yield return
                [ //IEEE754 64 bit infinity value
                    new RoundTripParameters()
                    {
                        TargetLength = 4,
                        ValueEncoding = Encoding.IEEE754,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (double)Double.PositiveInfinity,
                    (double)Double.PositiveInfinity
                ];
            foreach (var param in RoundTripParameters)
                yield return
                [ // Encoding Int specifying longer target length than needed
                    new RoundTripParameters()
                    {
                        TargetLength = 2,
                        ValueEncoding = Encoding.Int,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (ushort)500,
                    (ushort)500
                ];
            foreach (var param in RoundTripParameters)
                yield return
                [ // Encoding Int bool normal
					new RoundTripParameters()
					{
						TargetLength = 1,
						ValueEncoding = Encoding.Int,
						ScaleFactor = null,
						EndianFunction = param.EndianFunction,
						Signed = param.Signed,
						WordSwap = param.WordSwap
					},
					(bool)true,
					(bool)true
				];
            foreach (var param in RoundTripParameters)
                yield return
                [ // Encoding Int Byte high value
                    new RoundTripParameters()
                    {
                        TargetLength = 1,
                        ValueEncoding = Encoding.Int,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (byte)250,
                    (byte)250
                ]; 
            foreach (var param in RoundTripParameters)
                yield return
                [ // Encoding Int SByte negative
                    new RoundTripParameters()
                    {
                        TargetLength = 1,
                        ValueEncoding = Encoding.Int,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = true,
                        WordSwap = param.WordSwap
                    },
                    (sbyte)-100,
                    (sbyte)-100
                ];
            foreach (var param in RoundTripParameters)
                yield return
                [ // Encoding Int Ushort high value
                    new RoundTripParameters()
                    {
                        TargetLength = 1,
                        ValueEncoding = Encoding.Int,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = false,
                        WordSwap = param.WordSwap
                    },
                    (ushort)60000,
                    (ushort)60000
                ];
            foreach (var param in RoundTripParameters)
                yield return
                [ // Encoding Int Short negative
                    new RoundTripParameters()
                    {
                        TargetLength = 1,
                        ValueEncoding = Encoding.Int,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = true,
                        WordSwap = param.WordSwap
                    },
                    (short)-500,
                    (short)-500
                ]; 
            foreach (var param in RoundTripParameters)
                yield return
                [ // Encoding Int Double
                    new RoundTripParameters()
                    {
                        TargetLength = 1,
                        ValueEncoding = Encoding.Int,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = false,
                        WordSwap = param.WordSwap
                    },
                    (double)60000,
                    (double)60000
                ]; 
            foreach (var param in RoundTripParameters)
                yield return
                [ // Encoding Int Double negative
                    new RoundTripParameters()
                    {
                        TargetLength = 1,
                        ValueEncoding = Encoding.Int,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = true,
                        WordSwap = param.WordSwap
                    },
                    (double)-5000,
                    (double)-5000
                ];
            foreach (var param in RoundTripParameters)
                yield return
                [ // Encoding Int Unt32 with length of Uint64
                    new RoundTripParameters()
                    {
                        TargetLength = 4,
                        ValueEncoding = Encoding.Int,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = false,
                        WordSwap = param.WordSwap
                    },
                    UInt32.MaxValue,
                    UInt32.MaxValue
                ];
            foreach (var param in RoundTripParameters)
                yield return
                [ // Encoding Int Unt128 high value
                    new RoundTripParameters()
                    {
                        TargetLength = 8,
                        ValueEncoding = Encoding.Int,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = false,
                        WordSwap = param.WordSwap
                    },
                    (UInt128)UInt128.MaxValue,
                    (UInt128)UInt128.MaxValue
                ]; 
            foreach (var param in RoundTripParameters)
                yield return
                [ // Encoding Int Int128 normal
                    new RoundTripParameters()
                    {
                        TargetLength = 8,
                        ValueEncoding = Encoding.Int,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = true,
                        WordSwap = param.WordSwap
                    },
                    (Int128)Int128.MinValue,
                    (Int128)Int128.MinValue
                ];
            foreach (var param in RoundTripParameters)
                yield return
                [ // Encoding Int Int128 normal
                    new RoundTripParameters()
                    {
                        TargetLength = 8,
                        ValueEncoding = Encoding.Int,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = true,
                        WordSwap = param.WordSwap
                    },
                    (Int128)Int128.MinValue,
                    (Int128)Int128.MinValue
                ]; foreach (var param in RoundTripParameters)
                yield return
                [ // Encoding IntAndScaleFactor double
                    new RoundTripParameters()
                    {
                        TargetLength = 4,
                        ValueEncoding = Encoding.IntAndScaleFactor,
                        ScaleFactor = 0.01,
                        EndianFunction = param.EndianFunction,
                        Signed = true,
                        WordSwap = param.WordSwap
                    },
                    (double)5.88,
                    (double)5.88
                ];
            foreach (var param in RoundTripParameters)
                yield return
                [ // Encoding IntAndScaleFactor double
                    new RoundTripParameters()
                    {
                        TargetLength = 4,
                        ValueEncoding = Encoding.IntAndScaleFactor,
                        ScaleFactor = 0.01,
                        EndianFunction = param.EndianFunction,
                        Signed = true,
                        WordSwap = param.WordSwap
                    },
                    (double)-5.88,
                    (double)-5.88
                ];
            foreach (var param in RoundTripParameters)
                yield return
                [ // Encoding IntAndScaleFactor ushort high value
                    new RoundTripParameters()
                    {
                        TargetLength = 2,
                        ValueEncoding = Encoding.IntAndScaleFactor,
                        ScaleFactor = 0.01,
                        EndianFunction = param.EndianFunction,
                        Signed = false,
                        WordSwap = param.WordSwap
                    },
                    UInt16.MaxValue,
                    UInt16.MaxValue
                ];
            foreach (var param in RoundTripParameters)
                yield return
                [ // Encoding IntAndScaleFactor ushort small value only one register
                    new RoundTripParameters()
                    {
                        TargetLength = 1,
                        ValueEncoding = Encoding.IntAndScaleFactor,
                        ScaleFactor = 0.01,
                        EndianFunction = param.EndianFunction,
                        Signed = false,
                        WordSwap = param.WordSwap
                    },
                    (ushort)50,
                    (ushort)50
                ];
            foreach (var param in RoundTripParameters)
                yield return
                [ // Encoding IntAndScaleFactor short negative value
                    new RoundTripParameters()
                    {
                        TargetLength = 2,
                        ValueEncoding = Encoding.IntAndScaleFactor,
                        ScaleFactor = 0.01,
                        EndianFunction = param.EndianFunction,
                        Signed = true,
                        WordSwap = param.WordSwap
                    },
                    (double)-5000,
                    (double)-5000
                ];
        }

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public List<RoundTripParameters> RoundTripParameters = new List<RoundTripParameters>()
		{
			new(){EndianFunction = Endian.LittleEndian, WordSwap = false, Signed = false},
			new(){EndianFunction = Endian.LittleEndian, WordSwap = true, Signed = false},
			new(){EndianFunction = Endian.LittleEndian, WordSwap = false, Signed = true},
			new(){EndianFunction = Endian.LittleEndian, WordSwap = true, Signed = true},
			new(){EndianFunction = Endian.BigEndian, WordSwap = false, Signed = false},
			new(){EndianFunction = Endian.BigEndian, WordSwap = true, Signed = false},
			new(){EndianFunction = Endian.BigEndian, WordSwap = false, Signed = true},
			new(){EndianFunction = Endian.BigEndian, WordSwap = true, Signed = true},
		};
	}

	public class RoundTripParameters
	{
		public bool WordSwap;
		public Func<byte[], byte[]> EndianFunction= Endian.BigEndian;
		public int TargetLength;
		public Encoding ValueEncoding;
		public bool Signed;
		public double? ScaleFactor;
	}
}
