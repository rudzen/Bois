﻿using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;

namespace Salar.Bois.Serializers
{
	internal static class PrimitiveWriter
	{
		/// <summary>
		/// there is no data and the value is null
		/// </summary>
		internal static void WriteNullValue(BinaryWriter writer)
		{
			writer.Write(NumericSerializers.FlagNullable);
		}


		/// <summary>
		/// String - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, string str, Encoding encoding)
		{
			if (str == null)
			{
				WriteNullValue(writer);
			}
			else if (str.Length == 0)
			{
				NumericSerializers.WriteVarInt(writer, (int?)0);
			}
			else
			{
				var strBytes = encoding.GetBytes(str);
				// Int32
				NumericSerializers.WriteVarInt(writer, (int?)strBytes.Length);
				writer.Write(strBytes);
			}
		}

		/// <summary>
		/// char - Format: (Embedded-0-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..127
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, char c)
		{
			writer.Write((ushort)c);
		}

		/// <summary>
		/// char? - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, char? c)
		{
			NumericSerializers.WriteVarInt(writer, (ushort?)c);
		}

		/// <summary>
		/// bool - Format: (Embedded=true-0-0-0-0-0-0-0)
		/// Embeddable range: 0..127
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, bool b)
		{
			writer.Write(b);
		}

		/// <summary>
		/// bool? - Format: (Embedded=true-Nullable-0-0-0-0-0-0)
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, bool? b)
		{
			byte? val = null;
			if (b.HasValue)
				val = b.Value ? (byte)1 : (byte)0;

			NumericSerializers.WriteVarInt(writer, val);
		}

		/// <summary>
		/// DateTime - Format: (Kind:0-0-0-0-0-0-0-0) (dateTimeTicks:Embedded-0-0-0-0-0-0-0)[if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable kind range: always embeded
		/// Embeddable ticks range: 0..127
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, DateTime dateTime)
		{
			var kind = (byte)dateTime.Kind;

			if (dateTime == DateTime.MinValue)
			{
				writer.Write(kind);
				// min datetime indicator
				NumericSerializers.WriteVarInt(writer, 0L);
			}
			else if (dateTime == DateTime.MaxValue)
			{
				writer.Write(kind);
				// max datetime indicator
				NumericSerializers.WriteVarInt(writer, 1L);
			}
			else
			{
				writer.Write(kind);
				//Int64
				NumericSerializers.WriteVarInt(writer, dateTime.Ticks);
			}
		}

		/// <summary>
		/// DateTime? - Format: (Kind:Nullable-0-0-0-0-0-0-0) (dateTimeTicks:Embedded-0-0-0-0-0-0-0)[if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable kind range: always embeded
		/// Embeddable ticks range: 0..127
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, DateTime? dt)
		{
			if (dt == null)
			{
				WriteNullValue(writer);
				return;
			}
			var dateTime = dt.Value;
			var kind = (byte?)dateTime.Kind;

			if (dateTime == DateTime.MinValue)
			{
				NumericSerializers.WriteVarInt(writer, kind);
				// min datetime indicator
				NumericSerializers.WriteVarInt(writer, 0L);
			}
			else if (dateTime == DateTime.MaxValue)
			{
				NumericSerializers.WriteVarInt(writer, kind);
				// max datetime indicator
				NumericSerializers.WriteVarInt(writer, 1L);
			}
			else
			{
				NumericSerializers.WriteVarInt(writer, kind);
				//Int64
				NumericSerializers.WriteVarInt(writer, dateTime.Ticks);
			}
		}

		/// <summary>
		/// DateTimeOffset - Format: (Offset:Embedded-0-0-0-0-0-0-0)[if ofset not embedded?0-0-0-0-0-0-0-0] (dateTimeOffsetTicks:Embedded-0-0-0-0-0-0-0)[if ticks not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable offset range: 0..127
		/// Embeddable ticks range: 0..127
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, DateTimeOffset dateTimeOffset)
		{
			var offset = dateTimeOffset.Offset;
			short offsetMinutes;
			unchecked
			{
				offsetMinutes = (short)((offset.Hours * 60) + offset.Minutes);
			}
			// int16
			NumericSerializers.WriteVarInt(writer, offsetMinutes);

			// int64
			NumericSerializers.WriteVarInt(writer, dateTimeOffset.Ticks);
		}

		/// <summary>
		/// DateTimeOffset? - Format: (Offset:Embedded-Nullable-0-0-0-0-0-0)[if ofset not embedded?0-0-0-0-0-0-0-0] (dateTimeOffsetTicks:Embedded-0-0-0-0-0-0-0)[if ticks not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable offset range: 0..63
		/// Embeddable ticks range: 0..127
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, DateTimeOffset? dto)
		{
			if (dto == null)
			{
				WriteNullValue(writer);
				return;
			}
			var dateTimeOffset = dto.Value;

			var offset = dateTimeOffset.Offset;
			short? offsetMinutes;
			unchecked
			{
				offsetMinutes = (short)((offset.Hours * 60) + offset.Minutes);
			}
			// int16
			NumericSerializers.WriteVarInt(writer, offsetMinutes);

			// int64
			NumericSerializers.WriteVarInt(writer, dateTimeOffset.Ticks);
		}

		/// <summary>
		/// byte[] - Format: (Array Length:Embedded-Nullable-0-0-0-0-0-0) [if array length not embedded?0-0-0-0-0-0-0-0] (data:0-0-0-0-0-0-0-0)
		/// Embeddable Array Length range: 0..63
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, byte[] bytes)
		{
			if (bytes == null)
			{
				WriteNullValue(writer);
				return;
			}

			// uint doesn't deal with negative numbers
			NumericSerializers.WriteVarInt(writer, (uint?)bytes.Length);
			writer.Write(bytes);
		}

		/// <summary>
		/// String - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, Enum e)
		{
			if (e == null)
			{
				WriteNullValue(writer);
				return;
			}
			// Int32
			NumericSerializers.WriteVarInt(writer, (int?)((object)e));
		}


		/// <summary>
		/// TimeSpan - Format: (Embedded-0-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..127
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, TimeSpan timeSpan)
		{
			NumericSerializers.WriteVarInt(writer, timeSpan.Ticks);
		}

		/// <summary>
		/// TimeSpan? - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, TimeSpan? timeSpan)
		{
			if (timeSpan == null)
			{
				WriteNullValue(writer);
				return;
			}
			NumericSerializers.WriteVarInt(writer, (long?)timeSpan.Value.Ticks);
		}

		/// <summary>
		/// Version - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, Version version)
		{
			if (version == null)
			{
				WriteNullValue(writer);
				return;
			}
			WriteValue(writer, version.ToString(), Encoding.ASCII);
		}

		/// <summary>
		/// Same as String
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, Uri uri)
		{
			PrimitiveWriter.WriteValue(writer, uri?.ToString(), Encoding.UTF8);
		}

		/// <summary>
		/// Guid - Format: (Embedded-0-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..127
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, Guid guid)
		{
			if (guid == Guid.Empty)
			{
				// Int32
				NumericSerializers.WriteVarInt(writer, (uint)0);
				return;
			}

			var data = guid.ToByteArray();

			// Int32
			NumericSerializers.WriteVarInt(writer, (uint)data.Length);
			writer.Write(data);
		}

		/// <summary>
		/// Guid? - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, Guid? g)
		{
			if (g == null)
			{
				WriteNullValue(writer);
				return;
			}

			var guid = g.Value;
			if (guid == Guid.Empty)
			{
				// Int32
				NumericSerializers.WriteVarInt(writer, (uint?)0);
				return;
			}

			var data = guid.ToByteArray();

			// Int32
			NumericSerializers.WriteVarInt(writer, (uint?)data.Length);
			writer.Write(data);
		}

		/// <summary>
		/// DBNull? - Format: (Embedded=true-Nullable=true-0-0-0-0-0-0)
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, DBNull dbNull)
		{
			WriteNullValue(writer);
		}
		/// <summary>
		/// Same as Int32
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, Color color)
		{
			int argb = color.ToArgb();
			// Int32
			NumericSerializers.WriteVarInt(writer, argb);
		}

		/// <summary>
		/// Same as Nullable<Int32>
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, Color? color)
		{
			if (color == null)
			{
				WriteNullValue(writer);
				return;
			}
			int? argb = color.Value.ToArgb();
			// Int32
			NumericSerializers.WriteVarInt(writer, argb);
		}

		/// <summary>
		/// Obsolete - only background compability
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, DataSet ds, Encoding encoding)
		{
			if (ds == null)
			{
				WriteNullValue(writer);
				return;
			}
			var xml = SerializeDataTable(ds);
			WriteValue(writer, xml, encoding);
		}

		/// <summary>
		/// Obsolete - only background compability
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, DataTable dt, Encoding encoding)
		{
			if (dt == null)
			{
				WriteNullValue(writer);
				return;
			}
			var xml = SerializeDataTable(dt);
			WriteValue(writer, xml, encoding);
		}



		#region Private helpers

		private static string SerializeDataTable(DataTable dt)
		{
			using (var writer = new StringWriter())
			{
				dt.WriteXml(writer, XmlWriteMode.WriteSchema);
				return writer.ToString();
			}
		}

		private static string SerializeDataTable(DataSet dt)
		{
			using (var writer = new StringWriter())
			{
				dt.WriteXml(writer, XmlWriteMode.WriteSchema);
				return writer.ToString();
			}
		}
		
		#endregion

	}
}