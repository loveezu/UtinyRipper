﻿using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.Classes.AnimatorControllers
{
	public struct SkeletonMask : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			m_data = reader.ReadArray<SkeletonMaskElement>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public IReadOnlyList<SkeletonMaskElement> Data => m_data;

		private SkeletonMaskElement[] m_data;
	}
}
