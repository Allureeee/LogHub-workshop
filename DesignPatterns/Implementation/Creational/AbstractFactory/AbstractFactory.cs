using System;

namespace Implementation.Creational.AbstractFactory
{
	abstract class AbstractFactory
	{
		public abstract AbstractProductA CreateProductA();
		public abstract AbstractProductB CreateProductB();
	}
	class ConcreteFactory1 : AbstractFactory
	{
		public override AbstractProductA CreateProductA()
		{
			return new ProductA1();
		}

		public override AbstractProductB CreateProductB()
		{
			return new ProductB1();
		}
	}
	class ConcreteFactory2 : AbstractFactory
	{
		public override AbstractProductA CreateProductA()
		{
			return new ProductA2();
		}

		public override AbstractProductB CreateProductB()
		{
			return new ProductB2();
		}
	}

	abstract class AbstractProductA
	{
		public abstract string Describe();
	}

	abstract class AbstractProductB
	{
		public abstract string Describe();
	}

	class ProductA1 : AbstractProductA
	{
		public override string Describe() => "ProductA1 (семейство 1)";
	}

	class ProductB1 : AbstractProductB
	{
		public override string Describe() => "ProductB1 (семейство 1)";
	}

	class ProductA2 : AbstractProductA
	{
		public override string Describe() => "ProductA2 (семейство 2)";
	}

	class ProductB2 : AbstractProductB
	{
		public override string Describe() => "ProductB2 (семейство 2)";
	}

	/// <summary>
	/// Клиент знает только абстрактные типы. Какое семейство ему досталось,
	/// он выяснить не может и не должен — в этом и смысл паттерна.
	/// </summary>
	class Client
	{
		private readonly AbstractProductA _productA;
		private readonly AbstractProductB _productB;

		public Client(AbstractFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			// Оба продукта берутся у одной фабрики — поэтому они заведомо
			// совместимы между собой. Смешать A1 с B2 клиент не сможет.
			_productA = factory.CreateProductA();
			_productB = factory.CreateProductB();
		}

		public void Run()
		{
			Console.WriteLine($"  получено: {_productA.Describe()}");
			Console.WriteLine($"  получено: {_productB.Describe()}");
		}
	}

	/// <summary>
	/// Запуск канонической схемы: один и тот же клиент работает
	/// с двумя разными семействами продуктов.
	/// </summary>
	static class CanonicalDemo
	{
		public static void Run()
		{
			Console.WriteLine("Клиент с ConcreteFactory1:");
			new Client(new ConcreteFactory1()).Run();

			Console.WriteLine();
			Console.WriteLine("Клиент с ConcreteFactory2:");
			new Client(new ConcreteFactory2()).Run();

			Console.WriteLine();
			Console.WriteLine("Код клиента одинаковый. Менялась только фабрика,");
			Console.WriteLine("переданная в конструктор.");
		}
	}
}
