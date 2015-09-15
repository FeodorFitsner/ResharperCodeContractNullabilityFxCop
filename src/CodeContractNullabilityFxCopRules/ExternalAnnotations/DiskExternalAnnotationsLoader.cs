﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using CodeContractNullabilityFxCopRules.ExternalAnnotations.Storage;
using JetBrains.Annotations;

namespace CodeContractNullabilityFxCopRules.ExternalAnnotations
{
    /// <summary>
    /// Scans the filesystem for Resharper external annotations in xml files.
    /// </summary>
    /// <remarks>
    /// Resharper provides downloadable xml definitions that contain decoration of built-in .NET Framework types. When a class
    /// derives from such a built-in type, we need to have those definitions available because Resharper reports nullability
    /// annotation as unneeded when a base type is already decorated.
    /// </remarks>
    public static class DiskExternalAnnotationsLoader
    {
        [NotNull]
        private static readonly string CachePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"CodeContractNullabilityAnalyzer\external-annotations-cache.xml");

        [NotNull]
        public static ExternalAnnotationsMap Create()
        {
            try
            {
                ExternalAnnotationsMap map = GetCached();
                if (map.Count > 0)
                {
                    return map;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load Resharper external annotations.", ex);
            }

            throw new Exception("Failed to load Resharper external annotations.");
        }

        [NotNull]
        private static ExternalAnnotationsMap GetCached()
        {
            ExternalAnnotationsCache cached = TryGetCacheFromDisk();
            DateTime highestLastWriteTimeUtcOnDisk = cached != null ? GetHighestLastWriteTimeUtc() : DateTime.MinValue;

            if (cached == null || cached.LastWriteTimeUtc < highestLastWriteTimeUtcOnDisk)
            {
                ExternalAnnotationsMap newExternalAnnotations = ScanForMemberExternalAnnotations();
                cached = new ExternalAnnotationsCache(highestLastWriteTimeUtcOnDisk, newExternalAnnotations);
                SaveToDisk(cached);
            }

            return cached.ExternalAnnotations;
        }

        [CanBeNull]
        private static ExternalAnnotationsCache TryGetCacheFromDisk()
        {
            try
            {
                if (File.Exists(CachePath))
                {
                    var serializer = new DataContractSerializer(typeof (ExternalAnnotationsCache));
                    using (FileStream stream = File.OpenRead(CachePath))
                    {
                        return (ExternalAnnotationsCache) serializer.ReadObject(stream);
                    }
                }
            }
            catch (IOException)
            {
            }
            catch (XmlException)
            {
            }

            return null;
        }

        private static DateTime GetHighestLastWriteTimeUtc()
        {
            DateTime highestLastWriteTimeUtc = DateTime.MinValue;
            foreach (string path in EnumerateAnnotationFiles())
            {
                var fileInfo = new FileInfo(path);
                if (fileInfo.LastWriteTimeUtc > highestLastWriteTimeUtc)
                {
                    highestLastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
                }
            }

            return highestLastWriteTimeUtc;
        }

        private static void SaveToDisk([NotNull] ExternalAnnotationsCache cache)
        {
            EnsureDirectoryExists();

            var serializer = new DataContractSerializer(typeof (ExternalAnnotationsCache));
            using (FileStream stream = File.Create(CachePath))
            {
                serializer.WriteObject(stream, cache);
            }
        }

        private static void EnsureDirectoryExists()
        {
            string folder = Path.GetDirectoryName(CachePath);
            if (folder != null)
            {
                Directory.CreateDirectory(folder);
            }
        }

        [NotNull]
        private static ExternalAnnotationsMap ScanForMemberExternalAnnotations()
        {
            var result = new ExternalAnnotationsMap();
            var parser = new ExternalAnnotationDocumentParser();

            foreach (string path in EnumerateAnnotationFiles())
            {
                using (StreamReader reader = File.OpenText(path))
                {
                    parser.ProcessDocument(reader, result);
                }
            }

            Compact(result);
            return result;
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<string> EnumerateAnnotationFiles()
        {
            string localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            var pathsToTry = new[]
            {
                Path.Combine(localAppDataFolder, @"JetBrains\Installations\ReSharperPlatformVs14\ExternalAnnotations"),
                Path.Combine(localAppDataFolder, @"JetBrains\Installations\ReSharperPlatformVs12\ExternalAnnotations"),
                Path.Combine(localAppDataFolder, @"JetBrains\ReSharper\vAny\packages")
            };

            foreach (string path in pathsToTry)
            {
                if (Directory.Exists(path))
                {
                    return Directory.EnumerateFiles(path, "*.xml", SearchOption.AllDirectories);
                }
            }

            return new string[0];
        }

        private static void Compact([NotNull] ExternalAnnotationsMap externalAnnotations)
        {
            foreach (string key in externalAnnotations.Keys.ToList())
            {
                MemberNullabilityInfo annotation = externalAnnotations[key];
                if (!HasNullabilityDefined(annotation))
                {
                    externalAnnotations.Remove(key);
                }
            }
        }

        private static bool HasNullabilityDefined([NotNull] MemberNullabilityInfo info)
        {
            return info.HasNullabilityDefined || info.ParametersNullability.Count > 0;
        }
    }
}