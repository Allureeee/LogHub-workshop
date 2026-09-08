using System;
using System.Diagnostics;
using System.Threading;

namespace Implementation.Creational.Singleton
{
	public class DemoSingleton
	{
		// АНТИПРИМЕР: двойная проверка блокировки (double-checked locking) без volatile.
		// Без volatile процессор и JIT вправе переупорядочить запись ссылки и инициализацию
		// полей объекта, поэтому другой поток может получить ссылку на ещё не готовый экземпляр.
		// Правильный вариант — Lazy<T>, см. LazyConfigService.cs и главу Textbook/Порождающие/Singleton.md.
		private static DemoSingleton _instance;

		private DemoSingleton()
		{
		}

		public static DemoSingleton Instance()
		{
			if (_instance == null)
			{
				lock (lockObject)
				{
					if (_instance == null)
						_instance = new DemoSingleton();
				}
			}
		
			return _instance;
		}

		// АНТИПРИМЕР: возвращает _instance без инициализации.
		// Если Instance() ещё ни разу не вызывали, метод вернёт null,
		// и клиент получит NullReferenceException в неожиданном месте.
		public static DemoSingleton Instance1()
		{
			return _instance;
		}

		public void IncCounter()
		{
			Interlocked.Increment(ref _counter);
			// lock (this)
			// {
			// 	_counter++;
			// }

		}
		public int GetCounter()
		{ 
			return _counter;
		}
		private int _counter = 0;
		private static object lockObject = new object();
	}

	public class DemoSingletonApp
	{
		static public void Run()
		{
			var threadsCount = 1000;
			var threads = new Thread[threadsCount];

			var sw = new Stopwatch();
			sw.Start();

			for (int i = 0; i < threadsCount; i++)
			{
				threads[i] = new Thread(Start);
				threads[i].Start();
			}

			for (int i = 0; i < threadsCount; i++)
			{
				threads[i].Join();
			}
			sw.Stop();
			Console.WriteLine(sw.Elapsed);
			Console.WriteLine(DemoSingleton.Instance().GetCounter());
		}


		static void Start()
		{
			for (int i = 0; i < 10000; i++)
			{
				DemoSingleton.Instance().IncCounter();
			}
		}
	}
	}