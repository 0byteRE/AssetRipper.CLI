using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.AudioClips;
using uTinyRipper.Classes.Textures;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class AudioClip : NamedObject
	{
		public AudioClip(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadLoadType(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// Greater than 5.0.0b1
		/// </summary>
		public static bool IsReadIsTrackerFormat(Version version)
		{
#warning unknown
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1);
		}
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool IsReadAmbisonic(Version version)
		{
			return version.IsGreater(2017);
		}

		/// <summary>
		/// 5.0.0b1
		/// </summary>
		public static bool IsReadAudioClipFlags(Version version)
		{
#warning unknown
			return version.IsEqual(5, 0, 0, VersionType.Beta, 1);
		}
		/// <summary>
		/// Greater than 5.0.0b1
		/// </summary>
		public static bool IsReadCompressionFormat(Version version)
		{
#warning unknown
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1);
		}
		/// <summary>
		/// Not Release
		/// </summary>
		public static bool IsReadEditorResource(TransferInstructionFlags flags)
		{
			return !flags.IsRelease();
		}

		/// <summary>
		/// 2.0.0 to 3.0.0 exclusive
		/// </summary>
		public static bool IsReadDecompressOnLoad(Version version)
		{
			return version.IsGreaterEqual(2) && version.IsLess(3);
		}
		/// <summary>
		/// 2.6.0 to 5.0.0b1
		/// </summary>
		public static bool IsReadType(Version version)
		{
#warning unknown top version
			return version.IsGreaterEqual(2, 6) && version.IsLessEqual(5, 0, 0, VersionType.Beta, 1);
		}
		/// <summary>
		/// Less than 2.6.0
		/// </summary>
		public static bool IsReadLength(Version version)
		{
			return version.IsLess(2, 6);
		}
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool IsRead3D(Version version)
		{
			return version.IsGreaterEqual(2, 6);
		}
		/// <summary>
		/// 3.0.0 to 5.0.0 exclusive
		/// </summary>
		public static bool IsReadUseHardware(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 3.2.0 and greater
		/// </summary>
		public static bool IsReadStreamingInfo(Version version)
		{
			return version.IsGreaterEqual(3, 2);
		}
		/// <summary>
		/// 3.0.0 to 5.0.0 exclusive
		/// </summary>
		public static bool IsReadStream(Version version)
		{
			return version.IsGreaterEqual(3);
		}

		/// <summary>
		/// 5.0.0b1
		/// </summary>
		private static bool IsReadFSBResourceFirst(Version version)
		{
#warning unknown
			return version.IsEqual(5, 0, 0, VersionType.Beta, 1);
		}

		/// <summary>
		/// Greater than 5.0.0b1
		/// </summary>
		private static bool IsAlignTrackerFormat(Version version)
		{
#warning unknown
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1);
		}

		/// <summary>
		/// 2.0.x
		/// </summary>
		private static bool IsReadDecompressOnLoadFirst(Version version)
		{
			return version.IsEqual(2, 0);
		}
		/// <summary>
		/// 2.1.0 to 2.6.0 exclusive
		/// </summary>
		private static bool IsReadDecompressOnLoadSecond(Version version)
		{
			return version.IsGreaterEqual(2, 1) && version.IsLess(2, 6);
		}
		/// <summary>
		/// 2.6.0
		/// </summary>
		private static bool IsReadDecompressOnLoadThird(Version version)
		{
			return version.IsEqual(2, 6);
		}
		/// <summary>
		/// 3.2.0 and greater
		/// </summary>
		private static bool IsStreamInt32(Version version)
		{
			return version.IsGreaterEqual(3, 2);
		}
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsAlignBools(Version version)
		{
			return version.IsGreaterEqual(2, 1);
		}
		/// <summary>
		/// 2.1.0 to 3.0.0 exclusive or 3.2.0 and greater
		/// </summary>
		private static bool IsAlignAudioData(Version version)
		{
			return version.IsGreaterEqual(2, 1) && version.IsLess(3) || version.IsGreaterEqual(3, 2);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(5))
			{
				// old AudioClip asset format isn't compatible with new Engine version
				return 1;
			}
			if (version.IsGreaterEqual(3, 5))
			{
				return 4;
			}
			if (version.IsGreaterEqual(2, 6))
			{
				return 3;
			}
			// min version is 2nd
			return 2;
		}

		public bool CheckAssetIntegrity()
		{
			if (IsReadLoadType(File.Version))
			{
				using (ResourcesFile res = File.Collection.FindResourcesFile(File, FSBResource.Source))
				{
					return res != null;
				}
			}
			else if (IsReadStreamingInfo(File.Version))
			{
				if (LoadType == AudioClipLoadType.Streaming)
				{
					if (m_audioData == null)
					{
						using (ResourcesFile res = File.Collection.FindResourcesFile(File, StreamingInfo.Path))
						{
							return res != null;
						}
					}
				}
			}
			return true;
		}

		public IReadOnlyList<byte> GetAudioData()
		{
			if (IsReadLoadType(File.Version))
			{
				using (ResourcesFile res = File.Collection.FindResourcesFile(File, FSBResource.Source))
				{
					if (res == null)
					{
						return new byte[0];
					}

					if (StreamedResource.IsReadSize(File.Version))
					{
						byte[] data = new byte[FSBResource.Size];
						using (PartialStream resStream = new PartialStream(res.Stream, res.Offset, res.Size))
						{
							resStream.Position = FSBResource.Offset;
							resStream.ReadBuffer(data, 0, data.Length);
						}
						return data;
					}
					else
					{
						return new byte[0];
					}
				}
			}
			else
			{
				if (IsReadStreamingInfo(File.Version))
				{
					if (LoadType == AudioClipLoadType.Streaming)
					{
						if (m_audioData == null)
						{
							using (ResourcesFile res = File.Collection.FindResourcesFile(File, StreamingInfo.Path))
							{
								if (res == null)
								{
									return new byte[0];
								}

								byte[] data = new byte[FSBResource.Size];
								using (PartialStream resStream = new PartialStream(res.Stream, res.Offset, res.Size))
								{
									resStream.Position = StreamingInfo.Offset;
									resStream.ReadBuffer(data, 0, data.Length);
								}
								return data;
							}
						}
					}
				}
				return m_audioData;
			}
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadLoadType(reader.Version))
			{
				LoadType = (AudioClipLoadType)reader.ReadInt32();
				Channels = reader.ReadInt32();
				Frequency = reader.ReadInt32();
				BitsPerSample = reader.ReadInt32();
				Length = reader.ReadSingle();

				if (IsReadIsTrackerFormat(reader.Version))
				{
					IsTrackerFormat = reader.ReadBoolean();
				}
				if (IsReadAmbisonic(reader.Version))
				{
					Ambisonic = reader.ReadBoolean();
				}
				if (IsAlignTrackerFormat(reader.Version))
				{
					reader.AlignStream(AlignType.Align4);
				}

				if (IsReadAudioClipFlags(reader.Version))
				{
					AudioClipFlags = reader.ReadInt32();
				}
				if (IsReadFSBResourceFirst(reader.Version))
				{
					FSBResource.Read(reader);
				}

				SubsoundIndex = reader.ReadInt32();
				PreloadAudioData = reader.ReadBoolean();
				LoadInBackground = reader.ReadBoolean();
				Legacy3D = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);

				if (!IsReadFSBResourceFirst(reader.Version))
				{
					FSBResource.Read(reader);
				}

				if (IsReadType(reader.Version))
				{
					Type = (FMODSoundType)reader.ReadInt32();
				}
				if (IsReadCompressionFormat(reader.Version))
				{
					CompressionFormat = (AudioCompressionFormat)reader.ReadInt32();
				}
				reader.AlignStream(AlignType.Align4);

#if UNIVERSAL
				if (IsReadEditorResource(reader.Flags))
				{
					EditorResource.Read(reader);
					if (IsReadCompressionFormat(reader.Version))
					{
						EditorCompressionFormat = (AudioCompressionFormat)reader.ReadInt32();
					}
				}
#endif
			}
			else
			{
				if (IsReadDecompressOnLoadFirst(reader.Version))
				{
					DecompressOnLoad = reader.ReadBoolean();
				}

				Format = (FMODSoundFormat)reader.ReadInt32();
				if (IsReadType(reader.Version))
				{
					Type = (FMODSoundType)reader.ReadInt32();
				}
				if (IsReadLength(reader.Version))
				{
					Length = reader.ReadSingle();
					Frequency = reader.ReadInt32();
					Size = reader.ReadInt32();
				}

				if (IsReadDecompressOnLoadSecond(reader.Version))
				{
					DecompressOnLoad = reader.ReadBoolean();
				}
				if (IsRead3D(reader.Version))
				{
					Legacy3D = reader.ReadBoolean();
				}
				if (IsReadUseHardware(reader.Version))
				{
					UseHardware = reader.ReadBoolean();
				}
				if (IsAlignBools(reader.Version))
				{
					reader.AlignStream(AlignType.Align4);
				}

				if (IsStreamInt32(reader.Version))
				{
					LoadType = (AudioClipLoadType)reader.ReadInt32();
				}

				if (IsReadStreamingInfo(reader.Version))
				{
					bool isInnerData = true;
					if (LoadType == AudioClipLoadType.Streaming)
					{
						using (ResourcesFile res = File.Collection.FindResourcesFile(File, StreamingFileName))
						{
							isInnerData = res == null;
						}
					}
					if (isInnerData)
					{
						m_audioData = reader.ReadByteArray();
						reader.AlignStream(AlignType.Align4);
					}
					else
					{
						StreamingInfo.Read(reader, StreamingFileName);
					}
				}
				else
				{
					m_audioData = reader.ReadByteArray();
					if (IsAlignAudioData(reader.Version))
					{
						reader.AlignStream(AlignType.Align4);
					}
				}

				if (IsReadDecompressOnLoadThird(reader.Version))
				{
					DecompressOnLoad = reader.ReadBoolean();
				}

				if (IsReadStream(reader.Version))
				{
					if (!IsStreamInt32(reader.Version))
					{
						LoadType = reader.ReadBoolean() ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
					}
				}
			}
		}

		public override void ExportBinary(IExportContainer container, Stream stream)
		{
			if (IsReadLoadType(container.Version))
			{
				using (ResourcesFile res = File.Collection.FindResourcesFile(File, FSBResource.Source))
				{
					if (res == null)
					{
						Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{ValidName}' because resources file '{FSBResource.Source}' hasn't been found");
						return;
					}

					if (StreamedResource.IsReadSize(container.Version))
					{
						using (PartialStream resStream = new PartialStream(res.Stream, res.Offset, res.Size))
						{
							resStream.Position = FSBResource.Offset;
							resStream.CopyStream(stream, FSBResource.Size);
						}
					}
					else
					{
						// I think they read data by its type for this verison, so I can't even export raw data :/
						Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{ValidName}' because of unknown size");
					}
				}
			}
			else
			{
				if (IsReadStreamingInfo(container.Version))
				{
					if (LoadType == AudioClipLoadType.Streaming)
					{
						if (m_audioData == null)
						{
							using (ResourcesFile res = File.Collection.FindResourcesFile(File, StreamingInfo.Path))
							{
								if (res == null)
								{
									Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{ValidName}' because resources file '{StreamingInfo.Path}' hasn't been found");
									return;
								}

								using (PartialStream resStream = new PartialStream(res.Stream, res.Offset, res.Size))
								{
									resStream.Position = StreamingInfo.Offset;
									resStream.CopyStream(stream, StreamingInfo.Size);
								}
							}
						}
						else
						{
							stream.Write(m_audioData, 0, m_audioData.Length);
						}
					}
					else
					{
						stream.Write(m_audioData, 0, m_audioData.Length);
					}
				}
				else
				{
					stream.Write(m_audioData, 0, m_audioData.Length);
				}
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			throw new NotSupportedException();
			/*YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(container.ExportVersion);
			node.Add(LoadTypeName, (int)LoadType);
			node.Add(ChannelsName, Channels);
			node.Add(FrequencyName, Frequency);
			node.Add(BitsPerSampleName, BitsPerSample);
			node.Add(LengthName, Length);
			node.Add(IsTrackerFormatName, IsTrackerFormat);
			node.Add(AmbisonicName, Ambisonic);
			node.Add(SubsoundIndexName, SubsoundIndex);
			node.Add(PreloadAudioDataName, PreloadAudioData);
			node.Add(LoadInBackgroundName, LoadInBackground);
			node.Add(Legacy3DName, Legacy3D);
			node.Add(ResourceName, FSBResource.ExportYAML(container));
			node.Add(CompressionFormatName, (int)CompressionFormat);
			node.Add(EditorResourceName, EditorResource.ExportYAML(container));
			node.Add(EditorCompressionFormatName, (int)EditorCompressionFormat);
			return node;*/
		}

		public override string ExportExtension
		{
			get
			{
				if (IsReadType(File.Version))
				{
					switch (Type)
					{
						case FMODSoundType.ACC:
							return "m4a";
						case FMODSoundType.AIFF:
							return "aif";
						case FMODSoundType.IT:
							return "it";
						case FMODSoundType.MOD:
							return "mod";
						case FMODSoundType.MPEG:
							return "mp3";
						case FMODSoundType.OGGVORBIS:
							return "ogg";
						case FMODSoundType.S3M:
							return "s3m";
						case FMODSoundType.WAV:
							return "wav";
						case FMODSoundType.XM:
							return "xm";
						case FMODSoundType.XMA:
							return "wav";
						case FMODSoundType.VAG:
							return "vag";
						case FMODSoundType.AUDIOQUEUE:
							return "fsb";
						default:
							return "audioClip";
					}
				}
				else
				{
					switch (CompressionFormat)
					{
						case AudioCompressionFormat.PCM:
						case AudioCompressionFormat.ADPCM:
						case AudioCompressionFormat.Vorbis:
						case AudioCompressionFormat.MP3:
						case AudioCompressionFormat.GCADPCM:
							return "fsb";
						case AudioCompressionFormat.VAG:
						case AudioCompressionFormat.HEVAG:
							return "vag";
						case AudioCompressionFormat.XMA:
							return "wav";
						case AudioCompressionFormat.AAC:
							return "m4a";
						case AudioCompressionFormat.ATRAC9:
							return "at9";
						default:
							return "audioClip";
					}
				}
			}
		}

		public override bool IsValid
		{
			get
			{
				if (IsReadLoadType(File.Version))
				{
					return true;
				}
				if (IsReadStreamingInfo(File.Version))
				{
					if (LoadType == AudioClipLoadType.Streaming)
					{
						if (m_audioData == null)
						{
							return true;
						}
					}
				}
				return m_audioData.Length > 0;
			}
		}

		/// <summary>
		/// Stream previously
		/// </summary>
		public AudioClipLoadType LoadType { get; private set; }
		public int Channels { get; private set; }
		public int BitsPerSample { get; private set; }
		public bool IsTrackerFormat { get; private set; }
		public bool Ambisonic { get; private set; }
		public int AudioClipFlags { get; private set; }
		public int SubsoundIndex { get; private set; }
		public bool PreloadAudioData { get; private set; }
		public bool LoadInBackground { get; private set; }
		public AudioCompressionFormat CompressionFormat { get; private set; }
#if UNIVERSAL
		public AudioCompressionFormat EditorCompressionFormat { get; private set; }
#endif

		public bool DecompressOnLoad { get; private set; }
		public FMODSoundFormat Format { get; private set; }
		/// <summary>
		/// SoundType in some versions
		/// </summary>
		public FMODSoundType Type {get; private set; }
		public float Length { get; private set; }
		public int Frequency { get; private set; }
		public int Size { get; private set; }
		/// <summary>
		/// 3D previously
		/// </summary>
		public bool Legacy3D { get; private set; }
		public bool UseHardware { get; private set; }
		public IReadOnlyList<byte> AudioData => m_audioData;

		private string StreamingFileName => File.Name + "." + StreamingFileExtension;

		public const string StreamingFileExtension = "resS";

		public const string LoadTypeName = "m_LoadType";
		public const string ChannelsName = "m_Channels";
		public const string FrequencyName = "m_Frequency";
		public const string BitsPerSampleName = "m_BitsPerSample";
		public const string LengthName = "m_Length";
		public const string IsTrackerFormatName = "m_IsTrackerFormat";
		public const string AmbisonicName = "m_Ambisonic";
		public const string SubsoundIndexName = "m_SubsoundIndex";
		public const string PreloadAudioDataName = "m_PreloadAudioData";
		public const string LoadInBackgroundName = "m_LoadInBackground";
		public const string Legacy3DName = "m_Legacy3D";
		public const string ResourceName = "m_Resource";
		public const string CompressionFormatName = "m_CompressionFormat";
		public const string EditorResourceName = "m_EditorResource";
		public const string EditorCompressionFormatName = "m_EditorCompressionFormat";

		public StreamedResource FSBResource;
#if UNIVERSAL
		public StreamedResource EditorResource;
#endif
		public StreamingInfo StreamingInfo;

		private byte[] m_audioData;
	}
}
