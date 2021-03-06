﻿using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public abstract class Object : IAssetReadable, IYAMLDocExportable, IDependent
	{
		protected Object(AssetInfo assetInfo)
		{
			if (assetInfo == null)
			{
				throw new ArgumentNullException(nameof(assetInfo));
			}
			m_assetInfo = assetInfo;
			if (assetInfo.ClassID != ClassID)
			{
				throw new ArgumentException($"Try to initialize '{ClassID}' with '{assetInfo.ClassID}' asset data", nameof(assetInfo));
			}
		}

		protected Object(AssetInfo assetInfo, uint hideFlags):
			this(assetInfo)
		{
			ObjectHideFlags = hideFlags;
		}

		public static bool IsReadHideFlag(TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && !flags.IsForPrefab();
		}
		public static bool IsReadInstanceID(TransferInstructionFlags flags)
		{
			return flags.IsUnknown2();
		}

		public void Read(byte[] buffer)
		{
			using (MemoryStream stream = new MemoryStream(buffer))
			{
				using (AssetReader reader = new AssetReader(stream, File.Version, File.Platform, File.Flags))
				{
					Read(reader);

					if (reader.BaseStream.Position != buffer.Length)
					{
						throw new Exception($"Read less {reader.BaseStream.Position} than expected {buffer.Length}");
					}
				}
			}
		}

		public virtual void Read(AssetReader reader)
		{
			if (IsReadHideFlag(reader.Flags))
			{
				ObjectHideFlags = reader.ReadUInt32();
			}
#if UNIVERSAL
			if (IsReadInstanceID(reader.Flags))
			{
				InstanceID = reader.ReadInt32();
				LocalIdentfierInFile = reader.ReadInt64();
			}
#endif
		}

		/// <summary>
		/// Export object's content in such formats as txt or png
		/// </summary>
		/// <returns>Object's content</returns>
		public virtual void ExportBinary(IExportContainer container, Stream stream)
		{
			throw new NotSupportedException($"Type {GetType()} doesn't support binary export");
		}

		public YAMLDocument ExportYAMLDocument(IExportContainer container)
		{
			YAMLDocument document = new YAMLDocument();
			YAMLMappingNode node = ExportYAMLRoot(container);
			YAMLMappingNode root = document.CreateMappingRoot();
			root.Tag = ClassID.ToInt().ToString();
			root.Anchor = container.GetExportID(this).ToString();
			root.Add(ClassID.ToString(), node);

			return document;
		}

		public IEnumerable<Object> FetchDependencies(bool isLog = false)
		{
			return FetchDependencies(File, isLog);
		}

		public virtual IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
		}

		public virtual string ToLogString()
		{
			return $"{GetType().Name}[{PathID}]";
		}

		protected virtual YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_ObjectHideFlags", GetObjectHideFlags(container.Flags));
			return node;
		}

		private uint GetObjectHideFlags(TransferInstructionFlags flags)
		{
			if(IsReadHideFlag(flags))
			{
				return ObjectHideFlags;
			}
			if(ClassID == ClassIDType.GameObject)
			{
				GameObject go = (GameObject)this;
				int depth = go.GetRootDepth();
				return depth > 1 ? 1u : 0u;
			}
			return 0;
		}

		public ISerializedFile File => m_assetInfo.File;
		public ClassIDType ClassID => m_assetInfo.ClassID;
		public virtual bool IsValid => true;
		public virtual string ExportName => Path.Combine(AssetsKeyWord, ClassID.ToString());
		public virtual string ExportExtension => AssetExtension;
		public long PathID => m_assetInfo.PathID;
		
		public EngineGUID GUID => m_assetInfo.GUID;

		public uint ObjectHideFlags { get; private set; }
#if UNIVERSAL
		public int InstanceID { get; private set; }
		public long LocalIdentfierInFile { get; private set; }
#endif

		public const string AssetsKeyWord = "Assets";
		protected const string AssetExtension = "asset";

		private readonly AssetInfo m_assetInfo;
	}
}
