using System.Collections.Concurrent;
using System.Collections.Generic;
using Implementation.Creational.Singleton;

namespace Implementation.Tests.Singleton
{
	class ConfigServiceStub : IConfigService
	{
		public ConfigServiceStub()
		{
			_dic["count"] = "0";
		}
		public string GetValue(string key)
		{
			return _dic[key];
		}

		public void SetValue(string key, string value)
		{
			_dic[key] = value;
		}
		readonly ConcurrentDictionary<string, string> _dic = new ConcurrentDictionary<string, string>();
	}
}