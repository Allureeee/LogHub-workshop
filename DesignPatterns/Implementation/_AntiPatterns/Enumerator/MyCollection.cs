using System.Collections;

// АНТИПРИМЕР. Не копируйте этот код в лабораторные работы.
//
// MyCollection одновременно реализует IEnumerable и IEnumerator, то есть коллекция
// является собственным итератором и хранит позицию обхода в себе. Последствия:
//   * два вложенных foreach по одной коллекции сбивают друг другу позицию;
//   * коллекцию нельзя обходить из нескольких потоков;
//   * повторный обход требует ручного Reset().
// Именно поэтому в паттерне Iterator состояние обхода выносится в отдельный объект —
// см. Implementation/Behavioral/Iterator/ и лабораторную работу 3.
//
// Рядом лежит MyCollectionEnumerator — правильный вариант с вынесенным итератором.

namespace Implementation.AntiPatterns.Enumerator
{
	internal class MyCollection : IEnumerable, IEnumerator
	{
		private readonly ArrayList _items = new ArrayList();
		private int _current;
		public MyCollection(object[] items)
		{
			_items.AddRange(items);
		}

		public IEnumerator GetEnumerator()
		{
			Reset();
			return this;
		}
		public bool MoveNext()
		{
			if (_items.Count > _current)
			{
				_current++;
				return true;
			}
			return false;
		}

		public void Reset()
		{
			_current = 0;
		}

		public object Current
		{
			get { return _items[_current - 1]; }
		}
	}

	internal class MyCollectionEnumerator : IEnumerator
	{
		private readonly ArrayList _items;
		private int _current;

		public MyCollectionEnumerator(ArrayList items)
		{
			_items = items;
			_current = 0;
		}

		public bool MoveNext()
		{
			if (_items.Count > _current)
			{
				_current++;
				return true;
			}
			return false;
		}

		public void Reset()
		{
			_current = 0;
		}

		public object Current
		{
			get { return _items[_current-1]; }
		}
	}
}