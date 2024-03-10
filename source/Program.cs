// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Setup.Configuration;

namespace Sledge.VsExtensionDetector
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
	}

	public class DisposableAction : IDisposable
	{
		private Action _Action;
		public DisposableAction(Action action) => _Action = action;
		public void Dispose() => _Action.Invoke();
	}
}
