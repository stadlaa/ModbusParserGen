using System.Collections;
using System.Net;
using ModbusParserGen.Functions;
using System.Linq;

namespace ModbusParserGen.Tests
{
	public class ModbusParserTests
	{
		[Theory]
		[ClassData(typeof(ModbusParserRoundTripTestDatasets))]
		public void RoundTripTest<T>(RoundTripParameters param, T value, T expected) where T : notnull
		{
			ModbusParser parser = new ModbusParser(param.WordSwap,param.EndianFunction);
			var registers = parser.Serialize(value, param.TargetLength, param.ValueEncoding, param.Signed, param.ScaleFactor);
			var result = parser.Deserialize<T>(registers, param.ValueEncoding, param.Signed, param.ScaleFactor);

			if (typeof(T) == typeof(double))
                if (Double.IsInfinity((double)(object)value))
                    Assert.True(Double.IsInfinity((double)(object)result), $"{result} != {expected}");
                else
                    Assert.True(Math.Abs((double)(object)result-(double)(object)expected) < 0.00001,$"{result} != {expected}");
			

			if (typeof(T) == typeof(float))
                if(Single.IsInfinity((float)(object)value))
                    Assert.True(Single.IsInfinity((float)(object)result), $"{result} != {expected}");
                else
                    Assert.True(Math.Abs((float)(object)result - (float)(object)expected) < 0.00001, $"{result} != {expected}");
			
            if(typeof(T)==typeof(IPAddress)) 
                Assert.True(Equals(expected,result), $"{result} != {expected}");

            if (typeof(T) == typeof(byte[]))
                Assert.True(((byte[])(object)expected).SequenceEqual((byte[])(object)result), $"{result} != {expected}");

			Assert.Equal(expected, result);
		}
		
	}

	public class ModbusParserRoundTripTestDatasets : IEnumerable<object[]>
	{
		public IEnumerator<object[]> GetEnumerator()
		{ 
			foreach (var param in RoundTripParameterVariations)
                yield return
                [ //Utf 8 string normal
                    new RoundTripParameters()
                    {
                        TargetLength = 6,
                        ValueEncoding = ModbusEncoding.UTF8,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (string)"Hello World!",
					(string)"Hello World!"
                ];

            foreach (var param in RoundTripParameterVariations)
                yield return
                [ //Utf 8 string longer than target length
                    new RoundTripParameters()
                    {
                        TargetLength = 5,
                        ValueEncoding = ModbusEncoding.UTF8,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (string)"Hello World!",
                    (string)"Hello Worl"
                ];
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ //Utf 8 string shorter than target length
                    new RoundTripParameters()
                    {
                        TargetLength = 7,
                        ValueEncoding = ModbusEncoding.UTF8,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (string)"Hello World!!",
                    (string)"Hello World!!"
                ];
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ //Utf 8 string with multiple byte chars
                    new RoundTripParameters()
                    {
                        TargetLength = 8,
                        ValueEncoding = ModbusEncoding.UTF8,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (string)"Hello World!\u2502",
                    (string)"Hello World!\u2502"
                ];
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ //Utf 16 string normal
                    new RoundTripParameters()
                    {
                        TargetLength = 12,
                        ValueEncoding = ModbusEncoding.UTF16,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (string)"Hello World!",
                    (string)"Hello World!"
                ];
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ //Utf 16 string longer than target length
                    new RoundTripParameters()
                    {
                        TargetLength = 11,
                        ValueEncoding = ModbusEncoding.UTF16,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (string)"Hello World!",
                    (string)"Hello World"
                ];
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ //Utf 16 string shorter than target length
                    new RoundTripParameters()
                    {
                        TargetLength = 13,
                        ValueEncoding = ModbusEncoding.UTF16,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (string)"Hello World!",
                    (string)"Hello World!"
                ]; 
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ //IEEE754 32 bit negative value
                    new RoundTripParameters()
                    {
                        TargetLength = 2,
                        ValueEncoding = ModbusEncoding.IEEE754,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (float)-3.4E-38,
                    (float)-3.4E-38
                ];
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ //IEEE754 32 bit positive value
                    new RoundTripParameters()
                    {
                        TargetLength = 2,
                        ValueEncoding = ModbusEncoding.IEEE754,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (float)+3.4E38,
                    (float)+3.4E38
                ]; 
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ //IEEE754 32 bit infinity value
                    new RoundTripParameters()
                    {
                        TargetLength = 2,
                        ValueEncoding = ModbusEncoding.IEEE754,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    Single.PositiveInfinity,
                    Single.PositiveInfinity
                ];
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ //IEEE754 64 bit negative smallest value
                    new RoundTripParameters()
                    {
                        TargetLength = 4,
                        ValueEncoding = ModbusEncoding.IEEE754,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    -1.7E-308,
                    -1.7E-308
                ];
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ //IEEE754 64 bit positive biggest value
                    new RoundTripParameters()
                    {
                        TargetLength = 4,
                        ValueEncoding = ModbusEncoding.IEEE754,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    1.7E+308,
                    1.7E+308
                ];
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ //IEEE754 64 bit infinity value
                    new RoundTripParameters()
                    {
                        TargetLength = 4,
                        ValueEncoding = ModbusEncoding.IEEE754,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    Double.PositiveInfinity,
                    Double.PositiveInfinity
                ];
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ // Encoding Int specifying longer target length than needed
                    new RoundTripParameters()
                    {
                        TargetLength = 2,
                        ValueEncoding = ModbusEncoding.Int,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (ushort)500,
                    (ushort)500
                ];
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ // Encoding Int bool normal
					new RoundTripParameters()
					{
						TargetLength = 1,
						ValueEncoding = ModbusEncoding.Int,
						ScaleFactor = null,
						EndianFunction = param.EndianFunction,
						Signed = param.Signed,
						WordSwap = param.WordSwap
					},
					true,
					true
				];
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ // Encoding Int Byte high value
                    new RoundTripParameters()
                    {
                        TargetLength = 1,
                        ValueEncoding = ModbusEncoding.Int,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = param.Signed,
                        WordSwap = param.WordSwap
                    },
                    (byte)250,
                    (byte)250
                ]; 
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ // Encoding Int SByte negative
                    new RoundTripParameters()
                    {
                        TargetLength = 1,
                        ValueEncoding = ModbusEncoding.Int,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = true,
                        WordSwap = param.WordSwap
                    },
                    (sbyte)-100,
                    (sbyte)-100
                ];
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ // Encoding Int Ushort high value
                    new RoundTripParameters()
                    {
                        TargetLength = 1,
                        ValueEncoding = ModbusEncoding.Int,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = false,
                        WordSwap = param.WordSwap
                    },
                    (ushort)60000,
                    (ushort)60000
                ];
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ // Encoding Int Short negative
                    new RoundTripParameters()
                    {
                        TargetLength = 1,
                        ValueEncoding = ModbusEncoding.Int,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = true,
                        WordSwap = param.WordSwap
                    },
                    (short)-500,
                    (short)-500
                ]; 
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ // Encoding Int Double
                    new RoundTripParameters()
                    {
                        TargetLength = 1,
                        ValueEncoding = ModbusEncoding.Int,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = false,
                        WordSwap = param.WordSwap
                    },
                    (double)60000,
                    (double)60000
                ]; 
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ // Encoding Int Double negative
                    new RoundTripParameters()
                    {
                        TargetLength = 1,
                        ValueEncoding = ModbusEncoding.Int,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = true,
                        WordSwap = param.WordSwap
                    },
                    (double)-5000,
                    (double)-5000
                ];
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ // Encoding Int Unt32 with length of Uint64
                    new RoundTripParameters()
                    {
                        TargetLength = 4,
                        ValueEncoding = ModbusEncoding.Int,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = false,
                        WordSwap = param.WordSwap
                    },
                    UInt32.MaxValue,
                    UInt32.MaxValue
                ];
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ // Encoding Int Unt128 high value
                    new RoundTripParameters()
                    {
                        TargetLength = 8,
                        ValueEncoding = ModbusEncoding.Int,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = false,
                        WordSwap = param.WordSwap
                    },
                    UInt128.MaxValue,
                    UInt128.MaxValue
                ]; 
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ // Encoding Int Int128 normal
                    new RoundTripParameters()
                    {
                        TargetLength = 8,
                        ValueEncoding = ModbusEncoding.Int,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = true,
                        WordSwap = param.WordSwap
                    },
                    Int128.MinValue,
                    Int128.MinValue
                ];
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ // Encoding Int Int128 normal
                    new RoundTripParameters()
                    {
                        TargetLength = 8,
                        ValueEncoding = ModbusEncoding.Int,
                        ScaleFactor = null,
                        EndianFunction = param.EndianFunction,
                        Signed = true,
                        WordSwap = param.WordSwap
                    },
                    Int128.MinValue,
                    Int128.MinValue
                ]; foreach (var param in RoundTripParameterVariations)
                yield return
                [ // Encoding IntAndScaleFactor double
                    new RoundTripParameters()
                    {
                        TargetLength = 4,
                        ValueEncoding = ModbusEncoding.IntAndScaleFactor,
                        ScaleFactor = 0.01,
                        EndianFunction = param.EndianFunction,
                        Signed = true,
                        WordSwap = param.WordSwap
                    },
                    5.88,
                    5.88
                ];
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ // Encoding IntAndScaleFactor double
                    new RoundTripParameters()
                    {
                        TargetLength = 4,
                        ValueEncoding = ModbusEncoding.IntAndScaleFactor,
                        ScaleFactor = 0.01,
                        EndianFunction = param.EndianFunction,
                        Signed = true,
                        WordSwap = param.WordSwap
                    },
                    -5.88,
                    -5.88
                ];
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ // Encoding IntAndScaleFactor ushort high value
                    new RoundTripParameters()
                    {
                        TargetLength = 2,
                        ValueEncoding = ModbusEncoding.IntAndScaleFactor,
                        ScaleFactor = 0.01,
                        EndianFunction = param.EndianFunction,
                        Signed = false,
                        WordSwap = param.WordSwap
                    },
                    UInt16.MaxValue,
                    UInt16.MaxValue
                ];
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ // Encoding IntAndScaleFactor ushort small value only one register
                    new RoundTripParameters()
                    {
                        TargetLength = 1,
                        ValueEncoding = ModbusEncoding.IntAndScaleFactor,
                        ScaleFactor = 0.01,
                        EndianFunction = param.EndianFunction,
                        Signed = false,
                        WordSwap = param.WordSwap
                    },
                    (ushort)50,
                    (ushort)50
                ];
            foreach (var param in RoundTripParameterVariations)
                yield return
                [ // Encoding IntAndScaleFactor short negative value
                    new RoundTripParameters()
                    {
                        TargetLength = 2,
                        ValueEncoding = ModbusEncoding.IntAndScaleFactor,
                        ScaleFactor = 0.01,
                        EndianFunction = param.EndianFunction,
                        Signed = true,
                        WordSwap = param.WordSwap
                    },
                    (double)-5000,
                    (double)-5000
                ]; 
            foreach (var param in RoundTripParameterVariations)
	            yield return
	            [ // Encoding IPAddress ipv4 value
		            new RoundTripParameters()
		            {
			            TargetLength = 2,
			            ValueEncoding = ModbusEncoding.IPAddress,
			            ScaleFactor = null,
			            EndianFunction = param.EndianFunction,
			            Signed = param.Signed,
			            WordSwap = param.WordSwap
		            },
		            IPAddress.Parse("10.0.0.228"),
					IPAddress.Parse("10.0.0.228")
				];
            foreach (var param in RoundTripParameterVariations)
	            yield return
	            [ // Encoding IPAddress ipv6 value
		            new RoundTripParameters()
		            {
			            TargetLength = 8,
			            ValueEncoding = ModbusEncoding.IPAddress,
			            ScaleFactor = null,
			            EndianFunction = param.EndianFunction,
			            Signed = param.Signed,
			            WordSwap = param.WordSwap
		            },
		            IPAddress.Parse("2001:db8:abcd:1234:4c3a:21ff:fe56:abcd"),
		            IPAddress.Parse("2001:db8:abcd:1234:4c3a:21ff:fe56:abcd")
	            ];

            foreach (var param in RoundTripParameterVariations)
	            yield return
	            [ // Encoding Raw bytes
		            new RoundTripParameters()
		            {
			            TargetLength = 9,
			            ValueEncoding = ModbusEncoding.RawBytes,
			            ScaleFactor = null,
			            EndianFunction = param.EndianFunction,
			            Signed = param.Signed,
			            WordSwap = param.WordSwap
		            },
		            new byte[] {0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17},
					new byte[] {0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17}
				];
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public List<RoundTripParameters> RoundTripParameterVariations = new List<RoundTripParameters>()
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
		public ModbusEncoding ValueEncoding;
		public bool Signed;
		public double? ScaleFactor;
	}
}
