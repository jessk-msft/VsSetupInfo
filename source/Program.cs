// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Xml;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.VisualStudio.Setup.Configuration;

namespace VsSetupInfo
{
	class Program
	{
		public static Task<int> Main( string[] args )
		{
			int result = 0;
			try
			{
				SetupConfiguration configuration = new SetupConfiguration();
				using IDisposable configurationObject = CreateDisposableComObject(configuration);

				IEnumSetupInstances instances = configuration.EnumAllInstances();
				using IDisposable instancesObject = CreateDisposableComObject(instances);

				while (instances != null) 
				{
					ISetupInstance[] instanceArray = new ISetupInstance[1];
					instances.Next(1, instanceArray, out int numFetched);
					if (numFetched != 1)
					{
						break;
					}

					ISetupInstance instance = instanceArray[0];
					using IDisposable instanceObject = CreateDisposableComObject(instance);

					Console.WriteLine("Id: {0}", instance.GetInstanceId());
					Console.WriteLine("Name: {0}", instance.GetInstallationName());
					Console.WriteLine("DisplayName: {0}", instance.GetDisplayName());
					Console.WriteLine("Location: {0}", instance.GetInstallationPath());
					Console.WriteLine("Version: {0}", instance.GetInstallationVersion());

					if (instance is ISetupInstance2 instance2)
					{
						Console.WriteLine("State: {0}", instance2.GetState());
						Console.WriteLine("Properties: {0}", PropertiesToJsonString(instance2.GetProperties()));
						Console.WriteLine("Product: {0}", PackageToJsonString(instance2.GetProduct()));
						Console.WriteLine("ProductPath: {0}", instance2.GetProductPath());
						Console.WriteLine("EnginePath: {0}", instance2.GetEnginePath());
						foreach (ISetupPackageReference package in instance2.GetPackages())
						{
							Console.WriteLine("Package: {0}", PackageToJsonString(package));
						}
					}

					foreach (ISetupPackageReference package in GetUserExtensionPackages(instance.GetInstallationVersion()))
					{
						Console.WriteLine("Extension: {0}", PackageToJsonString(package));
					}

					Console.WriteLine("");
				}
			}
			catch (Exception e)
			{ 
				Console.WriteLine("Unhandled exception: {0}", e.Message);
				result = 1;
			}

			return Task.FromResult(result);
		}

		private static string ToJsonString(object obj)
		{
			return System.Text.Json.JsonSerializer.Serialize(obj);
		}

		private static string PropertiesToJsonString(ISetupPropertyStore store)
		{
			return ToJsonString(store?.GetNames()?.Select(n => new KeyValuePair<string, string>(n, store.GetValue(n)))?.ToList());
		}

		private static string PackageToJsonString(ISetupPackageReference package)
		{
			ISetupProductReference product = package as ISetupProductReference;
			ISetupProductReference2 product2 = package as ISetupProductReference2;

			return ToJsonString(new 
			{ 
				Id = package.GetId(), 
				Version = package.GetVersion(),
				Chip = package.GetChip(),
				Language = package.GetLanguage(),
				Branch = package.GetBranch(),
				Type = package.GetType(),
				UniqueId = package.GetUniqueId(),
				IsExtension = package.GetIsExtension(),
				ProductIsInstalled = product?.GetIsInstalled(),
				Product2SupportsExtensions = product2?.GetSupportsExtensions(),
			});
		}

		private static IDisposable CreateDisposableComObject(object obj)
		{
			return new DisposableAction(() => Marshal.ReleaseComObject(obj));
		}

		private static IEnumerable<ISetupPackageReference> GetUserExtensionPackages(string vsVersionString)
		{
			if (Version.TryParse(vsVersionString, out Version vsVersion))
			{
				string vsAppDataFolder = String.Format("{0}\\Microsoft\\VisualStudio", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
				foreach (string vsAppDataVersionFolder in TryEnumerateDirectories(vsAppDataFolder, String.Format("{0}.*", vsVersion.Major)))
				{
					foreach (string vsExtensionPackageFolder in TryEnumerateDirectories(String.Format("{0}\\Extensions", vsAppDataVersionFolder), "*"))
					{
						ISetupPackageReference package = ReadVsixPackageReference(vsExtensionPackageFolder);
						if (package != null) 
						{
							yield return package;
						}
					}
				}
			}
		}

		private static ISetupPackageReference ReadVsixPackageReference(string packageFolder)
		{
			try
			{
				UserExtensionPackageReference package = new UserExtensionPackageReference();

				string manifestFile = String.Format("{0}\\manifest.json", packageFolder);
				if (File.Exists(manifestFile)) 
				{
					JsonDocument document = JsonDocument.Parse(File.ReadAllText(manifestFile));

					if (document.RootElement.TryGetProperty("id", out JsonElement id))
						package.Id = id.GetString();
					if (document.RootElement.TryGetProperty("version", out JsonElement version))
						package.Version = version.GetString();
					if (document.RootElement.TryGetProperty("type", out JsonElement type))
						package.Type = type.GetString();
					if (document.RootElement.TryGetProperty("vsixId", out JsonElement vsixId))
						package.UniqueId = vsixId.GetString();
				}

				string vsixManifestFile = String.Format("{0}\\extension.vsixmanifest", packageFolder);
				if (File.Exists(vsixManifestFile))
				{
					XmlDocument document = new XmlDocument();
					using (XmlTextReader reader = new XmlTextReader(vsixManifestFile))
					{
						reader.Namespaces = false;
						document.Load(reader);
					}

					if (document.SelectSingleNode("PackageManifest/Metadata/Identity") is XmlElement identityElement)
					{
						if (package.Id == null)
							package.Id = identityElement.GetAttribute("Id");
						if (package.Language == null)
							package.Language = identityElement.GetAttribute("Language");
						if (package.Version == null)
							package.Version = identityElement.GetAttribute("Version");
					}
					if (document.SelectSingleNode("PackageManifest/Metadata/DisplayName") is XmlElement displayNameElement)
					{
						package.Branch = displayNameElement.InnerText;
					}
					if (document.SelectSingleNode("PackageManifest/Installation/InstallationTarget/ProductArchitecture") is XmlElement archElement)
					{
						package.Chip = archElement.InnerText;
					}
				}

				if (package.IsValid)
				{
					return package;
				}
			}
			catch (Exception e) 
			{
				Console.WriteLine("Exception reading folder \"{0}\". {1}", packageFolder, e.Message);
			}
			return null;
		}

		private static IEnumerable<string> TryEnumerateDirectories(string srcFolder, string srcPattern = "*", SearchOption srcOption = SearchOption.TopDirectoryOnly)
		{
			try
			{
				if (Directory.Exists(srcFolder))
				{
					return Directory.EnumerateDirectories(srcFolder, srcPattern, srcOption);
				}
			}
			catch {}
			return Enumerable.Empty<string>();
		}
	}

	public class UserExtensionPackageReference : ISetupPackageReference
	{
		public string Id { get; set; }
		public string Version { get; set; }
		public string Chip { get; set; }
		public string Language { get; set; }
		public string Branch { get; set; }
		public string Type { get; set; }
		public string UniqueId { get; set; }

		string ISetupPackageReference.GetId() => Id;
		string ISetupPackageReference.GetVersion() => Version;
		string ISetupPackageReference.GetChip() => Chip;
		string ISetupPackageReference.GetLanguage() => Language;
		string ISetupPackageReference.GetBranch() => Branch;
		string ISetupPackageReference.GetType() => Type;
		string ISetupPackageReference.GetUniqueId() => UniqueId;
		bool ISetupPackageReference.GetIsExtension() => true;

		public bool IsValid
		{
			get 
			{ 
				return typeof(UserExtensionPackageReference)
					.GetProperties(BindingFlags.Instance|BindingFlags.Public)
					.Any(p => !String.IsNullOrEmpty(p.GetValue(this) as string));
			}
		}
	}

	public class DisposableAction : IDisposable
	{
		private Action _Action;
		public DisposableAction(Action action) => _Action = action;
		public void Dispose() => _Action.Invoke();
	}
}
